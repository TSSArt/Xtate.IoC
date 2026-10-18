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

using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceProviderLookupLifetimeTest
{
	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task DisposedProvider_ImplementationEntryLookupRemainsAvailable(bool asynchronous)
	{
		var service = new object();
		var services = new ServiceCollection();
		services.AddConstant(service);
		services.AddForwarding<object, int>((_, _) => service);
		services.AddSharedTypeSync<GenericService<Any>>(SharedWithin.Container);
		var provider = new ServiceProvider(services);
		var key = TypeKey.ServiceKey<object, ValueTuple>();
		var entry = provider.GetImplementationEntry(key);
		Assert.IsNotNull(entry);

		if (asynchronous)
		{
			await provider.DisposeAsync();
		}
		else
		{
			// This case deliberately exercises synchronous disposal.
			// ReSharper disable once MethodHasAsyncOverload
			provider.Dispose();
		}

		Assert.AreSame(entry, provider.GetImplementationEntry(key));
		Assert.IsNull(provider.GetImplementationEntry(TypeKey.ServiceKey<string, ValueTuple>()));
		Assert.IsNotNull(provider.GetImplementationEntry(TypeKey.ServiceKey<object, int>()));
		Assert.IsNotNull(provider.GetImplementationEntry(TypeKey.ServiceKey<GenericService<int>, ValueTuple>()));
	}

	[TestMethod]
	public async Task AsyncServiceResolution_AfterDisposal_StillRejectsRegisteredService()
	{
		var services = new ServiceCollection();
		services.AddConstant(new object());
		var provider = new ServiceProvider(services);
		await provider.DisposeAsync();

		await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () => { await provider.GetService<object>(); });
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () =>
																 {
																	 await using var enumeration = provider.GetServices<object>().GetAsyncEnumerator();
																	 await enumeration.MoveNextAsync();
																 });
	}

	[TestMethod]
	public async Task ImplementationEntryLookup_WhileDisposalIsPending_RemainsAvailable()
	{
		var services = new ServiceCollection();
		services.AddConstant(new object());
		var provider = new ServiceProvider(services);
		var key = TypeKey.ServiceKey<object, ValueTuple>();
		var entry = provider.GetImplementationEntry(key);
		Assert.IsNotNull(entry);
		var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var owned = new PendingDisposable(started, release.Task);
		provider.ObjectsBin.AddSync(owned);
		var disposal = provider.DisposeAsync().AsTask();

		try
		{
			await Bounded(started.Task);
			Assert.IsFalse(disposal.IsCompleted);
			Assert.AreSame(entry, provider.GetImplementationEntry(key));
			Assert.IsNull(provider.GetImplementationEntry(TypeKey.ServiceKey<string, ValueTuple>()));
		}
		finally
		{
			release.TrySetResult(true);
			await Bounded(disposal);
		}

		GC.KeepAlive(owned);
	}

	[TestMethod]
	public async Task ExistingChild_CanResolveInheritedGenericRegistrationsAfterParentDisposal()
	{
		var services = new ServiceCollection();
		services.AddSharedTypeSync<GenericService<Any>>(SharedWithin.Container);
		var parent = new ServiceProvider(services);
		await using var child = new ServiceProviderScope(parent);
		await parent.DisposeAsync();

		var instance = child.GetRequiredServiceSync<GenericService<int>>();
		var factory = child.GetRequiredServiceSync<IServiceScopeFactory>();
		await using var grandchild = factory.CreateScope(additional =>
														 {
															 additional.AddSharedTypeSync<GenericService<Any>>(SharedWithin.Scope, Option.IfNotRegistered);
														 });

		Assert.AreSame(instance, grandchild.ServiceProvider.GetRequiredServiceSync<GenericService<int>>());
		Assert.IsNotNull(child.GetRequiredServiceSync<GenericService<string>>());
	}

	private static async Task Bounded(Task operation)
	{
		using var timeout = new CancellationTokenSource();
		Assert.AreSame(operation, await Task.WhenAny(operation, Task.Delay(millisecondsDelay: 10000, timeout.Token)), message: "Operation did not terminate.");
		await timeout.CancelAsync();
		await operation;
	}

	private sealed class PendingDisposable(TaskCompletionSource<bool> started, Task release) : IAsyncDisposable
	{
	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			started.SetResult(true);

			return new ValueTask(release);
		}

	#endregion
	}

	// Generic parameters distinguish service keys and reflection-matching test cases.
	// ReSharper disable once UnusedTypeParameter
	[UsedImplicitly] // Constructed through container reflection.
	private sealed class GenericService<T>;
}