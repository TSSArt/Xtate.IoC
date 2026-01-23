// Copyright © 2019-2026 Sergii Artemenko
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
///     Provides a thread-safe mechanism for managing asynchronous initialization of objects.
///     This base class coordinates initialization state and ensures initialization logic runs exactly once,
///     even when accessed concurrently from multiple threads.
/// </summary>
public abstract class AsyncInit
{
	private static readonly TaskCompletionSource Waiting = new();

	private static readonly TaskCompletionSource Running = new();

	private TaskCompletionSource? _tcs = Waiting;

	/// <summary>
	///     Creates a builder for asynchronous initialization of the specified instance.
	/// </summary>
	/// <typeparam name="T">The type of the instance to initialize.</typeparam>
	/// <param name="this">The instance to initialize.</param>
	/// <returns>A builder that can be used to chain initialization tasks.</returns>
	public static Builder<T> For<T>(T @this) => new(@this);

	/// <summary>
	///     Creates a builder for asynchronous initialization of the specified instance,
	///     with a base initialization task that must complete first.
	/// </summary>
	/// <typeparam name="T">The type of the instance to initialize.</typeparam>
	/// <param name="this">The instance to initialize.</param>
	/// <param name="baseInitializeAsync">The base class initialization task to await before running additional initialization.</param>
	/// <returns>A builder that can be used to chain initialization tasks.</returns>
	public static Builder<T> For<T>(T @this, ValueTask baseInitializeAsync) => new(@this, baseInitializeAsync);

	/// <summary>
	///     Throws an <see cref="InvalidOperationException" /> if the object has not been fully initialized.
	/// </summary>
	/// <exception cref="InvalidOperationException">The object has not been initialized.</exception>
	protected void ThrowIfNotInitialized()
	{
		if (_tcs is not null)
		{
			throw new InvalidOperationException(Resources.Exception_ObjectMustBeInitialized);
		}
	}

	/// <summary>
	///     Gets a task representing the current initialization state, or null if initialization should begin.
	///     This method uses lock-free interlocked operations to coordinate concurrent access.
	/// </summary>
	/// <returns>
	///     A <see cref="Task" /> to await if initialization is in progress or completed;
	///     null if the caller should proceed with initialization.
	/// </returns>
	private Task? RunTask()
	{
		while (true)
		{
			if (_tcs is not { } tcs)
			{
				return Task.CompletedTask;
			}

			if (tcs == Waiting)
			{
				if (Interlocked.CompareExchange(ref _tcs, Running, Waiting) != Waiting)
				{
					continue;
				}

				return null;
			}

			if (tcs != Running)
			{
				return tcs.Task;
			}

			var newTcs = new TaskCompletionSource();

			if (Interlocked.CompareExchange(ref _tcs, newTcs, Running) != Running)
			{
				continue;
			}

			return newTcs.Task;
		}
	}

	/// <summary>
	///     Marks the initialization as complete and signals any waiting tasks.
	///     This method uses lock-free interlocked operations to ensure thread-safety.
	/// </summary>
	private protected void Complete()
	{
		while (true)
		{
			var tcs = _tcs;

			if (tcs == Running)
			{
				if (Interlocked.CompareExchange(ref _tcs, value: null, Running) != Running)
				{
					continue;
				}

				return;
			}

			Infra.Assert(tcs is not null && tcs != Waiting);

			if (Interlocked.CompareExchange(ref _tcs, value: null, tcs) == tcs)
			{
				tcs.SetResult();

				return;
			}
		}
	}

	/// <summary>
	///     A growable array structure optimized for small collections, using inline storage for the first few items.
	/// </summary>
	/// <typeparam name="T">The type of elements stored in the array.</typeparam>
	private struct QArray<T>
	{
		private T _item0, _item1, _item2, _item3;

		private T[]? _items;

