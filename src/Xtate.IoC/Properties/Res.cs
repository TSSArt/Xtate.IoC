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

namespace Xtate.IoC;

/// <summary>
///     Provides methods for formatting strings using the specified culture.
/// </summary>
internal static class Res
{
	/// <summary>
	///     Formats a string using the specified format and a single argument.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <param name="arg">The argument to format.</param>
	/// <returns>A formatted string.</returns>
	public static string Format(string format, object? arg)
	{
		FriendlyTypeArg(ref arg);

		return string.Format(Resources.Culture, format, arg);
	}

	/// <summary>
	///     Formats a string using the specified format and two arguments.
	/// </summary>
	/// <param name="format">The format string.</param>
	/// <param name="arg0">The first argument to format.</param>
	/// <param name="arg1">The second argument to format.</param>
	/// <returns>A formatted string.</returns>
	public static string Format(string format, object? arg0, object? arg1)
	{
		FriendlyTypeArg(ref arg0);
		FriendlyTypeArg(ref arg1);

		return string.Format(Resources.Culture, format, arg0, arg1);
	}

	private static void FriendlyTypeArg(ref object? arg)
	{
		if (arg is Type type)
		{
			arg = new FriendlyType(type);
		}
	}

	private class FriendlyType(Type type)
	{
		public override string ToString() => type.FriendlyName();
	}
}