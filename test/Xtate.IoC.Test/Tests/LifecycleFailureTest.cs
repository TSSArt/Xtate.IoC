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

using System.Linq;
using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class LifecycleFailureTest
{
	[TestMethod]
	[DataRow(false, false, false)]
	[DataRow(false, false, true)]
	[DataRow(false, true, false)]
	[DataRow(false, true, true)]
	[DataRow(true, false, false)]
	[DataRow(true, false, true)]
	[DataRow(true, true, false)]
	[DataRow(true, true, true)]
	public async Task FailedInitialization_CompletesEveryWaiter_WithoutPublishingValue(bool result, bool pending, bool canceled)
	{
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();
		Exception failure = canceled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("initializer");
		var completion = NewCompletion<int>();
		var calls = 0;
		var initializer = new Initializer(
			result, () =>
					{
						Interlocked.Increment(ref calls);

						return new ValueTask<int>(completion.Task);
					});

		if (!pending)
		{
			completion.SetException(failure);
		}

		var waiters = Enumerable.Range(start: 0, count: 8).Select(_ => Observe(initializer.InitializeAsync)).ToArray();

		if (pending)
		{
			Assert.IsTrue(waiters.All(task => !task.IsCompleted));
			completion.SetException(failure);
		}

		foreach (var waiter in waiters)
		{
			Assert.AreSame(failure, await Bounded(waiter));
		}

		Assert.AreSame(failure, await Bounded(Observe(initializer.InitializeAsync)));
		Assert.AreEqual(expected: 1, calls);
		Assert.ThrowsExactly<InvalidOperationException>(initializer.CheckValue);

		if (canceled)
		{
			Assert.AreEqual(cancellation.Token, ((OperationCanceledException)failure).CancellationToken);
			Assert.IsTrue(initializer.InitializeAsync().AsTask().IsCanceled);
		}
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task SynchronouslyThrowingInitializer_CachesOriginalFailure(bool result)
	{
		var failure = new InvalidOperationException("synchronous initializer");
		var calls = 0;
		var initializer = new Initializer(
			result, () =>
					{
						calls ++;

						throw failure;
					});
		Assert.AreSame(failure, await Bounded(Observe(initializer.InitializeAsync)));
		Assert.AreSame(failure, await Bounded(Observe(initializer.InitializeAsync)));
		Assert.AreEqual(expected: 1, calls);
		Assert.ThrowsExactly<InvalidOperationException>(initializer.CheckValue);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task AlreadyCanceledInitializer_CachesCancellationToken(bool result)
	{
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();

		// The callback completes before the enclosing resource scope ends.
		// ReSharper disable once AccessToDisposedClosure
		var initializer = new Initializer(result, () => new ValueTask<int>(Task.FromCanceled<int>(cancellation.Token)));
		var first = await Bounded(Observe(initializer.InitializeAsync));
		var second = await Bounded(Observe(initializer.InitializeAsync));
		Assert.IsInstanceOfType<OperationCanceledException>(first);
		Assert.AreSame(first, second);
		Assert.AreEqual(cancellation.Token, ((OperationCanceledException)second).CancellationToken);
		Assert.ThrowsExactly<InvalidOperationException>(initializer.CheckValue);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task ConcurrentFailedInitialization_InvokesCallbackOnce(bool result)
	{
		var start = NewCompletion<bool>();
		var entered = NewCompletion<bool>();
		var completion = NewCompletion<int>();
		var remaining = 16;
		var calls = 0;
		var failure = new InvalidOperationException("concurrent initializer");
		var initializer = new Initializer(
			result, () =>
					{
						Interlocked.Increment(ref calls);

						return new ValueTask<int>(completion.Task);
					});
		var waiters = Enumerable.Range(start: 0, remaining)
								.Select(_ => Task.Run(async () =>
													  {
														  await start.Task.ConfigureAwait(false);
														  var observation = Observe(initializer.InitializeAsync);

														  if (Interlocked.Decrement(ref remaining) == 0)
														  {
															  entered.SetResult(true);
														  }

														  return await observation.ConfigureAwait(false);
													  }))
								.ToArray();
		start.SetResult(true);
		await Bounded(entered.Task);
		completion.SetException(failure);
		var errors = await Bounded(Task.WhenAll(waiters));
		Assert.AreEqual(expected: 1, calls);
		Assert.IsTrue(errors.All(error => ReferenceEquals(error, failure)));
	}

	[TestMethod]
	public async Task PendingAsyncCleanupFailure_StopsFurtherDisposal()
	{
		var bin = new ObjectsBin();
		var started = NewCompletion<bool>();
		var release = NewCompletion<bool>();
		var failure = new InvalidOperationException("pending cleanup");
		var instances = Enumerable.Range(start: 0, count: 3)
								  .Select(_ => new AsyncDisposable(async () =>
																   {
																	   started.TrySetResult(true);
																	   await release.Task.ConfigureAwait(false);

																	   throw failure;
																   }))
								  .ToArray();

		foreach (var instance in instances)
		{
			bin.AddSync(instance);
		}

		var observation = Observe(bin.DisposeAsync);

		try
		{
			await Bounded(started.Task);
			Assert.IsFalse(observation.IsCompleted);
			Assert.AreEqual(expected: 1, instances.Sum(instance => instance.Count));
		}
		finally
		{
			release.TrySetResult(true);
		}

		Assert.AreSame(failure, await Bounded(observation));
		Assert.AreEqual(expected: 1, instances.Sum(instance => instance.Count));
		await bin.DisposeAsync();
		Assert.AreEqual(expected: 1, instances.Sum(instance => instance.Count));
		GC.KeepAlive(instances);
	}

	[TestMethod]
	public async Task DisposalCallback_CanRunBinOperationOnAnotherThread()
	{
		var bin = new ObjectsBin();
		var late = new Disposable();
		var owned = new AsyncDisposable(async () =>
										{
											await Bounded(Task.Run(() => Assert.ThrowsExactly<ObjectDisposedException>(() => bin.AddSync(late))));
										});
		bin.AddSync(owned);
		await Bounded(bin.DisposeAsync().AsTask());
		Assert.AreEqual(expected: 1, late.Count);
		GC.KeepAlive(owned);
	}

	[TestMethod]
	public async Task ConcurrentProviderCleanup_KeepsSharedDependencyAliveUntilScopedCleanupFinishes()
	{
		var parent = new ServiceProvider(new ServiceCollection());
		var child = new ServiceProviderScope(parent);
		var shared = new Disposable();
		parent.SharedObjectsBin.AddSync(shared);
		var started = NewCompletion<bool>();
		var release = NewCompletion<bool>();
		var scoped = new AsyncDisposable(async () =>
										 {
											 started.SetResult(true);
											 await release.Task.ConfigureAwait(false);
											 Assert.AreEqual(expected: 0, shared.Count, message: "Scoped cleanup still owns its shared dependency.");
										 });
		parent.ObjectsBin.AddSync(scoped);
		var parentCleanup = parent.DisposeAsync().AsTask();
		await Bounded(started.Task);

		try
		{
			await child.DisposeAsync();
			Assert.AreEqual(expected: 0, shared.Count);
		}
		finally
		{
			release.SetResult(true);
			await Bounded(parentCleanup);
		}

		Assert.AreEqual(expected: 1, shared.Count);
		GC.KeepAlive(scoped);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task BinCleanup_StopsOnFirstFailure_WithoutRetrying(bool asynchronous)
	{
		var bin = new ObjectsBin();
		var failure = new InvalidOperationException("cleanup failed");
		var instances = new[] { new Disposable(failure), new Disposable(failure), new Disposable(failure) };

		foreach (var instance in instances)
		{
			bin.AddSync(instance);
		}

		var error = asynchronous
			? await Observe(bin.DisposeAsync)
			: Capture(bin.Dispose);
		Assert.AreSame(failure, error);
		Assert.AreEqual(expected: 1, instances.Sum(instance => instance.Count));
		await bin.DisposeAsync();

		// This case deliberately exercises synchronous disposal.
		// ReSharper disable once MethodHasAsyncOverload
		bin.Dispose();
		Assert.AreEqual(expected: 1, instances.Sum(instance => instance.Count));
		GC.KeepAlive(instances);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task BinCleanup_WithOneFailure_PreservesExceptionAndStack(bool asynchronous)
	{
		var failure = new InvalidOperationException("cleanup");
		var bin = new ObjectsBin();
		var instance = new Disposable(failure);
		bin.AddSync(instance);
		var error = asynchronous ? await Observe(bin.DisposeAsync) : Capture(bin.Dispose);
		Assert.AreSame(failure, error);
		StringAssert.Contains(error.StackTrace!, nameof(Disposable.Dispose));
		GC.KeepAlive(instance);
	}

	[TestMethod]
	public async Task AdditionAfterDrainStarts_IsCleanedAndRejected_WithoutWaitingForWinner()
	{
		var bin = new ObjectsBin();
		var started = NewCompletion<bool>();
		var release = NewCompletion<bool>();
		var owned = new AsyncDisposable(async () =>
										{
											started.SetResult(true);
											await release.Task.ConfigureAwait(false);
										});
		bin.AddSync(owned);
		var winner = bin.DisposeAsync().AsTask();
		await Bounded(started.Task);

		try
		{
			await Bounded(bin.DisposeAsync().AsTask());

			// This case deliberately exercises synchronous disposal.
			// ReSharper disable once MethodHasAsyncOverload
			bin.Dispose();
			var late = new Disposable();
			Assert.IsInstanceOfType<ObjectDisposedException>(await Observe(() => bin.AddAsync(late)));
			Assert.AreEqual(expected: 1, late.Count);
			Assert.IsFalse(winner.IsCompleted);
		}
		finally
		{
			release.SetResult(true);
			await Bounded(winner);
		}

		Assert.AreEqual(expected: 1, owned.Count);
		GC.KeepAlive(owned);
	}

	[TestMethod]
	public async Task ConcurrentAddAndDispose_DoesNotDoubleDispose()
	{
		for (var iteration = 0; iteration < 32; iteration ++)
		{
			var bin = new ObjectsBin();
			var instance = new Disposable();
			var start = NewCompletion<bool>();
			var add = Task.Run(async () =>
							   {
								   await start.Task.ConfigureAwait(false);

								   try { bin.AddSync(instance); }
								   catch (ObjectDisposedException) { }
							   });
			var dispose = Task.Run(async () =>
								   {
									   await start.Task.ConfigureAwait(false);
									   await bin.DisposeAsync().ConfigureAwait(false);
								   });
			start.SetResult(true);
			await Bounded(Task.WhenAll(add, dispose));
			Assert.IsTrue(instance.Count is 0 or 1, message: "An in-flight addition may miss disposal, but cannot be disposed twice.");
			GC.KeepAlive(instance);
		}
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task ProviderCleanup_DisposesScopedObjectsBeforeSharedDependencies(bool asynchronous)
	{
		var provider = new ServiceProvider(new ServiceCollection());
		var shared = new Disposable();
		var scoped = new DependencyAwareDisposable(shared);
		provider.SharedObjectsBin.AddSync(shared);
		provider.ObjectsBin.AddSync(scoped);

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

		Assert.IsTrue(scoped.IsDisposed);
		Assert.AreEqual(expected: 1, shared.Count);
		GC.KeepAlive(scoped);
	}

	[TestMethod]
	public async Task FinalizationPath_ReleasesSharedReferenceWithoutDisposingObjects()
	{
		var parent = new ServiceProvider(new ServiceCollection());
		var child = new FinalizableScope(parent);
		var shared = new Disposable();
		var scoped = new Disposable();
		parent.SharedObjectsBin.AddSync(shared);
		child.ObjectsBin.AddSync(scoped);

		child.ReleaseForFinalization();
		Assert.AreEqual(expected: 0, scoped.Count);
		Assert.AreEqual(expected: 0, shared.Count);

		await parent.DisposeAsync();
		Assert.AreEqual(expected: 1, shared.Count);
		Assert.AreEqual(expected: 0, scoped.Count);
		GC.KeepAlive(scoped);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task FailedAsyncScopeCleanup_ReleasesSharedReference(bool lastReference)
	{
		var parent = new ServiceProvider(new ServiceCollection());
		var child = new ServiceProviderScope(parent);
		var shared = new Disposable();
		parent.SharedObjectsBin.AddSync(shared);
		var failure = new InvalidOperationException("scoped cleanup");
		var completion = NewCompletion<bool>();
		var scoped = new AsyncDisposable(() => new ValueTask(completion.Task));
		child.ObjectsBin.AddSync(scoped);

		var disposal = child.DisposeAsync().AsTask();
		Assert.IsFalse(disposal.IsCompleted);

		if (lastReference)
		{
			await parent.DisposeAsync();
		}

		Assert.AreEqual(expected: 0, shared.Count);

		completion.SetException(failure);
		Assert.AreSame(failure, await Bounded(Observe(() => new ValueTask(disposal))));
		Assert.AreEqual(expected: 1, scoped.Count);
		Assert.AreEqual(lastReference ? 1 : 0, shared.Count);

		await child.DisposeAsync();
		await parent.DisposeAsync();
		Assert.AreEqual(expected: 1, shared.Count);
		GC.KeepAlive(scoped);
		GC.KeepAlive(shared);
	}

	[TestMethod]
	public async Task ExistingChild_OutlivesParent_AndReleasesLastSharedReferenceDespiteFailure()
	{
		var services = new ServiceCollection();
		var shared = new Disposable();
		services.AddShared(SharedWithin.Container, _ => shared);
		var parent = new ServiceProvider(services);
		var child = new ServiceProviderScope(parent);
		await parent.DisposeAsync();
		Assert.AreSame(shared, await child.GetRequiredService<Disposable>());
		Assert.AreEqual(expected: 0, shared.Count);
		var failure = new InvalidOperationException("child cleanup");
		var scoped = new Disposable(failure);
		child.ObjectsBin.AddSync(scoped);
		Assert.AreSame(failure, await Observe(child.DisposeAsync));
		Assert.AreEqual(expected: 1, shared.Count);
		Assert.AreEqual(expected: 1, scoped.Count);
		GC.KeepAlive(scoped);
	}

	[TestMethod]
	[DataRow(InstanceScope.Transient)]
	[DataRow(InstanceScope.Scoped)]
	[DataRow(InstanceScope.Singleton)]
	[DataRow(InstanceScope.Forwarding)]
	[DataRow(InstanceScope.ScopedExternal)]
	[DataRow(InstanceScope.SingletonExternal)]
	public async Task FailedInitialization_PreservesOwnershipUntilProviderCleanup(InstanceScope scope)
	{
		var failure = new InvalidOperationException("initialization");
		var instance = new FailedService(failure);
		var services = new ServiceCollection();
		services.AddEntry(TypeKey.ServiceKey<FailedService, ValueTuple>(), scope, new Func<IServiceProvider, ValueTuple, FailedService>((_, _) => instance));
		var provider = new ServiceProvider(services);
		Assert.AreSame(failure, await Observe(async () => { await provider.GetRequiredService<FailedService>(); }));
		Assert.AreEqual(expected: 0, instance.Count);
		await provider.DisposeAsync();
		Assert.AreEqual(scope is InstanceScope.Transient or InstanceScope.Scoped or InstanceScope.Singleton ? 1 : 0, instance.Count);
		GC.KeepAlive(instance);
	}

	[TestMethod]
	public async Task InFlightFactoryFinishingAfterDisposal_CleansAndRejectsOwnedInstance()
	{
		var completion = NewCompletion<Disposable>();
		var services = new ServiceCollection();
		services.AddTransient(_ => new ValueTask<Disposable>(completion.Task));
		var provider = new ServiceProvider(services);
		var request = Observe(async () => { await provider.GetRequiredService<Disposable>(); });
		await provider.DisposeAsync();
		var instance = new Disposable();
		completion.SetResult(instance);
		Assert.IsInstanceOfType<ObjectDisposedException>(await Bounded(request));
		Assert.AreEqual(expected: 1, instance.Count);
	}

	[TestMethod]
	public async Task CachedSyncAndAsyncFactories_RejectDisposedProvider()
	{
		var services = new ServiceCollection();
		services.AddTransient<object>(_ => new object());
		var provider = new ServiceProvider(services);
		var sync = provider.GetRequiredSyncFactory<object>();
		var asyncFactory = provider.GetRequiredFactory<object>();
		await provider.DisposeAsync();
		Assert.ThrowsExactly<ObjectDisposedException>(sync);
		Assert.IsInstanceOfType<ObjectDisposedException>(await Observe(async () => { await asyncFactory(); }));
	}

	private static TaskCompletionSource<T> NewCompletion<T>() => new(TaskCreationOptions.RunContinuationsAsynchronously);

	private static async Task Bounded(Task task)
	{
		using var timeout = new CancellationTokenSource();
		Assert.AreSame(task, await Task.WhenAny(task, Task.Delay(millisecondsDelay: 10000, timeout.Token)), message: "Operation did not terminate.");
		await timeout.CancelAsync();
		await task;
	}

	private static async Task<T> Bounded<T>(Task<T> task)
	{
		await Bounded((Task)task);

		return await task;
	}

	private static async Task<Exception> Observe(Func<ValueTask> operation)
	{
		try { await operation(); }
		catch (Exception exception) { return exception; }

		Assert.Fail("Expected an exception.");

		throw new InvalidOperationException();
	}

	private static Exception Capture(Action operation)
	{
		try { operation(); }
		catch (Exception exception) { return exception; }

		Assert.Fail("Expected an exception.");

		throw new InvalidOperationException();
	}

	private sealed class Initializer : IAsyncInitialization
	{
		private readonly AsyncInit<Initializer>? _noValue;

		private readonly AsyncInit<Initializer, int>? _value;

		public Initializer(bool result, Func<ValueTask<int>> initialize)
		{
			if (result) { _value = new AsyncInit<Initializer, int>(_ => initialize()); }
			else { _noValue = new AsyncInit<Initializer>(_ => Discard(initialize())); }
		}

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => _value is { } value ? AsyncInit.For(this).Run(value) : AsyncInit.For(this).Run(_noValue!);

	#endregion

		private static async ValueTask Discard(ValueTask<int> value) => await value.ConfigureAwait(false);

		public void CheckValue()
		{
			if (_value is { } value) { _ = value.Value; }
			else { _noValue!.EnsureInitialized(); }
		}
	}

	private class Disposable(Exception? failure = null) : IDisposable
	{
		public int Count;

	#region Interface IDisposable

		public void Dispose()
		{
			Interlocked.Increment(ref Count);

			if (failure is not null) throw failure;
		}

	#endregion
	}

	private sealed class AsyncDisposable(Func<ValueTask> dispose) : IAsyncDisposable
	{
		public int Count;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Interlocked.Increment(ref Count);

			return dispose();
		}

	#endregion
	}

	private sealed class DependencyAwareDisposable(Disposable shared) : IDisposable
	{
		public bool IsDisposed { get; private set; }

	#region Interface IDisposable

		public void Dispose()
		{
			Assert.AreEqual(expected: 0, shared.Count, message: "Scoped cleanup must finish before its shared dependency is disposed.");
			IsDisposed = true;
		}

	#endregion
	}

	private sealed class FinalizableScope(ServiceProvider parent) : ServiceProvider(parent)
	{
		public void ReleaseForFinalization() => Dispose(false);
	}

	private sealed class FailedService(Exception failure) : Disposable, IAsyncInitialization
	{
	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => new(Task.FromException(failure));

	#endregion
	}
}