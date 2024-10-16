﻿// Copyright © 2019-2024 Sergii Artemenko
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

public class ServiceProvider : IServiceProvider, IServiceScopeFactory, ITypeKeyAction, IDisposable, IAsyncDisposable
{
	private readonly CancellationTokenSource              _disposeTokenSource  = new();
	private readonly WeakReferenceCollection              _instancesForDispose = new();
	private readonly Cache<TypeKey, ImplementationEntry?> _services;
	private readonly SingletonContainer                   _singletonContainer;
	private readonly ServiceProvider?                     _sourceServiceProvider;

	private int _disposed;

	public ServiceProvider(IServiceCollection services)
	{
		_sourceServiceProvider = default;
		_singletonContainer = new SingletonContainer();
		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(services));

		Initialization(services);
	}

	protected ServiceProvider(ServiceProvider sourceServiceProvider, IServiceCollection? additionalServices = default)
	{
		_sourceServiceProvider = sourceServiceProvider;
		_singletonContainer = sourceServiceProvider._singletonContainer;
		_singletonContainer.AddReference();
		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(sourceServiceProvider, additionalServices));

		if (additionalServices is not null)
		{
			Initialization(additionalServices);
		}
	}

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(false);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IDisposable

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

#endregion

#region Interface IServiceProvider

	public ImplementationEntry? GetImplementationEntry(TypeKey typeKey)
	{
		XtateObjectDisposedException.ThrowIf(_disposed != 0, this);

		if (_services.TryGetValue(typeKey, out var entry))
		{
			return entry;
		}

		if (typeKey is SimpleTypeKey simpleKey)
		{
			return _services.GetOrAdd(typeKey, CopyEntries(simpleKey));
		}

		typeKey.DoTypedAction(this);

		_services.TryGetValue(typeKey, out entry);

		return entry;
	}

	public IInitializationHandler? InitializationHandler { get; private set; }

	public IServiceProviderActions[]? Actions { get; private set; }

	public CancellationToken DisposeToken => _disposeTokenSource.Token;

#endregion

#region Interface IServiceScopeFactory

	IServiceScope IServiceScopeFactory.CreateScope() => new ServiceProviderScope(this);

	IServiceScope IServiceScopeFactory.CreateScope(Action<IServiceCollection> configureServices)
	{
		var additionalServices = new ServiceCollection();
		configureServices(additionalServices);

		return new ServiceProviderScope(this, additionalServices);
	}

#endregion

#region Interface ITypeKeyAction

	void ITypeKeyAction.TypedAction<T, TArg>(TypeKey typeKey) => _services.TryAdd(typeKey, CreateEntries<T, TArg>((GenericTypeKey) typeKey));

