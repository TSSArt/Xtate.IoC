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

// Asynchronous initialization example.
// Shows how services that implement IAsyncInitialization are automatically awaited
// by the container before the resolved instance is returned.
// Demonstrates dependency ordering: a service depending on another async-initialized service
// observes the dependency already initialized when its own initialization completes.

public class SimpleAsyncService : IAsyncInitialization
{
	public string? Message { get; private set; }

#region Interface IAsyncInitialization

	Task IAsyncInitialization.Initialization => field ??= Init();

#endregion

	private async Task Init()
	{
		// Simulate async work
		await Task.Yield();

		Message = nameof(SimpleAsyncService) + " initialized";
	}
}

public class AsyncServiceWithDependency : IAsyncInitialization
{
	public required SimpleAsyncService SimpleAsyncService { private get; init; }

	public string? CombinedMessage { get; private set; }

#region Interface IAsyncInitialization

	Task IAsyncInitialization.Initialization => field ??= Init();

#endregion

	private async Task Init()
	{
		// Ensure asynchronous scheduling to demonstrate awaiting.
		await Task.Yield();

		// At this point the dependency's Initialization has already completed.
		CombinedMessage = SimpleAsyncService.Message + " -> " + nameof(AsyncServiceWithDependency) + " initialized";
	}
}

[TestClass]
public class AsyncInitializationExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<SimpleAsyncService>();
														 services.AddType<AsyncServiceWithDependency>();
													 });

		var simple = await container.GetRequiredService<SimpleAsyncService>();
		Assert.AreEqual(expected: "SimpleAsyncService initialized", simple.Message);

		var withDep = await container.GetRequiredService<AsyncServiceWithDependency>();
		Assert.AreEqual(expected: "SimpleAsyncService initialized -> AsyncServiceWithDependency initialized", withDep.CombinedMessage);
	}
}