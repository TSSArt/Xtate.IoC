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

// Demonstrates two dependency injection styles:
// 1. Constructor injection (ServiceA depends on ServiceB via constructor parameter).
// 2. Required property injection (ServiceC depends on ServiceD via required init property).
// Register all types with AddType<T>(). The container automatically supplies constructor arguments
// and sets required init-only properties.

public class ServiceB;

public class ServiceA(ServiceB serviceB)
{
	public ServiceB Dep => serviceB; // constructor-injected dependency
}

public class ServiceD;

public class ServiceC
{
	public required ServiceD ServiceD { private get; init; } // required property injection

	public ServiceD Dep => ServiceD;
}

[TestClass]
public class DependencyInjectionPatternsExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(s =>
													 {
														 s.AddType<ServiceB>();
														 s.AddType<ServiceA>(); // depends on ServiceB via constructor
														 s.AddType<ServiceD>();
														 s.AddType<ServiceC>(); // depends on ServiceD via required property
													 });

		var a = await container.GetRequiredService<ServiceA>();
		Assert.IsNotNull(a.Dep);

		var c = await container.GetRequiredService<ServiceC>();
		Assert.IsNotNull(c.Dep);
	}
}