		/// <summary>
		///     Gets a reference to the element at the specified index, growing the array if necessary.
		/// </summary>
		/// <param name="index">The zero-based index of the element to access.</param>
		/// <returns>A reference to the element at the specified index.</returns>
		[UnscopedRef]
		public ref T At(int index)
		{
			switch (index)
			{
				case 0:  return ref _item0;
				case 1:  return ref _item1;
				case 2:  return ref _item2;
				case 3:  return ref _item3;
				default: index -= 4; break;
			}

			if (_items is null)
			{
				_items = new T[GetNewCapacity(index + 1)];
			}
			else if (index >= _items.Length)
			{
				Array.Resize(ref _items, GetNewCapacity(index + 1));
			}

			return ref _items[index];
		}

		/// <summary>
		///     Calculates the new capacity for the array, doubling the current size or using the requested capacity, whichever is
		///     larger.
		/// </summary>
		/// <param name="capacity">The minimum required capacity.</param>
		/// <returns>The new capacity for the array.</returns>
		private readonly int GetNewCapacity(int capacity)
		{
			var newSize = _items is null ? 4 : 2 * _items.Length;

			return newSize < capacity ? capacity : newSize;
		}
	}

	/// <summary>
	///     Base class for initialization runners that execute initialization logic for instances of type
	///     <typeparamref name="T" />.
	/// </summary>
	/// <typeparam name="T">The type of instance being initialized.</typeparam>
	public abstract class Base<T> : AsyncInit
	{
		/// <summary>
		///     When overridden in a derived class, executes the initialization logic for the specified instance.
		/// </summary>
		/// <param name="instance">The instance to initialize.</param>
		/// <returns>A <see cref="ValueTask" /> representing the asynchronous initialization operation.</returns>
		protected abstract ValueTask InitAction(T instance);

		/// <summary>
		///     Runs the initialization logic, ensuring it executes exactly once even under concurrent access.
		/// </summary>
		/// <param name="instance">The instance to initialize.</param>
		/// <returns>A <see cref="ValueTask" /> representing the asynchronous initialization operation.</returns>
		internal ValueTask Run(T instance) => RunTask() is { } task ? new ValueTask(task) : InitAction(instance);
	}

