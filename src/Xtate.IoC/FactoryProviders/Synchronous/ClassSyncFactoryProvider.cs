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

internal class ClassSyncFactoryProvider(Type implementationType, Type serviceType) : ClassFactoryProvider(implementationType, serviceType, async: false)
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

	protected void FillParameters<TArg>(object?[] args, IServiceProvider serviceProvider, ref TArg? arg)
	{
		for (var i = 0; i < Parameters.Length; i ++)
		{
			var parameter = Parameters[i];

			if (TupleHelper.TryMatch(parameter.MemberType, ref arg, out var value))
			{
				args[i] = value;
			}
			else
			{
				var syncValueGetter = parameter.SyncValueGetter;

				Infra.NotNull(syncValueGetter);

				args[i] = syncValueGetter(serviceProvider);
			}
		}
	}

	protected void SetRequiredMembers<TArg>(object service, IServiceProvider serviceProvider, ref TArg? arg)
	{
		foreach (var requiredMember in RequiredMembers)
		{
			var setter = requiredMember.MemberSetter;

			Infra.NotNull(setter);

			if (TupleHelper.TryMatch(requiredMember.MemberType, ref arg, out var value))
			{
				setter(service, value);
			}
			else
			{
				var syncValueGetter = requiredMember.SyncValueGetter;

				Infra.NotNull(syncValueGetter);

				setter(service, syncValueGetter(serviceProvider));
			}
		}
	}
}

internal class ClassSyncFactoryProvider<TImplementation, TService> : ClassSyncFactoryProvider
{
	private readonly Func<object?[], TService> _factory;

	private ClassSyncFactoryProvider() : base(typeof(TImplementation), typeof(TService)) => _factory = (Func<object?[], TService>) Delegate;

	public static Delegate GetServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetService<TArg>;

	public static Delegate GetDecoratorServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetDecoratorService<TArg>;

	private TService GetService<TArg>(IServiceProvider serviceProvider, TArg? arg)
	{
		var args = RentArray();

		try
		{
			if (Parameters.Length > 0)
			{
				FillParameters(args, serviceProvider, ref arg);
			}

			var service = _factory(args);

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

	private TService GetDecoratorService<TArg>(IServiceProvider serviceProvider, TService? service, TArg? arg) => GetService<(TService?, TArg?)>(serviceProvider, (service, arg));

	private static class Nested
	{
		public static readonly ClassSyncFactoryProvider<TImplementation, TService> ProviderField = new();
	}
}