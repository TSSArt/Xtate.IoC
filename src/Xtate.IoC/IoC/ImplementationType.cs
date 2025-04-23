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
				throw new ArgumentException(Res.Format(Resources.Exception_InvalidType, type), nameof(type));
			}
		}

		_type = type;
		_openGenericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
	}

	private ImplementationType(Type openGenericType) => _type = _openGenericType = openGenericType;

	public Type Type => _type ?? throw new InvalidOperationException(Resources.Exception_ServiceTypeNotInitialized);

	public bool IsGeneric => _openGenericType is not null;

	public ImplementationType Definition => _openGenericType is not null ? new ImplementationType(_openGenericType) : default;

#region Interface IEquatable<ImplementationType>

	public bool Equals(ImplementationType other) => _type == other._type;

#endregion

	public static ImplementationType TypeOf<T>() => Infra.TypeInitHandle(() => Container<T>.Instance);

	public override bool Equals(object? obj) => obj is ImplementationType other && _type == other._type;

	public override int GetHashCode() => _type?.GetHashCode() ?? 0;

	public override string ToString() => _type?.FriendlyName() ?? string.Empty;

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

	private static Method FindMethod(IEnumerable<Method> methods)
	{
		Method? obsoleteMethod = null;
		Method? actualMethod = null;
		var multipleObsolete = false;
		var multipleActual = false;

		foreach (var method in methods)
		{
			if (!method.CanCreateMethodInfo())
			{
				continue;
			}

			if (method.HasObsoleteAttribute())
			{
				if (obsoleteMethod is null)
				{
					obsoleteMethod = method;
				}
				else
				{
					multipleObsolete = true;
				}
			}
			else
			{
				if (actualMethod is null)
				{
					actualMethod = method;
				}
				else
				{
					multipleActual = true;

					break;
				}
			}
		}

		if (multipleActual || (actualMethod is null && multipleObsolete))
		{
			throw new DependencyInjectionException(Resources.Exception_MoreThanOneMethodFound);
		}

		if ((actualMethod ?? obsoleteMethod) is { } resultMethod)
		{
			return resultMethod;
		}

		throw new DependencyInjectionException(Resources.Exception_NoMethodFound);
	}

	public MethodInfo GetMethodInfo<TService, TArg>(bool synchronousOnly)
	{
		try
		{
			var method = FindMethod(EnumerateMethods<TArg>(typeof(TService), synchronousOnly));

			return method.CreateMethodInfo();
		}
		catch (Exception ex)
		{
			var message = Res.Format(
				synchronousOnly
					? Resources.Exception_TypeDoesNotContainSyncMethodWithSignatureMethodCancellationToken
					: Resources.Exception_TypeDoesNotContainAsyncMethodWithSignatureMethodCancellationToken,
				_type, typeof(TService));

			throw new DependencyInjectionException(message, ex);
		}
	}

	private IEnumerable<Method> EnumerateMethods<TArg>(Type serviceType, bool synchronousOnly)
	{
		var implType = _openGenericType ?? _type;

		var allMethods = implType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
		var resolvedTypeArguments = _type.IsGenericType ? _type.GetGenericArguments() : null;

		foreach (var methodInfo in allMethods)
		{
			if (ValidParameters<TArg>(methodInfo, synchronousOnly))
			{
				var typeArguments = implType.IsGenericType ? implType.GetGenericArguments() : null;
				var methodArguments = methodInfo.IsGenericMethod ? methodInfo.GetGenericArguments() : null;

				if (StubType.TryMap(typeArguments, methodArguments, serviceType, GetReturnType(methodInfo, synchronousOnly)) &&
					StubType.TryMap(typeArguments, methodArguments, resolvedTypeArguments, typeArguments))
				{
					yield return new Method(implType, methodInfo, typeArguments, methodArguments);
				}
			}
		}
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

	private readonly struct Method(
		Type type,
		MethodInfo methodInfo,
		Type[]? typeArguments,
		Type[]? methodArguments)
	{
		private readonly Type[]? _methodArguments = methodArguments;

		private readonly MethodInfo _methodInfo = methodInfo;

		private readonly Type _type = type;

		private readonly Type[]? _typeArguments = typeArguments;

		public MethodInfo CreateMethodInfo()
		{
			var resultMethodInfo = _methodInfo;

			if (_typeArguments is not null)
			{
				var metadataToken = _methodInfo.MetadataToken;

				foreach (var mi in _type.MakeGenericType(_typeArguments).GetMethods())
				{
					if (mi.MetadataToken == metadataToken)
					{
						resultMethodInfo = mi;

						break;
					}
				}
			}

			return _methodArguments is null ? resultMethodInfo : resultMethodInfo.MakeGenericMethod(_methodArguments);
		}

		public bool HasObsoleteAttribute() => _methodInfo.GetCustomAttribute<ObsoleteAttribute>(inherit: false) is { IsError: false };

		public bool CanCreateMethodInfo()
		{
			if (_typeArguments is not null)
			{
				foreach (var arg in _typeArguments)
				{
					if (!StubType.IsResolvedType(arg))
					{
						return false;
					}
				}
			}

			if (_methodArguments is not null)
			{
				foreach (var arg in _methodArguments)
				{
					if (!StubType.IsResolvedType(arg))
					{
						return false;
					}
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