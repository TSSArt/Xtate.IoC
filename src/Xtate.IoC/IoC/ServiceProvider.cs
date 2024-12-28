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

public class ServiceProvider : IServiceProvider, IServiceScopeFactory, ITypeKeyAction, IDisposable, IAsyncDisposable
{
	private readonly CancellationTokenSource _disposeTokenSource = new();

	private readonly Cache<TypeKey, ImplementationEntry?> _services;

	private readonly ServiceProvider? _sourceServiceProvider;

	private int _disposed;

	public ServiceProvider(IServiceCollection services)
	{
		_sourceServiceProvider = null;
		SharedObjectsBin = new SharedObjectsBin();
		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(services));

		Initialization(services);
	}

	protected ServiceProvider(ServiceProvider sourceServiceProvider, IServiceCollection? additionalServices = null)
	{
		_sourceServiceProvider = sourceServiceProvider;
		SharedObjectsBin = sourceServiceProvider.SharedObjectsBin;
		SharedObjectsBin.AddReference();
		_services = new Cache<TypeKey, ImplementationEntry?>(GroupServices(sourceServiceProvider, additionalServices));

		if (additionalServices is not null)
		{
			Initialization(additionalServices);
		}
	}

	public ObjectsBin ObjectsBin { get; } = new();

	public SharedObjectsBin SharedObjectsBin { get; }

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
		var additionalServices = new SourceServiceCollection(this);
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
			AddRegistration(groupedServices, sourceServiceProvider: null, registration);
		}

		AddForwarding(groupedServices, static (serviceProvider, _) => serviceProvider);
		AddForwarding(groupedServices, static (serviceProvider, _) => (IServiceScopeFactory) serviceProvider);

		return groupedServices;
	}

	private void AddForwarding<T>(Dictionary<TypeKey, ImplementationEntry?> services, Func<IServiceProvider, Empty, T> evaluator) =>
		AddRegistration(services, sourceServiceProvider: null, new ServiceEntry(TypeKey.ServiceKeyFast<T, Empty>(), InstanceScope.Forwarding, evaluator));

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

	private ImplementationEntry? CopyEntries(SimpleTypeKey typeKey)
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
		ImplementationEntry? lastEntry = null;

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
		if (Interlocked.Exchange(ref _disposed, value: 1) == 0)
		{
			var isLastReference = SharedObjectsBin.RemoveReference();

			if (disposing)
			{
				_disposeTokenSource.Cancel();

				ObjectsBin.Dispose();

				if (isLastReference)
				{
					SharedObjectsBin.Dispose();
				}

				_disposeTokenSource.Dispose();
			}
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		if (Interlocked.Exchange(ref _disposed, value: 1) == 0)
		{
			await _disposeTokenSource.CancelAsync().ConfigureAwait(false);

			await ObjectsBin.DisposeAsync().ConfigureAwait(false);

			if (SharedObjectsBin.RemoveReference())
			{
				await SharedObjectsBin.DisposeAsync().ConfigureAwait(false);
			}

			_disposeTokenSource.Dispose();
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