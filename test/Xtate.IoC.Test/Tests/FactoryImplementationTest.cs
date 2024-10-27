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

namespace Xtate.IoC.Test;

[TestClass]
public class FactoryImplementationTest
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
		var instanceScope = InstanceScope.Transient;
		var synchronous = true;

		// Act
		_ = new FactoryImplementation<object>(_serviceCollection, instanceScope, synchronous);

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ImplementationKey<object, ValueTuple>()));
	}

	[TestMethod]
	public void For_ShouldRegisterService()
	{
		// Arrange
		var factoryImplementation = new FactoryImplementation<object>(_serviceCollection, InstanceScope.Transient, synchronous: true);

		// Act
		factoryImplementation.For<IService>();

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ServiceKey<IService, ValueTuple>()));
	}

	[TestMethod]
	public void For_WithArg_ShouldRegisterService()
	{
		// Arrange
		var factoryImplementation = new FactoryImplementation<object>(_serviceCollection, InstanceScope.Transient, synchronous: true);

		// Act
		factoryImplementation.For<IService, int>();

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ServiceKey<IService, int>()));
	}

	[TestMethod]
	public void For_WithTwoArgs_ShouldRegisterService()
	{
		// Arrange
		var factoryImplementation = new FactoryImplementation<object>(_serviceCollection, InstanceScope.Transient, synchronous: true);

		// Act
		factoryImplementation.For<IService, int, string>();

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ServiceKey<IService, (int, string)>()));
	}

	[TestMethod]
	public void For_WithSharedWithin_ShouldRegisterService()
	{
		// Arrange
		var factoryImplementation = new FactoryImplementation<object>(_serviceCollection, InstanceScope.Transient, synchronous: true);

		// Act
		factoryImplementation.For<IService>(SharedWithin.Scope);

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ServiceKey<IService, ValueTuple>()));
	}

	[TestMethod]
	public void For_WithArgAndSharedWithin_ShouldRegisterService()
	{
		// Arrange
		var factoryImplementation = new FactoryImplementation<object>(_serviceCollection, InstanceScope.Transient, synchronous: true);

		// Act
		factoryImplementation.For<IService, int>(SharedWithin.Scope);

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ServiceKey<IService, int>()));
	}

	[TestMethod]
	public void For_WithTwoArgsAndSharedWithin_ShouldRegisterService()
	{
		// Arrange
		var factoryImplementation = new FactoryImplementation<object>(_serviceCollection, InstanceScope.Transient, synchronous: true);

		// Act
		factoryImplementation.For<IService, int, string>(SharedWithin.Scope);

		// Assert
		Assert.IsTrue(_serviceCollection.IsRegistered(TypeKey.ServiceKey<IService, (int, string)>()));
	}

	private interface IService;
}