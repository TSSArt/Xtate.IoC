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

using System.Runtime.ExceptionServices;

namespace Xtate.IoC;

internal static class Infra
{
	/// <summary>
	///     Handles TypeInitializationException exceptions
	/// </summary>
	/// <typeparam name="T">Return Type</typeparam>
	/// <param name="getter">Method reads static field</param>
	/// <returns></returns>
	[ExcludeFromCodeCoverage]
	public static T TypeInitHandle<T>(Func<T> getter)
	{
		try
		{
			return getter();
		}
		catch (TypeInitializationException ex) when (ex.InnerException is { } innerException)
		{
			ExceptionDispatchInfo.Capture(innerException).Throw();

			throw;
		}
	}

	/// <summary>
	///     Checks for a condition; if the condition is <see langword="false" />, throws
	///     <see cref="InfrastructureException" /> exception.
	/// </summary>
	/// <param name="condition">
	///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
	///     returned to caller.
	/// </param>
	/// <exception cref="InfrastructureException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Assert([AssertionCondition(AssertionConditionType.IS_TRUE)] [DoesNotReturnIf(false)] bool condition)
	{
		if (!condition)
		{
			ThrowAssertion();
		}
	}

	/// <summary>
	///     Checks value for a null; if the value is <see langword="null" />, throws
	///     <see cref="InfrastructureException" /> exception.
	/// </summary>
	/// <param name="value">
	///     The value to check for null. If the value is not <see langword="null" />, execution returned to
	///     caller.
	/// </param>
	/// <exception cref="InfrastructureException"></exception>
	[AssertionMethod]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] [NotNull] object? value)
	{
		if (value is null)
		{
			ThrowAssertion();
		}
	}

	[DoesNotReturn]
	private static void ThrowAssertion() => throw new InfrastructureException(Resources.Exception_AssertionFailed);

	private static InfrastructureException GetUnexpectedException<T>(T value, string message)
	{
		if (value is null)
		{
			return new InfrastructureException(Res.Format(Resources.Exception_AssertUnexpected, message, arg1: @"null"));
		}

		var type = value.GetType();
		if (type.IsPrimitive || type.IsEnum)
		{
			return new InfrastructureException(Res.Format(Resources.Exception_AssertUnexpectedWithType, message, type, value));
		}

		if (value is Delegate)
		{
			return new InfrastructureException(Res.Format(Resources.Exception_AssertUnexpectedWithType, message, arg1: @"Delegate", value));
		}

		return new InfrastructureException(Res.Format(Resources.Exception_AssertUnexpected, message, type));
	}

	public static InfrastructureException UnexpectedValueException<T>(T value) => GetUnexpectedException(value, Resources.Exception_UnexpectedValue);
}