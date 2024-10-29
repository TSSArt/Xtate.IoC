﻿// Copyright © 2019-2024 Sergii Artemenko
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

using System.Buffers;
using System.Reflection;

namespace Xtate.IoC;

internal abstract class ClassFactoryProvider
{
	private const string RequiredMemberAttr = @"System.Runtime.CompilerServices.RequiredMemberAttribute";

	private static readonly MethodInfo GetFactory;

	private static readonly MethodInfo GetFactoryArg;

	private static readonly MethodInfo GetFactoryArg2;

	private static readonly MethodInfo GetSyncFactory;

	private static readonly MethodInfo GetSyncFactoryArg;

	private static readonly MethodInfo GetSyncFactoryArg2;

	private static readonly MethodInfo GetRequiredFactory;

	private static readonly MethodInfo GetRequiredFactoryArg;

	private static readonly MethodInfo GetRequiredFactoryArg2;

	private static readonly MethodInfo GetRequiredSyncFactory;

	private static readonly MethodInfo GetRequiredSyncFactoryArg;

	private static readonly MethodInfo GetRequiredSyncFactoryArg2;

	private static readonly MethodInfo GetServices;

	private static readonly MethodInfo GetServicesFactoryArg;

	private static readonly MethodInfo GetServicesFactoryArg2;

	private static readonly MethodInfo GetServicesSync;

	private static readonly MethodInfo GetServicesSyncFactoryArg;

	private static readonly MethodInfo GetServicesSyncFactoryArg2;

	protected readonly Delegate Delegate;

	protected readonly Member[] Parameters;

	protected readonly Member[] RequiredMembers;

	static ClassFactoryProvider()
	{
		GetServices = GetMethodInfo<ClassFactoryProvider>(nameof(GetServicesWrapper));
		GetServicesSync = GetMethodInfo<ClassFactoryProvider>(nameof(GetServicesSyncWrapper));
		GetFactory = GetMethodInfo<ClassFactoryProvider>(nameof(GetFactoryWrapper));
		GetRequiredFactory = GetMethodInfo<ClassFactoryProvider>(nameof(GetRequiredFactoryWrapper));
		GetSyncFactory = GetMethodInfo<ClassFactoryProvider>(nameof(GetSyncFactoryWrapper));
		GetRequiredSyncFactory = GetMethodInfo<ClassFactoryProvider>(nameof(GetRequiredSyncFactoryWrapper));
		GetServicesFactoryArg = GetMethodInfo<ClassFactoryProvider>(nameof(GetServicesFactoryArgWrapper));
		GetServicesSyncFactoryArg = GetMethodInfo<ClassFactoryProvider>(nameof(GetServicesSyncFactoryArgWrapper));
		GetFactoryArg = GetMethodInfo<ClassFactoryProvider>(nameof(GetFactoryArgWrapper));
		GetRequiredFactoryArg = GetMethodInfo<ClassFactoryProvider>(nameof(GetRequiredFactoryArgWrapper));
		GetSyncFactoryArg = GetMethodInfo<ClassFactoryProvider>(nameof(GetSyncFactoryArgWrapper));
		GetRequiredSyncFactoryArg = GetMethodInfo<ClassFactoryProvider>(nameof(GetRequiredSyncFactoryArgWrapper));
		GetServicesFactoryArg2 = GetMethodInfo<ClassFactoryProvider>(nameof(GetServicesFactoryArg2Wrapper));
		GetServicesSyncFactoryArg2 = GetMethodInfo<ClassFactoryProvider>(nameof(GetServicesSyncFactoryArg2Wrapper));
		GetFactoryArg2 = GetMethodInfo<ClassFactoryProvider>(nameof(GetFactoryArg2Wrapper));
		GetRequiredFactoryArg2 = GetMethodInfo<ClassFactoryProvider>(nameof(GetRequiredFactoryArg2Wrapper));
		GetSyncFactoryArg2 = GetMethodInfo<ClassFactoryProvider>(nameof(GetSyncFactoryArg2Wrapper));
		GetRequiredSyncFactoryArg2 = GetMethodInfo<ClassFactoryProvider>(nameof(GetRequiredSyncFactoryArg2Wrapper));
	}

