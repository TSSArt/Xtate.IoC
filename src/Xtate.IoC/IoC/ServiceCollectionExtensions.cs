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

public static class ServiceCollectionExtensions
{
	private static InstanceScope GetInstanceScope(SharedWithin sharedWithin) =>
		sharedWithin switch
		{
			SharedWithin.Container => InstanceScope.Singleton,
			SharedWithin.Scope     => InstanceScope.Scoped,
			_                      => throw Infra.Unmatched(sharedWithin)
		};

	private static void AddEntry(IServiceCollection services,
								 TypeKey serviceKey,
								 InstanceScope instanceScope,
								 Delegate factory) =>
		services.Add(new ServiceEntry(serviceKey, instanceScope, factory));

	private static void AddEntry(IServiceCollection services,
								 TypeKey serviceKey,
								 InstanceScope instanceScope,
								 Option option,
								 Delegate factory)
	{
		option.Validate(Option.IfNotRegistered);

		if (!option.Has(Option.IfNotRegistered) || !services.IsRegistered(serviceKey))
		{
			services.Add(new ServiceEntry(serviceKey, instanceScope, factory));
		}
	}

	extension(IServiceCollection services)
	{
		public void AddModule<TModule>() where TModule : IModule, new()
		{
			if (!services.IsRegistered(TypeKey.ImplementationKeyFast<TModule, Empty>()))
			{
				new TModule { Services = services }.AddServices();

				services.AddImplementationSync<TModule>().For<IModule>();
			}
		}

		public bool IsRegistered<T, TArg>() => services.IsRegistered(TypeKey.ServiceKey<T, TArg>());

		public bool IsRegistered<T>() => IsRegistered<T, Empty>(services);

		public bool IsRegistered<T, TArg1, TArg2>() => IsRegistered<T, (TArg1, TArg2)>(services);

		public void AddType<T, TArg>(Option option = Option.Default) where T : notnull =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, option, ImplementationAsyncFactoryProvider<T, TArg>.Delegate());

		public void AddType<T>(Option option = Option.Default) where T : notnull => AddType<T, Empty>(services, option);

		public void AddType<T, TArg1, TArg2>(Option option = Option.Default) where T : notnull => AddType<T, (TArg1, TArg2)>(services, option);

