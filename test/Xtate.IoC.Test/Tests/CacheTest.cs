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
public class CacheTest
{
	[TestMethod]
	public void Cache_ShouldInitializeWithInitialCapacity()
	{
		// Arrange
		const int initialCapacity = 10;
		var cache = new Cache<string, int>(initialCapacity);

		// Act
		var containsKey = cache.ContainsKey("test");

		// Assert
		Assert.IsFalse(containsKey);
	}

	[TestMethod]
	public void Cache_ShouldInitializeWithInitialCollection()
	{
		// Arrange
		var initialCollection = new List<KeyValuePair<string, int>>
								{
									new(key: "key1", value: 1),
									new(key: "key2", value: 2)
								};
		var cache = new Cache<string, int>(initialCollection);

		// Act
		var containsKey1 = cache.ContainsKey("key1");
		var containsKey2 = cache.ContainsKey("key2");

		// Assert
		Assert.IsTrue(containsKey1);
		Assert.IsTrue(containsKey2);
	}

	[TestMethod]
	public void Cache_TryGetValue_ShouldReturnCorrectValue()
	{
		// Arrange
		var cache = new Cache<string, int>(10);
		cache.TryAdd(key: "key", value: 42);

		// Act
		var result = cache.TryGetValue(key: "key", out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 42, value);
	}

	[TestMethod]
	public void Cache_ContainsKey_ShouldReturnTrueIfKeyExists()
	{
		// Arrange
		var cache = new Cache<string, int>(10);
		cache.TryAdd(key: "key", value: 42);

		// Act
		var containsKey = cache.ContainsKey("key");

		// Assert
		Assert.IsTrue(containsKey);
	}

	[TestMethod]
	public void Cache_GetOrAdd_ShouldAddValueIfKeyDoesNotExist()
	{
		// Arrange
		var cache = new Cache<string, int>(10);

		// Act
		var value = cache.GetOrAdd(key: "key", value: 42);

		// Assert
		Assert.AreEqual(expected: 42, value);
		Assert.IsTrue(cache.ContainsKey("key"));
	}

	[TestMethod]
	public void Cache_TryAdd_ShouldNotAddDuplicateKey()
	{
		// Arrange
		var cache = new Cache<string, int>(10);
		cache.TryAdd(key: "key", value: 42);

		// Act
		cache.TryAdd(key: "key", value: 100);
		var result = cache.TryGetValue(key: "key", out var value);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(expected: 42, value);
	}
}