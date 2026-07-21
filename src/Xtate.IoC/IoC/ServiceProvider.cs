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

public class ServiceProvider : IServiceProvider, IServiceScopeFactory, ITypeKeyAction, IDisposable, IAsyncDisposable
{
	private readonly Cache<TypeKey, ImplementationEntry?> _services;

	private readonly SharedObjectsBin _sharedObjectsBin;

	private readonly ServiceProvider? _sourceServiceProvider;

	private CancellationTokenSource? _disposeTokenSource;

	public ServiceProvider(IServiceCollection services)
	{
		_sourceServiceProvider = null;
		_sharedObjectsBin = new SharedObjectsBin();

		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(services));
		_disposeTokenSource = new CancellationTokenSource();
		DisposeToken = _disposeTokenSource.Token;

		Initialization(services);
	}

	protected ServiceProvider(ServiceProvider sourceServiceProvider, IServiceCollection? additionalServices = null)
	{
		_sourceServiceProvider = sourceServiceProvider;
		_sharedObjectsBin = sourceServiceProvider._sharedObjectsBin;
		_sharedObjectsBin.AddReference();

		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(additionalServices));
		_disposeTokenSource = new CancellationTokenSource();
		DisposeToken = _disposeTokenSource.Token;

		if (additionalServices is not null)
		{
			Initialization(additionalServices);
		}
	}

	public ObjectsBin ObjectsBin { get; } = new();

	public ObjectsBin SharedObjectsBin => _sharedObjectsBin;

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

	public CancellationToken DisposeToken { get; }

#endregion

#region Interface IServiceScopeFactory

	IServiceScope IServiceScopeFactory.CreateScope() => new ServiceProviderScope(this);

	IServiceScope IServiceScopeFactory.CreateScope(Action<IServiceCollection> configureServices)
	{
		var additionalServices = new SourceServiceCollection(this);
		configureServices(additionalServices);

		return new ServiceProviderScope(this, additionalServices);
	}

#endregion

#region Interface ITypeKeyAction

	void ITypeKeyAction.TypedAction<T, TArg>(TypeKey typeKey) => _services.TryAdd(typeKey, CreateEntries<T, TArg>((GenericTypeKey)typeKey));

