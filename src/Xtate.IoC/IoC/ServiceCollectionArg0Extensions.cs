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

public static class ServiceCollectionArg0Extensions
{
	extension(IServiceCollection services)
	{
		public bool IsRegistered<T>() => services.IsRegistered<T, Empty>();

		public void AddType<T>(Option option = Option.Default) where T : notnull => services.AddType<T, Empty>(option);

		public void AddTypeSync<T>(Option option = Option.Default) where T : notnull => services.AddTypeSync<T, Empty>(option);

		public DecoratorImplementation<T, Empty> AddDecorator<T>() where T : notnull => services.AddDecorator<T, Empty>();

		public DecoratorImplementation<T, Empty> AddDecoratorSync<T>() where T : notnull => services.AddDecoratorSync<T, Empty>();

		public ServiceImplementation<T, Empty> AddImplementation<T>() where T : notnull => services.AddImplementation<T, Empty>();

		public ServiceImplementation<T, Empty> AddImplementationSync<T>() where T : notnull => services.AddImplementationSync<T, Empty>();

		public void AddSharedType<T>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull => services.AddSharedType<T, Empty>(sharedWithin, option);

		public void AddSharedTypeSync<T>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull => services.AddSharedTypeSync<T, Empty>(sharedWithin, option);

		public DecoratorImplementation<T, Empty> AddSharedDecorator<T>(SharedWithin sharedWithin) where T : notnull => services.AddSharedDecorator<T, Empty>(sharedWithin);

		public DecoratorImplementation<T, Empty> AddSharedDecoratorSync<T>(SharedWithin sharedWithin) where T : notnull => services.AddSharedDecoratorSync<T, Empty>(sharedWithin);

		public ServiceImplementation<T, Empty> AddSharedImplementation<T>(SharedWithin sharedWithin) where T : notnull => services.AddSharedImplementation<T, Empty>(sharedWithin);

		public ServiceImplementation<T, Empty> AddSharedImplementationSync<T>(SharedWithin sharedWithin) where T : notnull => services.AddSharedImplementationSync<T, Empty>(sharedWithin);

		public void AddShared<T>(SharedWithin sharedWithin, Func<IServiceProvider, ValueTask<T>> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), sharedWithin.GetScope(), new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => factory(sp)));

		public void AddShared<T>(SharedWithin sharedWithin, Func<IServiceProvider, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), sharedWithin.GetScope(), new Func<IServiceProvider, Empty, T>((sp, _) => factory(sp)));

		public void AddSharedDecorator<T>(SharedWithin sharedWithin, Func<IServiceProvider, T, ValueTask<T>> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), sharedWithin.GetScope(), new Func<IServiceProvider, T, Empty, ValueTask<T>>((sp, decorated, _) => factory(sp, decorated)));

		public void AddSharedDecorator<T>(SharedWithin sharedWithin, Func<IServiceProvider, T, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), sharedWithin.GetScope(), new Func<IServiceProvider, T, Empty, T>((sp, decorated, _) => factory(sp, decorated)));

		public void AddTransient<T>(Func<IServiceProvider, ValueTask<T>> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => factory(sp)));

		public void AddTransient<T>(Func<IServiceProvider, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, Empty, T>((sp, _) => factory(sp)));

		public void AddTransientDecorator<T>(Func<IServiceProvider, T, ValueTask<T>> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, T, Empty, ValueTask<T>>((sp, decorated, _) => factory(sp, decorated)));

		public void AddTransientDecorator<T>(Func<IServiceProvider, T, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Transient, new Func<IServiceProvider, T, Empty, T>((sp, decorated, _) => factory(sp, decorated)));

		public void AddForwarding<T>(Func<IServiceProvider, ValueTask<T>> evaluator) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, ValueTask<T>>((sp, _) => evaluator(sp)));

		public void AddForwarding<T>(Func<IServiceProvider, T> evaluator) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, T>((sp, _) => evaluator(sp)));

		public void AddConstant<T>(T value) => services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, T>((_, _) => value));

		[Obsolete(message: "ValueTask<> shouldn't be passed as a constant. Pass Result or Convert ValueTask<> to Task<>.", error: true)]
		public void AddConstant<T>(ValueTask<T> value) =>
			services.AddEntry(TypeKey.ServiceKey<T, Empty>(), InstanceScope.Forwarding, new Func<IServiceProvider, Empty, ValueTask<T>>([ExcludeFromCodeCoverage] (_, _) => value));
	}
}