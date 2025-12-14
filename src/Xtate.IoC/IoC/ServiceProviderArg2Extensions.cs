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

public static class ServiceProviderArg2Extensions
{
	extension(IServiceProvider serviceProvider)
	{
		public ValueTask<T> GetRequiredService<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetRequiredService<T, (TArg1, TArg2)>((arg1, arg2));

		public ValueTask<T?> GetService<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) => serviceProvider.GetService<T, (TArg1, TArg2)>((arg1, arg2));

		public T GetRequiredServiceSync<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetRequiredServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

		public T? GetServiceSync<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) => serviceProvider.GetServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

		public IAsyncEnumerable<T> GetServices<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetServices<T, (TArg1, TArg2)>((arg1, arg2));

		public IEnumerable<T> GetServicesSync<T, TArg1, TArg2>(TArg1 arg1, TArg2 arg2) where T : notnull => serviceProvider.GetServicesSync<T, (TArg1, TArg2)>((arg1, arg2));

		public Func<TArg1, TArg2, IAsyncEnumerable<T>> GetServicesFactory<T, TArg1, TArg2>() where T : notnull =>
			serviceProvider.GetServicesFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IAsyncEnumerable<T>>>(static (_, _) => IAsyncEnumerable<T>.Empty);

		public Func<TArg1, TArg2, IEnumerable<T>> GetServicesSyncFactory<T, TArg1, TArg2>() where T : notnull =>
			serviceProvider.GetServicesSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IEnumerable<T>>>(static (_, _) => []);

		public Func<TArg1, TArg2, ValueTask<T>> GetRequiredFactory<T, TArg1, TArg2>() where T : notnull =>
			serviceProvider.GetRequiredFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T>>>();

		public Func<TArg1, TArg2, ValueTask<T?>> GetFactory<T, TArg1, TArg2>() => serviceProvider.GetFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T?>>>(static (_, _) => default);

		public Func<TArg1, TArg2, T> GetRequiredSyncFactory<T, TArg1, TArg2>() where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T>>();

		public Func<TArg1, TArg2, T?> GetSyncFactory<T, TArg1, TArg2>() => serviceProvider.GetSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T?>>(static (_, _) => default);
	}
}