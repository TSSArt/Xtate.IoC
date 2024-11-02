// Copyright © 2019-2024 Sergii Artemenko
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
///     Represents a base class for entries in the IoC container per service.
///     Derived classes should implement specific behaviors for service entries.
/// </summary>
public abstract class ImplementationEntry
{
	private readonly IServiceProvider ServiceProvider;
	private          DelegateEntry?   _delegateEntry;

	private ImplementationEntry _nextEntry;

	private ImplementationEntry? _previousEntry;

	/// <summary>
	///     Initializes a new instance of the <see cref="ImplementationEntry" /> class with the specified factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
	/// <param name="factory">The factory delegate used to create instances of the service.</param>
	protected ImplementationEntry(IServiceProvider serviceProvider, Delegate factory)
	{
		ServiceProvider = serviceProvider;
		Factory = factory;
		_nextEntry = this;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="ImplementationEntry" /> class by copying the factory delegate from
	///     another <see cref="ImplementationEntry" /> instance.
	/// </summary>
	/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
	/// <param name="sourceImplementationEntry">
	///     The source <see cref="ImplementationEntry" /> instance to copy the factory
	///     delegate from.
	/// </param>
	protected ImplementationEntry(IServiceProvider serviceProvider, ImplementationEntry sourceImplementationEntry)
	{
		ServiceProvider = serviceProvider;
		Factory = sourceImplementationEntry.Factory;
		_nextEntry = this;
	}

	public Delegate Factory { get; }

	private bool IsAsyncInitializationHandlerUsed() => ReferenceEquals(ServiceProvider.InitializationHandler, AsyncInitializationHandler.Instance);

	public abstract ImplementationEntry CreateNew(ServiceProvider serviceProvider);

	public abstract ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory);

	internal void AddToChain([NotNull] ref ImplementationEntry? lastEntry)
	{
		Infra.Assert(_previousEntry is null);
		Infra.Assert(ReferenceEquals(_nextEntry, this));

		if (lastEntry is not null)
		{
			_nextEntry = lastEntry._nextEntry;
			_previousEntry = lastEntry;
			lastEntry._nextEntry = this;
		}

		lastEntry = this;
	}

	internal Chain AsChain() => new(this);

	/// <summary>
	///     Gets the required service of type <typeparamref name="T" /> with the specified argument.
	///     Throws an exception if the service is not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the required service.</returns>
	/// <exception cref="DependencyInjectionException">Thrown if the service is not found.</exception>
	public async ValueTask<T> GetRequiredService<T, TArg>(TArg argument) where T : notnull
	{
		var instance = await GetService<T, TArg>(argument).ConfigureAwait(false);

		return instance is not null ? instance : throw MissedServiceException<T, TArg>();
	}

	/// <summary>
	///     Gets the service of type <typeparamref name="T" /> with the specified argument.
	///     Returns null if the service is not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the service or null if not found.</returns>
	public ValueTask<T?> GetService<T, TArg>(TArg argument) =>
		ServiceProvider.Actions is not { } actions
			? GetServiceNoActions<T, TArg>(argument)
			: GetServiceWithActions<T, TArg>(argument, actions);

	private async ValueTask<T?> GetServiceNoActions<T, TArg>(TArg argument)
	{
		var instance = await ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		var initTask = IsAsyncInitializationHandlerUsed()
			? AsyncInitializationHandler.InitializeAsync(instance)
			: CustomInitialize(ServiceProvider, instance);

		await initTask.ConfigureAwait(false);

		return instance;

		static Task CustomInitialize(IServiceProvider serviceProvider, T? obj)
		{
			if (serviceProvider.InitializationHandler is { } handler && handler.Initialize(obj))
			{
				return handler.InitializeAsync(obj);
			}

			return Task.CompletedTask;
		}
	}

	private async ValueTask<T?> GetServiceWithActions<T, TArg>(TArg argument, IServiceProviderActions[] actions)
	{
		var typeKey = TypeKey.ServiceKeyFast<T, TArg>();

		foreach (var action in actions)
		{
			action.ServiceRequesting(typeKey)?.ServiceRequesting<T, TArg>(argument);
		}

		var instance = await GetServiceNoActions<T, TArg>(argument).ConfigureAwait(false);

		for (var i = actions.Length - 1; i >= 0; i --)
		{
			actions[i].ServiceRequested(typeKey)?.ServiceRequested<T, TArg>(instance);
		}

		return instance;
	}

