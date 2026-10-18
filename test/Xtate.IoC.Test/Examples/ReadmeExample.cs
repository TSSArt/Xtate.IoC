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

namespace Xtate.IoC.Test.Examples;

[TestClass]
public class ReadmeExample
{
	[TestMethod]
	public async Task QuickStart()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddImplementation<MyService>().For<IMyService>();    // Transient
														 services.AddSharedType<MyRepository>(SharedWithin.Container); // Singleton
													 });

		var service = await container.GetRequiredService<IMyService>(); // Resolves transient instance
		Assert.IsInstanceOfType<MyService>(service);
		Assert.IsNotNull(((MyService)service).Repository);
	}

	// Kept public to mirror the README application contract.
	// ReSharper disable once MemberCanBePrivate.Global
	public interface IMyService;

	[UsedImplicitly] // Constructed and populated by the container.
	public sealed class MyService : IMyService
	{
		public required MyRepository Repository { get; init; }
	}

	[UsedImplicitly] // Constructed and populated by the container.
	public sealed class MyRepository;
}