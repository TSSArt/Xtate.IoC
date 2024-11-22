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

public static class ServiceProviderExtensions
{
	public static ValueTask<T> GetRequiredService<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredService<T, Empty>(default);

	public static ValueTask<T> GetRequiredService<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredService<T, TArg>(arg) ??
		new ValueTask<T>(Task.FromException<T>(new MissedServiceException<T, TArg>()));

	public static ValueTask<T> GetRequiredService<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetRequiredService<T, (TArg1, TArg2)>((arg1, arg2));

	public static ValueTask<T?> GetService<T>(this IServiceProvider serviceProvider) => serviceProvider.GetService<T, Empty>(default);

	public static ValueTask<T?> GetService<T, TArg>(this IServiceProvider serviceProvider, TArg arg) =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetService<T, TArg>(arg) ?? default;

	public static ValueTask<T?> GetService<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) => serviceProvider.GetService<T, (TArg1, TArg2)>((arg1, arg2));

	public static T GetRequiredServiceSync<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredServiceSync<T, Empty>(default);

	public static T GetRequiredServiceSync<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry
			? entry.GetRequiredServiceSync<T, TArg>(arg)
			: throw new MissedServiceException<T, TArg>();

	public static T GetRequiredServiceSync<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetRequiredServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

	public static T? GetServiceSync<T>(this IServiceProvider serviceProvider) => serviceProvider.GetServiceSync<T, Empty>(default);

	public static T? GetServiceSync<T, TArg>(this IServiceProvider serviceProvider, TArg arg) =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>()) is { } entry ? entry.GetServiceSync<T, TArg>(arg) : default;

	public static T? GetServiceSync<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) => serviceProvider.GetServiceSync<T, (TArg1, TArg2)>((arg1, arg2));

	public static IAsyncEnumerable<T> GetServices<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServices<T, Empty>(default);

	public static IAsyncEnumerable<T> GetServices<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServices<T, TArg>(arg) ?? AsyncEnumerable.Empty<T>();

	public static IAsyncEnumerable<T> GetServices<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetServices<T, (TArg1, TArg2)>((arg1, arg2));

	public static IEnumerable<T> GetServicesSync<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSync<T, Empty>(default);

	public static IEnumerable<T> GetServicesSync<T, TArg>(this IServiceProvider serviceProvider, TArg arg) where T : notnull =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSync<T, TArg>(arg) ?? [];

	public static IEnumerable<T> GetServicesSync<T, TArg1, TArg2>(this IServiceProvider serviceProvider, TArg1 arg1, TArg2 arg2) where T : notnull =>
		serviceProvider.GetServicesSync<T, (TArg1, TArg2)>((arg1, arg2));

	private static TDelegate GetServicesFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<TArg, IAsyncEnumerable<T>> GetServicesFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesFactoryBase<T, TArg, Func<TArg, IAsyncEnumerable<T>>>(static _ => AsyncEnumerable.Empty<T>());

	public static Func<TArg1, TArg2, IAsyncEnumerable<T>> GetServicesFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IAsyncEnumerable<T>>>(static (_, _) => AsyncEnumerable.Empty<T>());

	private static TDelegate GetServicesSyncFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<TArg, IEnumerable<T>> GetServicesSyncFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesSyncFactoryBase<T, TArg, Func<TArg, IEnumerable<T>>>(static _ => []);

	public static Func<TArg1, TArg2, IEnumerable<T>> GetServicesSyncFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetServicesSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, IEnumerable<T>>>(static (_, _) => []);

	private static TDelegate GetRequiredFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceDelegate<T, TArg, TDelegate>() ?? throw new MissedServiceException<T, TArg>();

	public static Func<ValueTask<T>> GetRequiredFactory<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactoryBase<T, Empty, Func<ValueTask<T>>>();

	public static Func<TArg, ValueTask<T>> GetRequiredFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetRequiredFactoryBase<T, TArg, Func<TArg, ValueTask<T>>>();

	public static Func<TArg1, TArg2, ValueTask<T>> GetRequiredFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetRequiredFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T>>>();

	private static TDelegate GetFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServiceDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<ValueTask<T?>> GetFactory<T>(this IServiceProvider serviceProvider) => serviceProvider.GetFactoryBase<T, Empty, Func<ValueTask<T?>>>(static () => default);

	public static Func<TArg, ValueTask<T?>> GetFactory<T, TArg>(this IServiceProvider serviceProvider) => serviceProvider.GetFactoryBase<T, TArg, Func<TArg, ValueTask<T?>>>(static _ => default);

	public static Func<TArg1, TArg2, ValueTask<T?>> GetFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) =>
		serviceProvider.GetFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, ValueTask<T?>>>(static (_, _) => default);

	private static TDelegate GetRequiredSyncFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider) where T : notnull where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceSyncDelegate<T, TArg, TDelegate>() ?? throw new MissedServiceException<T, TArg>();

	public static Func<T> GetRequiredSyncFactory<T>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, Empty, Func<T>>();

	public static Func<TArg, T> GetRequiredSyncFactory<T, TArg>(this IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactoryBase<T, TArg, Func<TArg, T>>();

	public static Func<TArg1, TArg2, T> GetRequiredSyncFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) where T : notnull =>
		serviceProvider.GetRequiredSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T>>();

	private static TDelegate GetSyncFactoryBase<T, TArg, TDelegate>(this IServiceProvider serviceProvider, TDelegate emptyDelegate) where TDelegate : Delegate =>
		serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServiceSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

	public static Func<T?> GetSyncFactory<T>(this IServiceProvider serviceProvider) => serviceProvider.GetSyncFactoryBase<T, Empty, Func<T?>>(static () => default);

	public static Func<TArg, T?> GetSyncFactory<T, TArg>(this IServiceProvider serviceProvider) => serviceProvider.GetSyncFactoryBase<T, TArg, Func<TArg, T?>>(static _ => default);

	public static Func<TArg1, TArg2, T?> GetSyncFactory<T, TArg1, TArg2>(this IServiceProvider serviceProvider) =>
		serviceProvider.GetSyncFactoryBase<T, (TArg1, TArg2), Func<TArg1, TArg2, T?>>(static (_, _) => default);
}