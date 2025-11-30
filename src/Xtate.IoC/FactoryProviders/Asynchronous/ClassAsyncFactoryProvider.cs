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

internal sealed class ClassAsyncFactoryProvider(Type implementationType) : ClassFactoryProvider(implementationType)
{
	private static readonly MethodInfo GetAsyncService;

	private static readonly MethodInfo GetRequiredAsyncService;

	static ClassAsyncFactoryProvider()
	{
		GetAsyncService = GetMethodInfo<ClassAsyncFactoryProvider>(nameof(GetServiceWrapper));
		GetRequiredAsyncService = GetMethodInfo<ClassAsyncFactoryProvider>(nameof(GetRequiredServiceWrapper));
	}

	protected override MethodInfo GetServiceMethodInfo => GetAsyncService;

	protected override MethodInfo GetRequiredServiceMethodInfo => GetRequiredAsyncService;

	private static ValueTask<object> GetRequiredServiceWrapper<T>(IServiceProvider serviceProvider) where T : notnull
	{
		var valueTask = serviceProvider.GetRequiredService<T>();

		return valueTask.IsCompletedSuccessfully ? new ValueTask<object>(valueTask.Result) : Wait(valueTask);

		static async ValueTask<object> Wait(ValueTask<T> valueTask) => await valueTask.ConfigureAwait(false);
	}

	private static ValueTask<object?> GetServiceWrapper<T>(IServiceProvider serviceProvider)
	{
		var valueTask = serviceProvider.GetService<T>();

		return valueTask.IsCompletedSuccessfully ? new ValueTask<object?>(valueTask.Result) : Wait(valueTask);

		static async ValueTask<object?> Wait(ValueTask<T?> valueTask) => await valueTask.ConfigureAwait(false);
	}

	private ValueTask FillParameters<TArg>(int start,
										   object?[] args,
										   IServiceProvider serviceProvider,
										   TArg? arg)
	{
		for (var i = start; i < Parameters.Length; i ++)
		{
			if (TupleHelper.TryMatch(Parameters[i].MemberType, ref arg, out var value))
			{
				args[i] = value;
			}
			else if (Parameters[i].AsyncValueGetter is { } asyncValueGetter)
			{
				var valueTask = asyncValueGetter(serviceProvider);

				if (!valueTask.IsCompletedSuccessfully)
				{
					return FillParametersWait(valueTask, i, args, serviceProvider, arg);
				}

				args[i] = valueTask.Result;
			}
			else
			{
				args[i] = Parameters[i].SyncValueGetter!(serviceProvider);
			}
		}

		return ValueTask.CompletedTask;
	}

	private async ValueTask FillParametersWait<TArg>(ValueTask<object?> valueTask,
													 int index,
													 object?[] args,
													 IServiceProvider serviceProvider,
													 TArg? arg)
	{
		args[index] = await valueTask.ConfigureAwait(false);

		await FillParameters(index + 1, args, serviceProvider, arg).ConfigureAwait(false);
	}

	private ValueTask SetRequiredMembers<TArg>(int start,
											   object service,
											   IServiceProvider serviceProvider,
											   TArg? arg)
	{
		for (var i = start; i < RequiredMembers.Length; i ++)
		{
			var setter = RequiredMembers[i].MemberSetter;
			Infra.NotNull(setter);

			if (TupleHelper.TryMatch(RequiredMembers[i].MemberType, ref arg, out var value))
			{
				setter(service, value);
			}
			else if (RequiredMembers[i].AsyncValueGetter is { } asyncValueGetter)
			{
				var valueTask = asyncValueGetter(serviceProvider);

				if (!valueTask.IsCompletedSuccessfully)
				{
					return SetRequiredMembersWait(valueTask, i, service, serviceProvider, arg);
				}

				setter(service, valueTask.Result);
			}
			else
			{
				setter(service, RequiredMembers[i].SyncValueGetter!(serviceProvider));
			}
		}

		return ValueTask.CompletedTask;
	}

