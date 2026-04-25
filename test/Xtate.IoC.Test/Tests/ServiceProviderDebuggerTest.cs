// Copyright © 2019-2026 Sergii Artemenko
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
		debugger.RegisterServices(1).RegisterService(serviceEntry);

		// Assert
		var expected = $"REG: {serviceEntry.InstanceScope,-18} | {serviceEntry.Key}";
		Assert.Contains(expected, stringWriter.ToString());
	}

	[TestMethod]
	public void ServiceRequesting_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var typeKey = TypeKey.ServiceKey<object, ValueTuple>();
		var context = ActionsContext.Create<object, ValueTuple>();

		// Act
		debugger.Event(ActionsEventType.ServiceRequesting, ref context);
		debugger.Event(ActionsEventType.ServiceRequested, ref context);

		// Assert
		Assert.Contains(typeKey.ToString() ?? string.Empty, stringWriter.ToString());
	}

	[TestMethod]
	public void FactoryCalling_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var context = ActionsContext.Create<object, ValueTuple>();
		var context2 = ActionsContext.Create<object, ValueTuple>();

		// Act
		debugger.Event(ActionsEventType.ServiceRequesting, ref context);
		debugger.Event(ActionsEventType.FactoryCalling, ref context2);
		debugger.Event(ActionsEventType.FactoryCalled, ref context2);
		debugger.Event(ActionsEventType.ServiceRequested, ref context);

		// Assert
		Assert.Contains(substring: "{#1}", stringWriter.ToString());
	}

	[TestMethod]
	public void FactoryCalled_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var context = ActionsContext.Create<object, ValueTuple>();
		var context2 = ActionsContext.Create<object, ValueTuple>();

		// Act
		debugger.Event(ActionsEventType.ServiceRequesting, ref context);
		debugger.Event(ActionsEventType.ServiceRequestRunning, ref context);
		debugger.Event(ActionsEventType.FactoryCalling, ref context2);
		debugger.Event(ActionsEventType.FactoryCallRunning, ref context2);
		debugger.Event(ActionsEventType.FactoryCalled, ref context2);
		debugger.Event(ActionsEventType.ServiceRequested, ref context);
		debugger.Dispose();

		// Assert
		Assert.Contains(substring: "STAT:", stringWriter.ToString());
	}

	[TestMethod]
	public void ServiceRequested_ShouldLogCorrectly()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var context = ActionsContext.Create<object, ValueTuple>();

		// Act
		debugger.Event(ActionsEventType.ServiceRequesting, ref context);
		debugger.Event(ActionsEventType.ServiceRequested, ref context);

		// Assert
		Assert.Contains(substring: "CACHED", stringWriter.ToString());
	}

	[TestMethod]
	public async Task FactoryCalled_MultiThreadLogTest()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);

		// Act
		var asyncDebug1 = AsyncDebug(debugger);
		var asyncDebug2 = AsyncDebug(debugger);
		var asyncDebug3 = AsyncDebug(debugger);

		await asyncDebug1;
		await asyncDebug2;
		await asyncDebug3;

		debugger.Dispose();

		// Assert
		Assert.Contains(substring: "STAT:", stringWriter.ToString());
		Assert.Contains(substring: "{#3}", stringWriter.ToString());
	}

	private static async Task AsyncDebug(ServiceProviderDebugger debugger)
	{
		var context1 = ActionsContext.Create<object, ValueTuple>();
		var context2 = ActionsContext.Create<object, ValueTuple>();
		var context3 = ActionsContext.Create<long, (int, long)>();
		var context4 = ActionsContext.Create<long, (int, long)>();

		debugger.Event(ActionsEventType.ServiceRequesting, ref context1);
		debugger.Event(ActionsEventType.FactoryCalling, ref context2);

		debugger.Event(ActionsEventType.ServiceRequesting, ref context3);
		debugger.Event(ActionsEventType.FactoryCalling, ref context4);

		await Task.Yield();

		debugger.Event(ActionsEventType.FactoryCalled, ref context4);
		debugger.Event(ActionsEventType.ServiceRequested, ref context3);

		debugger.Event(ActionsEventType.FactoryCalled, ref context2);
		debugger.Event(ActionsEventType.ServiceRequested, ref context1);
	}

	[TestMethod]
	public void FactoryCalling_100Times_ShouldThrowException()
	{
		// Arrange
		var stringWriter = new StringWriter();
		var debugger = new ServiceProviderDebugger(stringWriter);
		var context = ActionsContext.Create<object, ValueTuple>();

		// Act
		for (var i = 0; i < 500; i ++)
		{
			debugger.Event(ActionsEventType.FactoryCalling, ref context);
		}

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => debugger.Event(ActionsEventType.FactoryCalling, ref context));
	}
}