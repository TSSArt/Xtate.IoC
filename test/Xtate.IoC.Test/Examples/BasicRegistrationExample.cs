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

// ReSharper disable All
namespace Xtate.IoC.Examples;

// Basic transient registration and resolution.
// Shows how to register a simple service and resolve it asynchronously.

public class NoInterfaceService;

public interface ISimpleService;

public class SimpleService : ISimpleService;

public interface IService1;

public interface IService2;

public class MultiService : IService1, IService2;

[TestClass]
public class BasicRegistrationExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<NoInterfaceService>();
														 services.AddImplementation<SimpleService>().For<ISimpleService>();
														 services.AddImplementation<MultiService>().For<IService1>().For<IService2>();
													 });

		var service = await container.GetRequiredService<NoInterfaceService>();
		Assert.IsInstanceOfType<NoInterfaceService>(service);

		var simpleService = await container.GetRequiredService<ISimpleService>();
		Assert.IsInstanceOfType<SimpleService>(simpleService);

		var service1 = await container.GetRequiredService<IService1>();
		Assert.IsInstanceOfType<MultiService>(service1);

		var service2 = await container.GetRequiredService<IService2>();
		Assert.IsInstanceOfType<MultiService>(service2);
	}
}