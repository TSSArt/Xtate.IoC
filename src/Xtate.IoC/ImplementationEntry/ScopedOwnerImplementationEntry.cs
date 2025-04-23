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
///     Represents an entry for a scoped implementation in the IoC container. Instance owned by IoC.
/// </summary>
public class ScopedOwnerImplementationEntry : ScopedImplementationEntry
{
	private readonly ObjectsBin _objectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	public ScopedOwnerImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) => _objectsBin = serviceProvider.ObjectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="sourceEntry">The source implementation entry.</param>
	protected ScopedOwnerImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) => _objectsBin = serviceProvider.ObjectsBin;

	/// <summary>
	///     Creates a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>A new instance of <see cref="ScopedImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new ScopedOwnerImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new instance of the <see cref="ScopedImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	/// <returns>A new instance of <see cref="ScopedImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new ScopedOwnerImplementationEntry(serviceProvider, factory);

	protected override async ValueTask<T?> ExecuteFactoryBase<T, TArg>(TArg argument) where T : default
	{
		var instance = await base.ExecuteFactoryBase<T, TArg>(argument).ConfigureAwait(false);

		await _objectsBin.AddAsync(instance).ConfigureAwait(false);

		return instance;
	}
}