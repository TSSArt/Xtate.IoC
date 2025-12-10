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

// Demonstrates injecting a dependency via a factory delegate Func<ValueTask<T>>.
// Container provides the delegate automatically so the consumer can defer resolution
// or obtain multiple instances on demand.
// We show transient vs shared dependency behavior.

public class DepService
{
	public Guid Id { get; } = Guid.NewGuid();
}

public class SharedDepService
{
	public Guid Id { get; } = Guid.NewGuid();
}

public class ConsumerService(Func<ValueTask<DepService>> depFactory)
{
	private readonly Func<ValueTask<DepService>> _depFactory = depFactory;

	public async ValueTask<DepService> GetDependencyAsync() => await _depFactory();
}

public class SharedConsumerService(Func<ValueTask<SharedDepService>> depFactory)
{
	private readonly Func<ValueTask<SharedDepService>> _depFactory = depFactory;

	public async ValueTask<SharedDepService> GetDependencyAsync() => await _depFactory();
}

[TestClass]
public class FactoryDelegateInjectionExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(s =>
													 {
														 s.AddType<DepService>();      // transient dependency
														 s.AddType<ConsumerService>(); // consumes factory for DepService

														 s.AddSharedType<SharedDepService>(SharedWithin.Container); // shared dependency
														 s.AddType<SharedConsumerService>();                        // consumes factory for SharedDepService
													 });

		var consumer = await container.GetRequiredService<ConsumerService>();
		var dep1 = await consumer.GetDependencyAsync();
		var dep2 = await consumer.GetDependencyAsync();
		Assert.IsNotNull(dep1);
		Assert.IsNotNull(dep2);
		Assert.AreNotEqual(dep1.Id, dep2.Id); // transient -> different instances

		var sharedConsumer = await container.GetRequiredService<SharedConsumerService>();
		var shared1 = await sharedConsumer.GetDependencyAsync();
		var shared2 = await sharedConsumer.GetDependencyAsync();
		Assert.IsNotNull(shared1);
		Assert.IsNotNull(shared2);
		Assert.AreEqual(shared1.Id, shared2.Id); // singleton -> same instance
	}
}