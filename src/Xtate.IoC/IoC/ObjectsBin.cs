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

namespace Xtate.IoC;

/// <summary>
///     Holds object instances scheduled for aggregated disposal.
/// </summary>
/// <remarks>
///     <para>
///         This bin tracks disposable instances (implementing <see cref="IDisposable" /> or
///         <see cref="IAsyncDisposable" />) via a weak-reference stack. Non‑disposable instances are ignored but still
///         validate disposal state.
///     </para>
///     <para>
///         Calling <see cref="Dispose" /> or <see cref="DisposeAsync" /> transitions the bin to a disposed state. Any
///         subsequent attempt to add an object results in an <see cref="ObjectDisposedException" /> (after eagerly
///         disposing the
///         provided instance if it is disposable).
///     </para>
///     <para>
///         Thread-safety: All public and internal members are safe for concurrent use. Disposal is performed exactly once
///         using an atomic exchange on the underlying storage field.
///     </para>
/// </remarks>
public class ObjectsBin
{
	private WeakReferenceStack? _instancesForDispose = new();

	/// <summary>
	///     Asynchronously disposes all tracked instances and marks the bin as disposed.
	/// </summary>
	/// <remarks>
	///     Each instance is disposed using <see cref="Disposer.DisposeAsync" />. If multiple threads race to dispose,
	///     only the first succeeds; others observe a no-op. After completion the bin rejects further additions.
	/// </remarks>
	/// <returns>A task representing the asynchronous disposal operation.</returns>
	internal ValueTask DisposeAsync()
	{
		if (Interlocked.CompareExchange(ref _instancesForDispose, value: null, _instancesForDispose) is not { } instancesForDispose)
		{
			return ValueTask.CompletedTask;
		}

		while (instancesForDispose.TryPop(out var instance))
		{
			var valueTask = Disposer.DisposeAsync(instance);

			if (!valueTask.IsCompletedSuccessfully)
			{
				return Wait(valueTask, instancesForDispose);
			}
		}

		return ValueTask.CompletedTask;

		static async ValueTask Wait(ValueTask valueTask, WeakReferenceStack instancesForDispose)
		{
			await valueTask.ConfigureAwait(false);

			while (instancesForDispose.TryPop(out var instance))
			{
				await Disposer.DisposeAsync(instance).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	///     Synchronously disposes all tracked instances and marks the bin as disposed.
	/// </summary>
	/// <remarks>
	///     Each instance is disposed using <see cref="Disposer.Dispose" />. After completion the bin rejects further
	///     additions. Safe to call multiple times; only the first call performs work.
	/// </remarks>
	internal void Dispose()
	{
		if (Interlocked.CompareExchange(ref _instancesForDispose, value: null, _instancesForDispose) is { } instancesForDispose)
		{
			while (instancesForDispose.TryPop(out var instance))
			{
				Disposer.Dispose(instance);
			}
		}
	}

	/// <summary>
	///     Adds an instance to the bin for later disposal (asynchronously, when the bin is disposed).
	/// </summary>
	/// <typeparam name="T">Type of the instance being added.</typeparam>
	/// <param name="instance">The instance to track.</param>
	/// <returns>
	///     A completed <see cref="ValueTask" /> if the instance is tracked (or ignored because it is not disposable),
	///     or a task representing asynchronous immediate disposal followed by an exception if the bin is already disposed.
	/// </returns>
	/// <exception cref="ObjectDisposedException">
	///     Thrown if the bin has already been disposed. If the instance is disposable it is disposed prior to throwing.
	/// </exception>
	/// <remarks>
	///     Non‑disposable instances are skipped but still validate that the bin is not disposed.
	///     Disposable instances are stored on a weak-reference stack allowing garbage collection
	///     if they become otherwise unreachable before bin disposal.
	/// </remarks>
	public ValueTask AddAsync<T>(T instance)
	{
		if (!Disposer.IsDisposable(instance))
		{
			ObjectDisposedException.ThrowIf(_instancesForDispose is null, this);

			return ValueTask.CompletedTask;
		}

		if (_instancesForDispose is { } instancesForDispose)
		{
			instancesForDispose.Push(instance);

			return ValueTask.CompletedTask;
		}

		var valueTask = Disposer.DisposeAsync(instance);

		ObjectDisposedException.ThrowIf(condition: valueTask.IsCompletedSuccessfully, this);

		return Wait(valueTask, this);

		static async ValueTask Wait(ValueTask valueTask, ObjectsBin objectsBin)
		{
			await valueTask.ConfigureAwait(false);

			ObjectDisposedException.ThrowIf(condition: true, objectsBin);
		}
	}

	/// <summary>
	///     Adds an instance to the bin for later disposal (synchronously, when the bin is disposed).
	/// </summary>
	/// <typeparam name="T">Type of the instance being added.</typeparam>
	/// <param name="instance">The instance to track.</param>
	/// <exception cref="ObjectDisposedException">
	///     Thrown if the bin has already been disposed. If the instance is disposable it is disposed prior to throwing.
	/// </exception>
	/// <remarks>
	///     Mirrors <see cref="AddAsync{T}" /> but uses synchronous disposal operations. Non‑disposable instances are ignored
	///     after validating disposal state.
	/// </remarks>
	public void AddSync<T>(T instance)
	{
		if (!Disposer.IsDisposable(instance))
		{
			ObjectDisposedException.ThrowIf(_instancesForDispose is null, this);

			return;
		}

		if (_instancesForDispose is { } instancesForDispose)
		{
			instancesForDispose.Push(instance);
		}
		else
		{
			Disposer.Dispose(instance);

			ObjectDisposedException.ThrowIf(condition: true, this);
		}
	}
}