	protected ClassFactoryProvider(Type implementationType)
	{
		var factory = GetConstructor(implementationType);
		var parameters = factory.GetParameters();

		Delegate = CreateDelegate(factory, parameters);

		if (parameters.Length > 0)
		{
			Parameters = new Member[parameters.Length];

			for (var i = 0; i < parameters.Length; i ++)
			{
				var parameter = new Parameter(parameters[i]);

				Parameters[i] = new Member(CreateGetterDelegate(parameter), parameter);
			}
		}
		else
		{
			Parameters = [];
		}

		RequiredMembers = EnumerateRequiredMembers(implementationType).ToArray();
	}

	protected abstract MethodInfo GetServiceMethodInfo { get; }

	protected abstract MethodInfo GetRequiredServiceMethodInfo { get; }

	protected static MethodInfo GetMethodInfo<T>(string name)
	{
		var methodInfo = typeof(T).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

		Infra.NotNull(methodInfo);

		return methodInfo;
	}

	protected Delegate CreateGetterDelegate(MemberBase member)
	{
		if (IsEnumerable(member.Type, out var async) is { } serviceType1)
		{
			return CreateDelegate(async ? GetServices : GetServicesSync, serviceType1);
		}

		if (IsFunc(member.Type) is { } resultType)
		{
			if (IsValueTask(resultType) is { } serviceType)
			{
				return CreateDelegate(member.IsNotNull(@"00") ? GetRequiredFactory : GetFactory, serviceType);
			}

			return CreateDelegate(member.IsNotNull(@"0") ? GetRequiredSyncFactory : GetSyncFactory, resultType);
		}

		if (IsFunc2(member.Type) is { Type: { } resultType2, ArgType: { } argType })
		{
			if (IsEnumerable(resultType2, out async) is { } serviceType2)
			{
				return CreateDelegate(async ? GetServicesFactoryArg : GetServicesSyncFactoryArg, serviceType2, argType);
			}

			if (IsValueTask(resultType2) is { } serviceType3)
			{
				return CreateDelegate(member.IsNotNull(@"10") ? GetRequiredFactoryArg : GetFactoryArg, serviceType3, argType);
			}

			return CreateDelegate(member.IsNotNull(@"1") ? GetRequiredSyncFactoryArg : GetSyncFactoryArg, resultType2, argType);
		}

		if (IsFunc3(member.Type) is { Type: { } resultType3, ArgType1: { } argType1, ArgType2: { } argType2 })
		{
			if (IsEnumerable(resultType3, out async) is { } serviceType2)
			{
				return CreateDelegate(async ? GetServicesFactoryArg2 : GetServicesSyncFactoryArg2, serviceType2, argType1, argType2);
			}

			if (IsValueTask(resultType3) is { } serviceType3)
			{
				return CreateDelegate(member.IsNotNull(@"20") ? GetRequiredFactoryArg2 : GetFactoryArg2, serviceType3, argType1, argType2);
			}

			return CreateDelegate(member.IsNotNull(@"2") ? GetRequiredSyncFactoryArg2 : GetSyncFactoryArg2, resultType3, argType1, argType2);
		}

		return CreateDelegate(member.IsNotNull() ? GetRequiredServiceMethodInfo : GetServiceMethodInfo, member.Type);
	}

	private static Type? IsEnumerable(Type type, out bool async)
	{
		if (type.IsGenericType)
		{
			async = type.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>);

			if (async || type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				return type.GetGenericArguments()[0];
			}
		}

		async = false;

