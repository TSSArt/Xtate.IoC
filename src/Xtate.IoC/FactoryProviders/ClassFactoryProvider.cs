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

using System.Buffers;
using System.Reflection;

namespace Xtate.IoC;

internal abstract partial class ClassFactoryProvider
{
	private const string RequiredMemberAttr = @"System.Runtime.CompilerServices.RequiredMemberAttribute";

	private static readonly GetterDelegateCreator[] GetterDelegateCreators =
	[
		new EnumerableGetterDelegateCreator(),
		new FuncNoArgsGetterDelegateCreator(),
		new FuncArg1GetterDelegateCreator(),
		new FuncArg2GetterDelegateCreator(),
		new FuncArg3GetterDelegateCreator(),
		new FuncArg4GetterDelegateCreator()
	];

	private static readonly ArrayPool<object?> ArgsPool = ArrayPool<object?>.Create(maxArrayLength: 32, maxArraysPerBucket: 64);

	protected readonly Delegate Delegate;

	protected readonly Member[] Parameters;

	protected readonly Member[] RequiredMembers;

	protected ClassFactoryProvider(Type implementationType)
	{
		var factory = GetConstructor(implementationType);
		var parameters = factory?.GetParameters();

		Delegate = factory is not null && parameters is not null ? CreateDelegate(factory, parameters) : CreateDelegate(implementationType);

		if (parameters?.Length > 0)
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

		RequiredMembers = [..EnumerateRequiredMembers(implementationType)];
	}

	protected abstract MethodInfo GetServiceMethodInfo { get; }

	protected abstract MethodInfo GetRequiredServiceMethodInfo { get; }

	protected static MethodInfo GetMethodInfo<T>(string name)
	{
		var methodInfo = typeof(T).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

		Infra.NotNull(methodInfo);

		return methodInfo;
	}

	private Delegate CreateGetterDelegate(MemberBase member)
	{
		foreach (var getterDelegateCreator in GetterDelegateCreators)
		{
			if (getterDelegateCreator.TryCreate(member) is { } getterDelegate)
			{
				return getterDelegate;
			}
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

		return null;
	}

	private static Type? IsFunc(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<>) ? type.GetGenericArguments()[0] : null;

	private static Type? IsValueTask(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>) ? type.GetGenericArguments()[0] : null;

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

	private static ConstructorInfo? GetConstructor(Type implementationType)
	{
		try
		{
			return FindConstructorInfo(implementationType.IsValueType, implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
		}
		catch (Exception ex)
		{
			throw new DependencyInjectionException(Resources.Exception_CantFindFactoryForType(implementationType), ex);
		}
	}

	private static ConstructorInfo? FindConstructorInfo(bool isValueType, IEnumerable<ConstructorInfo> constructorInfos)
	{
		ConstructorInfo? obsoleteConstructorInfo = null;
		ConstructorInfo? actualConstructorInfo = null;
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

		if (isValueType)
		{
			return null;
		}

		throw new DependencyInjectionException(Resources.Exception_NoConstructorFound);
	}

	protected DependencyInjectionException GetFactoryException(Exception ex) => new(Resources.Exception_FactoryOfRaisedException(Delegate.Method.ReturnType), ex);

	protected object?[] RentArray() => Parameters.Length > 0 ? ArgsPool.Rent(Parameters.Length) : [];

	protected void ReturnArray(object?[] array)
	{
		switch (Parameters.Length)
		{
			case 0:
				return;

			case 4:
				array[3] = null;

				goto case 3;

			case 3:
				array[2] = null;

				goto case 2;

			case 2:
				array[1] = null;

				goto case 1;

			case 1:
				array[0] = null;

				break;

			default:
#if NETSTANDARD2_1 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0_OR_GREATER
				array.AsSpan(start: 0, Parameters.Length).Clear();
#else
				Array.Clear(array, index: 0, Parameters.Length);
#endif

				break;
		}

		ArgsPool.Return(array);
	}

	private static Delegate CreateDelegate(Type type)
	{
		var arrayParameter = Expression.Parameter(typeof(object?[]));

		var serviceExpression = Expression.New(type);

		return Expression.Lambda(serviceExpression, arrayParameter).Compile();
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

	private abstract class GetterDelegateCreator
	{
		public abstract Delegate? TryCreate(MemberBase member);
	}

	internal abstract class MemberBase
	{
		public abstract Type Type { get; }

		public abstract bool IsNotNull(string path = "");

		public abstract Action<object, object?>? CreateSetter();
	}

	private class Parameter(ParameterInfo parameterInfo) : MemberBase
	{
		public override Type Type => parameterInfo.ParameterType;

		public override bool IsNotNull(string path = "") => !NullabilityHelper.IsNullable(parameterInfo, path);

		public override Action<object, object?>? CreateSetter() => null;
	}

	protected class Field(FieldInfo fieldInfo) : MemberBase
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
}