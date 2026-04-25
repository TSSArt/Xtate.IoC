// Copyright © 2019-2026 Sergii Artemenko
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

internal readonly struct ImplementationType : IEquatable<ImplementationType>
{
	private readonly Type? _openGenericType;

	private readonly Type _type;

	private ImplementationType(Type type, bool validate)
	{
		if (validate)
		{
			if (type.IsInterface || type.IsAbstract)
			{
				throw new ArgumentException(Resources.Exception_InvalidType(type), nameof(type));
			}
		}

		_type = type;
		_openGenericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
	}

	private ImplementationType(Type openGenericType) => _type = _openGenericType = openGenericType;

	public Type Type => _type ?? throw new InvalidOperationException(Resources.Exception_ImplementationTypeNotInitialized);

	public bool IsGeneric => _openGenericType is not null;

	public ImplementationType Definition => _openGenericType is not null ? new ImplementationType(_openGenericType) : default;

#region Interface IEquatable<ImplementationType>

	public bool Equals(ImplementationType other) => _type == other._type;

#endregion

	public static ImplementationType TypeOf<T>() => Infra.TypeInitHandle(() => Container<T>.Instance);

	public override bool Equals(object? obj) => obj is ImplementationType other && _type == other._type;

	public override int GetHashCode() => _type?.GetHashCode() ?? 0;

	public override string ToString() => _type?.FriendlyName ?? string.Empty;

	public bool TryConstruct(ServiceType serviceType, out ImplementationType resultImplementationType)
	{
		if (EnumerateContracts(serviceType.Type).FirstOrDefault(static contract => contract.CanCreateType()) is { IsDefault: false } contract)
		{
			resultImplementationType = new ImplementationType(contract.CreateType(), validate: false);

			return true;
		}

		resultImplementationType = default;

		return false;
	}

	private IEnumerable<Contract> EnumerateContracts(Type serviceType)
	{
		var implType = _openGenericType ?? _type;

		for (var type = implType; type is not null; type = type.BaseType)
		{
			if (TryMap(type, serviceType) is { } args)
			{
				yield return new Contract(implType, args);
			}
		}

		foreach (var itf in implType.GetInterfaces())
		{
			if (TryMap(itf, serviceType) is { } args)
			{
				yield return new Contract(implType, args);
			}
		}
	}

	private static MethodInfo FindMethod(IEnumerable<MethodInfo> methodInfos)
	{
		MethodInfo? obsoleteMethodInfo = null;
		MethodInfo? actualMethodInfo = null;
		var multipleObsolete = false;
		var multipleActual = false;

		foreach (var methodInfo in methodInfos)
		{
			if (methodInfo.GetCustomAttribute<ObsoleteAttribute>(inherit: false) is { IsError: false })
			{
				if (obsoleteMethodInfo is null)
				{
					obsoleteMethodInfo = methodInfo;
				}
				else
				{
					multipleObsolete = true;
				}
			}
			else
			{
				if (actualMethodInfo is null)
				{
					actualMethodInfo = methodInfo;
				}
				else
				{
					multipleActual = true;

					break;
				}
			}
		}

		if (multipleActual || (actualMethodInfo is null && multipleObsolete))
		{
			throw new DependencyInjectionException(Resources.Exception_MoreThanOneMethodFound);
		}

		if ((actualMethodInfo ?? obsoleteMethodInfo) is { } resultMethod)
		{
			return resultMethod;
		}

		throw new DependencyInjectionException(Resources.Exception_NoMethodFound);
	}

	public MethodInfo GetMethodInfo<TService, TArg>(bool synchronousOnly)
	{
		try
		{
			return FindMethod(EnumerateMethods<TArg>(typeof(TService), synchronousOnly));
		}
		catch (Exception ex)
		{
			var message = synchronousOnly
				? Resources.Exception_TypeDoesNotContainSyncMethodWithSignatureMethodCancellationToken(_type, typeof(TService))
				: Resources.Exception_TypeDoesNotContainAsyncMethodWithSignatureMethodCancellationToken(_type, typeof(TService));

			throw new DependencyInjectionException(message, ex);
		}
	}

	private IEnumerable<MethodInfo> EnumerateMethods<TArg>(Type serviceType, bool synchronousOnly)
	{
		var implType = _openGenericType ?? _type;

		var allMethods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
		var resolvedTypeArguments = _type.IsGenericType ? _type.GetGenericArguments() : null;

		foreach (var methodInfo in allMethods)
		{
			var typeArgs = implType.IsGenericType ? implType.GetGenericArguments() : null;
			var methodArgs = methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments() : null;

			if (!StubType.TryMap(typeArgs, methodArgs, serviceType, GetReturnType(methodInfo, synchronousOnly)))
			{
				continue;
			}

			if (!StubType.TryMap(typeArgs, methodArgs, resolvedTypeArguments, typeArgs))
			{
				continue;
			}

			if (typeArgs is not null && !Array.TrueForAll(typeArgs, StubType.IsResolvedType))
			{
				continue;
			}

			if (methodArgs is not null && !Array.TrueForAll(methodArgs, StubType.IsResolvedType))
			{
				continue;
			}

			var typedMethodInfo = MakeTypedMethodInfo(methodInfo, typeArgs, methodArgs);

			if (ValidParameters<TArg>(typedMethodInfo, synchronousOnly))
			{
				yield return typedMethodInfo;
			}
		}
	}

	private static MethodInfo MakeTypedMethodInfo(MethodInfo methodInfo, Type[]? typeArgs, Type[]? methodArgs)
	{
		if (typeArgs is not null)
		{
			var metadataToken = methodInfo.MetadataToken;

			foreach (var mi in methodInfo.ReflectedType!.MakeGenericType(typeArgs).GetMethods())
			{
				if (mi.MetadataToken == metadataToken)
				{
					methodInfo = mi;

					break;
				}
			}
		}

		return methodArgs is not null ? methodInfo.MakeGenericMethod(methodArgs) : methodInfo;
	}

	private static bool ValidParameters<TArg>(MethodBase methodBase, bool synchronousOnly)
	{
		foreach (var parameterInfo in methodBase.GetParameters())
		{
			if (!synchronousOnly && parameterInfo.ParameterType == typeof(CancellationToken))
			{
				continue;
			}

			if (TupleHelper.IsMatch<TArg>(parameterInfo.ParameterType))
			{
				continue;
			}

			return false;
		}

		return true;
	}

	private static Type GetReturnType(MethodInfo methodInfo, bool synchronousOnly)
	{
		if (!synchronousOnly && methodInfo.ReturnType is { IsGenericType: true } rt && rt.GetGenericTypeDefinition() == typeof(ValueTask<>))
		{
			return rt.GetGenericArguments()[0];
		}

		return methodInfo.ReturnType;
	}

	private Type[]? TryMap(Type type1, Type type2)
	{
		var implementationArguments = _openGenericType?.GetGenericArguments() ?? [];

		if (StubType.TryMap(implementationArguments, typesToMap2: null, type1, type2) &&
			StubType.TryMap(typesToMap1: null, typesToMap2: null, implementationArguments, _type.GetGenericArguments()))
		{
			return implementationArguments;
		}

		return null;
	}

	private readonly struct Contract(Type type, Type[] args)
	{
		private readonly Type[] _args = args;

		private readonly Type _type = type;

		public bool IsDefault => _type is null;

		public Type CreateType() => _args.Length > 0 ? _type.MakeGenericType(_args) : _type;

		public bool CanCreateType()
		{
			foreach (var arg in _args)
			{
				if (!StubType.IsResolvedType(arg))
				{
					return false;
				}
			}

			return true;
		}
	}

	private static class Container<T>
	{
		public static readonly ImplementationType Instance = new(typeof(T), validate: true);
	}
}