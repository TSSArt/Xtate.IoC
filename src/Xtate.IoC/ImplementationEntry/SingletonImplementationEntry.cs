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

using System.Diagnostics;
using ValueTuple = System.ValueTuple;

namespace Xtate.IoC;

/// <summary>
///     Represents an entry for a singleton implementation in the IoC container. Instance owned by IoC.
/// </summary>
public class SingletonImplementationEntry : ImplementationEntry
{
    private readonly SingletonContainer _singletonContainer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SingletonImplementationEntry" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="factory">The factory delegate.</param>
    public SingletonImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) => _singletonContainer = new SingletonContainer();

    /// <summary>
    ///     Initializes a new instance of the <see cref="SingletonImplementationEntry" /> class
    ///     by copying the source entry.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="sourceEntry">The source entry to copy.</param>
    protected SingletonImplementationEntry(ServiceProvider serviceProvider, SingletonImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) =>
        _singletonContainer = sourceEntry._singletonContainer;

    /// <summary>
    ///     Creates a new instance of the <see cref="SingletonImplementationEntry" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A new instance of <see cref="SingletonImplementationEntry" />.</returns>
    public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new SingletonImplementationEntry(serviceProvider, this);

    /// <summary>
    ///     Creates a new instance of the <see cref="SingletonImplementationEntry" /> class with a factory delegate.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="factory">The factory delegate.</param>
    /// <returns>A new instance of <see cref="SingletonImplementationEntry" />.</returns>
    public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new SingletonImplementationEntry(serviceProvider, factory);

    /// <summary>
    ///     Executes the base factory delegate and returns the created instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="argument">The argument to pass to the factory delegate.</param>
    /// <returns>A task representing the asynchronous operation, with the created instance as the result.</returns>
    protected virtual ValueTask<T?> ExecuteFactoryBase<T, TArg>(TArg argument) => base.ExecuteFactory<T, TArg>(argument);

    /// <summary>
    ///     Executes the factory delegate and returns the created instance, ensuring it is a singleton.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="argument">The argument to pass to the factory delegate.</param>
    /// <returns>A task representing the asynchronous operation, with the created instance as the result.</returns>
    protected override ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
    {
        lock (_singletonContainer)
        {
            if (_singletonContainer.Instance is not Task<T?> task)
            {
                if (ArgumentType.TypeOf<TArg>().IsEmpty)
                {
                    _singletonContainer.Instance = task = ExecuteFactoryBase<T, TArg>(argument).AsTask();
                }
                else
                {
                    task = ExecuteFactoryWithArg<T, TArg>(argument);
                }
            }

            return new ValueTask<T?>(task);
        }
    }

    /// <summary>
    ///     Executes the factory delegate synchronously and returns the created instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="argument">The argument to pass to the factory delegate.</param>
    /// <returns>The created instance.</returns>
    protected override T? ExecuteFactorySync<T, TArg>(TArg argument) where T : default
    {
        EnsureSynchronousContext<T, TArg>();

        var valueTask = ExecuteFactory<T, TArg>(argument);

        Debug.Assert(valueTask.IsCompleted);

        return valueTask.Result;
    }

    /// <summary>
    ///     Executes the factory delegate with an argument and returns the created instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <param name="argument">The argument to pass to the factory delegate.</param>
    /// <returns>A task representing the asynchronous operation, with the created instance as the result.</returns>
    private Task<T?> ExecuteFactoryWithArg<T, TArg>(TArg argument)
    {
        // ValueTuple<TArg> used instead of TArg as TKey type in Dictionary to support NULLs as key value
        if (_singletonContainer.Instance is not Dictionary<ValueTuple<TArg>, Task<T?>> dictionary)
        {
            _singletonContainer.Instance = dictionary = [];
        }

        if (!dictionary.TryGetValue(ValueTuple.Create(argument), out var task))
        {
            task = ExecuteFactoryBase<T, TArg>(argument).AsTask();

            dictionary.Add(ValueTuple.Create(argument), task);
        }

        return task;
    }

    /// <summary>
    ///     Container for holding the singleton instance.
    /// </summary>
    private sealed class SingletonContainer
    {
        public object? Instance;
    }
}