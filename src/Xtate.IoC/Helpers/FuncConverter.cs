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

using ValueTuple = System.ValueTuple;

namespace Xtate.IoC;

internal static class FuncConverter
{
    /// <summary>
    ///     A set of Func types with varying numbers of generic parameters.
    /// </summary>
    private static readonly Type[] FuncSet =
    [
        typeof(Func<>),
        typeof(Func<,>),
        typeof(Func<,,>),
        typeof(Func<,,,>),
        typeof(Func<,,,,>),
        typeof(Func<,,,,,>),
        typeof(Func<,,,,,,>),
        typeof(Func<,,,,,,,>),
        typeof(Func<,,,,,,,,>),
        typeof(Func<,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,,,,,,>),
        typeof(Func<,,,,,,,,,,,,,,,,>)
    ];

    /// <summary>
    ///     A map of ValueTuple types with varying numbers of generic parameters.
    /// </summary>
    private static readonly Type[] NumToValueTupleMap =
    [
        typeof(ValueTuple),
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>),
        typeof(ValueTuple<,,,,,,,>)
    ];

    /// <summary>
    ///     Casts a delegate to a specified delegate type.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate to cast to.</typeparam>
    /// <param name="func">The delegate to cast.</param>
    /// <returns>The cast delegate.</returns>
    /// <exception cref="InvalidCastException">Thrown when the delegate cannot be cast to the specified type.</exception>
    public static TDelegate Cast<TDelegate>(Delegate func) where TDelegate : Delegate => (TDelegate)Cast(func, typeof(TDelegate));

    /// <summary>
    ///     Casts a delegate to a specified type.
    /// </summary>
    /// <param name="func">The delegate to cast.</param>
    /// <param name="toType">The type to cast the delegate to.</param>
    /// <returns>The cast delegate.</returns>
    /// <exception cref="InvalidCastException">Thrown when the delegate cannot be cast to the specified type.</exception>
    private static Delegate Cast(Delegate func, Type toType)
    {
        if (func.GetType() == toType)
        {
            return func;
        }

        if (!toType.IsGenericType || Array.IndexOf(FuncSet, toType.GetGenericTypeDefinition()) < 0)
        {
            throw new InvalidCastException(Res.Format(Resources.Exception_CantCastForwardDelegate, func.GetType(), toType));
        }

        var toArgs = toType.GetGenericArguments();
        var args = toArgs.Length > 1 ? new ParameterExpression[toArgs.Length - 1] : [];

        for (var i = 0; i < args.Length; i ++)
        {
            args[i] = Expression.Parameter(toArgs[i]);
        }

        var arg = args.Length switch
                  {
                      0 => Expression.Default(typeof(Empty)),
                      1 => (Expression)args[0],
                      _ => CreateSingleArgument(args, start: 0)
                  };

        return Expression.Lambda(Expression.Invoke(Expression.Constant(func), arg), args).Compile();
    }

    /// <summary>
    ///     Creates a single argument expression from an array of parameter expressions.
    /// </summary>
    /// <param name="args">The array of parameter expressions.</param>
    /// <param name="start">The starting index in the array.</param>
    /// <returns>A NewExpression representing the single argument.</returns>
    private static NewExpression CreateSingleArgument(ParameterExpression[] args, int start)
    {
        Expression[] valueTupleArgs;
        var length = args.Length - start;

        if (length > 7)
        {
            valueTupleArgs = new Expression[8];
            Array.Copy(args, start, valueTupleArgs, destinationIndex: 0, length: 7);
            valueTupleArgs[7] = CreateSingleArgument(args, start + 7);
        }
        else
        {
            valueTupleArgs = new Expression[length];
            Array.Copy(args, start, valueTupleArgs, destinationIndex: 0, length);
        }

        var types = Array.ConvertAll(valueTupleArgs, static e => e.Type);
        var valueTupleType = NumToValueTupleMap[types.Length].MakeGenericType(types);

        return Expression.New(valueTupleType.GetConstructor(types)!, valueTupleArgs);
    }
}