#endregion

	private void Initialization(IServiceCollection services)
	{
		InitializationHandler = GetInitializationHandlerService();

		if (GetActionsService() is { } actions)
		{
			Actions = actions;

			foreach (var action in actions)
			{
				if (action.RegisterServices() is { } serviceProviderDataActions)
				{
					foreach (var service in services)
					{
						serviceProviderDataActions.RegisterService(service);
					}
				}
			}
		}
	}

	private Dictionary<TypeKey, ImplementationEntry?> GroupServices(IServiceCollection services)
	{
		const int internalServicesCount = 3; /*IInitializationHandler, IServiceProvider, IServiceScopeFactory*/

		var groupedServices = new Dictionary<TypeKey, ImplementationEntry?>(services.Count + internalServicesCount);

		AddForwarding(groupedServices, static (_, _) => AsyncInitializationHandler.Instance);

		foreach (var registration in services)
		{
			AddRegistration(groupedServices, sourceServiceProvider: default, registration);
		}

		AddForwarding(groupedServices, static (serviceProvider, _) => serviceProvider);
		AddForwarding(groupedServices, static (serviceProvider, _) => (IServiceScopeFactory) serviceProvider);

		return groupedServices;
	}

	private void AddForwarding<T>(Dictionary<TypeKey, ImplementationEntry?> services, Func<IServiceProvider, Empty, T> evaluator) =>
		AddRegistration(services, sourceServiceProvider: default, new ServiceEntry(TypeKey.ServiceKeyFast<T, Empty>(), InstanceScope.Forwarding, evaluator));

	private IEnumerable<KeyValuePair<TypeKey, ImplementationEntry?>> GroupServices(ServiceProvider sourceServiceProvider, IServiceCollection? services)
	{
		if (services?.Count is not ({ } count and > 0))
		{
			return [];
		}

		var groupedServices = new Dictionary<TypeKey, ImplementationEntry?>(count);

		foreach (var registration in services)
		{
			AddRegistration(groupedServices, sourceServiceProvider, registration);
		}

		return groupedServices.AsEnumerable();
	}

	private void AddRegistration(Dictionary<TypeKey, ImplementationEntry?> services, ServiceProvider? sourceServiceProvider, in ServiceEntry service)
	{
		var simpleKey = service.Key as SimpleTypeKey;
		var key = simpleKey ?? ((GenericTypeKey) service.Key).DefinitionKey;

		if (!services.TryGetValue(key, out var lastEntry))
		{
			if (sourceServiceProvider?.GetImplementationEntry(key) is { } sourceEntry)
			{
				foreach (var entry in sourceEntry.AsChain())
				{
					entry.CreateNew(this).AddToChain(ref lastEntry);
				}
			}
		}

		CreateImplementationEntry(service).AddToChain(ref lastEntry);
		services[key] = lastEntry;
	}

	private ImplementationEntry? GetImplementationEntry(SimpleTypeKey typeKey)
	{
		if (!_services.TryGetValue(typeKey, out var entry))
		{
			entry = _services.GetOrAdd(typeKey, CopyEntries(typeKey));
		}

		return entry;
	}

	private IInitializationHandler? GetInitializationHandlerService()
	{
		var entry = GetImplementationEntry((SimpleTypeKey) TypeKey.ServiceKeyFast<IInitializationHandler, Empty>());

		Infra.NotNull(entry);

		return entry.GetServiceSync<IInitializationHandler, Empty>(default);
	}

	private IServiceProviderActions[]? GetActionsService()
	{
		var entry = GetImplementationEntry((SimpleTypeKey) TypeKey.ServiceKeyFast<IServiceProviderActions, Empty>());

		return entry?.GetServicesSync<IServiceProviderActions, Empty>(default).ToArray();
	}

	internal void RegisterInstanceForDispose<T>(T? instance)
	{
		if (Disposer.IsDisposable(instance) && (instance is not ServiceProvider serviceProvider || serviceProvider != this))
		{
			AddForDispose(instance);
		}
	}

	internal void RegisterSingletonInstanceForDispose<T>(T? instance)
	{
		if (Disposer.IsDisposable(instance) && (instance is not ServiceProvider serviceProvider || serviceProvider != this))
		{
			_singletonContainer.AddForDispose(instance);
		}
	}

	private ImplementationEntry? CopyEntries(SimpleTypeKey typeKey)
	{
		ImplementationEntry? lastEntry = default;

		if (_sourceServiceProvider?.GetImplementationEntry(typeKey) is { } sourceEntry)
		{
			foreach (var entry in sourceEntry.AsChain())
			{
				entry.CreateNew(this).AddToChain(ref lastEntry);
			}
		}

		return lastEntry;
	}

	private ImplementationEntry? CreateEntries<T, TArg>(GenericTypeKey typeKey)
	{
		ImplementationEntry? lastEntry = default;

		if (GetImplementationEntry(typeKey.DefinitionKey) is { } genericEntry)
		{
			foreach (var entry in genericEntry.AsChain())
			{
				var factory = entry.Factory switch
							  {
								  Func<DelegateFactory> func                          => func().GetDelegate<T, TArg>(),
								  Func<IServiceProvider, TArg, ValueTask<T?>> func    => func,
								  Func<IServiceProvider, TArg, T?> func               => func,
								  Func<IServiceProvider, T, TArg, ValueTask<T?>> func => func,
								  Func<IServiceProvider, T, TArg, T?> func            => func,
								  _                                                   => default
							  };

				if (factory is not null)
				{
					entry.CreateNew(this, factory).AddToChain(ref lastEntry);
				}
			}
		}

		return lastEntry;
	}

	private ImplementationEntry CreateImplementationEntry(in ServiceEntry service) =>
		service.InstanceScope switch
		{
			InstanceScope.Singleton  => new SingletonImplementationEntry(this, service.Factory),
			InstanceScope.Scoped     => new ScopedImplementationEntry(this, service.Factory),
			InstanceScope.Transient  => new TransientImplementationEntry(this, service.Factory),
			InstanceScope.Forwarding => new ForwardingImplementationEntry(this, service.Factory),
			_                        => throw Infra.Unmatched(service.InstanceScope)
		};

	private void AddForDispose(object instance)
	{
		XtateObjectDisposedException.ThrowIf(_disposed != 0, this);

		_instancesForDispose.Put(instance);
	}

	~ServiceProvider() => Dispose(false);

	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.Exchange(ref _disposed, value: 1) == 0)
		{
			var isLastReference = _singletonContainer.RemoveReference();

			if (disposing)
			{
				_disposeTokenSource.Cancel();

				while (_instancesForDispose.TryTake(out var instance))
				{
					Disposer.Dispose(instance);
				}

				if (isLastReference)
				{
					_singletonContainer.Dispose();
				}

				_disposeTokenSource.Dispose();
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (Interlocked.Exchange(ref _disposed, value: 1) == 0)
		{
			// ReSharper disable once MethodHasAsyncOverload
			_disposeTokenSource.Cancel();

			while (_instancesForDispose.TryTake(out var instance))
			{
				await Disposer.DisposeAsync(instance).ConfigureAwait(false);
			}

			if (_singletonContainer.RemoveReference())
			{
				await _singletonContainer.DisposeAsync().ConfigureAwait(false);
			}

			_disposeTokenSource.Dispose();
		}
	}

	private sealed class SingletonContainer : IDisposable, IAsyncDisposable
	{
		private WeakReferenceCollection? _instancesForDispose = new();
		private int                      _referenceCount;

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			if (Interlocked.CompareExchange(ref _instancesForDispose, value: default, _instancesForDispose) is { } instancesForDispose)
			{
				while (instancesForDispose.TryTake(out var instance))
				{
					await Disposer.DisposeAsync(instance).ConfigureAwait(false);
				}
			}
		}

	#endregion

	#region Interface IDisposable

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _instancesForDispose, value: default, _instancesForDispose) is { } instancesForDispose)
			{
				while (instancesForDispose.TryTake(out var instance))
				{
					Disposer.Dispose(instance);
				}
			}
		}

	#endregion

		public void AddReference() => Interlocked.Increment(ref _referenceCount);

		public bool RemoveReference() => Interlocked.Decrement(ref _referenceCount) == -1;

		public void AddForDispose(object instance)
		{
			var instancesForDispose = _instancesForDispose;

			XtateObjectDisposedException.ThrowIf(instancesForDispose is null, this);

			instancesForDispose.Put(instance);
		}
	}
}