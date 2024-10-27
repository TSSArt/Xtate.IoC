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

using System.Linq;

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceImplementationTest
{
	private ServiceCollection _serviceCollection = default!;

	[TestInitialize]
	public void Setup()
	{
		_serviceCollection = [];
	}

	[TestMethod]
	public void Constructor_ShouldAddServiceEntry()
	{
		// Arrange
		const InstanceScope instanceScope = InstanceScope.Singleton;
		const bool synchronous = true;

		// Act
		_ = new ServiceImplementation<object, object>(_serviceCollection, instanceScope, synchronous);

		// Assert
		var key = TypeKey.ImplementationKey<object, object>();
		Assert.IsTrue(_serviceCollection.IsRegistered(key));
	}

	[TestMethod]
	public void For_ShouldAddServiceEntry_WhenOptionIsDefault()
	{
		// Arrange
		const InstanceScope instanceScope = InstanceScope.Singleton;
		const bool synchronous = true;
		var serviceImplementation = new ServiceImplementation<object, object>(_serviceCollection, instanceScope, synchronous);

		// Act
		serviceImplementation.For<object>();

		// Assert
		var key = TypeKey.ServiceKey<object, object>();
		Assert.IsTrue(_serviceCollection.IsRegistered(key));
	}

	[TestMethod]
	public void For_ShouldNotAddServiceEntry_WhenOptionIfNotRegisteredAndServiceIsRegistered()
	{
		// Arrange
		var instanceScope = InstanceScope.Singleton;
		var synchronous = true;
		var serviceImplementation = new ServiceImplementation<object, object>(_serviceCollection, instanceScope, synchronous);
		var key = TypeKey.ServiceKey<object, object>();
		_serviceCollection.Add(new ServiceEntry(key, instanceScope, null!));

		// Act
		serviceImplementation.For<object>(Option.IfNotRegistered);

		// Assert
		Assert.AreEqual(expected: 1, _serviceCollection.Count(entry => entry.Key.Equals(key)));
	}

	[TestMethod]
	public void For_ShouldAddServiceEntry_WhenOptionIfNotRegisteredAndServiceIsNotRegistered()
	{
		// Arrange
		var instanceScope = InstanceScope.Singleton;
		var synchronous = true;
		var serviceImplementation = new ServiceImplementation<object, object>(_serviceCollection, instanceScope, synchronous);

		// Act
		serviceImplementation.For<object>(Option.IfNotRegistered);

		// Assert
		var key = TypeKey.ServiceKey<object, object>();
		Assert.IsTrue(_serviceCollection.IsRegistered(key));
	}
}