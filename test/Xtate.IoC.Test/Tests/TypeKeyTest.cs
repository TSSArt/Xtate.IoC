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

using Empty = System.ValueTuple;

namespace Xtate.IoC.Test;

[TestClass]
public class TypeKeyTest
{
	[TestMethod]
	public void ServiceKey_SameType_ReturnsSameInstance()
	{
		// Assert  
		Assert.AreSame(TypeKey.ServiceKey<int, Empty>(), TypeKey.ServiceKey<int, Empty>());
		Assert.AreSame(TypeKey.ServiceKey<DateTime, sbyte>(), TypeKey.ServiceKey<DateTime, sbyte>());
	}

	[TestMethod]
	public void ServiceKey_DifferentTypes_ReturnsDifferentInstances()
	{
		// Assert  
		Assert.AreNotEqual(TypeKey.ServiceKey<int, Empty>(), TypeKey.ServiceKey<long, Empty>());
		Assert.AreNotEqual(TypeKey.ServiceKey<int, Empty>(), TypeKey.ServiceKey<int, byte>());
	}

	[TestMethod]
	public void ImplementationKey_SameType_ReturnsSameInstance()
	{
		// Assert  
		Assert.AreSame(TypeKey.ImplementationKey<int, Empty>(), TypeKey.ImplementationKey<int, Empty>());
		Assert.AreSame(TypeKey.ImplementationKey<DateTime, sbyte>(), TypeKey.ImplementationKey<DateTime, sbyte>());
	}

	[TestMethod]
	public void ImplementationKey_DifferentTypes_ReturnsDifferentInstances()
	{
		// Assert  
		Assert.AreNotEqual(TypeKey.ImplementationKey<int, Empty>(), TypeKey.ImplementationKey<long, Empty>());
		Assert.AreNotEqual(TypeKey.ImplementationKey<int, Empty>(), TypeKey.ImplementationKey<int, byte>());
	}

	[TestMethod]
	public void ServiceKey_ImplementationKey_DifferentInstances()
	{
		// Assert  
		Assert.AreNotEqual(TypeKey.ServiceKey<int, Empty>(), TypeKey.ImplementationKey<int, Empty>());
		Assert.AreNotEqual(TypeKey.ServiceKey<object, string>(), TypeKey.ImplementationKey<object, string>());
	}

	[TestMethod]
	public void ImplementationKey_DefinitionKey_Equals()
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
	public void ServiceKey_DefinitionKey_Equals()
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
	public void ServiceKey_GenericToString_ReturnsExpectedString()
	{
		// Arrange  
		var key = TypeKey.ServiceKey<GenericClass<object>, int>();

		// Assert  
		Assert.AreEqual(expected: "TypeKeyTest.GenericClass<object>(int p1)", key.ToString());
	}

	[TestMethod]
	public void ImplementationKey_GenericToString_ReturnsExpectedString()
	{
		// Arrange  
		var key = TypeKey.ImplementationKey<GenericClass<object>, int>();

		// Assert  
		Assert.AreEqual(expected: "^TypeKeyTest.GenericClass<object>(int p1)", key.ToString());
	}

	[TestMethod]
	public void ServiceKey_SimpleToString_ReturnsExpectedString()
	{
		// Arrange  
		var key = TypeKey.ServiceKey<object, Empty>();

		// Assert  
		Assert.AreEqual(expected: "object", key.ToString());
	}

	[TestMethod]
	public void ImplementationKey_SimpleToString_ReturnsExpectedString()
	{
		// Arrange  
		var key = TypeKey.ImplementationKey<object, Empty>();

		// Assert  
		Assert.AreEqual(expected: "^object", key.ToString());
	}

	[TestMethod]
	public void ServiceKey_DoTypedAction_ExecutesWithoutException()
	{
		// Arrange  
		var key = TypeKey.ServiceKey<object, Empty>();

		// Act  
		key.DoTypedAction(new TypedActionClass());
	}

	[TestMethod]
	public void ImplementationKey_DoTypedAction_ExecutesWithoutException()
	{
		// Arrange  
		var key = TypeKey.ImplementationKey<object, Empty>();

		// Act  
		key.DoTypedAction(new TypedActionClass());
	}

	[TestMethod]
	public void GenericServiceKey_DefinitionKey_StringIsValid()
	{
		// Arrange  
		var key = (GenericTypeKey) TypeKey.ServiceKey<GenericClass<object>, int>();

		// Act  
		var str = key.DefinitionKey.ToString();

		// Assert
		Assert.AreEqual(expected: "TypeKeyTest.GenericClass<T>", str);
	}

	[TestMethod]
	public void GenericImplementationKey_DefinitionKey_StringIsValid()
	{
		// Arrange  
		var key = (GenericTypeKey) TypeKey.ImplementationKey<GenericClass<object>, int>();

		// Act  
		var str = key.DefinitionKey.ToString();

		// Assert
		Assert.AreEqual(expected: "^TypeKeyTest.GenericClass<T>", str);
	}

	private class TypedActionClass : ITypeKeyAction
	{
	#region Interface ITypeKeyAction

		public void TypedAction<T, TArg>(TypeKey typeKey) { }

	#endregion
	}

	[UsedImplicitly]
	private class GenericClass<T> : List<T>;
}