	/// <summary>
	///     A builder for composing multiple asynchronous initialization tasks for instances of type <typeparamref name="T" />.
	///     This is a ref struct to enable stack-only allocation and avoid heap allocations for common scenarios.
	/// </summary>
	/// <typeparam name="T">The type of instance being initialized.</typeparam>
	/// <param name="instance">The instance to initialize.</param>
	/// <param name="baseInitializeAsync">The base initialization task to complete before running additional initialization.</param>
	public ref struct Builder<T>(T instance, ValueTask baseInitializeAsync)
	{
		private int _length = 0;

		private QArray<Base<T>> _runners;

		/// <summary>
		///     Initializes a new instance of the <see cref="Builder{T}" /> struct for the specified instance.
		/// </summary>
		/// <param name="instance">The instance to initialize.</param>
		/// <exception cref="InvalidOperationException">
		///     The base class implements <see cref="IAsyncInitialization" /> and should
		///     use the other constructor.
		/// </exception>
		public Builder(T instance) : this(instance, ValueTask.CompletedTask)
		{
			if (Nested.IsBaseAsyncInitialization)
			{
				throw new InvalidOperationException(string.Format(Resources.Format_BaseClassImplementsIAsyncInitialization(typeof(T).BaseType!)));
			}
		}

		/// <summary>
		///     Adds an initialization task to the builder chain.
		/// </summary>
		/// <param name="init">The initialization runner to execute.</param>
		/// <returns>This builder instance for method chaining.</returns>
		private Builder<T> RunBase(Base<T> init)
		{
			Infra.NotNull(init);

			_runners.At(_length ++) = init;

			return this;
		}

		/// <summary>
		///     Adds an <see cref="AsyncInit{T}" /> initialization task to the builder chain.
		/// </summary>
		/// <param name="init">The initialization runner to execute.</param>
		/// <returns>This builder instance for method chaining.</returns>
		public Builder<T> Run(AsyncInit<T> init) => RunBase(init);

		/// <summary>
		///     Adds an <see cref="AsyncInit{T, TResult}" /> initialization task to the builder chain.
		/// </summary>
		/// <typeparam name="TResult">The type of result produced by the initialization.</typeparam>
		/// <param name="init">The initialization runner to execute.</param>
		/// <returns>This builder instance for method chaining.</returns>
		public Builder<T> Run<TResult>(AsyncInit<T, TResult> init) => RunBase(init);

		/// <summary>
		///     Implicitly converts the builder to a <see cref="ValueTask" /> that executes all queued initialization tasks.
		/// </summary>
		/// <param name="builder">The builder to execute.</param>
		public static implicit operator ValueTask(Builder<T> builder) => builder.ExecuteAsync();

		/// <summary>
		///     Executes all queued initialization tasks in sequence.
		/// </summary>
		/// <returns>A <see cref="ValueTask" /> representing the asynchronous execution of all initialization tasks.</returns>
		private ValueTask ExecuteAsync()
		{
			if (!baseInitializeAsync.IsCompletedSuccessfully)
			{
				return ExecuteAsyncWait(instance, baseInitializeAsync, start: 0, _length, _runners);
			}

			for (var i = 0; i < _length; i ++)
			{
				var valueTask = _runners.At(i).Run(instance);

				if (!valueTask.IsCompletedSuccessfully)
				{
					return ExecuteAsyncWait(instance, valueTask, i + 1, _length, _runners);
				}
			}

			return ValueTask.CompletedTask;
		}

		/// <summary>
		///     Asynchronously waits for a task to complete and then executes remaining initialization tasks.
		/// </summary>
		/// <param name="instance">The instance being initialized.</param>
		/// <param name="notCompletedTask">The task to await before continuing.</param>
		/// <param name="start">The index of the first remaining initialization task to execute.</param>
		/// <param name="length">The total number of initialization tasks.</param>
		/// <param name="runners">The array of initialization runners.</param>
		/// <returns>A <see cref="ValueTask" /> representing the asynchronous execution.</returns>
		private static async ValueTask ExecuteAsyncWait(T instance,
														ValueTask notCompletedTask,
														int start,
														int length,
														QArray<Base<T>> runners)
		{
			await notCompletedTask.ConfigureAwait(false);

			for (var i = start; i < length; i ++)
			{
				await runners.At(i).Run(instance).ConfigureAwait(false);
			}
		}

		/// <summary>
		///     Nested static class for cached type information.
		/// </summary>
		private static class Nested
		{
			/// <summary>
			///     Indicates whether the base type implements <see cref="IAsyncInitialization" />.
			/// </summary>
			public static readonly bool IsBaseAsyncInitialization = typeof(T).BaseType != null && typeof(IAsyncInitialization).IsAssignableFrom(typeof(T).BaseType);
		}
	}
}

public class AsyncInit<T>(Func<T, ValueTask> initAction) : AsyncInit.Base<T>
{
	protected override ValueTask InitAction(T instance)
	{
		var valueTask = initAction(instance);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return InitActionWait(valueTask);
		}

		Complete();

		return ValueTask.CompletedTask;
	}

	private async ValueTask InitActionWait(ValueTask valueTask)
	{
		await valueTask.ConfigureAwait(false);

		Complete();
	}

	public void EnsureInitialized() => ThrowIfNotInitialized();
}

public class AsyncInit<T, TResult>(Func<T, ValueTask<TResult>> initAction) : AsyncInit.Base<T>
{
	private TResult _value = default!;

	public TResult Value
	{
		get
		{
			ThrowIfNotInitialized();

			return _value;
		}
	}

	public static implicit operator TResult(AsyncInit<T, TResult> init) => init.Value;

	protected override ValueTask InitAction(T instance)
	{
		var valueTask = initAction(instance);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return InitActionWait(valueTask);
		}

		_value = valueTask.Result;

		Complete();

		return ValueTask.CompletedTask;
	}

	private async ValueTask InitActionWait(ValueTask<TResult> valueTask)
	{
		_value = await valueTask.ConfigureAwait(false);

		Complete();
	}
}