		return default;
	}

	private static Type? IsFunc(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>) ? type.GetGenericArguments()[0] : default;

	private static (Type? Type, Type? ArgType) IsFunc2(Type type) =>
		type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>) && type.GetGenericArguments() is { } args ? (args[1], args[0]) : default;

	private static (Type? Type, Type? ArgType1, Type? ArgType2) IsFunc3(Type type) =>
		type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,,>) && type.GetGenericArguments() is { } args ? (args[2], args[0], args[1]) : default;

	private static Type? IsValueTask(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>) ? type.GetGenericArguments()[0] : default;

	private static Delegate CreateDelegate(MethodInfo methodInfo, params Type[] args)
	{
		methodInfo = methodInfo.MakeGenericMethod(args);

		return methodInfo.CreateDelegate(typeof(Func<,>).MakeGenericTypeExt(typeof(IServiceProvider), methodInfo.ReturnType));
	}

	private IEnumerable<Member> EnumerateRequiredMembers(Type type)
	{
		foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
		{
			if (fieldInfo.CustomAttributes.Any(data => data.AttributeType.FullName == RequiredMemberAttr))
			{
				var field = new Field(fieldInfo);

				yield return new Member(CreateGetterDelegate(field), field);
			}
		}

		foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
		{
			if (propertyInfo.CustomAttributes.Any(data => data.AttributeType.FullName == RequiredMemberAttr))
			{
				var property = new Property(propertyInfo);

				yield return new Member(CreateGetterDelegate(property), property);
			}
		}
	}

	private static ConstructorInfo GetConstructor(Type implementationType)
	{
		try
		{
			return FindConstructorInfo(implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Res.Format(Resources.Exception_CantFindFactoryForType, implementationType), ex);
		}
	}

	private static ConstructorInfo FindConstructorInfo(IEnumerable<ConstructorInfo> constructorInfos)
	{
		ConstructorInfo? obsoleteConstructorInfo = default;
		ConstructorInfo? actualConstructorInfo = default;
		var multipleObsolete = false;
		var multipleActual = false;

		foreach (var constructorInfo in constructorInfos)
		{
			if (constructorInfo.GetCustomAttribute<ObsoleteAttribute>(false) is { IsError: false })
			{
				if (obsoleteConstructorInfo is null)
				{
					obsoleteConstructorInfo = constructorInfo;
				}
				else
				{
					multipleObsolete = true;
				}
			}
			else
			{
				if (actualConstructorInfo is null)
				{
					actualConstructorInfo = constructorInfo;
				}
				else
				{
					multipleActual = true;

					break;
				}
			}
		}

		if (multipleActual || (actualConstructorInfo is null && multipleObsolete))
		{
			throw new DependencyInjectionException(Resources.Exception_MoreThanOneConstructorFound);
		}

		if ((actualConstructorInfo ?? obsoleteConstructorInfo) is { } resultConstructorInfo)
		{
			return resultConstructorInfo;
		}

		throw new DependencyInjectionException(Resources.Exception_NoConstructorFound);
	}

	protected DependencyInjectionException GetFactoryException(Exception ex) => new(Res.Format(Resources.Exception_FactoryOfRaisedException, Delegate.Method.ReturnType), ex);

	protected object?[] RentArray() => Parameters.Length > 0 ? ArrayPool<object?>.Shared.Rent(Parameters.Length) : [];

	protected void ReturnArray(object?[] array)
	{
		if (Parameters.Length == 0)
		{
			return;
		}

		Array.Clear(array, index: 0, Parameters.Length);
		ArrayPool<object?>.Shared.Return(array);
	}

	private static Delegate CreateDelegate(ConstructorInfo factory, ParameterInfo[] parameters)
	{
		var arrayParameter = Expression.Parameter(typeof(object?[]));

		var args = parameters.Length > 0 ? new Expression[parameters.Length] : [];

		for (var i = 0; i < args.Length; i ++)
		{
			var itemExpression = Expression.ArrayIndex(arrayParameter, Expression.Constant(i));
			args[i] = Expression.Convert(itemExpression, parameters[i].ParameterType);
		}

		var serviceExpression = Expression.New(factory, args);

		return Expression.Lambda(serviceExpression, arrayParameter).Compile();
	}

	protected abstract class MemberBase
	{
		public abstract Type Type { get; }

		public abstract bool IsNotNull(string path = "");

		public abstract Action<object, object?>? CreateSetter();
	}

	private class Parameter(ParameterInfo parameterInfo) : MemberBase
	{
		public override Type Type => parameterInfo.ParameterType;

		public override bool IsNotNull(string path = "") => !NullabilityHelper.IsNullable(parameterInfo, path);

		public override Action<object, object?>? CreateSetter() => default;
	}

	private class Field(FieldInfo fieldInfo) : MemberBase
	{
		public override Type Type => fieldInfo.FieldType;

		public override bool IsNotNull(string path = "") => !NullabilityHelper.IsNullable(fieldInfo, path);

		public override Action<object, object?> CreateSetter()
		{
			var service = Expression.Parameter(typeof(object));
			var value = Expression.Parameter(typeof(object));
			var field = Expression.Field(Expression.Convert(service, fieldInfo.DeclaringType!), fieldInfo);
			var body = Expression.Assign(field, Expression.Convert(value, fieldInfo.FieldType));

			return Expression.Lambda<Action<object, object?>>(body, service, value).Compile();
		}
	}

	private class Property(PropertyInfo propertyInfo) : MemberBase
	{
		public override Type Type => propertyInfo.PropertyType;

		public override bool IsNotNull(string path = "") => !NullabilityHelper.IsNullable(propertyInfo, path);

		public override Action<object, object?> CreateSetter()
		{
			var service = Expression.Parameter(typeof(object));
			var value = Expression.Parameter(typeof(object));
			var property = Expression.Property(Expression.Convert(service, propertyInfo.DeclaringType!), propertyInfo);
			var body = Expression.Assign(property, Expression.Convert(value, propertyInfo.PropertyType));

			return Expression.Lambda<Action<object, object?>>(body, service, value).Compile();
		}
	}

	protected readonly struct Member
	{
		public readonly Func<IServiceProvider, ValueTask<object?>>? AsyncValueGetter;

		public readonly Action<object, object?>? MemberSetter;

		public readonly Type MemberType;

		public readonly Func<IServiceProvider, object?>? SyncValueGetter;

		public Member(Delegate valueGetter, MemberBase memberBase)
		{
			if (valueGetter is Func<IServiceProvider, ValueTask<object?>> asyncValueGetter)
			{
				AsyncValueGetter = asyncValueGetter;
			}
			else
			{
				SyncValueGetter = (Func<IServiceProvider, object?>) valueGetter;
			}

			MemberType = memberBase.Type;
			MemberSetter = memberBase.CreateSetter();
		}
	}

