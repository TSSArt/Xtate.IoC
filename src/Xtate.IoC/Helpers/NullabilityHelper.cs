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

using System.Reflection;

namespace Xtate.IoC;

/// <summary>
///     Helper class for determining the nullability of various members.
/// </summary>
internal static class NullabilityHelper
{
	private const int CanBeNull = 2;

	private const string NullableAttr = @"System.Runtime.CompilerServices.NullableAttribute";

	private const string NullableContextAttr = @"System.Runtime.CompilerServices.NullableContextAttribute";

	/// <summary>
	///     Determines if a parameter is nullable.
	/// </summary>
	/// <param name="parameter">The parameter to check.</param>
	/// <param name="path">The path to the member.</param>
	/// <returns>True if the parameter is nullable, otherwise false.</returns>
	public static bool IsNullable(ParameterInfo parameter, string path) => IsNullable(parameter.ParameterType, parameter.CustomAttributes, parameter.Member, path);

	/// <summary>
	///     Determines if a field is nullable.
	/// </summary>
	/// <param name="field">The field to check.</param>
	/// <param name="path">The path to the member.</param>
	/// <returns>True if the field is nullable, otherwise false.</returns>
	public static bool IsNullable(FieldInfo field, string path) => IsNullable(field.FieldType, field.CustomAttributes, field.DeclaringType, path);

	/// <summary>
	///     Determines if a property is nullable.
	/// </summary>
	/// <param name="property">The property to check.</param>
	/// <param name="path">The path to the member.</param>
	/// <returns>True if the property is nullable, otherwise false.</returns>
	public static bool IsNullable(PropertyInfo property, string path) => IsNullable(property.PropertyType, property.CustomAttributes, property.DeclaringType, path);

	/// <summary>
	///     Determines if a member is nullable.
	/// </summary>
	/// <param name="memberType">The type of the member.</param>
	/// <param name="attributes">The custom attributes of the member.</param>
	/// <param name="declaringType">The declaring type of the member.</param>
	/// <param name="path">The path to the member.</param>
	/// <returns>True if the member is nullable, otherwise false.</returns>
	private static bool IsNullable(Type memberType,
								   IEnumerable<CustomAttributeData> attributes,
								   MemberInfo? declaringType,
								   string path)
	{
		var index = 0;

		if (FindType(memberType, path, ref index, level: 0) is not { } type)
		{
			return false;
		}

		if (Nullable.GetUnderlyingType(type) is not null)
		{
			return true;
		}

		if (type.IsValueType)
		{
			return false;
		}

		return CheckNullableAttribute(attributes, declaringType, index);
	}

	/// <summary>
	///     Finds the type of the member based on the path.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <param name="path">The path to the member.</param>
	/// <param name="index">The index of the member.</param>
	/// <param name="level">The level of the member.</param>
	/// <returns>The type of the member if found, otherwise null.</returns>
	private static Type? FindType(Type type,
								  string path,
								  ref int index,
								  int level)
	{
		if (level == path.Length)
		{
			return type;
		}

		if (IsBytePresent(ref type))
		{
			index ++;
		}

		if (type.IsGenericType)
		{
			var pos = '0';

			foreach (var argType in type.GetGenericArguments())
			{
				if (pos ++ == path[level])
				{
					return FindType(argType, path, ref index, level + 1);
				}

				Walk(argType, ref index);
			}
		}

		return null;
	}

	/// <summary>
	///     Walks through the type and updates the index.
	/// </summary>
	/// <param name="type">The type to walk through.</param>
	/// <param name="index">The index to update.</param>
	private static void Walk(Type type, ref int index)
	{
		if (IsBytePresent(ref type))
		{
			index ++;
		}

		if (type.IsGenericType)
		{
			foreach (var argType in type.GetGenericArguments())
			{
				Walk(argType, ref index);
			}
		}
	}

	/// <summary>
	///     Checks if extra byte is present in the type.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <returns>True if a byte is present, otherwise false.</returns>
	private static bool IsBytePresent(ref Type type)
	{
		if (!type.IsValueType)
		{
			return true;
		}

		if (!type.IsGenericType)
		{
			return false;
		}

		if (Nullable.GetUnderlyingType(type) is not { } underlyingType)
		{
			return true;
		}

		type = underlyingType;

		return underlyingType.IsGenericType;
	}

	/// <summary>
	///     Checks the nullable attribute of a member.
	/// </summary>
	/// <param name="attributes">The custom attributes of the member.</param>
	/// <param name="declaringType">The declaring type of the member.</param>
	/// <param name="index">The index of the member.</param>
	/// <returns>True if the member is nullable, otherwise false.</returns>
	private static bool CheckNullableAttribute(IEnumerable<CustomAttributeData> attributes, MemberInfo? declaringType, int index)
	{
		if (attributes.FirstOrDefault(data => data.AttributeType.FullName == NullableAttr) is { } nData)
		{
			var argument = nData.ConstructorArguments[0];

			if (argument.ArgumentType == typeof(byte[]))
			{
				var bytes = (IReadOnlyList<CustomAttributeTypedArgument>) argument.Value!;

				return (byte) bytes[index].Value! == CanBeNull;
			}

			return (byte) argument.Value! == CanBeNull;
		}

		for (; declaringType != null; declaringType = declaringType.DeclaringType)
		{
			if (declaringType.CustomAttributes.FirstOrDefault(data => data.AttributeType.FullName == NullableContextAttr) is { } ncData)
			{
				return (byte) ncData.ConstructorArguments[0].Value! == CanBeNull;
			}
		}

		return false;
	}
}