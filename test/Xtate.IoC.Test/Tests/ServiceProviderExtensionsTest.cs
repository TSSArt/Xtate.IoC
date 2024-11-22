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
public class ServiceProviderExtensionsTest
{
	[TestMethod]
	public async Task GetService_WithTwoArgs_ReturnsService()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetService<Service, int, int>(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void GetServiceSync_WithTwoArgs_ReturnsService()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetServiceSync<Service, int, int>(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void GetServiceSync_WithoutArgs_ReturnsNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetServiceSync<Service>();

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public async Task GetServices_WithTwoArgs_ReturnsListOfServices()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();

		await foreach (var vc in sp.GetServices<Service, int, int>(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesSync_WithArg_ReturnsListOfServices()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();

		foreach (var vc in sp.GetServicesSync<Service, int>(4))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesSync_WithoutArgs_ReturnsListOfServices()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();

		foreach (var vc in sp.GetServicesSync<Service>())
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesSync_WithoutArgs_ReturnsEmptyList()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		using var enumerator = sp.GetServicesSync<Service>().GetEnumerator();
		var result = enumerator.MoveNext();

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GetServicesSync_WithTwoArgs_ReturnsListOfServices()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();

		foreach (var vc in sp.GetServicesSync<Service, int, int>(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public async Task GetServicesFactory_WithTwoArgs_ReturnsListOfServices()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();

		await foreach (var vc in sp.GetServicesFactory<Service, int, int>()(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public void GetServicesFactorySync_WithTwoArgs_ReturnsListOfServices()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var list = new List<Service>();

		foreach (var vc in sp.GetServicesSyncFactory<Service, int, int>()(arg1: 1, arg2: 2))
		{
			list.Add(vc);
		}

		// Assert
		Assert.AreEqual(expected: 1, list.Count);
		Assert.IsNotNull(list[0]);
	}

	[TestMethod]
	public async Task GetServicesFactory_WithoutArgs_ReturnsEmptyList()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var enumerable = sp.GetServicesFactory<Service, int, int>()(arg1: 0, arg2: 1);
		await using var asyncEnumerator = enumerable.GetAsyncEnumerator();
		var next = await asyncEnumerator.MoveNextAsync();

		// Assert
		Assert.IsFalse(next);
	}

	[TestMethod]
	public void GetServicesSyncFactory_WithTwoArgs_ReturnsEmptyList()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var enumerable = sp.GetServicesSyncFactory<Service, int, int>()(arg1: 0, arg2: 1);
		using var enumerator = enumerable.GetEnumerator();
		var next = enumerator.MoveNext();

		// Assert
		Assert.IsFalse(next);
	}

	[TestMethod]
	public void GetServicesSyncFactory_WithArg_ReturnsEmptyList()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var enumerable = sp.GetServicesSyncFactory<Service, int>()(2);
		using var enumerator = enumerable.GetEnumerator();
		var next = enumerator.MoveNext();

		// Assert
		Assert.IsFalse(next);
	}

	[TestMethod]
	public async Task GetRequiredFactory_WithTwoArgs_ReturnsService()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredFactory<Service, int, int>()(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task GetFactory_WithTwoArgs_ReturnsService()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Service, int, int>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetFactory<Service, int, int>()(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task GetFactory_WithTwoArgs_ReturnsNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetFactory<Service, int, int>()(arg1: 1, arg2: 2);

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public void GetRequiredSyncFactory_WithoutArgs_ThrowsException()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act & Assert
		Assert.ThrowsException<MissedServiceException<Service, (int, int)>>(sp.GetRequiredSyncFactory<Service, int, int>);
	}

	[TestMethod]
	public void GetSyncFactory_WithArg_ReturnsNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetSyncFactory<Service, int>()(3);

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public void GetSyncFactory_WithTwoArgs_ReturnsNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetSyncFactory<Service, int, int>()(arg1: 3, arg2: 3);

		// Assert
		Assert.IsNull(service);
	}

	[UsedImplicitly]
	private class Service;
}