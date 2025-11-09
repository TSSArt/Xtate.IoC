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

public static class ServiceProviderExtensions
{
	extension(IServiceProvider serviceProvider)
	{
		public ValueTask<T> GetRequiredService<T>() where T : notnull => serviceProvider.GetRequiredService<T, Empty>(default);

		public ValueTask<T> GetRequiredService<T, TArg>(TArg arg) where T : notnull =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredService<T, TArg>(arg) ??
			new ValueTask<T>(Task.FromException<T>(MissedServiceException.Create<T, TArg>()));

		public ValueTask<T> GetRequiredService<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetRequiredService<T, (TArg1, TArg2)>((arg1, arg2));

		public ValueTask<T?> GetService<T>() => serviceProvider.GetService<T, Empty>(default);

		public ValueTask<T?> GetService<T, TArg>(TArg arg) => serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetService<T, TArg>(arg) ?? default;

		public ValueTask<T?> GetService<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) => serviceProvider.GetService<T, (TArg1, TArg2)>((arg1, arg2));

		public T GetRequiredServiceSync<T>() where T : notnull => serviceProvider.GetRequiredServiceSync<T, Empty>(default);

		public T GetRequiredServiceSync<T, TArg>(TArg arg) where T : notnull =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry
				? entry.GetRequiredServiceSync<T, TArg>(arg)
				: throw MissedServiceException.Create<T, TArg>();

		public T GetRequiredServiceSync<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetRequiredServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

		public T? GetServiceSync<T>() => serviceProvider.GetServiceSync<T, Empty>(default);

		public T? GetServiceSync<T, TArg>(TArg arg) => serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry ? entry.GetServiceSync<T, TArg>(arg) : default;

		public T? GetServiceSync<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) => serviceProvider.GetServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

		public IAsyncEnumerable<T> GetServices<T>() where T : notnull => serviceProvider.GetServices<T, Empty>(default);

		public IAsyncEnumerable<T> GetServices<T, TArg>(TArg arg) where T : notnull =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServices<T, TArg>(arg) ?? IAsyncEnumerable<T>.Empty;

		public IAsyncEnumerable<T> GetServices<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetServices<T, (TArg1, TArg2)>((arg1, arg2));

		public IEnumerable<T> GetServicesSync<T>() where T : notnull => serviceProvider.GetServicesSync<T, Empty>(default);

		public IEnumerable<T> GetServicesSync<T, TArg>(TArg arg) where T : notnull => serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSync<T, TArg>(arg) ?? [];

		public IEnumerable<T> GetServicesSync<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetServicesSync<T, (TArg1, TArg2)>((arg1, arg2));

		private TDelegate GetServicesFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		public Func<TArg, IAsyncEnumerable<T>> GetServicesFactory<T, TArg>() where T : notnull =>
			serviceProvider.GetServicesFactoryBase<T, TArg, Func<TArg, IAsyncEnumerable<T>>>(static _ => IAsyncEnumerable<T>.Empty);

		public Func<TArg1, TArg2, IAsyncEnumerable<T>> GetServicesFactory<T, TArg1, TArg2>() where T : notnull =>
			serviceProvider.GetServicesFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IAsyncEnumerable<T>>>(static (_, _) => IAsyncEnumerable<T>.Empty);

		private TDelegate GetServicesSyncFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		public Func<TArg, IEnumerable<T>> GetServicesSyncFactory<T, TArg>() where T : notnull => serviceProvider.GetServicesSyncFactoryBase<T, TArg, Func<TArg, IEnumerable<T>>>(static _ => []);

		public Func<TArg1, TArg2, IEnumerable<T>> GetServicesSyncFactory<T, TArg1, TArg2>() where T : notnull =>
			serviceProvider.GetServicesSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IEnumerable<T>>>(static (_, _) => []);

		private TDelegate GetRequiredFactoryBase<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceDelegate<T, TArg, TDelegate>() ?? throw MissedServiceException.Create<T, TArg>();

		public Func<ValueTask<T>> GetRequiredFactory<T>() where T : notnull => serviceProvider.GetRequiredFactoryBase<T, Empty, Func<ValueTask<T>>>();

		public Func<TArg, ValueTask<T>> GetRequiredFactory<T, TArg>() where T : notnull => serviceProvider.GetRequiredFactoryBase<T, TArg, Func<TArg, ValueTask<T>>>();

		public Func<TArg1, TArg2, ValueTask<T>> GetRequiredFactory<T, TArg1, TArg2>() where T : notnull =>
			serviceProvider.GetRequiredFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T>>>();

		private TDelegate GetFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServiceDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		public Func<ValueTask<T?>> GetFactory<T>() => serviceProvider.GetFactoryBase<T, Empty, Func<ValueTask<T?>>>(static () => default);

		public Func<TArg, ValueTask<T?>> GetFactory<T, TArg>() => serviceProvider.GetFactoryBase<T, TArg, Func<TArg, ValueTask<T?>>>(static _ => default);

		public Func<TArg1, TArg2, ValueTask<T?>> GetFactory<T, TArg1, TArg2>() => serviceProvider.GetFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T?>>>(static (_, _) => default);

		private TDelegate GetRequiredSyncFactoryBase<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceSyncDelegate<T, TArg, TDelegate>() ?? throw MissedServiceException.Create<T, TArg>();

		public Func<T> GetRequiredSyncFactory<T>() where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, Empty, Func<T>>();

		public Func<TArg, T> GetRequiredSyncFactory<T, TArg>() where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, TArg, Func<TArg, T>>();

		public Func<TArg1, TArg2, T> GetRequiredSyncFactory<T, TArg1, TArg2>() where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T>>();

		private TDelegate GetSyncFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServiceSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		public Func<T?> GetSyncFactory<T>() => serviceProvider.GetSyncFactoryBase<T, Empty, Func<T?>>(static () => default);

		public Func<TArg, T?> GetSyncFactory<T, TArg>() => serviceProvider.GetSyncFactoryBase<T, TArg, Func<TArg, T?>>(static _ => default);

		public Func<TArg1, TArg2, T?> GetSyncFactory<T, TArg1, TArg2>() => serviceProvider.GetSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T?>>(static (_, _) => default);
	}
}