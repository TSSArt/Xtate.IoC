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
///     Represents an entry for a forward implementation in the IoC container.
///     Each time services are needed, the delegate is called. Instance not owned by IoC.
/// </summary>
public class ForwardingImplementationEntry : ImplementationEntry
{
	/// <summary>
	///     Initializes a new instance of the <see cref="ForwardingImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	public ForwardingImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) { }

	/// <summary>
	///     Initializes a new instance of the <see cref="ForwardingImplementationEntry" /> class.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="sourceEntry">The source implementation entry.</param>
	protected ForwardingImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) { }

	/// <summary>
	///     Creates a new instance of the <see cref="ForwardingImplementationEntry" /> class with the specified service
	///     provider.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>A new instance of <see cref="ForwardingImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new ForwardingImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new instance of the <see cref="ForwardingImplementationEntry" /> class with the specified service
	///     provider and factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <param name="factory">The factory delegate.</param>
	/// <returns>A new instance of <see cref="ForwardingImplementationEntry" />.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new ForwardingImplementationEntry(serviceProvider, factory);
}