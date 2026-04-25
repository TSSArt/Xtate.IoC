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
///     Represents a base class for service implementation entries registered in the IoC container.
///     Each entry encapsulates a factory delegate (either synchronous or asynchronous) able to produce
///     an instance of the requested service (optionally a decorator that depends on a previous entry).
///     Entries are chained to support multiple registrations (decorators or service enumeration).
///     Derived classes provide specialized behavior (scoping, lifetime control).
///     Thread-safety: Instances are used concurrently; factory delegates must be thread-safe.
/// </summary>
public abstract class ImplementationEntry
{
	private readonly IServiceProvider _serviceProvider;

	private DelegateEntry? _delegateEntry;

	private ImplementationEntry _nextEntry;

	private ImplementationEntry? _previousEntry;

	/// <summary>
	///     Initializes a new instance of the <see cref="ImplementationEntry" /> class with the specified factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
	/// <param name="factory">The factory delegate used to create instances of the service (or a decorator).</param>
	protected ImplementationEntry(IServiceProvider serviceProvider, Delegate factory)
	{
		_serviceProvider = serviceProvider;
		Factory = factory;
		_nextEntry = this;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="ImplementationEntry" /> class by copying the factory
	///     delegate from another <see cref="ImplementationEntry" /> instance.
	/// </summary>
	/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
	/// <param name="sourceImplementationEntry">The source entry whose factory delegate is reused.</param>
	protected ImplementationEntry(IServiceProvider serviceProvider, ImplementationEntry sourceImplementationEntry)
	{
		_serviceProvider = serviceProvider;
		Factory = sourceImplementationEntry.Factory;
		_nextEntry = this;
	}

	/// <summary>
	///     Gets the raw factory delegate registered for this entry.
	///     Supported delegate shapes:
	///     <list type="bullet">
	///         <item>
	///             <description>
	///                 <c>Func&lt;IServiceProvider,TArg,T?&gt;</c>
	///             </description>
	///         </item>
	///         <item>
	///             <description>
	///                 <c>Func&lt;IServiceProvider,TArg,ValueTask&lt;T?&gt;&gt;</c>
	///             </description>
	///         </item>
	///         <item>
	///             <description><c>Func&lt;IServiceProvider,T,TArg,T?&gt;</c> (decorator)</description>
	///         </item>
	///         <item>
	///             <description><c>Func&lt;IServiceProvider,T,TArg,ValueTask&lt;T?&gt;&gt;</c> (decorator)</description>
	///         </item>
	///     </list>
	/// </summary>
	public Delegate Factory { get; }

	/// <summary>
	///     Creates a new entry of the same semantic kind (lifetime strategy) bound to a different
	///     <see cref="ServiceProvider" />.
	/// </summary>
	/// <param name="serviceProvider">The target service provider.</param>
	/// <returns>A new <see cref="ImplementationEntry" /> instance.</returns>
	public abstract ImplementationEntry CreateNew(ServiceProvider serviceProvider);

	/// <summary>
	///     Creates a new entry of the same semantic kind using a supplied factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The target service provider.</param>
	/// <param name="factory">The factory delegate replacing the current one.</param>
	/// <returns>A new <see cref="ImplementationEntry" /> instance with the specified factory.</returns>
	public abstract ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory);

	/// <summary>
	///     Adds this entry to the circular chain headed by <paramref name="lastEntry" />.
	///     The method mutates linkage only once; assertions guard against repeated additions.
	/// </summary>
	/// <param name="lastEntry">Reference to the last entry variable maintained externally.</param>
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

	/// <summary>
	///     Returns an enumerable wrapper allowing iteration over the chain starting after <c>lastEntry</c>.
	/// </summary>
	internal Chain AsChain() => new(this);

	/// <summary>
	///     Gets the required service of type <typeparamref name="T" /> with the specified argument.
	///     Throws an exception if the service is not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>A task whose result is the required service instance.</returns>
	/// <exception cref="DependencyInjectionException">Thrown if the service is not found.</exception>
	public ValueTask<T> GetRequiredService<T, TArg>(TArg argument) where T : notnull
	{
		var valueTask = GetService<T, TArg>(argument);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return Wait(valueTask);
		}

