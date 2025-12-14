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

public static class ServiceCollectionArg2Extensions
{
	extension(IServiceCollection services)
	{
		public bool IsRegistered<T, TArg1, TArg2>() => services.IsRegistered<T, (TArg1, TArg2)>();

		public void AddType<T, TArg1, TArg2>(Option option = Option.Default) where T : notnull => services.AddType<T, (TArg1, TArg2)>(option);

		public void AddTypeSync<T, TArg1, TArg2>(Option option = Option.Default) where T : notnull => services.AddTypeSync<T, (TArg1, TArg2)>(option);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddDecorator<T, TArg1, TArg2>() where T : notnull => services.AddDecorator<T, (TArg1, TArg2)>();

		public DecoratorImplementation<T, (TArg1, TArg2)> AddDecoratorSync<T, TArg1, TArg2>() where T : notnull => services.AddDecoratorSync<T, (TArg1, TArg2)>();

		public ServiceImplementation<T, (TArg1, TArg2)> AddImplementation<T, TArg1, TArg2>() where T : notnull => services.AddImplementation<T, (TArg1, TArg2)>();

		public ServiceImplementation<T, (TArg1, TArg2)> AddImplementationSync<T, TArg1, TArg2>() where T : notnull => services.AddImplementationSync<T, (TArg1, TArg2)>();

		public void AddSharedType<T, TArg1, TArg2>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull => services.AddSharedType<T, (TArg1, TArg2)>(sharedWithin, option);

		public void AddSharedTypeSync<T, TArg1, TArg2>(SharedWithin sharedWithin, Option option = Option.Default) where T : notnull =>
			services.AddSharedTypeSync<T, (TArg1, TArg2)>(sharedWithin, option);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecorator<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			services.AddSharedDecorator<T, (TArg1, TArg2)>(sharedWithin);

		public DecoratorImplementation<T, (TArg1, TArg2)> AddSharedDecoratorSync<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			services.AddSharedDecoratorSync<T, (TArg1, TArg2)>(sharedWithin);

		public ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementation<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			services.AddSharedImplementation<T, (TArg1, TArg2)>(sharedWithin);

		public ServiceImplementation<T, (TArg1, TArg2)> AddSharedImplementationSync<T, TArg1, TArg2>(SharedWithin sharedWithin) where T : notnull =>
			services.AddSharedImplementationSync<T, (TArg1, TArg2)>(sharedWithin);

		public void AddShared<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), sharedWithin.GetScope(),
				new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddShared<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, TArg1, TArg2, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, (TArg1, TArg2)>(), sharedWithin.GetScope(), new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddSharedDecorator<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg1, TArg2, ValueTask<T>> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), sharedWithin.GetScope(),
				new Func<IServiceProvider, T, (TArg1, TArg2), ValueTask<T>>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddSharedDecorator<T, TArg1, TArg2>(SharedWithin sharedWithin, Func<IServiceProvider, T, TArg1, TArg2, T> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), sharedWithin.GetScope(),
				new Func<IServiceProvider, T, (TArg1, TArg2), T>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddTransient<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient, new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddTransient<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient, new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddTransientDecorator<T, TArg1, TArg2>(Func<IServiceProvider, T, TArg1, TArg2, ValueTask<T>> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient,
				new Func<IServiceProvider, T, (TArg1, TArg2), ValueTask<T>>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddTransientDecorator<T, TArg1, TArg2>(Func<IServiceProvider, T, TArg1, TArg2, T> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Transient,
				new Func<IServiceProvider, T, (TArg1, TArg2), T>((sp, decorated, arg) => factory(sp, decorated, arg.Item1, arg.Item2)));

		public void AddForwarding<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, ValueTask<T>> factory) =>
			services.AddEntry(
				TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Forwarding, new Func<IServiceProvider, (TArg1, TArg2), ValueTask<T>>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));

		public void AddForwarding<T, TArg1, TArg2>(Func<IServiceProvider, TArg1, TArg2, T> factory) =>
			services.AddEntry(TypeKey.ServiceKey<T, (TArg1, TArg2)>(), InstanceScope.Forwarding, new Func<IServiceProvider, (TArg1, TArg2), T>((sp, arg) => factory(sp, arg.Item1, arg.Item2)));
	}
}