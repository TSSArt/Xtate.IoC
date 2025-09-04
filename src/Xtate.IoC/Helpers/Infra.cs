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

using System.Runtime.ExceptionServices;

namespace Xtate.IoC;

internal static class Infra
{
    /// <summary>
    ///     Handles TypeInitializationException exceptions.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    /// <param name="getter">Method that reads a static field.</param>
    /// <returns>The result of the getter method.</returns>
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
    ///     an <see cref="InvalidOperationException" /> exception.
    /// </summary>
    /// <param name="condition">
    ///     The conditional expression to evaluate. If the condition is <see langword="true" />, execution
    ///     returns to the caller.
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
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
    ///     Checks a value for null; if the value is <see langword="null" />, throws
    ///     an <see cref="InvalidOperationException" /> exception.
    /// </summary>
    /// <param name="value">
    ///     The value to check for null. If the value is not <see langword="null" />, execution returns to
    ///     the caller.
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
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
    private static void ThrowAssertion() => throw new InvalidOperationException(Resources.Exception_AssertionFailed);

    public static InvalidOperationException Unmatched<T>(T value) => new(Res.Format(Resources.Exception_AssertUnmatched, typeof(T).FullName, value));
}