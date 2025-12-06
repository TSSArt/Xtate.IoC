// Copyright © 2019-2025 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Xtate.IoC;

/// <summary>
///     Provides a concurrent container of weak references to objects,
///     allowing instances to be added and later retrieved if they are still alive,
///     while automatically reclaiming and reusing <see cref="GCHandle" /> resources.
/// </summary>
/// <remarks>
///     Internally, the bag uses a single buffer to store weak handles and a free pool for reclaimed handles.
///     Periodic compaction segregates live and collected entries, minimizing contention and avoiding long-lived strong
///     references.
///     A lightweight finalizer-based <see cref="Cleaner" /> triggers cleanup and handle reuse without requiring explicit
///     calls.
/// </remarks>
internal sealed class WeakReferenceSet : IDisposable
{
	/// <summary>
	///     Pool of <see cref="GCHandle" /> instances available for reuse.
	/// </summary>
	private readonly ConcurrentQueue<GCHandle> _freeHandles = [];

	/// <summary>
	///     Buffer of weak handles currently holding targets.
	/// </summary>
	private readonly ConcurrentQueue<GCHandle> _handles = [];

	/// <summary>
	///     A weak <see cref="GCHandle" /> to a <see cref="Cleaner" /> instance that triggers periodic cleanup.
	/// </summary>
	private GCHandle _cleaner;

	/// <summary>
	///     Initializes a new instance of the <see cref="WeakReferenceSet" /> class and
	///     sets up a weakly-referenced <see cref="Cleaner" /> to perform background maintenance.
	/// </summary>
	public WeakReferenceSet() => _cleaner = GCHandle.Alloc(new Cleaner(this), GCHandleType.Weak);

#region Interface IDisposable

	/// <summary>
	///     Releases all resources used by the bag, freeing handles and suppressing finalization.
	/// </summary>
	public void Dispose()
	{
		FreeHandles();

		GC.SuppressFinalize(this);
	}

#endregion

	/// <summary>
	///     Adds an instance to the bag using a weak reference.
	/// </summary>
	/// <param name="instance">The object instance to store. If <c>null</c>, the call is ignored.</param>
	/// <remarks>
	///     The object is stored via a <see cref="GCHandleType.Weak" /> handle. If a recycled handle is available,
	///     it is reused to reduce allocations. A <see cref="Cleaner" /> is ensured to exist to keep the bag compact.
	/// </remarks>
	public void Add(object instance)
	{
		ObjectDisposedException.ThrowIf(!_cleaner.IsAllocated, this);

		if (instance is null)
		{
			return;
		}

		if (_freeHandles.TryDequeue(out var handle))
		{
			handle.Target = instance;
		}
		else
		{
			handle = GCHandle.Alloc(instance, GCHandleType.Weak);
		}

		_handles.Enqueue(handle);

		_cleaner.Target ??= new Cleaner(this);
	}

	/// <summary>
	///     Attempts to retrieve a live instance from the bag.
	/// </summary>
	/// <param name="instance">When this method returns <c>true</c>, contains a live object instance; otherwise <c>null</c>.</param>
	/// <returns><c>true</c> if a live instance was found; otherwise, <c>false</c>.</returns>
	/// <remarks>
	///     Retrieval scans the buffer and reclaims collected entries into the free pool for reuse.
	/// </remarks>
	public bool TryTake([NotNullWhen(true)] out object? instance)
	{
		ObjectDisposedException.ThrowIf(!_cleaner.IsAllocated, this);

		if (TryTakeLiveInstance(out instance))
		{
			return true;
		}

		lock (_handles)
		{
			return TryTakeLiveInstance(out instance);
		}
	}

	/// <summary>
	///     Internal retrieval helper that scans a buffer for a live target.
	/// </summary>
	/// <param name="instance">When this method returns <c>true</c>, contains a live object instance; otherwise <c>null</c>.</param>
	/// <returns><c>true</c> if a live instance was found; otherwise, <c>false</c>.</returns>
	private bool TryTakeLiveInstance([NotNullWhen(true)] out object? instance)
	{
		while (_handles.TryDequeue(out var handle))
		{
			if (handle.Target is not { } target)
			{
				_freeHandles.Enqueue(handle);

				continue;
			}

			instance = target;

			handle.Target = null;

			_freeHandles.Enqueue(handle);

			return true;
		}

		instance = null;

		return false;
	}

	/// <summary>
	///     Performs maintenance: compacts the buffer by moving live handles back and reclaiming dead ones.
	///     Reclaimed handles are kept for reuse and only freed upon dispose/finalization.
	/// </summary>
	private void Clean()
	{
		while (_freeHandles.TryDequeue(out var handle))
		{
			handle.Free();
		}

		var count = _handles.Count;

		if (count <= 32)
		{
			return;
		}

		lock (_handles)
		{
			for (; count > 0 && _handles.TryDequeue(out var handle); count --)
			{
				if (handle.Target is null)
				{
					_freeHandles.Enqueue(handle);
				}
				else
				{
					_handles.Enqueue(handle);
				}
			}
		}
	}

	/// <summary>
	///     Frees all allocated handles in both the free pool and active buffer, and releases the cleaner handle if allocated.
	/// </summary>
	/// <remarks>
	///     This method is used by <see cref="Dispose" /> and the finalizer to ensure no <see cref="GCHandle" /> instances
	///     remain allocated. It is safe to call multiple times.
	/// </remarks>
	private void FreeHandles()
	{
		if (!_cleaner.IsAllocated)
		{
			return;
		}

		_cleaner.Free();

		while (_freeHandles.TryDequeue(out var handle))
		{
			handle.Free();
		}

		while (_handles.TryDequeue(out var handle))
		{
			handle.Free();
		}
	}

	/// <summary>
	///     Finalizer that ensures all handles are freed.
	/// </summary>
	~WeakReferenceSet() => FreeHandles();

	/// <summary>
	///     Lightweight helper whose finalizer triggers bag cleanup without holding strong references.
	/// </summary>
	/// <param name="weakReferenceSet">
	///     The owning <see cref="WeakReferenceSet" /> to clean when this helper is
	///     collected.
	/// </param>
	private class Cleaner(WeakReferenceSet weakReferenceSet)
	{
		/// <summary>
		///     Finalizer that invokes <see cref="WeakReferenceSet.Clean" /> on the owning bag instance.
		/// </summary>
		~Cleaner() => weakReferenceSet.Clean();
	}
}