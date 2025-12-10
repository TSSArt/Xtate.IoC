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

// Demonstrates registering and resolving synchronous vs asynchronous services.
// Sync service: registered with AddTypeSync<T>() and resolved via GetRequiredServiceSync<T>().
// Async service: registered with AddType<T>() (or async factory) and resolved via GetRequiredService<T>().
// Attempting to resolve an async-initialized type synchronously throws a DependencyInjectionException.

public class SyncService;

public class AsyncInitService : IAsyncInitialization
{
	public string Data { get; private set; } = string.Empty;

#region Interface IAsyncInitialization

	Task IAsyncInitialization.Initialization => field ??= Init();

#endregion

	private async Task Init()
	{
		await Task.Yield();
		Data = "Initialized";
	}
}

[TestClass]
public class SyncVsAsyncServicesExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddTypeSync<SyncService>();  // synchronous construction
														 services.AddType<AsyncInitService>(); // asynchronous initialization
													 });

		// Synchronous resolution of SyncService
		var syncInst1 = container.GetRequiredServiceSync<SyncService>();
		Assert.IsNotNull(syncInst1);

		// Asynchronous resolution of synchronous SyncService is acceptable too
		var syncInst2 = await container.GetRequiredService<SyncService>();
		Assert.IsNotNull(syncInst2);

		// Asynchronous resolution of AsyncInitService (awaits initialization internally)
		var asyncInst = await container.GetRequiredService<AsyncInitService>();
		Assert.AreEqual(expected: "Initialized", asyncInst.Data);

		// Synchronous resolution attempt for async initialized service should throw
		Assert.ThrowsExactly<DependencyInjectionException>(() => container.GetRequiredServiceSync<AsyncInitService>());
	}
}