#region Wrappers

	private static object GetServicesWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServices<T>();

	private static object GetServicesSyncWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSync<T>();

	private static object GetRequiredFactoryWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T>();

	private static object GetFactoryWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetFactory<T>();

	private static object GetRequiredSyncFactoryWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T>();

	private static object GetSyncFactoryWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetSyncFactory<T>();

	private static object GetRequiredFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T, TArg>();

	private static object GetFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) => serviceProvider.GetFactory<T, TArg>();

	private static object GetRequiredSyncFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T, TArg>();

	private static object GetSyncFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) => serviceProvider.GetSyncFactory<T, TArg>();

	private static object GetServicesFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesFactory<T, TArg>();

	private static object GetServicesSyncFactoryArgWrapper<T, TArg>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSyncFactory<T, TArg>();

	private static object GetRequiredFactoryArg2Wrapper<T, TArg1, TArg2>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T, TArg1, TArg2>();

	private static object GetFactoryArg2Wrapper<T, TArg1, TArg2>(IServiceProvider serviceProvider) => serviceProvider.GetFactory<T, TArg1, TArg2>();

	private static object GetRequiredSyncFactoryArg2Wrapper<T, TArg1, TArg2>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T, TArg1, TArg2>();

	private static object GetSyncFactoryArg2Wrapper<T, TArg1, TArg2>(IServiceProvider serviceProvider) => serviceProvider.GetSyncFactory<T, TArg1, TArg2>();

	private static object GetServicesFactoryArg2Wrapper<T, TArg1, TArg2>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesFactory<T, TArg1, TArg2>();

	private static object GetServicesSyncFactoryArg2Wrapper<T, TArg1, TArg2>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSyncFactory<T, TArg1, TArg2>();

#endregion
}