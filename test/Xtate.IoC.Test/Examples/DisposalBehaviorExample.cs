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

// Demonstrates difference between AddConstant / AddForwarding (Forwarding scope) and AddType (Transient scope)
// regarding lifetime & disposal responsibility.
// Forwarding entries (AddConstant / AddForwarding) are NOT disposed by the container.
// Transient entries (AddType) are tracked and disposed when the provider (container or scope) is disposed.

public sealed class ConstantService : IDisposable
{
	public bool Disposed { get; private set; }

#region Interface IDisposable

	public void Dispose() => Disposed = true;

#endregion
}

public sealed class ForwardedService : IDisposable
{
	public bool Disposed { get; private set; }

#region Interface IDisposable

	public void Dispose() => Disposed = true;

#endregion
}

// Renamed to avoid clash with other example's TransientService
public sealed class DisposableTransientService : IDisposable
{
	public bool Disposed { get; private set; }

#region Interface IDisposable

	public void Dispose() => Disposed = true;

#endregion
}

public sealed class ScopedLambdaService : IDisposable
{
	public bool Disposed { get; private set; }

#region Interface IDisposable

	public void Dispose() => Disposed = true;

#endregion
}

public sealed class ContainerLambdaService : IDisposable
{
	public bool Disposed { get; private set; }

#region Interface IDisposable

	public void Dispose() => Disposed = true;

#endregion
}

[TestClass]
public class DisposalBehaviorExample
{
	[TestMethod]
	public async ValueTask Disposal()
	{
		DisposableTransientService transientA, transientB;
		ScopedLambdaService scopedLambdaRoot;
		ContainerLambdaService containerLambda;
		ConstantService constantService;
		ForwardedService forwardedService;

		await using (var container = Container.Create(services =>
													  {
														  // Constant, Forwarding lambda, Shared lambda registrations (container does not own disposal)
														  services.AddConstant(new ConstantService());
														  services.AddForwarding(_ => new ForwardedService());

														  // Transient and Shared registrations (container tracks and disposes)
														  services.AddTransient(_ => new DisposableTransientService());
														  services.AddShared(SharedWithin.Scope, _ => new ScopedLambdaService());
														  services.AddShared(SharedWithin.Container, _ => new ContainerLambdaService());
													  }))
		{
			// Resolve several transients
			transientA = await container.GetRequiredService<DisposableTransientService>();
			transientB = await container.GetRequiredService<DisposableTransientService>();
			scopedLambdaRoot = await container.GetRequiredService<ScopedLambdaService>();
			containerLambda = await container.GetRequiredService<ContainerLambdaService>();
			constantService = await container.GetRequiredService<ConstantService>();
			forwardedService = await container.GetRequiredService<ForwardedService>();

			// Root scope transient service (to show separate scope disposal)
			var scopeFactory = await container.GetRequiredService<IServiceScopeFactory>();
			ScopedLambdaService scopedLambdaInner;

			await using (var scope = scopeFactory.CreateScope())
			{
				var scopedProvider = scope.ServiceProvider;
				scopedLambdaInner = await scopedProvider.GetRequiredService<ScopedLambdaService>();
				Assert.IsFalse(scopedLambdaInner.Disposed);
			}

			Assert.IsTrue(scopedLambdaInner.Disposed); // Disposed by Container

			// Before disposal nothing is disposed yet
			Assert.IsFalse(containerLambda.Disposed);
			Assert.IsFalse(scopedLambdaRoot.Disposed);
			Assert.IsFalse(transientA.Disposed);
			Assert.IsFalse(transientB.Disposed);
			Assert.IsFalse(constantService.Disposed);
			Assert.IsFalse(forwardedService.Disposed);
		}

		Assert.IsTrue(containerLambda.Disposed);   // Disposed by Container
		Assert.IsTrue(scopedLambdaRoot.Disposed);  // Disposed by Container
		Assert.IsTrue(transientA.Disposed);        // Disposed by Container
		Assert.IsTrue(transientB.Disposed);        // Disposed by Container
		Assert.IsFalse(constantService.Disposed);  // Needed explicit Dispose since not disposed by Container
		Assert.IsFalse(forwardedService.Disposed); // Needed explicit Dispose since not disposed by Container

		constantService.Dispose();
		forwardedService.Dispose();

		Assert.IsTrue(constantService.Disposed);
		Assert.IsTrue(forwardedService.Disposed);
	}
}