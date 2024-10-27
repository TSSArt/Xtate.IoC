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

using System.Diagnostics;
using ValueTuple = System.ValueTuple;

namespace Xtate.IoC;

/// <summary>
///     Represents an entry for a scoped implementation in the IoC container. Instance owned by IoC.
/// </summary>
internal sealed class ScopedImplementationEntry : ImplementationEntry
{
	private readonly ServiceProvider _serviceProvider;
	private readonly object          _syncRoot = new();
	private          object?         _instance;

	/// <summary>
	///     Initializes a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	public ScopedImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(factory) => _serviceProvider = serviceProvider;

	/// <summary>
	///     Initializes a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="sourceEntry">The source implementation entry.</param>
	private ScopedImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(sourceEntry) => _serviceProvider = serviceProvider;

	/// <summary>
	///     Gets the service provider.
	/// </summary>
	protected override IServiceProvider ServiceProvider => _serviceProvider;

	/// <summary>
	///     Creates a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>A new instance of <see cref="ScopedImplementationEntry" />.</returns>
	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new ScopedImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	/// <returns>A new instance of <see cref="ScopedImplementationEntry" />.</returns>
	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new ScopedImplementationEntry(serviceProvider, factory);

	/// <summary>
	///     Executes the factory delegate asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the instance.</returns>
	private async ValueTask<T?> ExecuteFactoryInternal<T, TArg>(TArg argument)
	{
		var instance = await base.ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);

			return instance;
		}
		catch
		{
			await Disposer.DisposeAsync(instance).ConfigureAwait(false);

			throw;
		}
	}

	/// <summary>
	///     Executes the factory delegate asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the instance.</returns>
	private protected override ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
	{
		lock (_syncRoot)
		{
			if (_instance is not Task<T?> task)
			{
				if (ArgumentType.TypeOf<TArg>().IsEmpty)
				{
					_instance = task = ExecuteFactoryInternal<T, TArg>(argument).AsTask();
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
	///     Executes the factory delegate synchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>The instance.</returns>
	private protected override T? ExecuteFactorySync<T, TArg>(TArg argument) where T : default
	{
		EnsureSynchronousContext<T, TArg>();

		var valueTask = ExecuteFactory<T, TArg>(argument);

		Debug.Assert(valueTask.IsCompleted);

		return valueTask.Result;
	}

	/// <summary>
	///     Executes the factory delegate with an argument asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the instance.</returns>
	private Task<T?> ExecuteFactoryWithArg<T, TArg>(TArg argument)
	{
		if (_instance is not Dictionary<ValueTuple<TArg>, Task<T?>> dictionary)
		{
			// ValueTuple<TArg> used instead of TArg as TKey type in Dictionary to support NULLs as key value
			_instance = dictionary = [];
		}

		if (!dictionary.TryGetValue(ValueTuple.Create(argument), out var task))
		{
			task = ExecuteFactoryInternal<T, TArg>(argument).AsTask();

			dictionary.Add(ValueTuple.Create(argument), task);
		}

		return task;
	}
}