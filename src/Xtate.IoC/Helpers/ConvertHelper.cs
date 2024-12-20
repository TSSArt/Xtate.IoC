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

namespace Xtate.IoC;

internal static class ConvertHelper<TFrom, TTo>
{
	/// <summary>
	///     A delegate that converts an object of type <typeparamref name="TFrom" /> to an object of type
	///     <typeparamref name="TTo" />.
	/// </summary>
	public static readonly Func<TFrom, TTo> Convert = GetConverter();

	/// <summary>
	///     Creates a converter function that converts an object of type <typeparamref name="TFrom" /> to an object of type
	///     <typeparamref name="TTo" />.
	/// </summary>
	/// <returns>
	///     A function that converts an object of type <typeparamref name="TFrom" /> to an object of type
	///     <typeparamref name="TTo" />.
	/// </returns>
	private static Func<TFrom, TTo> GetConverter()
	{
		var arg = Expression.Parameter(typeof(TFrom));
		var body = Expression.Convert(arg, typeof(TTo));

		return Expression.Lambda<Func<TFrom, TTo>>(body, arg).Compile();
	}
}