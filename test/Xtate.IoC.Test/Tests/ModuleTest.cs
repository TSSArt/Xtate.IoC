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

namespace Xtate.IoC.Test;

[TestClass]
public class ModuleTest
{
	[TestMethod]
	[Obsolete(message: "Suppressing warning for AddConstant", error: true)]
	public void AddConstant_ShouldAddValueTask()
	{
		// Arrange & Act
		using var container = Container.Create(services => services.AddConstant(new ValueTask<int>()));

		// Assert
		// No assertion needed as we are testing the AddConstant method
	}

	[TestMethod]
	public async Task AddModule_ShouldReturnDefaultShortValue_Async()
	{
		// Arrange
		await using var container = Container.Create(services => services.AddModule<MyModule<short>>());

		// Act
		var val = await container.GetRequiredService<short>();

		// Assert
		Assert.AreEqual(expected: 0, val);
	}

	[TestMethod]
	public void AddModule_ShouldReturnDefaultShortValue()
	{
		// Arrange
		using var container = Container.Create(services => services.AddModule<MyModule<short>>());

		// Act
		var val = container.GetRequiredServiceSync<short>();

		// Assert
		Assert.AreEqual(expected: 0, val);
	}

	[TestMethod]
	public void AddModule1_ShouldReturnDefaultIntValueAndModuleInstance()
	{
		// Arrange
		using var containerNo = Container.Create<MyModule1>();
		using var container = Container.Create<MyModule1>(_ => { });

		// Act
		var val = container.GetRequiredServiceSync<int>();
		var mod = container.GetRequiredServiceSync<MyModule1>();

		// Assert
		Assert.AreEqual(expected: 0, val);
		Assert.IsNotNull(mod);
	}

	[TestMethod]
	public void AddModule2_ShouldReturnDefaultIntAndLongValuesAndModuleInstance()
	{
		// Arrange
		using var containerNo = Container.Create<MyModule2, MyModule<object>>();
		using var container = Container.Create<MyModule2, MyModule<object>>(_ => { });

		// Act
		var val1 = container.GetRequiredServiceSync<int>();
		var val2 = container.GetRequiredServiceSync<long>();
		var mod = container.GetRequiredServiceSync<MyModule2>();

		// Assert
		Assert.AreEqual(expected: 0, val1);
		Assert.AreEqual(expected: 0, val2);
		Assert.IsNotNull(mod);
	}

	[TestMethod]
	public void AddModule3_ShouldReturnDefaultIntLongByteValuesAndModuleInstance()
	{
		// Arrange
		using var containerNo = Container.Create<MyModule3, MyModule<object>, MyModule<object>>();
		using var container = Container.Create<MyModule3, MyModule<object>, MyModule<object>>(_ => { });

		// Act
		var val1 = container.GetRequiredServiceSync<int>();
		var val2 = container.GetRequiredServiceSync<long>();
		var val3 = container.GetRequiredServiceSync<byte>();
		var mod = container.GetRequiredServiceSync<MyModule3>();

		// Assert
		Assert.AreEqual(expected: 0, val1);
		Assert.AreEqual(expected: 0, val2);
		Assert.AreEqual(expected: 0, val3);
		Assert.IsNotNull(mod);
	}

	[TestMethod]
	public void AddModule4_ShouldReturnDefaultIntLongByteSbyteValuesAndModuleInstance()
	{
		// Arrange
		using var containerNo = Container.Create<MyModule4, MyModule<object>, MyModule<object>, MyModule<object>>();
		using var container = Container.Create<MyModule4, MyModule<object>, MyModule<object>, MyModule<object>>(_ => { });

		// Act
		var val1 = container.GetRequiredServiceSync<int>();
		var val2 = container.GetRequiredServiceSync<long>();
		var val3 = container.GetRequiredServiceSync<byte>();
		var val4 = container.GetRequiredServiceSync<sbyte>();
		var mod = container.GetRequiredServiceSync<MyModule4>();

		// Assert
		Assert.AreEqual(expected: 0, val1);
		Assert.AreEqual(expected: 0, val2);
		Assert.AreEqual(expected: 0, val3);
		Assert.AreEqual(expected: 0, val4);
		Assert.IsNotNull(mod);
	}

	[TestMethod]
	public void AddModule5_ShouldReturnDefaultIntLongByteSbyteUlongValuesAndModuleInstance()
	{
		// Arrange
		using var containerNo = Container.Create<MyModule5, MyModule<object>, MyModule<object>, MyModule<object>, MyModule<object>>();
		using var container = Container.Create<MyModule5, MyModule<object>, MyModule<object>, MyModule<object>, MyModule<object>>(_ => { });

		// Act
		var val1 = container.GetRequiredServiceSync<int>();
		var val2 = container.GetRequiredServiceSync<long>();
		var val3 = container.GetRequiredServiceSync<byte>();
		var val4 = container.GetRequiredServiceSync<sbyte>();
		var val5 = container.GetRequiredServiceSync<ulong>();
		var mod = container.GetRequiredServiceSync<MyModule5>();

		// Assert
		Assert.AreEqual(expected: 0, val1);
		Assert.AreEqual(expected: 0, val2);
		Assert.AreEqual(expected: 0, val3);
		Assert.AreEqual(expected: 0, val4);
		Assert.AreEqual(expected: 0ul, val5);
		Assert.IsNotNull(mod);
	}

	private class MyModule<T> : Module where T : new()
	{
		protected override void AddServices()
		{
			Services.AddConstant(new T());
		}
	}

	private class MyModule1 : Module<MyModule<int>>
	{
		protected override void AddServices()
		{
			Services.AddForwarding(_ => new MyModule1());
		}
	}

	private class MyModule2 : Module<MyModule<int>, MyModule<long>>
	{
		protected override void AddServices()
		{
			Services.AddForwarding(_ => new MyModule2());
		}
	}

	private class MyModule3 : Module<MyModule<int>, MyModule<long>, MyModule<byte>>
	{
		protected override void AddServices()
		{
			Services.AddForwarding(_ => new MyModule3());
		}
	}

	private class MyModule4 : Module<MyModule<int>, MyModule<long>, MyModule<byte>, MyModule<sbyte>>
	{
		protected override void AddServices()
		{
			Services.AddForwarding(_ => new MyModule4());
		}
	}

	private class MyModule5 : Module<MyModule<int>, MyModule<long>, MyModule<byte>, MyModule<sbyte>, MyModule<ulong>>
	{
		protected override void AddServices()
		{
			Services.AddForwarding(_ => new MyModule5());
		}
	}
}