#endregion

	private void Initialization(IServiceCollection services)
	{
		InitializationHandler = GetInitializationHandlerService();

		if (GetActionsService() is { } actions)
		{
			Actions = actions;

			foreach (var action in actions)
			{
				if (action.RegisterServices(services.Count) is { } serviceProviderDataActions)
				{
					foreach (var service in services)
					{
						serviceProviderDataActions.RegisterService(service);
					}
				}
			}
		}
	}

	private IEnumerable<KeyValuePair<TypeKey, ImplementationEntry?>> GroupServices(IServiceCollection? services)
	{
		var servicesCount = services?.Count ?? 0;

		if (_sourceServiceProvider is not null && servicesCount == 0)
		{
			return [];
		}

		const int internalServicesCount = 3; /*IInitializationHandler, IServiceProvider, IServiceScopeFactory*/
		var groupedServices = new Dictionary<TypeKey, ImplementationEntry?>(servicesCount + internalServicesCount);

		if (_sourceServiceProvider is null)
		{
			AddRegistration(groupedServices, new ServiceEntry(TypeKey.ServiceKeyFast<IInitializationHandler, Empty>(), InstanceScope.Forwarding, GetInitializationHandler));

			static IInitializationHandler GetInitializationHandler(IServiceProvider serviceProvider, Empty _) => AsyncInitializationHandler.Instance;
		}

		if (services is not null)
		{
			foreach (var registration in services)
			{
				AddRegistration(groupedServices, registration);
			}
		}

		var serviceProviderKey = TypeKey.ServiceKeyFast<IServiceProvider, Empty>();

		if (_sourceServiceProvider is null || groupedServices.ContainsKey(serviceProviderKey))
		{
			AddRegistration(groupedServices, new ServiceEntry(serviceProviderKey, InstanceScope.Forwarding, GetServiceProvider));

			static IServiceProvider GetServiceProvider(IServiceProvider serviceProvider, Empty _) => serviceProvider;
		}

		var serviceScopeFactoryKey = TypeKey.ServiceKeyFast<IServiceScopeFactory, Empty>();

		if (_sourceServiceProvider is null || groupedServices.ContainsKey(serviceScopeFactoryKey))
		{
			AddRegistration(groupedServices, new ServiceEntry(serviceScopeFactoryKey, InstanceScope.Forwarding, GetServiceScopeFactory));

			static IServiceScopeFactory GetServiceScopeFactory(IServiceProvider serviceProvider, Empty _) => (IServiceScopeFactory)serviceProvider;
		}

		return groupedServices.AsEnumerable();
	}

	private void AddRegistration(Dictionary<TypeKey, ImplementationEntry?> services, in ServiceEntry service)
	{
		var simpleTypeKey = service.Key as SimpleTypeKey;
		var typeKey = simpleTypeKey ?? ((GenericTypeKey)service.Key).DefinitionKey;

		if (!services.TryGetValue(typeKey, out var lastEntry) && simpleTypeKey is not null)
		{
			lastEntry = CopyEntries(simpleTypeKey);
		}

		CreateImplementationEntry(service).AddToChain(ref lastEntry);
		services[typeKey] = lastEntry;
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
		var entry = GetImplementationEntry((SimpleTypeKey)TypeKey.ServiceKeyFast<IInitializationHandler, Empty>());

		Infra.NotNull(entry);

		return entry.GetServiceSync<IInitializationHandler, Empty>(default);
	}

	private IServiceProviderActions[]? GetActionsService()
	{
		var entry = GetImplementationEntry((SimpleTypeKey)TypeKey.ServiceKeyFast<IServiceProviderActions, Empty>());

		return entry?.GetServicesSync<IServiceProviderActions, Empty>(default).ToArray();
	}

	private ImplementationEntry? CopyEntries(TypeKey typeKey)
	{
		ImplementationEntry? lastEntry = null;

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
		var lastEntry = CopyEntries(typeKey);

		if (_services.TryGetValue(typeKey.DefinitionKey, out var genericEntry) && genericEntry is not null)
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
								  _                                                   => null
							  };

				if (factory is not null)
				{
					entry.CreateNew(this, factory).AddToChain(ref lastEntry);
				}
			}
		}

		return lastEntry;
	}

	protected virtual ImplementationEntry CreateImplementationEntry(ServiceEntry service) =>
		service.InstanceScope switch
		{
			InstanceScope.Transient         => new TransientImplementationEntry(this, service.Factory),
			InstanceScope.Forwarding        => new ForwardingImplementationEntry(this, service.Factory),
			InstanceScope.Scoped            => new ScopedOwnerImplementationEntry(this, service.Factory),
			InstanceScope.ScopedExternal    => new ScopedImplementationEntry(this, service.Factory),
			InstanceScope.Singleton         => new SingletonOwnerImplementationEntry(this, service.Factory),
			InstanceScope.SingletonExternal => new SingletonImplementationEntry(this, service.Factory),
			_                               => throw Infra.Unmatched(service.InstanceScope)
		};

	~ServiceProvider() => Dispose(false);

	protected virtual void Dispose(bool disposing)
	{
		if (Interlocked.Exchange(ref _disposeTokenSource, value: null) is { } disposeTokenSource)
		{
			var isLastReference = _sharedObjectsBin.RemoveReference();

			if (disposing)
			{
				disposeTokenSource.Cancel();

				ObjectsBin.Dispose();

				if (isLastReference)
				{
					_sharedObjectsBin.Dispose();
				}

				disposeTokenSource.Dispose();
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (Interlocked.Exchange(ref _disposeTokenSource, value: null) is { } disposeTokenSource)
		{
			await disposeTokenSource.CancelAsync().ConfigureAwait(false);

			await ObjectsBin.DisposeAsync().ConfigureAwait(false);

			if (_sharedObjectsBin.RemoveReference())
			{
				await _sharedObjectsBin.DisposeAsync().ConfigureAwait(false);
			}

			disposeTokenSource.Dispose();
		}
	}

	internal class SourceServiceCollection(ServiceProvider serviceProvider) : ServiceCollection, IServiceCollection
	{
	#region Interface IServiceCollection

		bool IServiceCollection.IsRegistered(TypeKey key)
		{
			if (IsRegistered(key))
			{
				return true;
			}

			for (var sp = serviceProvider; sp is not null; sp = sp._sourceServiceProvider)
			{
				if (sp.GetImplementationEntry(key) is not null)
				{
					return true;
				}
			}

			return false;
		}

	#endregion
	}
}