	private async ValueTask SetRequiredMembersWait<TArg>(ValueTask<object?> valueTask,
														 int index,
														 object service,
														 IServiceProvider serviceProvider,
														 TArg? arg)
	{
		RequiredMembers[index].MemberSetter!(service, await valueTask.ConfigureAwait(false));

		await SetRequiredMembers(index + 1, service, serviceProvider, arg).ConfigureAwait(false);
	}

	public ValueTask<TService> GetDecoratorService<TService, TArg>(IServiceProvider serviceProvider, TService? service, TArg? arg) =>
		GetService<TService, (TService?, TArg?)>(serviceProvider, (service, arg));

	public ValueTask<TService> GetService<TService, TArg>(IServiceProvider serviceProvider, TArg? arg)
	{
		var valueTask = CreateInstance<TService, TArg>(serviceProvider, arg);

		if (RequiredMembers.Length == 0)
		{
			return valueTask;
		}

		if (valueTask.IsCompletedSuccessfully)
		{
			return PostCreateInstance(valueTask.Result, serviceProvider, arg);
		}

		return GetServiceWait(valueTask, serviceProvider, arg);
	}

	private async ValueTask<TService> GetServiceWait<TService, TArg>(ValueTask<TService> valueTask, IServiceProvider serviceProvider, TArg? arg)
	{
		var service = await valueTask.ConfigureAwait(false);

		return await PostCreateInstance(service, serviceProvider, arg).ConfigureAwait(false);
	}

	private ValueTask<TService> CreateInstance<TService, TArg>(IServiceProvider serviceProvider, TArg? arg)
	{
		var factory = (Func<object?[], TService>) Delegate;

		if (Parameters.Length == 0)
		{
			try
			{
				return new ValueTask<TService>(factory([]));
			}
			catch (Exception ex)
			{
				throw GetFactoryException(ex);
			}
		}

		var args = RentArray();
		ValueTask valueTask;

		try
		{
			valueTask = FillParameters(start: 0, args, serviceProvider, arg);

			if (valueTask.IsCompletedSuccessfully)
			{
				var service = factory(args);
				ReturnArray(args);

				return new ValueTask<TService>(service);
			}
		}
		catch (Exception ex)
		{
			ReturnArray(args);

			throw GetFactoryException(ex);
		}

		return CreateInstanceWait<TService>(valueTask, args);
	}

	private async ValueTask<TService> CreateInstanceWait<TService>(ValueTask valueTask, object?[] args)
	{
		try
		{
			await valueTask.ConfigureAwait(false);

			return ((Func<object?[], TService>) Delegate)(args);
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

	private ValueTask<TService> PostCreateInstance<TService, TArg>(TService service, IServiceProvider serviceProvider, TArg? arg)
	{
		ValueTask valueTask;

		try
		{
			valueTask = SetRequiredMembers(start: 0, service!, serviceProvider, arg);

			if (valueTask.IsCompletedSuccessfully)
			{
				return new ValueTask<TService>(service);
			}
		}
		catch (Exception ex)
		{
			throw GetFactoryException(ex);
		}

		return PostCreateInstanceWait(valueTask, service);
	}

	private async ValueTask<TService> PostCreateInstanceWait<TService>(ValueTask valueTask, TService service)
	{
		try
		{
			await valueTask.ConfigureAwait(false);

			return service;
		}
		catch (Exception ex)
		{
			throw GetFactoryException(ex);
		}
	}
}

internal static class ClassAsyncFactoryProvider<TImplementation, TService>
{
	public static Delegate GetServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetService<TService, TArg>;

	public static Delegate GetDecoratorServiceDelegate<TArg>() => Infra.TypeInitHandle(() => Nested.ProviderField).GetDecoratorService<TService, TArg>;

	private static class Nested
	{
		public static readonly ClassAsyncFactoryProvider ProviderField = new(typeof(TImplementation));
	}
}