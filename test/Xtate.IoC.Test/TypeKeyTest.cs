#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Empty = System.ValueTuple;

namespace Xtate.IoC.Test;

[TestClass]
public class TypeKeyTest
{
	[TestMethod]
	public void SimpleTypeKeyTest()
	{
		// Assert

		Assert.AreSame(TypeKey.ServiceKey<int, Empty>(), TypeKey.ServiceKey<int, Empty>());
		Assert.AreSame(TypeKey.ServiceKey<DateTime, sbyte>(), TypeKey.ServiceKey<DateTime, sbyte>());
		Assert.AreNotEqual(TypeKey.ServiceKey<int, Empty>(), TypeKey.ServiceKey<long, Empty>());
		Assert.AreNotEqual(TypeKey.ServiceKey<int, Empty>(), TypeKey.ServiceKey<int, byte>());

		Assert.AreSame(TypeKey.ImplementationKey<int, Empty>(), TypeKey.ImplementationKey<int, Empty>());
		Assert.AreSame(TypeKey.ImplementationKey<DateTime, sbyte>(), TypeKey.ImplementationKey<DateTime, sbyte>());
		Assert.AreNotEqual(TypeKey.ImplementationKey<int, Empty>(), TypeKey.ImplementationKey<long, Empty>());
		Assert.AreNotEqual(TypeKey.ImplementationKey<int, Empty>(), TypeKey.ImplementationKey<int, byte>());

		Assert.AreNotEqual(TypeKey.ServiceKey<int, Empty>(), TypeKey.ImplementationKey<int, Empty>());
		Assert.AreNotEqual(TypeKey.ServiceKey<object, string>(), TypeKey.ImplementationKey<object, string>());
	}

	[TestMethod]
	public void DefinitionImplementationTypeKeyTest()
	{
		// Arrange
		var key1 = ((GenericTypeKey) TypeKey.ImplementationKey<GenericClass<int>, int>()).DefinitionKey;
		var key2 = ((GenericTypeKey) TypeKey.ImplementationKey<GenericClass<long>, int>()).DefinitionKey;

		// Act
		object key3 = key1;

		// Assert

		Assert.IsTrue(key1.Equals(key3));
		Assert.IsTrue(key1.Equals(key2));
		Assert.IsFalse(key1.Equals(new object()));
	}

	[TestMethod]
	public void DefinitionServiceTypeKeyTest()
	{
		// Arrange
		var key1 = ((GenericTypeKey) TypeKey.ServiceKey<GenericClass<int>, int>()).DefinitionKey;
		var key2 = ((GenericTypeKey) TypeKey.ServiceKey<GenericClass<long>, int>()).DefinitionKey;

		// Act
		object key3 = key1;

		// Assert

		Assert.IsTrue(key1.Equals(key3));
		Assert.IsTrue(key1.Equals(key2));
	}

	[TestMethod]
	public void ServiceGenericToStringTypeKeyTest()
	{
		// Arrange
		var key = TypeKey.ServiceKey<GenericClass<object>, int>();

		// Assert
		Assert.AreEqual("SRV:GenericClass<object>(int)", key.ToString());
	}

	[TestMethod]
	public void ImplementationGenericToStringTypeKeyTest()
	{
		// Arrange
		var key = TypeKey.ImplementationKey<GenericClass<object>, int>();

		// Assert
		Assert.AreEqual("IMP:GenericClass<object>(int)", key.ToString());
	}

	[TestMethod]
	public void ServiceSimpleToStringTypeKeyTest()
	{
		// Arrange
		var key = TypeKey.ServiceKey<object, Empty>();

		// Assert
		Assert.AreEqual("SRV:object", key.ToString());
	}

	[TestMethod]
	public void ImplementationSimpleToStringTypeKeyTest()
	{
		// Arrange
		var key = TypeKey.ImplementationKey<object, Empty>();

		// Assert
		Assert.AreEqual("IMP:object", key.ToString());
	}

	[TestMethod]
	public void ServiceSimpleDoTypedActionTypeKeyTest()
	{
		// Arrange
		var key = TypeKey.ServiceKey<object, Empty>();

		// Act
		key.DoTypedAction(new TypedActionClass());
	}

	[TestMethod]
	public void ImplementationSimpleDoTypedActionTypeKeyTest()
	{
		// Arrange
		var key = TypeKey.ImplementationKey<object, Empty>();

		// Assert
		key.DoTypedAction(new TypedActionClass());
	}

	private class TypedActionClass : ITypeKeyAction
	{
		public void TypedAction<T, TArg>(TypeKey typeKey) { }
	}

	[UsedImplicitly]
	private class GenericClass<T> : List<T>;
}