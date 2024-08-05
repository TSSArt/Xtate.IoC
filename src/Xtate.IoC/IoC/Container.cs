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

[MustDisposeResource]
public sealed class Container : IServiceProvider, IDisposable, IAsyncDisposable
{
	private readonly IServiceProvider _serviceProvider;

	private Container(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

#region Interface IAsyncDisposable

	public ValueTask DisposeAsync() => Disposer.DisposeAsync(_serviceProvider);

#endregion

#region Interface IDisposable

	public void Dispose() => Disposer.Dispose(_serviceProvider);

#endregion

#region Interface IServiceProvider

	public ImplementationEntry? GetImplementationEntry(TypeKey typeKey) => _serviceProvider.GetImplementationEntry(typeKey);

	public CancellationToken DisposeToken => _serviceProvider.DisposeToken;

	public IInitializationHandler? InitializationHandler => _serviceProvider.InitializationHandler;

	public IServiceProviderDebugger? Debugger => _serviceProvider.Debugger;

#endregion

	[MustDisposeResource]
	public static Container Create(Action<IServiceCollection> addServices)
	{
		var services = new ServiceCollection();

		addServices(services);

		return new Container(services.BuildProvider());
	}

	[MustDisposeResource]
	public static Container Create<TModule>(Action<IServiceCollection>? addServices = default) where TModule : IModule, new()
	{
		var services = new ServiceCollection();
		services.AddModule<TModule>();

		addServices?.Invoke(services);

		return new Container(services.BuildProvider());
	}

	[MustDisposeResource]
	public static Container Create<TModule1, TModule2>(Action<IServiceCollection>? addServices = default) 
		where TModule1 : IModule, new() 
		where TModule2 : IModule, new()
	{
		var services = new ServiceCollection();
		services.AddModule<TModule1>();
		services.AddModule<TModule2>();

		addServices?.Invoke(services);

		return new Container(services.BuildProvider());
	}

	[MustDisposeResource]
	public static Container Create<TModule1, TModule2, TModule3>(Action<IServiceCollection>? addServices = default) 
		where TModule1 : IModule, new() 
		where TModule2 : IModule, new() 
		where TModule3 : IModule, new()
	{
		var services = new ServiceCollection();
		services.AddModule<TModule1>();
		services.AddModule<TModule2>();
		services.AddModule<TModule3>();

		addServices?.Invoke(services);

		return new Container(services.BuildProvider());
	}

	[MustDisposeResource]
	public static Container Create<TModule1, TModule2, TModule3, TModule4>(Action<IServiceCollection>? addServices = default) 
		where TModule1 : IModule, new() 
		where TModule2 : IModule, new() 
		where TModule3 : IModule, new()
		where TModule4 : IModule, new()
	{
		var services = new ServiceCollection();
		services.AddModule<TModule1>();
		services.AddModule<TModule2>();
		services.AddModule<TModule3>();
		services.AddModule<TModule4>();

		addServices?.Invoke(services);

		return new Container(services.BuildProvider());
	}
}