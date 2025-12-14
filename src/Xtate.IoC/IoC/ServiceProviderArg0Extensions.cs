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

public static class ServiceProviderArg0Extensions
{
	extension(IServiceProvider serviceProvider)
	{
		public ValueTask<T> GetRequiredService<T>() where T : notnull => serviceProvider.GetRequiredService<T, Empty>(default);

		public ValueTask<T?> GetService<T>() => serviceProvider.GetService<T, Empty>(default);

		public T GetRequiredServiceSync<T>() where T : notnull => serviceProvider.GetRequiredServiceSync<T, Empty>(default);

		public T? GetServiceSync<T>() => serviceProvider.GetServiceSync<T, Empty>(default);

		public IAsyncEnumerable<T> GetServices<T>() where T : notnull => serviceProvider.GetServices<T, Empty>(default);

		public IEnumerable<T> GetServicesSync<T>() where T : notnull => serviceProvider.GetServicesSync<T, Empty>(default);

		public Func<ValueTask<T>> GetRequiredFactory<T>() where T : notnull => serviceProvider.GetRequiredFactoryBase<T, Empty, Func<ValueTask<T>>>();

		public Func<ValueTask<T?>> GetFactory<T>() => serviceProvider.GetFactoryBase<T, Empty, Func<ValueTask<T?>>>(static () => default);

		public Func<T> GetRequiredSyncFactory<T>() where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, Empty, Func<T>>();

		public Func<T?> GetSyncFactory<T>() => serviceProvider.GetSyncFactoryBase<T, Empty, Func<T?>>(static () => default);
	}
}