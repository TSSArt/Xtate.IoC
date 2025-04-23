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
using ValueTuple = System.ValueTuple;

namespace Xtate.IoC;

internal readonly struct ArgumentType : IFormattable
{
	private readonly Type? _type;

	private ArgumentType(Type type) => _type = type != typeof(ValueTuple) ? type : null;

	public bool IsEmpty => _type is null;

#region Interface IFormattable

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		if (_type is null)
		{
			return string.Empty;
		}

		if (format is null || format.Length == 0 || format.Split('|') is not [var dlm, var arg] || !TryGetArgFormat(arg, out var argFormat, out var index))
		{
			return _type.FriendlyName();
		}

		var sb = new StringBuilder();

		string? delimiter = null;

		foreach (var type in _type.DecomposeType())
		{
			if (delimiter is not null)
			{
				sb.Append(delimiter);
			}
			else
			{
				delimiter = dlm;
			}

			sb.AppendFriendlyName(type).Append(' ').AppendFormat(formatProvider, argFormat, index ++);
		}

		return sb.ToString();
	}

#endregion

	private static bool TryGetArgFormat(string arg, [MaybeNullWhen(false)] out string argFormat, out int index)
	{
		index = 0;
		argFormat = null;

		if (string.IsNullOrEmpty(arg) || arg[0] is '0' or '1')
		{
			return false;
		}

		var digits = 0;
		var badChars = 0;

		foreach (var ch in arg)
		{
			if (ch is '0' or '1')
			{
				digits ++;
			}
			else if (!char.IsLetter(ch) && ch != '_')
			{
				badChars ++;
			}
		}

		if (badChars > 0 || digits > 1)
		{
			return false;
		}

		if (digits == 0)
		{
			argFormat = arg;

			return true;
		}

		var sb = new StringBuilder(arg.Length + 2);

		foreach (var ch in arg)
		{
			if (ch is '0' or '1')
			{
				index = ch - '0';
				sb.Append(@"{0}");
			}
			else
			{
				sb.Append(ch);
			}
		}

		argFormat = sb.ToString();

		return true;
	}

	public static ArgumentType TypeOf<T>() => new(typeof(T));

	public override string ToString() => ToString(format: null, formatProvider: null);
}