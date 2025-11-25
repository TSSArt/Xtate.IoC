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

// Demonstrates instance scopes:
// Transient      -> AddType<T>() : new instance every resolution.
// Scoped         -> AddSharedType<T>(SharedWithin.Scope) : one instance per scope.
// Container-wide -> AddSharedType<T>(SharedWithin.Container) : one instance for entire container (singleton).
// The example creates a root container and a child scope and compares instances.

public class TransientService;

public class ScopedService;

public class SingletonService;

[TestClass]
public class InstanceScopesExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<TransientService>();
														 services.AddSharedType<ScopedService>(SharedWithin.Scope);
														 services.AddSharedType<SingletonService>(SharedWithin.Container);
													 });

		// Root container resolutions
		var transientA = await container.GetRequiredService<TransientService>();
		var transientB = await container.GetRequiredService<TransientService>();
		var scopedRootA = await container.GetRequiredService<ScopedService>();
		var scopedRootB = await container.GetRequiredService<ScopedService>();
		var singletonRootA = await container.GetRequiredService<SingletonService>();
		var singletonRootB = await container.GetRequiredService<SingletonService>();

		// Create child scope
		var scopeFactory = await container.GetRequiredService<IServiceScopeFactory>();
		await using var scope = scopeFactory.CreateScope();
		var scopedProvider = scope.ServiceProvider;

		var transientScopeA = await scopedProvider.GetRequiredService<TransientService>();
		var transientScopeB = await scopedProvider.GetRequiredService<TransientService>();
		var scopedChildA = await scopedProvider.GetRequiredService<ScopedService>();
		var scopedChildB = await scopedProvider.GetRequiredService<ScopedService>();
		var singletonChildA = await scopedProvider.GetRequiredService<SingletonService>();
		var singletonChildB = await scopedProvider.GetRequiredService<SingletonService>();

		// Transient: always different
		Assert.AreNotSame(transientA, transientB);
		Assert.AreNotSame(transientScopeA, transientScopeB);
		Assert.AreNotSame(transientA, transientScopeA);

		// Scoped: same within same provider, different across root and child scope
		Assert.AreSame(scopedRootA, scopedRootB);
		Assert.AreSame(scopedChildA, scopedChildB);
		Assert.AreNotSame(scopedRootA, scopedChildA);

		// Singleton (container): same across all resolutions and scopes
		Assert.AreSame(singletonRootA, singletonRootB);
		Assert.AreSame(singletonChildA, singletonChildB);
		Assert.AreSame(singletonRootA, singletonChildA);
	}
}