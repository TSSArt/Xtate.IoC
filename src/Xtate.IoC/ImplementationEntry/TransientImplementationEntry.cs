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

namespace Xtate.IoC;

/// <summary>
///     Represents an entry for a transient implementation in the IoC container.
///     Each time services are needed, the delegate is called. Instance owned by IoC.
/// </summary>
internal sealed class TransientImplementationEntry : ImplementationEntry
{
	private readonly ServiceProvider _serviceProvider;

	/// <summary>
	///     Initializes a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	public TransientImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(factory) => _serviceProvider = serviceProvider;

	/// <summary>
	///     Initializes a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="sourceEntry">The source entry.</param>
	private TransientImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(sourceEntry) => _serviceProvider = serviceProvider;

	/// <summary>
	///     Gets the service provider.
	/// </summary>
	protected override IServiceProvider ServiceProvider => _serviceProvider;

	/// <summary>
	///     Creates a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>A new instance of <see cref="TransientImplementationEntry" />.</returns>
	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new TransientImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new instance of the <see cref="TransientImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	/// <returns>A new instance of <see cref="TransientImplementationEntry" />.</returns>
	internal override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new TransientImplementationEntry(serviceProvider, factory);

	/// <summary>
	///     Executes the factory asynchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the instance.</returns>
	private protected override async ValueTask<T?> ExecuteFactory<T, TArg>(TArg argument) where T : default
	{
		var instance = await base.ExecuteFactory<T, TArg>(argument).ConfigureAwait(false);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);
		}
		catch
		{
			await Disposer.DisposeAsync(instance).ConfigureAwait(false);

			throw;
		}

		return instance;
	}

	/// <summary>
	///     Executes the factory synchronously.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <typeparam name="TArg">The type of the argument.</typeparam>
	/// <param name="argument">The argument.</param>
	/// <returns>The instance.</returns>
	private protected override T? ExecuteFactorySync<T, TArg>(TArg argument) where T : default
	{
		var instance = base.ExecuteFactorySync<T, TArg>(argument);

		try
		{
			_serviceProvider.RegisterInstanceForDispose(instance);
		}
		catch
		{
			Disposer.Dispose(instance);

			throw;
		}

		return instance;
	}
}