	/// <summary>
	///     Gets the required service of type <typeparamref name="T" /> with the specified argument.
	///     Throws an exception if the service is not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>The required service.</returns>
	/// <exception cref="DependencyInjectionException">Thrown if the service is not found.</exception>
	public T GetRequiredServiceSync<T, TArg>(TArg argument) where T : notnull
	{
		var instance = ExecuteFactorySync<T, TArg>(argument);

		if (instance is not null && IsAsyncInitializationHandlerUsed())
		{
			if (AsyncInitializationHandler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
		else
		{
			CustomInitialize(ServiceProvider, instance);
		}

		return instance;

		static void CustomInitialize(IServiceProvider serviceProvider, [NotNull] T? instance)
		{
			if (instance is null)
			{
				throw MissedServiceException<T, TArg>();
			}

			if (serviceProvider.InitializationHandler is { } handler && handler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
	}

	/// <summary>
	///     Gets the service of type <typeparamref name="T" /> with the specified argument.
	///     Returns null if the service is not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>The service or null if not found.</returns>
	public T? GetServiceSync<T, TArg>(TArg argument)
	{
		var instance = ExecuteFactorySync<T, TArg>(argument);

		if (IsAsyncInitializationHandlerUsed())
		{
			if (AsyncInitializationHandler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
		else
		{
			CustomInitialize(ServiceProvider, instance);
		}

		return instance;

		static void CustomInitialize(IServiceProvider serviceProvider, T? instance)
		{
			if (serviceProvider.InitializationHandler is { } handler && handler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
	}

	public static DependencyInjectionException MissedServiceException<T, TArg>() =>
		ArgumentType.TypeOf<TArg>().IsEmpty
			? new DependencyInjectionException(Res.Format(Resources.Exception_ServiceMissedInContainer, typeof(T)))
			: new DependencyInjectionException(Res.Format(Resources.Exception_ServiceArgMissedInContainer, typeof(T), ArgumentType.TypeOf<TArg>()));

	private static DependencyInjectionException TypeUsedInSynchronousInstantiationException<T>() => new(Res.Format(Resources.Exception_TypeUsedInSynchronousInstantiation, typeof(T)));

	private static DependencyInjectionException ServiceNotAvailableInSynchronousContextException<T>() => new(Res.Format(Resources.Exception_ServiceNotAvailableInSynchronousContext, typeof(T)));

	protected virtual ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) =>
		ServiceProvider.Actions is not { } actions
			? ExecuteFactoryNoActions<T, TArg>(argument)
			: ExecuteFactoryWithActions<T, TArg>(argument, actions);

	private ValueTask<T?> ExecuteFactoryNoActions<T, TArg>(TArg argument) =>
		Factory switch
		{
			Func<IServiceProvider, TArg, ValueTask<T?>> factory    => factory(ServiceProvider, argument),
			Func<IServiceProvider, TArg, T?> factory               => new ValueTask<T?>(factory(ServiceProvider, argument)),
			Func<IServiceProvider, T, TArg, ValueTask<T?>> factory => GetDecoratorAsync(factory, argument),
			Func<IServiceProvider, T, TArg, T?> factory            => GetDecoratorAsync(factory, argument),
			_                                                      => throw Infra.Unmatched(Factory)
		};

	private async ValueTask<T?> ExecuteFactoryWithActions<T, TArg>(TArg argument, IServiceProviderActions[] actions)
	{
		var typeKey = TypeKey.ServiceKeyFast<T, TArg>();

		foreach (var action in actions)
		{
			action.FactoryCalling(typeKey)?.FactoryCalling<T, TArg>(argument);
		}

		var instance = await ExecuteFactoryNoActions<T, TArg>(argument).ConfigureAwait(false);

		for (var i = actions.Length - 1; i >= 0; i --)
		{
			actions[i].FactoryCalled(typeKey)?.FactoryCalled<T, TArg>(instance);
		}

		return instance;
	}

	protected virtual T? ExecuteFactorySync<T, TArg>(TArg argument) =>
		ServiceProvider.Actions is not { } actions
			? ExecuteFactorySyncNoActions<T, TArg>(argument)
			: ExecuteFactorySyncWithActions<T, TArg>(argument, actions);

	private T? ExecuteFactorySyncNoActions<T, TArg>(TArg argument) =>
		Factory switch
		{
			Func<IServiceProvider, TArg, T?> factory       => factory(ServiceProvider, argument),
			Func<IServiceProvider, T, TArg, T?> factory    => GetDecoratorSync(factory, argument),
			Func<IServiceProvider, TArg, ValueTask<T?>>    => throw ServiceNotAvailableInSynchronousContextException<T>(),
			Func<IServiceProvider, T, TArg, ValueTask<T?>> => throw ServiceNotAvailableInSynchronousContextException<T>(),
			_                                              => throw Infra.Unmatched(Factory)
		};

	private T? ExecuteFactorySyncWithActions<T, TArg>(TArg argument, IServiceProviderActions[] actions)
	{
		var typeKey = TypeKey.ServiceKeyFast<T, TArg>();

		foreach (var action in actions)
		{
			action.FactoryCalling(typeKey)?.FactoryCalling<T, TArg>(argument);
		}

		var instance = ExecuteFactorySyncNoActions<T, TArg>(argument);

		for (var i = actions.Length - 1; i >= 0; i --)
		{
			actions[i].FactoryCalled(typeKey)?.FactoryCalled<T, TArg>(instance);
		}

		return instance;
	}

	private protected void EnsureSynchronousContext<T, TArg>()
	{
		switch (Factory)
		{
			case Func<IServiceProvider, TArg, T?>:
			case Func<IServiceProvider, T, TArg, T?>:
				return;

			case Func<IServiceProvider, TArg, ValueTask<T?>>:
			case Func<IServiceProvider, T, TArg, ValueTask<T?>>:
				throw ServiceNotAvailableInSynchronousContextException<T>();

			default:
				throw Infra.Unmatched(Factory);
		}
	}

	private async ValueTask<T?> GetDecoratorAsync<T, TArg>(Func<IServiceProvider, T, TArg, ValueTask<T?>> factory, TArg argument) =>
		_previousEntry is not null && await _previousEntry.GetService<T, TArg>(argument).ConfigureAwait(false) is { } decoratedService
			? await factory(ServiceProvider, decoratedService, argument).ConfigureAwait(false)
			: default;

	private async ValueTask<T?> GetDecoratorAsync<T, TArg>(Func<IServiceProvider, T, TArg, T?> factory, TArg argument) =>
		_previousEntry is not null && await _previousEntry.GetService<T, TArg>(argument).ConfigureAwait(false) is { } decoratedService
			? factory(ServiceProvider, decoratedService, argument)
			: default;

	private T? GetDecoratorSync<T, TArg>(Func<IServiceProvider, T, TArg, T?> factory, TArg argument) =>
		_previousEntry is not null && _previousEntry.GetServiceSync<T, TArg>(argument) is { } decoratedService
			? factory(ServiceProvider, decoratedService, argument)
			: default;

	/// <summary>
	///     Asynchronously gets all services of type <typeparamref name="T" /> with the specified argument.
	/// </summary>
	/// <typeparam name="T">The type of the services to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>An asynchronous enumerable of the services.</returns>
	public async IAsyncEnumerable<T> GetServices<T, TArg>(TArg argument)
	{
		foreach (var entry in AsChain())
		{
			var result = await entry.GetService<T, TArg>(argument).ConfigureAwait(false);

			if (result is not null)
			{
				yield return result;
			}
		}
	}

	/// <summary>
	///     Synchronously gets all services of type <typeparamref name="T" /> with the specified argument.
	/// </summary>
	/// <typeparam name="T">The type of the services to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>An enumerable of the services.</returns>
	public IEnumerable<T> GetServicesSync<T, TArg>(TArg argument)
	{
		foreach (var entry in AsChain())
		{
			if (entry.GetServiceSync<T, TArg>(argument) is { } instance)
			{
				yield return instance;
			}
		}
	}

	/// <summary>
	///     Gets a delegate that asynchronously gets all services of type <typeparamref name="T" /> with the specified
	///     argument.
	/// </summary>
	/// <typeparam name="T">The type of the services to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
	/// <returns>A delegate that asynchronously gets all services.</returns>
	public TDelegate GetServicesDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is ServicesDelegateEntry<TDelegate> servicesDelegateEntry)
			{
				return servicesDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, IAsyncEnumerable<T>>(GetServices<T, TArg>));
		_delegateEntry = new ServicesDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	/// <summary>
	///     Gets a delegate that synchronously gets all services of type <typeparamref name="T" /> with the specified argument.
	/// </summary>
	/// <typeparam name="T">The type of the services to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
	/// <returns>A delegate that synchronously gets all services.</returns>
	public TDelegate GetServicesSyncDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is ServicesDelegateEntry<TDelegate> servicesDelegateEntry)
			{
				return servicesDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, IEnumerable<T>>(GetServicesSync<T, TArg>));
		_delegateEntry = new ServicesDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	/// <summary>
	///     Gets a delegate that asynchronously gets the required service of type <typeparamref name="T" /> with the specified
	///     argument.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
	/// <returns>A delegate that asynchronously gets the required service.</returns>
	public TDelegate GetRequiredServiceDelegate<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is RequiredServiceDelegateEntry<TDelegate> requiredServiceDelegateEntry)
			{
				return requiredServiceDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, ValueTask<T>>(GetRequiredService<T, TArg>));
		_delegateEntry = new RequiredServiceDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	/// <summary>
	///     Gets a delegate that asynchronously gets the service of type <typeparamref name="T" /> with the specified argument.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
	/// <returns>A delegate that asynchronously gets the service.</returns>
	public TDelegate GetServiceDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is ServiceDelegateEntry<TDelegate> serviceDelegateEntry)
			{
				return serviceDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, ValueTask<T?>>(GetService<T, TArg>));
		_delegateEntry = new ServiceDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	/// <summary>
	///     Gets a delegate that synchronously gets the required service of type <typeparamref name="T" /> with the specified
	///     argument.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
	/// <returns>A delegate that synchronously gets the required service.</returns>
	public TDelegate GetRequiredServiceSyncDelegate<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is RequiredServiceSyncDelegateEntry<TDelegate> requiredServiceSyncDelegateEntry)
			{
				return requiredServiceSyncDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, T>(GetRequiredServiceSync<T, TArg>));
		_delegateEntry = new RequiredServiceSyncDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	/// <summary>
	///     Gets a delegate that synchronously gets the service of type <typeparamref name="T" /> with the specified argument.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
	/// <returns>A delegate that synchronously gets the service.</returns>
	public TDelegate GetServiceSyncDelegate<T, TArg, TDelegate>() where TDelegate : Delegate
	{
		for (var entry = _delegateEntry; entry is not null; entry = entry.Next)
		{
			if (entry is ServiceSyncDelegateEntry<TDelegate> serviceSyncDelegateEntry)
			{
				return serviceSyncDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, T?>(GetServiceSync<T, TArg>));
		_delegateEntry = new ServiceSyncDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

		return newDelegate;
	}

	internal struct Chain(ImplementationEntry lastEntry) : IEnumerable<ImplementationEntry>, IEnumerator<ImplementationEntry>
	{
	#region Interface IDisposable

		readonly void IDisposable.Dispose() { }

	#endregion

	#region Interface IEnumerable

		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerable<ImplementationEntry>

		readonly IEnumerator<ImplementationEntry> IEnumerable<ImplementationEntry>.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerator

		public bool MoveNext()
		{
			var ok = !ReferenceEquals(Current, lastEntry);
			Current = (Current ?? lastEntry)._nextEntry;

			return ok;
		}

		void IEnumerator.Reset() => Current = default!;

		readonly object IEnumerator.Current => Current;

	#endregion

	#region Interface IEnumerator<ImplementationEntry>

		public ImplementationEntry Current { get; private set; } = default!;

	#endregion

		public readonly Chain GetEnumerator()
		{
			Infra.Assert(lastEntry._nextEntry._previousEntry is null); // Entry should be last entry in the chain

			return new Chain(lastEntry);
		}
	}

	private abstract class DelegateEntry(DelegateEntry? next)
	{
		public DelegateEntry? Next { get; } = next;
	}

	private class ServicesDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		public T Delegate { get; } = @delegate;
	}

	private class RequiredServiceDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		public T Delegate { get; } = @delegate;
	}

	private class ServiceDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		public T Delegate { get; } = @delegate;
	}

	private class RequiredServiceSyncDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		public T Delegate { get; } = @delegate;
	}

	private class ServiceSyncDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		public T Delegate { get; } = @delegate;
	}
}