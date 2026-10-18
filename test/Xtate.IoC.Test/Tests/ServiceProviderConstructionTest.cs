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

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceProviderConstructionTest
{
	[TestMethod]
	[DataRow(false, false)]
	[DataRow(false, true)]
	[DataRow(true, false)]
	[DataRow(true, true)]
	public void FailedInitialization_CleansOwnedObjectsAndReleasesReference(bool child, bool cleanupFails)
	{
		using var parent = new ServiceProvider(new ServiceCollection());
		var failure = new InvalidOperationException("registration failed");
		var cleanup = cleanupFails ? new InvalidOperationException("cleanup failed") : null;
		var actions = new FailingActions(failure, cleanup);
		var shared = new Tracked();
		ServiceProvider? failedProvider = null;
		var services = new ServiceCollection();
		services.AddTransient<IServiceProviderActions>(provider =>
													   {
														   failedProvider = (ServiceProvider)provider;
														   failedProvider.SharedObjectsBin.AddSync(shared);

														   return actions;
													   });

		var actual = Assert.ThrowsExactly<InvalidOperationException>(() =>
																	 {
																		 // The callback completes before the enclosing resource scope ends.
																		 // ReSharper disable once AccessToDisposedClosure
																		 _ = child ? new ServiceProviderScope(parent, services) : new ServiceProvider(services);
																	 });

		Assert.AreSame(cleanup ?? failure, actual);
		Assert.AreEqual(expected: 1, actions.DisposeCount);
		Assert.IsNotNull(failedProvider);
		Assert.IsTrue(failedProvider.DisposeToken.IsCancellationRequested);
		Assert.AreEqual(child ? 0 : 1, shared.DisposeCount);
		failedProvider.Dispose();

		// Dispose here to assert cleanup; the using declaration also covers assertion failures.
		// ReSharper disable once DisposeOnUsingVariable
		parent.Dispose();
		Assert.AreEqual(expected: 1, actions.DisposeCount);
		Assert.AreEqual(expected: 1, shared.DisposeCount);
		GC.KeepAlive(shared);
	}

	[TestMethod]
	public void FailedInitialization_LastCleanupExceptionWins()
	{
		var failure = new InvalidOperationException("registration failed");
		var scopedCleanup = new InvalidOperationException("scoped cleanup failed");
		var sharedCleanup = new InvalidOperationException("shared cleanup failed");
		var actions = new FailingActions(failure, scopedCleanup);
		var shared = new Tracked(sharedCleanup);
		var services = new ServiceCollection();
		services.AddTransient<IServiceProviderActions>(provider =>
													   {
														   ((ServiceProvider)provider).SharedObjectsBin.AddSync(shared);

														   return actions;
													   });

		var actual = Assert.ThrowsExactly<InvalidOperationException>(() => new ServiceProvider(services));

		Assert.AreSame(sharedCleanup, actual);
		Assert.AreEqual(expected: 1, actions.DisposeCount);
		Assert.AreEqual(expected: 1, shared.DisposeCount);
		GC.KeepAlive(shared);
	}

	[TestMethod]
	public void FailedRegistrationEnumeration_ReleasesChildReference()
	{
		using var parent = new ServiceProvider(new ServiceCollection());
		var shared = new Tracked();
		parent.SharedObjectsBin.AddSync(shared);
		var failure = new InvalidOperationException("enumeration failed");

		// The callback completes before the enclosing resource scope ends.
		// ReSharper disable once AccessToDisposedClosure
		var actual = Assert.ThrowsExactly<InvalidOperationException>(() => new ServiceProviderScope(parent, new ThrowingCollection(failure)));
		Assert.AreSame(failure, actual);

		// Dispose here to assert cleanup; the using declaration also covers assertion failures.
		// ReSharper disable once DisposeOnUsingVariable
		parent.Dispose();
		Assert.AreEqual(expected: 1, shared.DisposeCount);
		GC.KeepAlive(shared);
	}

	[TestMethod]
	public void FailedInitialization_DoesNotInvokeDerivedDisposalHook()
	{
		var failure = new InvalidOperationException("registration failed");
		var actions = new FailingActions(failure, cleanup: null);
		var services = new ServiceCollection();
		services.AddTransient<IServiceProviderActions>(_ => actions);

		Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => new GuardedProvider(services)));
		Assert.AreEqual(expected: 1, actions.DisposeCount);
	}

	[TestMethod]
	public void FailedInitialization_DoesNotDisposeExternallyOwnedActions()
	{
		var failure = new InvalidOperationException("registration failed");
		var actions = new FailingActions(failure, cleanup: null);
		var services = new ServiceCollection();
		services.AddConstant<IServiceProviderActions>(actions);

		Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => new ServiceProvider(services)));
		Assert.AreEqual(expected: 0, actions.DisposeCount);
	}

	private sealed class FailingActions(Exception failure, Exception? cleanup) : IServiceProviderActions, IDisposable
	{
		public int DisposeCount { get; private set; }

	#region Interface IDisposable

		public void Dispose()
		{
			DisposeCount ++;

			if (cleanup is not null)
			{
				throw cleanup;
			}
		}

	#endregion

	#region Interface IServiceProviderActions

		public IServiceProviderDataActions RegisterServices(int count) => throw failure;

		public IServiceProviderDataActions? Event(ActionsEventType type, ref ActionsContext context) => null;

	#endregion
	}

	private sealed class GuardedProvider(IServiceCollection services) : ServiceProvider(services)
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				throw new InvalidOperationException("The derived disposal hook must not run during base construction.");
			}

			base.Dispose(disposing);
		}
	}

	private sealed class Tracked(Exception? failure = null) : IDisposable
	{
		public int DisposeCount { get; private set; }

	#region Interface IDisposable

		public void Dispose()
		{
			DisposeCount ++;

			if (failure is not null)
			{
				throw failure;
			}
		}

	#endregion
	}

	private sealed class ThrowingCollection(Exception failure) : IServiceCollection
	{
	#region Interface IEnumerable

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion

	#region Interface IEnumerable<ServiceEntry>

		public IEnumerator<ServiceEntry> GetEnumerator() => throw failure;

	#endregion

	#region Interface IReadOnlyCollection<ServiceEntry>

		public int Count => 1;

	#endregion

	#region Interface IServiceCollection

		public void Add(ServiceEntry serviceEntry) => throw new NotSupportedException();

		public bool IsRegistered(TypeKey key) => false;

	#endregion
	}
}