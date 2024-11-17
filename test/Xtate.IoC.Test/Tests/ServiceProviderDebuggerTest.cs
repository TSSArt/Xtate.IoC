// Copyright � 2019-2024 Sergii Artemenko
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

using System.IO;

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceProviderDebuggerTest
{
	[TestMethod]
	public void RegisterService_ShouldWriteCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var serviceEntry = new ServiceEntry(TypeKey.ServiceKey<object, ValueTuple>(), InstanceScope.Transient, () => new object());

		// Act
		debugger.RegisterServices().RegisterService(serviceEntry);

		// Assert
		var expected = $"REG: {serviceEntry.InstanceScope,-18} | {serviceEntry.Key}";
		Assert.IsTrue(stringWriter.ToString().Contains(expected));
	}

	[TestMethod]
	public void ServiceRequesting_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();

		// Act
		debugger.ServiceRequesting(typeKey);
		debugger.ServiceRequested(typeKey);

		// Assert
		Assert.IsTrue(stringWriter.ToString().Contains(typeKey.ToString() ?? string.Empty));
	}

	[TestMethod]
	public void FactoryCalling_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();

		// Act
		debugger.ServiceRequesting(typeKey);
		debugger.FactoryCalling(typeKey);
		debugger.FactoryCalled(typeKey);
		debugger.ServiceRequested(typeKey);

		// Assert
		Assert.IsTrue(stringWriter.ToString().Contains("{#1}"));
	}

	[TestMethod]
	public void FactoryCalled_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();

		// Act
		debugger.ServiceRequesting(typeKey);
		debugger.FactoryCalling(typeKey);
		debugger.FactoryCalled(typeKey);
		debugger.ServiceRequested(typeKey);
		debugger.Dispose();

		// Assert
		Assert.IsTrue(stringWriter.ToString().Contains("STAT:"));
	}

	[TestMethod]
	public void ServiceRequested_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();

		// Act
		debugger.ServiceRequesting(typeKey);
		debugger.ServiceRequested(typeKey);

		// Assert
		Assert.IsTrue(stringWriter.ToString().Contains("CACHED"));
	}

	[TestMethod]
	public async Task FactoryCalled_MultiThreadLogTest()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();
		var typeKey2 = TypeKey.ServiceKey<long, (int, long)>();

		// Act
		var asyncDebug1 = AsyncDebug(debugger, typeKey, typeKey2);
		var asyncDebug2 = AsyncDebug(debugger, typeKey, typeKey2);
		var asyncDebug3 = AsyncDebug(debugger, typeKey, typeKey2);

		await asyncDebug1;
		await asyncDebug2;
		await asyncDebug3;

		debugger.Dispose();

		// Assert
		Assert.IsTrue(stringWriter.ToString().Contains("STAT:"));
		Assert.IsTrue(stringWriter.ToString().Contains("{#3}"));
	}

	private static async Task AsyncDebug(ServiceProviderDebugger debugger, TypeKey typeKey, TypeKey typeKey2)
	{
		debugger.ServiceRequesting(typeKey);
		debugger.FactoryCalling(typeKey);

		debugger.ServiceRequesting(typeKey2);
		debugger.FactoryCalling(typeKey2);

		await Task.Yield();

		debugger.FactoryCalled(typeKey2);
		debugger.ServiceRequested(typeKey2);

		debugger.FactoryCalled(typeKey);
		debugger.ServiceRequested(typeKey);
	}

	[TestMethod]
	public void FactoryCalling_100Times_ShouldThrowException()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();

		// Act
		for (var i = 0; i < 100; i ++)
		{
			debugger.FactoryCalling(typeKey);
		}

		// Assert
		Assert.ThrowsException<DependencyInjectionException>(() => debugger.FactoryCalling(typeKey));
	}
}