// Copyright © 2019-2024 Sergii Artemenko
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

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceCollectionTest
{
	[TestMethod]
	public async Task ServiceCollection_ShouldBuildProvider_WithRequiredServiceScopeFactory()
	{
		// Arrange
		var sc = new ServiceCollectionNew();
		var sp = sc.BuildProvider();

		// Act
		var serviceScope = await sp.GetRequiredService<IServiceScopeFactory>();

		// Assert
		Assert.IsNotNull(serviceScope);
	}

	private class Forwarding : ForwardingImplementationEntry
	{
		public Forwarding(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) { }

		private Forwarding(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) { }

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new Forwarding(serviceProvider, this);

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new Forwarding(serviceProvider, factory);
	}

	private class Transient : TransientImplementationEntry
	{
		public Transient(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) { }

		private Transient(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) { }

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new Transient(serviceProvider, this);

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new Transient(serviceProvider, factory);
	}

	private class ScopedOwner : ScopedOwnerImplementationEntry
	{
		public ScopedOwner(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) { }

		private ScopedOwner(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) { }

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new ScopedOwner(serviceProvider, this);

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new ScopedOwner(serviceProvider, factory);
	}

	private class SingletonOwner : SingletonOwnerImplementationEntry
	{
		public SingletonOwner(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) { }

		private SingletonOwner(ServiceProvider serviceProvider, SingletonOwnerImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) { }

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new SingletonOwner(serviceProvider, this);

		public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new SingletonOwner(serviceProvider, factory);
	}

	private class ServiceProviderNew(ServiceCollectionNew services) : ServiceProvider(services)
	{
		protected override void Dispose(bool disposing) { }

		protected override ValueTask DisposeAsyncCore() => default;

		protected override ImplementationEntry CreateImplementationEntry(ServiceEntry service) =>
			service.InstanceScope switch
			{
				InstanceScope.Transient  => new Transient(this, service.Factory),
				InstanceScope.Forwarding => new Forwarding(this, service.Factory),
				InstanceScope.Scoped     => new ScopedOwner(this, service.Factory),
				InstanceScope.Singleton  => new SingletonOwner(this, service.Factory),
				_                        => base.CreateImplementationEntry(service)
			};
	}

	private class ServiceCollectionNew : ServiceCollection
	{
		public override IServiceProvider BuildProvider() => new ServiceProviderNew(this);
	}
}