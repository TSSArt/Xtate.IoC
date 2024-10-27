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
public class StubTypeTest
{
	[TestMethod]
	public void IsResolvedType_ShouldReturnTrueForValidTypes()
	{
		// Arrange
		var validType1 = typeof(int[]);
		var validType2 = typeof(List<List<int>>);

		// Act
		var result1 = StubType.IsResolvedType(validType1);
		var result2 = StubType.IsResolvedType(validType2);

		// Assert
		Assert.IsTrue(result1);
		Assert.IsTrue(result2);
	}

	[TestMethod]
	public void IsResolvedType_ShouldReturnFalseForInvalidTypes()
	{
		// Arrange
		var invalidType1 = typeof(List<List<Any>>);
		var invalidType2 = typeof(List<>);

		// Act
		var result1 = StubType.IsResolvedType(invalidType1);
		var result2 = StubType.IsResolvedType(invalidType2);

		// Assert
		Assert.IsFalse(result1);
		Assert.IsFalse(result2);
	}

	[TestMethod]
	public void TryMap_ShouldReturnTrueForValidMappings()
	{
		// Arrange
		var type1 = typeof(int);
		var type2 = typeof(int);
		var type3 = typeof(List<Any[]>);
		var type4 = typeof(List<int[]>);

		// Act & Assert
		Assert.IsTrue(StubType.TryMap(typesToMap1: null, typesToMap2: null, type1, arg2: null));
		Assert.IsTrue(StubType.TryMap(typesToMap1: null, typesToMap2: null, arg1: null, type2));
		Assert.IsTrue(StubType.TryMap(typesToMap1: null, typesToMap2: null, type3, type4));
	}

	[TestMethod]
	public void TryMap_ShouldReturnFalseForInvalidMappings()
	{
		// Arrange
		var type1 = typeof(List<Any>);
		var type2 = typeof(List<Any[]>);
		var type3 = typeof(List<string>);
		var type4 = typeof(Any[]);
		var type5 = typeof(int);

		// Act & Assert
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, arg1: null, type1));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, type1, arg2: null));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, type2, type3));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, type3, type2));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, type4, arg2: null));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, arg1: null, type4));
		Assert.IsFalse(StubType.TryMap(typesToMap1: null, typesToMap2: null, [type5], [type5, type5]));
	}

	[TestMethod]
	public void UpdateType_ShouldUpdateGenericArgumentToVoid()
	{
		// Arrange
		var args = typeof(GenericClass<>).GetGenericArguments();

		// Act
		StubType.TryMap(args, typesToMap2: null, args[0], arg2: null);

		// Assert
		Assert.AreEqual(typeof(void), args[0]);
	}

	// ReSharper disable All
	public class GenericClass<T> { }

	// ReSharper restore All
}