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
///     Internally, the set uses a single buffer to store weak handles and a free pool for reclaimed handles.
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
	private readonly ConcurrentQueue<WeakGCHandle<object>> _freeHandles = [];

	/// <summary>
	///     Buffer of weak handles currently holding targets.
	/// </summary>
	private readonly ConcurrentQueue<WeakGCHandle<object>> _handles = [];

	/// <summary>
	///     Provides thread-safe read and write access to the protected resource.
	/// </summary>
	/// <remarks>
	///     This lock allows multiple threads to read concurrently while ensuring exclusive access for write
	///     operations. It should be used to synchronize access to shared data within the containing class.
	/// </remarks>
	private readonly ReaderWriterLockSlim _lock = new();

	/// <summary>
	///     A weak <see cref="GCHandle" /> to a <see cref="Cleaner" /> instance that triggers periodic cleanup.
	/// </summary>
	private GCHandle _cleanerHandle;

	/// <summary>
	///     Initializes a new instance of the <see cref="WeakReferenceSet" /> class.
	/// </summary>
	/// <remarks>
	///     The constructor creates a weak <see cref="GCHandle" /> to a lightweight <see cref="Cleaner" /> object.
	///     The cleaner's finalizer periodically compacts the set, reclaiming collected handles and reusing them,
	///     without holding strong references to the owning set.
	/// </remarks>
	public WeakReferenceSet() => _cleanerHandle = GCHandle.Alloc(new Cleaner(this), GCHandleType.Weak);

#region Interface IDisposable

	/// <summary>
	///     Releases all resources used by the set, freeing handles and suppressing finalization.
	/// </summary>
	/// <remarks>
	///     This method frees both active and reclaimed <see cref="GCHandle" /> instances and suppresses the finalizer
	///     to avoid double-free. It is safe to call multiple times.
	/// </remarks>
	public void Dispose()
	{
		FreeHandles();

		GC.SuppressFinalize(this);
	}

#endregion

	/// <summary>
	///     Adds an instance to the set using a weak reference.
	/// </summary>
	/// <param name="instance">The object instance to store. If <c>null</c>, the call is ignored.</param>
	/// <remarks>
	///     The object is stored via a <see cref="GCHandleType.Weak" /> handle. If a recycled handle is available,
	///     it is reused to reduce allocations. A <see cref="Cleaner" /> is ensured to exist to keep the set compact.
	/// </remarks>
	public void Add(object instance)
	{
		ObjectDisposedException.ThrowIf(!_cleanerHandle.IsAllocated, this);

		if (_freeHandles.TryDequeue(out var handle))
		{
			handle.SetTarget(instance);
			_handles.Enqueue(handle);
		}
		else
		{
			_handles.Enqueue(new WeakGCHandle<object>(instance));
		}
	}

	private bool TryDequeueSafe(out WeakGCHandle<object> handle)
	{
		if (_handles.TryDequeue(out handle))
		{
			return true;
		}

		try
		{
			_lock.EnterReadLock();

			return _handles.TryDequeue(out handle);
		}
		finally
		{
			_lock.ExitReadLock();
		}
	}

	/// <summary>
	///     Attempts to retrieve a live instance from the set.
	/// </summary>
	/// <param name="instance">When this method returns <c>true</c>, contains a live object instance; otherwise <c>null</c>.</param>
	/// <returns><c>true</c> if a live instance was found; otherwise, <c>false</c>.</returns>
	/// <remarks>
	///     Retrieval scans the buffer and reclaims collected entries into the free pool for reuse.
	/// </remarks>
	public bool TryTake([NotNullWhen(true)] out object? instance)
	{
		ObjectDisposedException.ThrowIf(!_cleanerHandle.IsAllocated, this);

		while (TryDequeueSafe(out var handle))
		{
			if (handle.TryGetTarget(out instance))
			{
				handle.SetTarget(null!);

				_freeHandles.Enqueue(handle);

				return true;
			}

			_freeHandles.Enqueue(handle);
		}

		instance = null;

		return false;
	}

	/// <summary>
	///     Performs maintenance: compacts the buffer by moving live handles back and reclaiming dead ones.
	///     Reclaimed handles are kept for reuse and only freed upon dispose/finalization.
	/// </summary>
	/// <remarks>
	///     This method disposes previously reclaimed handles to release their underlying <see cref="GCHandle" /> resources
	///     and then iteratively compacts the active buffer in fixed-size batches to reduce contention.
	///     After cleaning, a new <see cref="Cleaner" /> instance is assigned to the weak cleaner handle's target
	///     (if still allocated) to ensure future periodic maintenance cycles.
	/// </remarks>
	private void Clean()
	{
		while (_freeHandles.TryDequeue(out var handle))
		{
			handle.Dispose();
		}

		for (var count = _handles.Count; count > 0; count --)
		{
			WeakGCHandle<object> handle;

			try
			{
				_lock.EnterWriteLock();

				if (!_handles.TryDequeue(out handle))
				{
					break;
				}

				if (handle.TryGetTarget(out _))
				{
					_handles.Enqueue(handle);

					continue;
				}
			}
			finally
			{
				_lock.ExitWriteLock();
			}

			_freeHandles.Enqueue(handle);
		}

		if (_cleanerHandle.IsAllocated)
		{
			_cleanerHandle.Target = new Cleaner(this);
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
		if (!_cleanerHandle.IsAllocated)
		{
			return;
		}

		try
		{
			_cleanerHandle.Free();
		}
		catch (InvalidOperationException)
		{
			return; // Already freed
		}

		while (_freeHandles.TryDequeue(out var handle))
		{
			handle.Dispose();
		}

		while (TryDequeueSafe(out var handle))
		{
			handle.Dispose();
		}
	}

	/// <summary>
	///     Finalizer that ensures all handles are freed.
	/// </summary>
	~WeakReferenceSet() => FreeHandles();

	/// <summary>
	///     Lightweight helper whose finalizer triggers set cleanup without holding strong references.
	/// </summary>
	/// <param name="weakReferenceSet">
	///     The owning <see cref="WeakReferenceSet" /> to clean when this helper is
	///     collected.
	/// </param>
	private class Cleaner(WeakReferenceSet weakReferenceSet)
	{
		/// <summary>
		///     Finalizer that invokes <see cref="WeakReferenceSet.Clean" /> on the owning set instance.
		/// </summary>
		~Cleaner() => weakReferenceSet.Clean();
	}
}