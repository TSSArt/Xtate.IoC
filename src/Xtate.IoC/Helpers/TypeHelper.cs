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

using System.Text;

namespace Xtate.IoC;

internal static class TypeHelper
{
	/// <summary>
	///     Tries to get the simple name of the specified type.
	/// </summary>
	/// <param name="type">The type to get the simple name of.</param>
	/// <returns>The simple name of the type, or null if the type does not have a simple name.</returns>
	private static string? TryGetTypeAlias(Type type)
	{
		if (type.IsEnum)
		{
			return null;
		}

		var name = Type.GetTypeCode(type) switch
				   {
					   TypeCode.Boolean => @"bool",
					   TypeCode.Byte    => @"byte",
					   TypeCode.Char    => @"char",
					   TypeCode.Decimal => @"decimal",
					   TypeCode.Double  => @"double",
					   TypeCode.Int16   => @"short",
					   TypeCode.Int32   => @"int",
					   TypeCode.Int64   => @"long",
					   TypeCode.SByte   => @"sbyte",
					   TypeCode.Single  => @"float",
					   TypeCode.String  => @"string",
					   TypeCode.UInt16  => @"ushort",
					   TypeCode.UInt32  => @"uint",
					   TypeCode.UInt64  => @"ulong",
					   _                => null
				   };

		if (name is not null)
		{
			return name;
		}

		if (type == typeof(IntPtr))
		{
			return @"nint";
		}

		if (type == typeof(UIntPtr))
		{
			return @"nuint";
		}

		if (type == typeof(object))
		{
			return @"object";
		}

		if (type == typeof(void))
		{
			return @"void";
		}

		return null;
	}

	private static string? TryGetSimpleName(Type type)
	{
		if (TryGetTypeAlias(type) is { } typeAlias)
		{
			return typeAlias;
		}

		if (!type.IsNested && !type.IsGenericType)
		{
			return type.Name;
		}

		return null;
	}

	private static Type GetDeclaringType(Type type)
	{
		if (!type.IsGenericType || type.DeclaringType is not { IsGenericType: true } declaringType)
		{
			return type.DeclaringType!;
		}

		var parentArguments = new Type[declaringType.GetGenericArguments().Length];
		Array.Copy(type.GetGenericArguments(), parentArguments, parentArguments.Length);

		return declaringType.MakeGenericType(parentArguments);
	}

	private static bool ContainsGenericArgs(Type type)
	{
		if (!type.IsGenericType)
		{
			return false;
		}

		if (!type.IsNested)
		{
			return true;
		}

		if (type.DeclaringType is { IsGenericType: true } declaringType)
		{
			return type.GetGenericArguments().Length > declaringType.GetGenericArguments().Length;
		}

		return true;
	}

	private static IEnumerable<Type> GetGenericArgs(Type type)
	{
		if (!type.IsNested)
		{
			return type.GetGenericArguments();
		}

		if (type.DeclaringType is { IsGenericType: true })
		{
			return EnumerateOwnArgs(type);
		}

		return type.GetGenericArguments();

		static IEnumerable<Type> EnumerateOwnArgs(Type type)
		{
			var args = type.GetGenericArguments();

			for (var i = type.DeclaringType!.GetGenericArguments().Length; i < args.Length; i ++)
			{
				yield return args[i];
			}
		}
	}

	/// <param name="type">The generic type definition.</param>
	extension(Type type)
	{
		/// <summary>
		///     Gets the friendly name of the specified type.
		/// </summary>
		/// <returns>The friendly name of the type.</returns>
		public string FriendlyName => TryGetSimpleName(type) ?? new StringBuilder().AppendNotSimpleName(type).ToString();

		/// <summary>
		///     Determines whether the specified type is a tuple.
		/// </summary>
		/// <returns>True if the type is a tuple; otherwise, false.</returns>
		private bool IsTuple
		{
			get
			{
				if (!type.IsGenericType)
				{
					return false;
				}

				var typeDef = type.GetGenericTypeDefinition();

				return typeDef == typeof(ValueTuple<>) ||
					   typeDef == typeof(ValueTuple<,>) ||
					   typeDef == typeof(ValueTuple<,,>) ||
					   typeDef == typeof(ValueTuple<,,>) ||
					   typeDef == typeof(ValueTuple<,,,>) ||
					   typeDef == typeof(ValueTuple<,,,,>) ||
					   typeDef == typeof(ValueTuple<,,,,,>) ||
					   typeDef == typeof(ValueTuple<,,,,,,>) ||
					   typeDef == typeof(ValueTuple<,,,,,,,>) ||
					   typeDef == typeof(ValueTuple<,,,,,,,>);
			}
		}

