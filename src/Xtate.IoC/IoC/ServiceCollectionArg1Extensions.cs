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

public static class ServiceCollectionArg1Extensions
{
	extension(IServiceCollection services)
	{
		public bool IsRegistered<T, TArg>() => services.IsRegistered(TypeKey.ServiceKey<T, TArg>());

		public void AddType<T, TArg>(Option option = Option.Default) where T : notnull =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, option, ImplementationAsyncFactoryProvider<T, TArg>.Delegate());

		public void AddTypeSync<T, TArg>(Option option = Option.Default) where T : notnull =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, option, ImplementationSyncFactoryProvider<T, TArg>.Delegate());

		public DecoratorImplementation<T, TArg> AddDecorator<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

		public DecoratorImplementation<T, TArg> AddDecoratorSync<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

		public ServiceImplementation<T, TArg> AddImplementation<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

		public ServiceImplementation<T, TArg> AddImplementationSync<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

		public void AddSharedType<T, TArg>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), sharedWithin.GetScope(), option, ImplementationAsyncFactoryProvider<T, TArg>.Delegate());

		public void AddSharedTypeSync<T, TArg>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), sharedWithin.GetScope(), option, ImplementationSyncFactoryProvider<T, TArg>.Delegate());

		public DecoratorImplementation<T, TArg> AddSharedDecorator<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, sharedWithin.GetScope(), synchronous: false);

		public DecoratorImplementation<T, TArg> AddSharedDecoratorSync<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, sharedWithin.GetScope(), synchronous: true);

		public ServiceImplementation<T, TArg> AddSharedImplementation<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, sharedWithin.GetScope(), synchronous: false);

		public ServiceImplementation<T, TArg> AddSharedImplementationSync<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, sharedWithin.GetScope(), synchronous: true);

		public void AddShared<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), sharedWithin.GetScope(), factory);

		public void AddShared<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, TArg, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), sharedWithin.GetScope(), factory);

		public void AddSharedDecorator<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), sharedWithin.GetScope(), factory);

		public void AddSharedDecorator<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, TArg>(), sharedWithin.GetScope(), factory);

		public void AddTransient<T, TArg>(Func<IServiceProvider, TArg, ValueTask<T>> factory) => services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransient<T, TArg>(Func<IServiceProvider, TArg, T> factory) => services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransientDecorator<T, TArg>(Func<IServiceProvider, T, TArg, ValueTask<T>> factory) => services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransientDecorator<T, TArg>(Func<IServiceProvider, T, TArg, T> factory) => services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddForwarding<T, TArg>(Func<IServiceProvider, TArg, ValueTask<T>> evaluator) => services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);

		public void AddForwarding<T, TArg>(Func<IServiceProvider, TArg, T> evaluator) => services.AddEntry(TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);
	}
}