		public void AddTypeSync<T, TArg>(Option option = Option.Default) where T : notnull =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, option, ImplementationSyncFactoryProvider<T, TArg>.Delegate());

		public void AddTypeSync<T>(Option option = Option.Default) where T : notnull => AddTypeSync<T, Empty>(services, option);

		public void AddTypeSync<T, TArg1, TArg2>(Option option = Option.Default) where T : notnull => AddTypeSync<T, (TArg1, TArg2)>(services, option);

		public DecoratorImplementation<T, TArg> AddDecorator<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

		public DecoratorImplementation<T, Empty> AddDecorator<T>() where T : notnull => AddDecorator<T, Empty>(services);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddDecorator<T, TArg1, TArg2>() where T : notnull => AddDecorator<T, (TArg1, TArg2)>(services);

		public DecoratorImplementation<T, TArg> AddDecoratorSync<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

		public DecoratorImplementation<T, Empty> AddDecoratorSync<T>() where T : notnull => AddDecoratorSync<T, Empty>(services);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddDecoratorSync<T, TArg1, TArg2>() where T : notnull => AddDecoratorSync<T, (TArg1, TArg2)>(services);

		public ServiceImplementation<T, TArg> AddImplementation<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

		public ServiceImplementation<T, Empty> AddImplementation<T>() where T : notnull => AddImplementation<T, Empty>(services);

		public ServiceImplementation<T, (TArg1, TArg2)> AddImplementation<T, TArg1, TArg2>() where T : notnull => AddImplementation<T, (TArg1, TArg2)>(services);

		public ServiceImplementation<T, TArg> AddImplementationSync<T, TArg>() where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

		public ServiceImplementation<T, Empty> AddImplementationSync<T>() where T : notnull => AddImplementationSync<T, Empty>(services);

		public ServiceImplementation<T, (TArg1, TArg2)> AddImplementationSync<T, TArg1, TArg2>() where T : notnull => AddImplementationSync<T, (TArg1, TArg2)>(services);

		public FactoryImplementation<T> AddFactory<T>() where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

		public FactoryImplementation<T> AddFactorySync<T>() where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

		public void AddSharedType<T, TArg>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), option, ImplementationAsyncFactoryProvider<T, TArg>.Delegate());

		public void AddSharedType<T>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull => AddSharedType<T, Empty>(services, sharedWithin, option);

		public void AddSharedType<T, TArg1, TArg2>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull => AddSharedType<T, (TArg1, TArg2)>(services, sharedWithin, option);

		public void AddSharedTypeSync<T, TArg>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), option, ImplementationSyncFactoryProvider<T, TArg>.Delegate());

		public void AddSharedTypeSync<T>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull => AddSharedTypeSync<T, Empty>(services, sharedWithin, option);

		public void AddSharedTypeSync<T, TArg1, TArg2>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull =>
			AddSharedTypeSync<T, (TArg1, TArg2)>(services, sharedWithin, option);

		public DecoratorImplementation<T, TArg> AddSharedDecorator<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, GetInstanceScope(sharedWithin), synchronous: false);

		public DecoratorImplementation<T, Empty> AddSharedDecorator<T>(SharedWithin sharedWithin) where T : notnull => AddSharedDecorator<T, Empty>(services, sharedWithin);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecorator<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			AddSharedDecorator<T, (TArg1, TArg2)>(services, sharedWithin);

		public DecoratorImplementation<T, TArg> AddSharedDecoratorSync<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, GetInstanceScope(sharedWithin), synchronous: true);

		public DecoratorImplementation<T, Empty> AddSharedDecoratorSync<T>(SharedWithin sharedWithin) where T : notnull => AddSharedDecoratorSync<T, Empty>(services, sharedWithin);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecoratorSync<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			AddSharedDecoratorSync<T, (TArg1, TArg2)>(services, sharedWithin);

		public ServiceImplementation<T, TArg> AddSharedImplementation<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, GetInstanceScope(sharedWithin), synchronous: false);

		public ServiceImplementation<T, Empty> AddSharedImplementation<T>(SharedWithin sharedWithin) where T : notnull => AddSharedImplementation<T, Empty>(services, sharedWithin);

		public ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementation<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			AddSharedImplementation<T, (TArg1, TArg2)>(services, sharedWithin);

		public ServiceImplementation<T, TArg> AddSharedImplementationSync<T, TArg>(SharedWithin sharedWithin) where T : notnull => new(services, GetInstanceScope(sharedWithin), synchronous: true);

		public ServiceImplementation<T, Empty> AddSharedImplementationSync<T>(SharedWithin sharedWithin) where T : notnull => AddSharedImplementationSync<T, Empty>(services, sharedWithin);

		public ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementationSync<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			AddSharedImplementationSync<T, (TArg1, TArg2)>(services, sharedWithin);

		public FactoryImplementation<T> AddSharedFactory<T>(SharedWithin sharedWithin) where T : notnull => new(services, GetInstanceScope(sharedWithin), synchronous: false);

		public FactoryImplementation<T> AddSharedFactorySync<T>(SharedWithin sharedWithin) where T : notnull => new(services, GetInstanceScope(sharedWithin), synchronous: true);

		public void AddShared<T>(SharedWithin sharedWithin, Func<IServiceProvider, ValueTask<T>> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => factory(sp)));

		public void AddShared<T>(SharedWithin sharedWithin, Func<IServiceProvider, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, Empty, T>((sp, _) => factory(sp)));

		public void AddShared<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, TArg, ValueTask<T>> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

		public void AddShared<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, TArg, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

		public void AddShared<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin),
				new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddShared<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, TArg1, TArg2, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddSharedDecorator<T>(SharedWithin sharedWithin, Func<IServiceProvider, T, ValueTask<T>> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, T, Empty, ValueTask<T>>((sp, decorated, _) => factory(sp, decorated)));

		public void AddSharedDecorator<T>(SharedWithin sharedWithin, Func<IServiceProvider, T, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), GetInstanceScope(sharedWithin), new Func<IServiceProvider, T, Empty, T>((sp, decorated, _) => factory(sp, decorated)));

		public void AddSharedDecorator<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg, ValueTask<T>> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

		public void AddSharedDecorator<T, TArg>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, TArg>(), GetInstanceScope(sharedWithin), factory);

		public void AddSharedDecorator<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg1, TArg2, ValueTask<T>> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin),
				new Func<IServiceProvider, T, (TArg1, TArg2), ValueTask<T>>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddSharedDecorator<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg1, TArg2, T> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), GetInstanceScope(sharedWithin),
				new Func<IServiceProvider, T, (TArg1, TArg2), T>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddTransient<T>(Func<IServiceProvider, ValueTask<T>> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => factory(sp)));

		public void AddTransient<T>(Func<IServiceProvider, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, Empty, T>((sp, _) => factory(sp)));

		public void AddTransient<T, TArg>(Func<IServiceProvider, TArg, ValueTask<T>> factory) => AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransient<T, TArg>(Func<IServiceProvider, TArg, T> factory) => AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransient<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient, new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddTransient<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient, new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddTransientDecorator<T>(Func<IServiceProvider, T, ValueTask<T>> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, T, Empty, ValueTask<T>>((sp, decorated, _) => factory(sp, decorated)));

		public void AddTransientDecorator<T>(Func<IServiceProvider, T, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, T, Empty, T>((sp, decorated, _) => factory(sp, decorated)));

		public void AddTransientDecorator<T, TArg>(Func<IServiceProvider, T, TArg, ValueTask<T>> factory) => AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransientDecorator<T, TArg>(Func<IServiceProvider, T, TArg, T> factory) => AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Transient, factory);

		public void AddTransientDecorator<T, TArg1, TArg2>(Func<IServiceProvider, T, TArg1, TArg2, ValueTask<T>> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient,
				new Func<IServiceProvider, T, (TArg1, TArg2), ValueTask<T>>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddTransientDecorator<T, TArg1, TArg2>(Func<IServiceProvider, T, TArg1, TArg2, T> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient,
				new Func<IServiceProvider, T, (TArg1, TArg2), T>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddForwarding<T>(Func<IServiceProvider, ValueTask<T>> evaluator) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => evaluator(sp)));

		public void AddForwarding<T>(Func<IServiceProvider, T> evaluator) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, T>((sp, _) => evaluator(sp)));

		public void AddForwarding<T, TArg>(Func<IServiceProvider, TArg, ValueTask<T>> evaluator) => AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);

		public void AddForwarding<T, TArg>(Func<IServiceProvider, TArg, T> evaluator) => AddEntry(services, TypeKey.ServiceKey<T, TArg>(), InstanceScope.Forwarding, evaluator);

		public void AddForwarding<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
			AddEntry(
				services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Forwarding, new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddForwarding<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, T> factory) =>
			AddEntry(services, TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Forwarding, new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddConstant<T>(T value) => AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, T>((_, _) => value));

		[Obsolete(message: "ValueTask<> shouldn't be passed as a constant. Pass Result or Convert ValueTask<> to Task<>.", error: true)]
		public void AddConstant<T>(ValueTask<T> value) =>
			AddEntry(services, TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, ValueTask<T>>([ExcludeFromCodeCoverage](_, _) => value));
	}
}