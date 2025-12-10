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
///     Represents an entry for a transient implementation in the IoC container.
///     Each time services are needed, the delegate is called. Instance owned by IoC.
/// </summary>
public class TransientImplementationEntry : ImplementationEntry
{
	/// <summary>
	///     Bin that tracks created transient instances for disposal when the owning <see cref="ServiceProvider" /> is
	///     disposed.
	/// </summary>
	/// <remarks>
	///     Non‑disposable instances are ignored by the bin. Disposable instances are registered so their lifetime is tied to
	///     the service provider even though they are transient.
	/// </remarks>
	private readonly ObjectsBin _objectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	public TransientImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) => _objectsBin = serviceProvider.ObjectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="sourceEntry">The source entry.</param>
	protected TransientImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) => _objectsBin = serviceProvider.ObjectsBin;

	/// <summary>
	///     Creates a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>A new instance of <see cref="TransientImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new TransientImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	/// <returns>A new instance of <see cref="TransientImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new TransientImplementationEntry(serviceProvider, factory);

	/// <summary>
	///     Executes the factory asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the instance.</returns>
	protected override ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
	{
		var valueTask = base.ExecuteFactory<T, TArg>(argument);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return ExecuteFactoryWait(valueTask);
		}

		if (valueTask.Result is not { } instance)
		{
			return new ValueTask<T?>(default(T?));
		}

		var addValueTask = _objectsBin.AddAsync(instance);

		return addValueTask.IsCompletedSuccessfully ? new ValueTask<T?>(instance) : Wait(addValueTask, instance);

		static async ValueTask<T?> Wait(ValueTask valueTask, T value)
		{
			await valueTask.ConfigureAwait(false);

			return value;
		}
	}

	/// <summary>
	///     Awaits the asynchronous factory result, registers the produced instance in the disposal bin and returns it.
	/// </summary>
	/// <typeparam name="T">The instance type.</typeparam>
	/// <param name="valueTask">The factory task to await.</param>
	/// <returns>The created instance (maybe null).</returns>
	/// <remarks>
	///     Used when the underlying factory <see cref="ValueTask{TResult}" /> did not complete synchronously, centralizing bin
	///     registration logic for the asynchronous path.
	/// </remarks>
	private async ValueTask<T?> ExecuteFactoryWait<T>(ValueTask<T?> valueTask)
	{
		var instance = await valueTask.ConfigureAwait(false);

		await _objectsBin.AddAsync(instance).ConfigureAwait(false);

		return instance;
	}

	/// <summary>
	///     Executes the factory synchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>The instance.</returns>
	protected override T? ExecuteFactorySync<T, TArg>(TArg argument) where T : default
	{
		var instance = base.ExecuteFactorySync<T, TArg>(argument);

		_objectsBin.AddSync(instance);

		return instance;
	}
}