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

using System.Reflection;

namespace Xtate.IoC;

internal sealed class ClassSyncFactoryProvider(Type implementationType) : ClassFactoryProvider(implementationType)
{
	private static readonly MethodInfo GetSyncService;

	private static readonly MethodInfo GetRequiredSyncService;

	static ClassSyncFactoryProvider()
	{
		GetSyncService = GetMethodInfo<ClassSyncFactoryProvider>(nameof(GetServiceSyncWrapper));
		GetRequiredSyncService = GetMethodInfo<ClassSyncFactoryProvider>(nameof(GetRequiredServiceSyncWrapper));
	}

	protected override MethodInfo GetServiceMethodInfo => GetSyncService;

	protected override MethodInfo GetRequiredServiceMethodInfo => GetRequiredSyncService;

	private static object GetRequiredServiceSyncWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredServiceSync<T>();

	private static object GetServiceSyncWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetServiceSync<T>()!;

	private void FillParameters<TArg>(object?[] args, IServiceProvider serviceProvider, ref TArg? arg)
	{
		for (var i = 0; i < Parameters.Length; i ++)
		{
			if (TupleHelper.TryMatch(Parameters[i].MemberType, ref arg, out var value))
			{
				args[i] = value;
			}
			else
			{
				var syncValueGetter = Parameters[i].SyncValueGetter;

				Infra.NotNull(syncValueGetter);

				args[i] = syncValueGetter(serviceProvider);
			}
		}
	}

	private void SetRequiredMembers<TArg>(object service, IServiceProvider serviceProvider, ref TArg? arg)
	{
		for (var i = 0; i < RequiredMembers.Length; i ++)
		{
			var setter = RequiredMembers[i].MemberSetter;

			Infra.NotNull(setter);

			if (TupleHelper.TryMatch(RequiredMembers[i].MemberType, ref arg, out var value))
			{
				setter(service, value);
			}
			else
			{
				var syncValueGetter = RequiredMembers[i].SyncValueGetter;

				Infra.NotNull(syncValueGetter);

				setter(service, syncValueGetter(serviceProvider));
			}
		}
	}

	public TService GetService<TService, TArg>(IServiceProvider serviceProvider, TArg? arg)
	{
		var args = RentArray();

		try
		{
			if (Parameters.Length > 0)
			{
				FillParameters(args, serviceProvider, ref arg);
			}

			var service = ((Func<object?[], TService>) Delegate)(args);

			if (RequiredMembers.Length > 0)
			{
				SetRequiredMembers(service!, serviceProvider, ref arg);
			}

			return service;
		}
		catch (Exception ex)
		{
			throw GetFactoryException(ex);
		}
		finally
		{
			ReturnArray(args);
		}
	}

	public TService GetDecoratorService<TService, TArg>(IServiceProvider serviceProvider, TService? service, TArg? arg) => GetService<TService, (TService?, TArg?)>(serviceProvider, (service, arg));
}

internal static class ClassSyncFactoryProvider<TImplementation, TService>
{
	public static Delegate GetServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetService<TService, TArg>;

	public static Delegate GetDecoratorServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetDecoratorService<TService, TArg>;

	private static class Nested
	{
		public static readonly ClassSyncFactoryProvider ProviderField = new(typeof(TImplementation));
	}
}