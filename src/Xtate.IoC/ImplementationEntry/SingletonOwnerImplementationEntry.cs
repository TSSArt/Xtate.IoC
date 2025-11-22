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
///     Represents an entry for a singleton implementation in the IoC container. Instance owned by IoC.
/// </summary>
public class SingletonOwnerImplementationEntry : SingletonImplementationEntry
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
	///     Initializes a new instance of the <see cref="SingletonOwnerImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	public SingletonOwnerImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) => _objectsBin = serviceProvider.SharedObjectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="SingletonOwnerImplementationEntry" /> class
	///     by copying the source entry.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="sourceEntry">The source entry to copy.</param>
	protected SingletonOwnerImplementationEntry(ServiceProvider serviceProvider, SingletonOwnerImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) =>
		_objectsBin = serviceProvider.SharedObjectsBin;

	/// <summary>
	///     Creates a new instance of the <see cref="SingletonOwnerImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>A new instance of <see cref="SingletonOwnerImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new SingletonOwnerImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new instance of the <see cref="SingletonOwnerImplementationEntry" /> class with a factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	/// <returns>A new instance of <see cref="SingletonOwnerImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new SingletonOwnerImplementationEntry(serviceProvider, factory);

	protected override ValueTask<T?> ExecuteFactoryBase<T, TArg>(TArg argument) where T : default
	{
		var valueTask = base.ExecuteFactoryBase<T, TArg>(argument);

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

	private async ValueTask<T?> ExecuteFactoryWait<T>(ValueTask<T?> valueTask)
	{
		var instance = await valueTask.ConfigureAwait(false);

		await _objectsBin.AddAsync(instance).ConfigureAwait(false);

		return instance;
	}
}