		/// <summary>
		///     Creates a generic type using the specified type arguments.
		/// </summary>
		/// <param name="arg1">The first type argument.</param>
		/// <param name="arg2">The second type argument.</param>
		/// <returns>The constructed generic type.</returns>
		public Type MakeGenericTypeExt(Type arg1, Type arg2) => type.MakeGenericType(arg1, arg2);

		/// <summary>
		///     Creates a generic type using the specified type arguments.
		/// </summary>
		/// <param name="arg1">The first type argument.</param>
		/// <param name="arg2">The second type argument.</param>
		/// <param name="arg3">The third type argument.</param>
		/// <returns>The constructed generic type.</returns>
		public Type MakeGenericTypeExt(Type arg1,
									   Type arg2,
									   Type arg3) =>
			type.MakeGenericType(arg1, arg2, arg3);

		/// <summary>
		///     Creates a generic type using the specified type arguments.
		/// </summary>
		/// <param name="arg1">An array of type arguments.</param>
		/// <param name="arg2">The additional type argument.</param>
		/// <returns>The constructed generic type.</returns>
		public Type MakeGenericTypeExt(Type[] arg1, Type arg2)
		{
			var args = new Type[arg1.Length + 1];
			Array.Copy(arg1, args, arg1.Length);
			args[arg1.Length] = arg2;

			return type.MakeGenericType(args);
		}

		/// <summary>
		///     Creates an instance of the specified type.
		/// </summary>
		/// <typeparam name="T">The type of the instance to create.</typeparam>
		/// <returns>An instance of the specified type.</returns>
		public T CreateInstance<T>() => (T) Activator.CreateInstance(type)!;

		public IEnumerable<Type> DecomposeType()
		{
			if (!type.IsTuple)
			{
				yield return type;

				yield break;
			}

			var genericArguments = type.GetGenericArguments();

			for (var i = 0; i < genericArguments.Length; i ++)
			{
				if (i == 7)
				{
					foreach (var itemType in DecomposeType(genericArguments[7]))
					{
						yield return itemType;
					}

					yield break;
				}

				yield return genericArguments[i];
			}
		}
	}

	/// <param name="sb">The StringBuilder to append to.</param>
	extension(StringBuilder sb)
	{
		private StringBuilder AppendQualifiedName(Type type)
		{
			if (type.IsNested && !type.IsGenericParameter)
			{
				AppendFriendlyName(sb, GetDeclaringType(type)).Append('.');
			}

			var name = type.Name;

			if (ContainsGenericArgs(type))
			{
				sb.Append(name, startIndex: 0, name.IndexOf('`'));
			}
			else
			{
				sb.Append(name);
			}

			return sb;
		}

		/// <summary>
		///     Appends the friendly name of the specified type to the StringBuilder.
		/// </summary>
		/// <param name="type">The type to get the friendly name of.</param>
		public StringBuilder AppendFriendlyName(Type type) => TryGetSimpleName(type) is { } name ? sb.Append(name) : sb.AppendNotSimpleName(type);

		private StringBuilder AppendNotSimpleName(Type type)
		{
			if (type.IsTuple)
			{
				return sb.Append('(').AppendTupleArgs(type).Append(')');
			}

			return ContainsGenericArgs(type) ? AppendGenericType(sb, type) : AppendQualifiedName(sb, type);
		}

		/// <summary>
		///     Appends the generic type name to the StringBuilder.
		/// </summary>
		/// <param name="type">The generic type to append.</param>
		/// <returns>The StringBuilder with the appended generic type name.</returns>
		private StringBuilder AppendGenericType(Type type)
		{
			AppendQualifiedName(sb, type);

			var first = true;

			foreach (var t in GetGenericArgs(type))
			{
				AppendFriendlyName(sb.Append(first ? @"<" : @", "), t);
				first = false;
			}

			return sb.Append('>');
		}

		/// <summary>
		///     Appends the tuple arguments to the StringBuilder.
		/// </summary>
		/// <param name="type">The tuple type to append.</param>
		private StringBuilder AppendTupleArgs(Type type)
		{
			string? delimiter = null;

			foreach (var typeItem in DecomposeType(type))
			{
				if (delimiter is not null)
				{
					sb.Append(delimiter);
				}
				else
				{
					delimiter = @", ";
				}

				sb.AppendFriendlyName(typeItem);
			}

			return sb;
		}
	}
}