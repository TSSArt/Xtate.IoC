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

public static class ServiceProviderArg1Extensions
{
	extension(IServiceProvider serviceProvider)
	{
		public ValueTask<T> GetRequiredService<T, TArg>(TArg arg) where T : notnull =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredService<T, TArg>(arg) ?? throw MissedServiceException.Create<T, TArg>();

		public ValueTask<T?> GetService<T, TArg>(TArg arg) => serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetService<T, TArg>(arg) ?? default;

		public T GetRequiredServiceSync<T, TArg>(TArg arg) where T : notnull =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry
				? entry.GetRequiredServiceSync<T, TArg>(arg)
				: throw MissedServiceException.Create<T, TArg>();

		public T? GetServiceSync<T, TArg>(TArg arg) => serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry ? entry.GetServiceSync<T, TArg>(arg) : default;

		public IAsyncEnumerable<T> GetServices<T, TArg>(TArg arg) where T : notnull =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServices<T, TArg>(arg) ?? IAsyncEnumerable<T>.Empty;

		public IEnumerable<T> GetServicesSync<T, TArg>(TArg arg) where T : notnull => serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSync<T, TArg>(arg) ?? [];

		public Func<TArg, IAsyncEnumerable<T>> GetServicesFactory<T, TArg>() where T : notnull =>
			serviceProvider.GetServicesFactoryBase<T, TArg, Func<TArg, IAsyncEnumerable<T>>>(static _ => IAsyncEnumerable<T>.Empty);

		public Func<TArg, IEnumerable<T>> GetServicesSyncFactory<T, TArg>() where T : notnull => serviceProvider.GetServicesSyncFactoryBase<T, TArg, Func<TArg, IEnumerable<T>>>(static _ => []);

		public Func<TArg, ValueTask<T>> GetRequiredFactory<T, TArg>() where T : notnull => serviceProvider.GetRequiredFactoryBase<T, TArg, Func<TArg, ValueTask<T>>>();

		public Func<TArg, ValueTask<T?>> GetFactory<T, TArg>() => serviceProvider.GetFactoryBase<T, TArg, Func<TArg, ValueTask<T?>>>(static _ => default);

		public Func<TArg, T> GetRequiredSyncFactory<T, TArg>() where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, TArg, Func<TArg, T>>();

		public Func<TArg, T?> GetSyncFactory<T, TArg>() => serviceProvider.GetSyncFactoryBase<T, TArg, Func<TArg, T?>>(static _ => default);
	}
}