		return valueTask.Result is { } instance ? new ValueTask<T>(instance) : ValueTask.FromException<T>(MissedServiceException.Create<T, TArg>());

		static async ValueTask<T> Wait(ValueTask<T?> valueTask)
		{
			var instance = await valueTask.ConfigureAwait(false);

			return instance is not null ? instance : throw MissedServiceException.Create<T, TArg>();
		}
	}

	/// <summary>
	///     Gets the service of type <typeparamref name="T" /> with the specified argument.
	///     Returns <c>null</c> if the service is not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>A task whose result is the service instance or <c>null</c>.</returns>
	public ValueTask<T?> GetService<T, TArg>(TArg argument) =>
		_serviceProvider.Actions is { } actionsArray
			? WithActions<T, TArg>(ActionsEventType.ServiceRequest, argument, actionsArray, index: 0)
			: GetServiceNoActions<T, TArg>(argument);

	/// <summary>
	///     Retrieves a service without invoking provider actions (fast path).
	///     Performs initialization if an instance is produced.
	/// </summary>
	private ValueTask<T?> GetServiceNoActions<T, TArg>(TArg argument)
	{
		var valueTask = ExecuteFactory<T, TArg>(argument);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return GetServiceNoActionsWait(valueTask);
		}

		if (valueTask.Result is not { } instance)
		{
			return new ValueTask<T?>(default(T?));
		}

		var initValueTask = ReferenceEquals(_serviceProvider.InitializationHandler, AsyncInitializationHandler.Instance)
			? AsyncInitializationHandler.InitializeAsync(instance)
			: CustomInitializeAsync(instance);

		return initValueTask.IsCompletedSuccessfully ? new ValueTask<T?>(instance) : Wait(initValueTask, instance);

		static async ValueTask<T?> Wait(ValueTask valueTask, T value)
		{
			await valueTask.ConfigureAwait(false);

			return value;
		}
	}

	/// <summary>
	///     Await continuation for <see cref="GetServiceNoActions{T,TArg}" /> when factory returned an incomplete task.
	/// </summary>
	private async ValueTask<T?> GetServiceNoActionsWait<T>(ValueTask<T?> valueTask)
	{
		var instance = await valueTask.ConfigureAwait(false);

		if (instance is null)
		{
			return default;
		}

		var initValueTask = ReferenceEquals(_serviceProvider.InitializationHandler, AsyncInitializationHandler.Instance)
			? AsyncInitializationHandler.InitializeAsync(instance)
			: CustomInitializeAsync(instance);

		await initValueTask.ConfigureAwait(false);

		return instance;
	}

	/// <summary>
	///     Performs custom (possibly async) initialization using a non-async handler.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <param name="obj">Instance to initialize (maybe null).</param>
	/// <returns>A <see cref="ValueTask" /> representing initialization.</returns>
	private ValueTask CustomInitializeAsync<T>(T? obj)
	{
		if (_serviceProvider.InitializationHandler is { } handler && handler.Initialize(obj))
		{
			return handler.InitializeAsync(obj);
		}

		return ValueTask.CompletedTask;
	}

	private static void CallEvent<T, TArg>(IServiceProviderActions actions, ActionsEventType type, ref DataActionsContext<T, TArg> context) =>
		actions.Event(type, ref context.ActionsContext)?.Event(type, ref context);

	/// <summary>
	///     Retrieves a service executing provider-wide actions around the request lifecycle.
	/// </summary>
	private ValueTask<T?> WithActions<T, TArg>(ActionsEventType eventType,
											   TArg argument,
											   IServiceProviderActions[] actionsArray,
											   int index)
	{
		Infra.Assert(eventType is ActionsEventType.ServiceRequest or ActionsEventType.FactoryCall);

		DataActionsContext<T, TArg>.Container? container = null;
		var context = new DataActionsContext<T, TArg>(argument);
		var actions = actionsArray[index];

		try
		{
			CallEvent(actions, eventType | ActionsEventType.Enter, ref context);

			var valueTask = ++ index < actionsArray.Length
				? WithActions<T, TArg>(eventType, argument, actionsArray, index)
				: eventType is ActionsEventType.ServiceRequest
					? GetServiceNoActions<T, TArg>(argument)
					: ExecuteFactoryNoActions<T, TArg>(argument);

			if (valueTask.IsCompleted)
			{
				context.Instance = valueTask.Result;
				CallEvent(actions, eventType | ActionsEventType.Exit, ref context);

				return valueTask;
			}

			container = new DataActionsContext<T, TArg>.Container { Context = context };

			try
			{
				return WithActionsWait(eventType, valueTask, actions, container);
			}
			finally
			{
				CallEvent(actions, eventType | ActionsEventType.SyncExit, ref container.Context);
			}
		}
		catch (Exception ex)
		{
			return HandleException(eventType, ex, actions, ref container is not null ? ref container.Context : ref context);
		}
	}

	private static async ValueTask<T?> WithActionsWait<T, TArg>(ActionsEventType eventType,
																ValueTask<T?> valueTask,
																IServiceProviderActions actions,
																DataActionsContext<T, TArg>.Container container)
	{
		Infra.Assert(eventType is ActionsEventType.ServiceRequest or ActionsEventType.FactoryCall);

		try
		{
			container.Context.Instance = await valueTask.ConfigureAwait(false);
			CallEvent(actions, eventType | ActionsEventType.Exit, ref container.Context);

			return container.Context.Instance;
		}
		catch (Exception ex)
		{
			container.Context.Exception = ex;
			CallEvent(actions, eventType | ActionsEventType.Error, ref container.Context);

			throw;
		}
	}

	private static ValueTask<T?> HandleException<T, TArg>(ActionsEventType eventType,
														  Exception exception,
														  IServiceProviderActions actions,
														  ref DataActionsContext<T, TArg> context)
	{
		Infra.Assert(eventType is ActionsEventType.ServiceRequest or ActionsEventType.FactoryCall);

		try
		{
			context.Exception = exception;
			CallEvent(actions, eventType | ActionsEventType.Error, ref context);
		}
		catch (Exception ex)
		{
			return ValueTask.FromException<T?>(ex);
		}

		return ValueTask.FromException<T?>(exception);
	}

	/// <summary>
	///     Gets the required service of type <typeparamref name="T" /> synchronously with the specified argument.
	///     Throws an exception if not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>The required service.</returns>
	/// <exception cref="DependencyInjectionException">Thrown if the service is not found.</exception>
	public T GetRequiredServiceSync<T, TArg>(TArg argument) where T : notnull => GetServiceSync<T, TArg>(argument) ?? throw MissedServiceException.Create<T, TArg>();

	/// <summary>
	///     Gets the service of type <typeparamref name="T" /> synchronously with the specified argument
	///     or <c>null</c> if not found.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <returns>The service or null if not found.</returns>
	public T? GetServiceSync<T, TArg>(TArg argument) =>
		_serviceProvider.Actions is { } actionsArray
			? SyncWithActions<T, TArg>(ActionsEventType.ServiceRequest, argument, actionsArray, index: 0)
			: GetServiceSyncNoActions<T, TArg>(argument);

	/// <summary>
	///     Gets the service of type <typeparamref name="T" /> with the specified argument.
	///     Returns null if the service is not found.
	///     Executes actions before and after the service is requested.
	/// </summary>
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <typeparam name="TArg">The type of the argument to pass to the service factory.</typeparam>
	/// <param name="eventType">The type of the event to trigger.</param>
	/// <param name="argument">The argument to pass to the service factory.</param>
	/// <param name="actionsArray">The array of actions to execute.</param>
	/// <param name="index">The index of the action to execute.</param>
	/// <returns>The service or null if not found.</returns>
	private T? SyncWithActions<T, TArg>(ActionsEventType eventType,
										TArg argument,
										IServiceProviderActions[] actionsArray,
										int index)
	{
		Infra.Assert(eventType is ActionsEventType.ServiceRequest or ActionsEventType.FactoryCall);

		var context = new DataActionsContext<T, TArg>(argument);
		var actions = actionsArray[index];

		try
		{
			CallEvent(actions, eventType | ActionsEventType.Enter, ref context);

			context.Instance = ++ index < actionsArray.Length
				? SyncWithActions<T, TArg>(eventType, argument, actionsArray, index)
				: eventType is ActionsEventType.ServiceRequest
					? GetServiceSyncNoActions<T, TArg>(argument)
					: ExecuteFactorySyncNoActions<T, TArg>(argument);

			CallEvent(actions, eventType | ActionsEventType.Exit, ref context);

			return context.Instance;
		}
		catch (Exception ex)
		{
			context.Exception = ex;
			CallEvent(actions, eventType | ActionsEventType.Error, ref context);

			throw;
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
	private T? GetServiceSyncNoActions<T, TArg>(TArg argument)
	{
		var instance = ExecuteFactorySync<T, TArg>(argument);

		if (instance is null)
		{
			return default;
		}

		if (ReferenceEquals(_serviceProvider.InitializationHandler, AsyncInitializationHandler.Instance))
		{
			if (AsyncInitializationHandler.Initialize(instance))
			{
				throw TypeUsedInSynchronousInstantiationException<T>();
			}
		}
		else
		{
			CustomInitialize(_serviceProvider, instance);
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

	/// <summary>
	///     Creates an exception indicating a type requiring async initialization was used in a sync context.
	/// </summary>
	private static DependencyInjectionException TypeUsedInSynchronousInstantiationException<T>() => new(Resources.Exception_TypeUsedInSynchronousInstantiation(typeof(T)));

	/// <summary>
	///     Creates an exception indicating a service is not available synchronously because its factory is async-only.
	/// </summary>
	private static DependencyInjectionException ServiceNotAvailableInSynchronousContextException<T>() => new(Resources.Exception_ServiceNotAvailableInSynchronousContext(typeof(T)));

	/// <summary>
	///     Executes the factory (async-aware) optionally wrapping with provider actions.
	/// </summary>
	protected virtual ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) =>
		_serviceProvider.Actions is { } actionsArray
			? WithActions<T, TArg>(ActionsEventType.FactoryCall, argument, actionsArray, index: 0)
			: ExecuteFactoryNoActions<T, TArg>(argument);

	/// <summary>
	///     Executes the factory without invoking actions.
	///     Handles decorator chain resolution.
	/// </summary>
	private ValueTask<T?> ExecuteFactoryNoActions<T, TArg>(TArg argument) =>
		Factory switch
		{
			Func<IServiceProvider, TArg, ValueTask<T?>> factory    => factory(_serviceProvider, argument),
			Func<IServiceProvider, TArg, T?> factory               => new ValueTask<T?>(factory(_serviceProvider, argument)),
			Func<IServiceProvider, T, TArg, ValueTask<T?>> factory => GetDecoratorAsync(factory, argument),
			Func<IServiceProvider, T, TArg, T?> factory            => GetDecoratorAsync(factory, argument),
			_                                                      => throw Infra.Unmatched(Factory)
		};

	/// <summary>
	///     Executes the factory synchronously optionally invoking actions.
	/// </summary>
	protected virtual T? ExecuteFactorySync<T, TArg>(TArg argument) =>
		_serviceProvider.Actions is { } actionsArray
			? SyncWithActions<T, TArg>(ActionsEventType.FactoryCall, argument, actionsArray, index: 0)
			: ExecuteFactorySyncNoActions<T, TArg>(argument);

	/// <summary>
	///     Executes the factory synchronously without actions.
	///     Throws if the delegate is async-only.
	/// </summary>
	private T? ExecuteFactorySyncNoActions<T, TArg>(TArg argument) =>
		Factory switch
		{
			Func<IServiceProvider, TArg, T?> factory       => factory(_serviceProvider, argument),
			Func<IServiceProvider, T, TArg, T?> factory    => GetDecoratorSync(factory, argument),
			Func<IServiceProvider, TArg, ValueTask<T?>>    => throw ServiceNotAvailableInSynchronousContextException<T>(),
			Func<IServiceProvider, T, TArg, ValueTask<T?>> => throw ServiceNotAvailableInSynchronousContextException<T>(),
			_                                              => throw Infra.Unmatched(Factory)
		};

	/// <summary>
	///     Ensures the current factory supports synchronous invocation; otherwise throws.
	///     Used by derived entries needing sync semantics validation.
	/// </summary>
	/// <typeparam name="T">Service type.</typeparam>
	/// <typeparam name="TArg">Argument type.</typeparam>
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

	/// <summary>
	///     Executes an asynchronous decorator factory resolving the prior service in the chain.
	/// </summary>
	private ValueTask<T?> GetDecoratorAsync<T, TArg>(Func<IServiceProvider, T, TArg, ValueTask<T?>> factory, TArg argument)
	{
		if (_previousEntry is null)
		{
			return new ValueTask<T?>(default(T));
		}

		var valueTask = _previousEntry.GetService<T, TArg>(argument);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return GetDecoratorAsyncWait(valueTask, factory, argument);
		}

		return valueTask.Result is { } decoratedService ? factory(_serviceProvider, decoratedService, argument) : new ValueTask<T?>(default(T));
	}

	private async ValueTask<T?> GetDecoratorAsyncWait<T, TArg>(ValueTask<T?> valueTask, Func<IServiceProvider, T, TArg, ValueTask<T?>> factory, TArg argument) =>
		await valueTask.ConfigureAwait(false) is { } decoratedService ? await factory(_serviceProvider, decoratedService, argument).ConfigureAwait(false) : default;

	/// <summary>
	///     Executes a synchronous decorator factory that returns a value synchronously.
	/// </summary>
	private ValueTask<T?> GetDecoratorAsync<T, TArg>(Func<IServiceProvider, T, TArg, T?> factory, TArg argument)
	{
		if (_previousEntry is null)
		{
			return new ValueTask<T?>(default(T));
		}

		var valueTask = _previousEntry.GetService<T, TArg>(argument);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return GetDecoratorAsyncWait(valueTask, factory, argument);
		}

		return valueTask.Result is { } decoratedService ? new ValueTask<T?>(factory(_serviceProvider, decoratedService, argument)) : new ValueTask<T?>(default(T));
	}

	private async ValueTask<T?> GetDecoratorAsyncWait<T, TArg>(ValueTask<T?> valueTask, Func<IServiceProvider, T, TArg, T?> factory, TArg argument) =>
		await valueTask.ConfigureAwait(false) is { } decoratedService ? factory(_serviceProvider, decoratedService, argument) : default;

	/// <summary>
	///     Executes a synchronous decorator factory within a synchronous call chain.
	/// </summary>
	private T? GetDecoratorSync<T, TArg>(Func<IServiceProvider, T, TArg, T?> factory, TArg argument) =>
		_previousEntry is not null && _previousEntry.GetServiceSync<T, TArg>(argument) is { } decoratedService
			? factory(_serviceProvider, decoratedService, argument)
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
	/// <returns>An enumerable services.</returns>
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
	/// <typeparam name="TDelegate">The delegate type requested.</typeparam>
	/// <returns>A delegate that asynchronously retrieves all services.</returns>
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
			if (entry is ServicesSyncDelegateEntry<TDelegate> servicesSyncDelegateEntry)
			{
				return servicesSyncDelegateEntry.Delegate;
			}
		}

		var newDelegate = FuncConverter.Cast<TDelegate>(new Func<TArg, IEnumerable<T>>(GetServicesSync<T, TArg>));
		_delegateEntry = new ServicesSyncDelegateEntry<TDelegate>(newDelegate, _delegateEntry);

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

	/// <summary>
	///     Represents an enumerator over the circular chain of <see cref="ImplementationEntry" /> instances.
	///     Iteration starts from the entry following <c>lastEntry</c> and stops once <c>lastEntry</c> is revisited.
	/// </summary>
	internal struct Chain(ImplementationEntry lastEntry) : IEnumerable<ImplementationEntry>, IEnumerator<ImplementationEntry>
	{
	#region Interface IDisposable

		/// <inheritdoc />
		readonly void IDisposable.Dispose() { }

	#endregion

	#region Interface IEnumerable

		/// <inheritdoc />
		readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerable<ImplementationEntry>

		/// <inheritdoc />
		readonly IEnumerator<ImplementationEntry> IEnumerable<ImplementationEntry>.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerator

		/// <summary>
		///     Advances to the next entry in the chain.
		/// </summary>
		/// <returns><c>true</c> if advanced; <c>false</c> when iteration is complete.</returns>
		public bool MoveNext()
		{
			var ok = !ReferenceEquals(Current, lastEntry);
			Current = (Current ?? lastEntry)._nextEntry;

			return ok;
		}

		/// <summary>
		///     Resets enumeration to the initial position (before first element).
		/// </summary>
		void IEnumerator.Reset() => Current = null!;

		/// <inheritdoc />
		readonly object IEnumerator.Current => Current;

	#endregion

	#region Interface IEnumerator<ImplementationEntry>

		/// <summary>
		///     Gets the current entry.
		/// </summary>
		public ImplementationEntry Current { get; private set; } = null!;

	#endregion

		/// <summary>
		///     Gets an enumerator instance (value-type) for use in <c>foreach</c>.
		///     Validates that <c>lastEntry</c> is indeed the tail of the chain.
		/// </summary>
		public readonly Chain GetEnumerator()
		{
			Infra.Assert(lastEntry._nextEntry._previousEntry is null); // Entry should be last entry in the chain

			return new Chain(lastEntry);
		}
	}

	/// <summary>
	///     Base node for cached delegate wrappers associated with this entry.
	/// </summary>
	private abstract class DelegateEntry(DelegateEntry? next)
	{
		/// <summary>
		///     Next cached delegate node.
		/// </summary>
		public DelegateEntry? Next { get; } = next;
	}

	/// <summary>
	///     Stores a delegate returning asynchronous enumeration of services.
	/// </summary>
	private class ServicesDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		/// <summary>
		///     The cached delegate instance.
		/// </summary>
		public T Delegate { get; } = @delegate;
	}

	/// <summary>
	///     Stores a delegate retrieving a required service asynchronously.
	/// </summary>
	private class RequiredServiceDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		/// <summary>
		///     The cached delegate instance.
		/// </summary>
		public T Delegate { get; } = @delegate;
	}

	/// <summary>
	///     Stores a delegate retrieving an optional service asynchronously.
	/// </summary>
	private class ServiceDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		/// <summary>
		///     The cached delegate instance.
		/// </summary>
		public T Delegate { get; } = @delegate;
	}

	/// <summary>
	///     Stores a delegate returning synchronous enumeration of services.
	/// </summary>
	private class ServicesSyncDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		/// <summary>
		///     The cached delegate instance.
		/// </summary>
		public T Delegate { get; } = @delegate;
	}

	/// <summary>
	///     Stores a delegate retrieving a required service synchronously.
	/// </summary>
	private class RequiredServiceSyncDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		/// <summary>
		///     The cached delegate instance.
		/// </summary>
		public T Delegate { get; } = @delegate;
	}

	/// <summary>
	///     Stores a delegate retrieving an optional service synchronously.
	/// </summary>
	private class ServiceSyncDelegateEntry<T>(T @delegate, DelegateEntry? next) : DelegateEntry(next)
	{
		/// <summary>
		///     The cached delegate instance.
		/// </summary>
		public T Delegate { get; } = @delegate;
	}
}