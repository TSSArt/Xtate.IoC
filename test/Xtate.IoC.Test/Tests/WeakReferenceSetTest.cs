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

using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class WeakReferenceSetTest
{
	[ExcludeFromCodeCoverage]
	private static bool IsGcCollectsAll => !RuntimeInformation.FrameworkDescription.StartsWith(value: "Mono", StringComparison.OrdinalIgnoreCase);

	[TestMethod]
	public void WeakReferenceStack_ShouldStoreAndRetrieveAllObjects()
	{
		// Arrange
		var wrs = new WeakReferenceSet();
		var objects = Enumerable.Range(start: 0, count: 16).Select(_ => CreateObject()).ToList();

		// Act
		foreach (var o in objects)
		{
			wrs.Add(o);
		}

		while (wrs.TryTake(out var obj))
		{
			objects.Remove(obj);
		}

		// Assert
		Assert.IsEmpty(objects);
	}

	private static void AddObjects(WeakReferenceSet wrs, int count)
	{
		for (var i = 0; i < count; i ++)
		{
			wrs.Add(CreateObject());
		}
	}

	private static object CreateObject() => new();

	private static void GcCollect()
	{
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		GC.WaitForPendingFinalizers();
	}

	[TestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(8)]
	[DataRow(16)]
	public void WeakReferenceStack_ShouldCollectAllObjects_WhenGcCollectsAll(int n)
	{
		if (!IsGcCollectsAll)
		{
			return;
		}

		// Arrange
		var wrs = new WeakReferenceSet();
		AddObjects(wrs, n);

		// Act
		GcCollect();
		var result = wrs.TryTake(out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void WeakReferenceStack_ShouldCollectSomeObjects_WhenGcCollectsAll()
	{
		if (!IsGcCollectsAll)
		{
			return;
		}

		// Arrange
		var wrs = new WeakReferenceSet();
		var list = new object[8];
		FillList(wrs, list, count: 8);

		list[0] = null!;
		list[1] = null!;
		list[4] = null!;
		list[5] = null!;
		list[7] = null!;

		// Act
		GcCollect();
		AddObjects(wrs, count: 1);

		var count = 0;

		while (wrs.TryTake(out _))
		{
			count ++;
		}

		// Assert
		Assert.AreEqual(expected: 4, count);
		Assert.HasCount(expected: 8, list);
	}

	private static void FillList(WeakReferenceSet wrs, object[] list, int count)
	{
		for (var i = 0; i < count; i ++)
		{
			list[i] = CreateObject();
			wrs.Add(list[i]);
		}
	}

	[TestMethod]
	public void WeakReferenceStack_ShouldHandleMultiThreadedAccess()
	{
		// Arrange
		var wrs = new WeakReferenceSet();

		var thread1 = new Thread(o => Put((WeakReferenceSet) o!));
		var thread2 = new Thread(o => Put((WeakReferenceSet) o!));
		var thread3 = new Thread(o => Take((WeakReferenceSet) o!));
		var thread4 = new Thread(o => Take((WeakReferenceSet) o!));

		// Act
		thread1.Start(wrs);
		thread2.Start(wrs);
		thread3.Start(wrs);
		thread4.Start(wrs);

		thread1.Join();
		thread2.Join();
		thread3.Join();
		thread4.Join();

		// Assert
		while (wrs.TryTake(out _)) { }
	}

	private static void Put(WeakReferenceSet wrs)
	{
		for (var i = 0; i < 10000; i ++)
		{
			wrs.Add(new object());
		}
	}

	private static void Take(WeakReferenceSet wrs)
	{
		while (wrs.TryTake(out _)) { }
	}

	[TestMethod]
	public void Put_ShouldAddObjectToCollection()
	{
		// Arrange
		var wrs = new WeakReferenceSet();
		var obj = new object();

		// Act
		wrs.Add(obj);

		// Assert
		Assert.IsTrue(wrs.TryTake(out var retrievedObj));
		Assert.AreSame(obj, retrievedObj);
	}

	[TestMethod]
	public void Put_ShouldNotAddNullObjectToCollection()
	{
		// Arrange
		var wrs = new WeakReferenceSet();

		// Act
		wrs.Add(null!);

		// Assert
		Assert.IsFalse(wrs.TryTake(out _));
	}

	[TestMethod]
	public void TryTake_ShouldReturnFalseWhenCollectionIsEmpty()
	{
		// Arrange
		var wrs = new WeakReferenceSet();

		// Act
		var result = wrs.TryTake(out var obj);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNull(obj);
	}

	[TestMethod]
	public void TryTake_ShouldReturnTrueAndRemoveObjectFromCollection()
	{
		// Arrange
		var wrs = new WeakReferenceSet();
		var obj = new object();
		wrs.Add(obj);

		// Act
		var result = wrs.TryTake(out var retrievedObj);

		// Assert
		Assert.IsTrue(result);
		Assert.AreSame(obj, retrievedObj);
		Assert.IsFalse(wrs.TryTake(out _));
	}

	[TestMethod]
	public void WeakReferenceStack_ShouldHandleConcurrentAccess()
	{
		// Arrange
		var wrs = new WeakReferenceSet();
		var obj1 = new object();
		var obj2 = new object();

		// Act
		var thread1 = new Thread(() => wrs.Add(obj1));
		var thread2 = new Thread(() => wrs.Add(obj2));
		thread1.Start();
		thread2.Start();
		thread1.Join();
		thread2.Join();

		// Assert
		var retrievedObjects = new List<object>();

		while (wrs.TryTake(out var obj))
		{
			retrievedObjects.Add(obj);
		}

		CollectionAssert.Contains(retrievedObjects, obj1);
		CollectionAssert.Contains(retrievedObjects, obj2);
	}

	[TestMethod]
	public void Dispose_ShouldFreeHandles_AndSubsequentOperationsAreSafe()
	{
		// Arrange
		var wrs = new WeakReferenceSet();
		AddObjects(wrs, count: 32);

		// Act
		wrs.Dispose();

		// Assert
		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<ObjectDisposedException>(() => wrs.TryTake(out _));
		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<ObjectDisposedException>(() => wrs.Add(null!));

		// Further Act/Assert: calling Dispose again should be safe
		wrs.Dispose();

		Assert.ThrowsExactly<ObjectDisposedException>(() => wrs.TryTake(out _));
		Assert.ThrowsExactly<ObjectDisposedException>(() => wrs.Add(null!));
	}
		
	[TestMethod]
	public void Cleaner_Finalization_ShouldTriggerCompaction_AndPreserveLiveItems()
	{
		if (!IsGcCollectsAll)
		{
			return;
		}

		// Arrange
		var wrs = new WeakReferenceSet();
		var list = new object[10];
		FillList(wrs, list, count: 10);

		// drop references to some objects to make them collectible
		list[2] = null!;
		list[3] = null!;
		list[6] = null!;
		list[9] = null!;

		// Act
		GcCollect(); // should collect some targets and run Cleaner to compact
		AddObjects(wrs, count: 2); // ensure cleaner exists and activity continues

		var liveCount = 0;
		while (wrs.TryTake(out _))
		{
			liveCount ++;
		}

		// 10 initial - 4 collected + 2 added = expected 8 remaining
		Assert.AreEqual(expected: 8, liveCount);
		Assert.HasCount(expected: 10, list);
	}

	[TestMethod]
	public void Cleaner_ShouldCompactLargeBuffer_With256Items_PreservingLiveOnes()
	{
		if (!IsGcCollectsAll)
		{
			return;
		}

		// Arrange
		var wrs = new WeakReferenceSet();
		var list = new object[256];

		FillList(wrs, list, count: 256);

		// Drop references for half of the items to make them collectible
		for (var i = 0; i < 256; i += 2)
		{
			list[i] = null!;
		}

		// Act
		GcCollect(); // should collect ~128 targets and run Cleaner to compact (threshold is 32)
		AddObjects(wrs, count: 4); // keep activity and ensure cleaner remains

		var liveCount = 0;
		while (wrs.TryTake(out _))
		{
			liveCount ++;
		}

		// Assert
		// 256 initial - 128 collected (approx) + 4 added = 132 expected remaining
		// Under "GC collects all" setting used in tests, collected ones should be gone deterministically.
		Assert.AreEqual(expected: 132, liveCount);
		Assert.HasCount(expected: 256, list);
	}
}