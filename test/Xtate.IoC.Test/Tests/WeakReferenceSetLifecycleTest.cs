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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class WeakReferenceSetLifecycleTest
{
	[TestMethod]
	public void CompactionAfterDisposal_DoesNotRecreateCleanerOrHandles()
	{
		var set = new WeakReferenceSet();
		var instance = new object();
		set.Add(instance);
		var clean = GetClean(set);

		set.Dispose();
		clean();
		set.Dispose();

		AssertReleased(set);
		Assert.IsFalse(set.TryTake(out _));
		Assert.ThrowsExactly<ObjectDisposedException>(() => set.Add(instance));
		GC.KeepAlive(instance);
	}

	[TestMethod]
	public async Task ConcurrentCompactionAndRetrieval_PreserveEveryLiveInstance()
	{
		using var set = new WeakReferenceSet();
		var instances = Enumerable.Range(start: 0, count: 512).Select(_ => new object()).ToArray();

		foreach (var instance in instances)
		{
			set.Add(instance);
		}

		var clean = GetClean(set);
		var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var maintenance = Task.Run(async () =>
								   {
									   await start.Task.ConfigureAwait(false);

									   for (var pass = 0; pass < 32; pass ++)
									   {
										   clean();
									   }
								   });
		var retrieval = Task.Run(async () =>
								 {
									 await start.Task.ConfigureAwait(false);
									 var retrieved = new HashSet<object>();

									 // The callback completes before the enclosing resource scope ends.
									 // ReSharper disable once AccessToDisposedClosure
									 while (set.TryTake(out var instance))
									 {
										 Assert.IsTrue(retrieved.Add(instance), message: "An instance was retrieved twice.");
									 }

									 return retrieved;
								 });

		start.SetResult(true);
		await Bounded(Task.WhenAll(maintenance, retrieval));

		CollectionAssert.AreEquivalent(instances, (await retrieval).ToArray());
		GC.KeepAlive(instances);
	}

	[TestMethod]
	public async Task AddTakeCompactionAndDisposal_LeaveNoHandlesInEitherQueue()
	{
		for (var iteration = 0; iteration < 32; iteration ++)
		{
			using var set = new WeakReferenceSet();
			var instances = Enumerable.Range(start: 0, count: 128).Select(_ => new object()).ToArray();

			foreach (var instance in instances)
			{
				set.Add(instance);
			}

			var clean = GetClean(set);
			var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			var operations = new[]
							 {
								 () =>
								 {
									 foreach (var instance in instances)
									 {
										 try
										 {
											 // The callback completes before the enclosing resource scope ends.
											 // ReSharper disable once AccessToDisposedClosure
											 set.Add(instance);
										 }
										 catch (ObjectDisposedException)
										 {
											 // An addition that loses the disposal race is rejected.
										 }
									 }
								 },
								 () =>
								 {
									 for (var pass = 0; pass < 128; pass ++)
									 {
										 // The callback completes before the enclosing resource scope ends.
										 // ReSharper disable once AccessToDisposedClosure
										 set.TryTake(out _);
									 }
								 },
								 () =>
								 {
									 for (var pass = 0; pass < 16; pass ++)
									 {
										 clean();
									 }
								 },
								 set.Dispose,
								 set.Dispose
							 };
			var tasks = operations.Select(operation => Task.Run(async () =>
																{
																	await start.Task.ConfigureAwait(false);
																	operation();
																}))
								  .ToArray();

			start.SetResult(true);
			await Bounded(Task.WhenAll(tasks));

			AssertReleased(set);
			Assert.IsFalse(set.TryTake(out _));

			// The callback completes before the enclosing resource scope ends.
			// ReSharper disable once AccessToDisposedClosure
			Assert.ThrowsExactly<ObjectDisposedException>(() => set.Add(new object()));
			GC.KeepAlive(instances);
		}
	}

	private static Action GetClean(WeakReferenceSet set) =>
		(Action)Delegate.CreateDelegate(typeof(Action), set, typeof(WeakReferenceSet).GetMethod(name: "Clean", BindingFlags.Instance | BindingFlags.NonPublic)!);

	private static void AssertReleased(WeakReferenceSet set)
	{
		// Inspect resource ownership without adding instrumentation to the production implementation.
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		var type = typeof(WeakReferenceSet);
		var active = (ICollection)type.GetField(name: "_handles", flags)!.GetValue(set)!;
		var recycled = (ICollection)type.GetField(name: "_freeHandles", flags)!.GetValue(set)!;
		var cleaner = (GCHandle)type.GetField(name: "_cleanerHandle", flags)!.GetValue(set)!;
		Assert.AreEqual(expected: 0, active.Count, message: "Active handles remained after disposal.");
		Assert.AreEqual(expected: 0, recycled.Count, message: "Recycled handles remained after disposal.");
		Assert.IsFalse(cleaner.IsAllocated, message: "The cleaner handle was recreated after disposal.");
	}

	private static async Task Bounded(Task operation)
	{
		using var timeout = new CancellationTokenSource();
		Assert.AreSame(operation, await Task.WhenAny(operation, Task.Delay(millisecondsDelay: 10000, timeout.Token)), message: "Concurrent operations did not terminate.");
		await timeout.CancelAsync();
		await operation;
	}
}