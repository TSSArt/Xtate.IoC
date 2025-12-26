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

[ExcludeFromCodeCoverage]
[SuppressMessage(category: "ReSharper", checkId: "UnusedType.Global")]
[SuppressMessage(category: "ReSharper", checkId: "UnusedMember.Global")]
public static class ServiceProviderArg3Extensions
{
	extension(IServiceProvider serviceProvider)
	{
		public ValueTask<T> GetRequiredService<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) where T : notnull =>
			serviceProvider.GetRequiredService<T, (TArg1, TArg2, TArg3)>((arg1, arg2, arg3));

		public ValueTask<T?> GetService<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) => serviceProvider.GetService<T, (TArg1, TArg2, TArg3)>((arg1, arg2, arg3));

		public T GetRequiredServiceSync<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) where T : notnull =>
			serviceProvider.GetRequiredServiceSync<T, (TArg1, TArg2, TArg3)>((arg1, arg2, arg3));

		public T? GetServiceSync<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) => serviceProvider.GetServiceSync<T, (TArg1, TArg2, TArg3)>((arg1, arg2, arg3));

		public IAsyncEnumerable<T> GetServices<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) where T : notnull =>
			serviceProvider.GetServices<T, (TArg1, TArg2, TArg3)>((arg1, arg2, arg3));

		public IEnumerable<T> GetServicesSync<T, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3) where T : notnull =>
			serviceProvider.GetServicesSync<T, (TArg1, TArg2, TArg3)>((arg1, arg2, arg3));

		public Func<TArg1, TArg2, TArg3, IAsyncEnumerable<T>> GetServicesFactory<T, TArg1, TArg2, TArg3>() where T : notnull =>
			serviceProvider.GetServicesFactoryBase<T, (TArg1, TArg2, TArg3), Func<TArg1, TArg2, TArg3, IAsyncEnumerable<T>>>(static (_, _, _) => IAsyncEnumerable<T>.Empty);

		public Func<TArg1, TArg2, TArg3, IEnumerable<T>> GetServicesSyncFactory<T, TArg1, TArg2, TArg3>() where T : notnull =>
			serviceProvider.GetServicesSyncFactoryBase<T, (TArg1, TArg2, TArg3), Func<TArg1, TArg2, TArg3, IEnumerable<T>>>(static (_, _, _) => []);

		public Func<TArg1, TArg2, TArg3, ValueTask<T>> GetRequiredFactory<T, TArg1, TArg2, TArg3>() where T : notnull =>
			serviceProvider.GetRequiredFactoryBase<T, (TArg1, TArg2, TArg3), Func<TArg1, TArg2, TArg3, ValueTask<T>>>();

		public Func<TArg1, TArg2, TArg3, ValueTask<T?>> GetFactory<T, TArg1, TArg2, TArg3>() =>
			serviceProvider.GetFactoryBase<T, (TArg1, TArg2, TArg3), Func<TArg1, TArg2, TArg3, ValueTask<T?>>>(static (_, _, _) => default);

		public Func<TArg1, TArg2, TArg3, T> GetRequiredSyncFactory<T, TArg1, TArg2, TArg3>() where T : notnull =>
			serviceProvider.GetRequiredSyncFactoryBase<T, (TArg1, TArg2, TArg3), Func<TArg1, TArg2, TArg3, T>>();

		public Func<TArg1, TArg2, TArg3, T?> GetSyncFactory<T, TArg1, TArg2, TArg3>() =>
			serviceProvider.GetSyncFactoryBase<T, (TArg1, TArg2, TArg3), Func<TArg1, TArg2, TArg3, T?>>(static (_, _, _) => default);
	}
}