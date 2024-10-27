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

using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class WeakReferenceCollectionTest
{
	[ExcludeFromCodeCoverage]
	private static bool IsGcCollectsAll => !RuntimeInformation.FrameworkDescription.StartsWith(value: "Mono", StringComparison.OrdinalIgnoreCase);

	[TestMethod]
	public void WeakReferenceCollection_ShouldStoreAndRetrieveAllObjects()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();
		var objects = Enumerable.Range(start: 0, count: 16).Select(_ => CreateObject()).ToList();

		// Act
		foreach (var o in objects)
		{
			wrc.Put(o);
		}

		while (wrc.TryTake(out var obj))
		{
			objects.Remove(obj);
		}

		// Assert
		Assert.AreEqual(expected: 0, objects.Count);
	}

	private static void AddObjects(WeakReferenceCollection wrc, int count)
	{
		for (var i = 0; i < count; i ++)
		{
			wrc.Put(CreateObject());
		}
	}

	private static object CreateObject() => new();

	private static void GcCollect()
	{
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		GC.WaitForPendingFinalizers();
	}

	[DataTestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(8)]
	[DataRow(16)]
	public void WeakReferenceCollection_ShouldCollectAllObjects_WhenGcCollectsAll(int n)
	{
		if (!IsGcCollectsAll)
		{
			return;
		}

		// Arrange
		var wrc = new WeakReferenceCollection();
		AddObjects(wrc, n);

		// Act
		GcCollect();
		var result = wrc.TryTake(out _);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void WeakReferenceCollection_ShouldCollectSomeObjects_WhenGcCollectsAll()
	{
		if (!IsGcCollectsAll)
		{
			return;
		}

		// Arrange
		var wrc = new WeakReferenceCollection();
		var list = new object[8];
		FillList(wrc, list, count: 8);

		list[0] = null!;
		list[1] = null!;
		list[4] = null!;
		list[5] = null!;
		list[7] = null!;

		// Act
		GcCollect();
		AddObjects(wrc, count: 1);

		var count = 0;
		while (wrc.TryTake(out _))
		{
			count ++;
		}

		// Assert
		Assert.AreEqual(expected: 4, count);
	}

	private static void FillList(WeakReferenceCollection wrc, object[] list, int count)
	{
		for (var i = 0; i < count; i ++)
		{
			list[i] = CreateObject();
			wrc.Put(list[i]);
		}
	}

	[TestMethod]
	public void WeakReferenceCollection_ShouldHandleMultiThreadedAccess()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();

		var thread1 = new Thread(o => Put((WeakReferenceCollection) o!));
		var thread2 = new Thread(o => Put((WeakReferenceCollection) o!));
		var thread3 = new Thread(o => Take((WeakReferenceCollection) o!));
		var thread4 = new Thread(o => Take((WeakReferenceCollection) o!));

		// Act
		thread1.Start(wrc);
		thread2.Start(wrc);
		thread3.Start(wrc);
		thread4.Start(wrc);

		thread1.Join();
		thread2.Join();
		thread3.Join();
		thread4.Join();

		// Assert
		while (wrc.TryTake(out _)) { }
	}

	private static void Put(WeakReferenceCollection wrc)
	{
		for (var i = 0; i < 10000; i ++)
		{
			wrc.Put(new object());
		}
	}

	private static void Take(WeakReferenceCollection wrc)
	{
		while (wrc.TryTake(out _)) { }
	}

	[TestMethod]
	public void Put_ShouldAddObjectToCollection()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();
		var obj = new object();

		// Act
		wrc.Put(obj);

		// Assert
		Assert.IsTrue(wrc.TryTake(out var retrievedObj));
		Assert.AreSame(obj, retrievedObj);
	}

	[TestMethod]
	public void Put_ShouldNotAddNullObjectToCollection()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();

		// Act
		wrc.Put(null!);

		// Assert
		Assert.IsFalse(wrc.TryTake(out _));
	}

	[TestMethod]
	public void TryTake_ShouldReturnFalseWhenCollectionIsEmpty()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();

		// Act
		var result = wrc.TryTake(out var obj);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNull(obj);
	}

	[TestMethod]
	public void TryTake_ShouldReturnTrueAndRemoveObjectFromCollection()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();
		var obj = new object();
		wrc.Put(obj);

		// Act
		var result = wrc.TryTake(out var retrievedObj);

		// Assert
		Assert.IsTrue(result);
		Assert.AreSame(obj, retrievedObj);
		Assert.IsFalse(wrc.TryTake(out _));
	}

	[TestMethod]
	public void WeakReferenceCollection_ShouldHandleConcurrentAccess()
	{
		// Arrange
		var wrc = new WeakReferenceCollection();
		var obj1 = new object();
		var obj2 = new object();

		// Act
		var thread1 = new Thread(() => wrc.Put(obj1));
		var thread2 = new Thread(() => wrc.Put(obj2));
		thread1.Start();
		thread2.Start();
		thread1.Join();
		thread2.Join();

		// Assert
		var retrievedObjects = new List<object>();
		while (wrc.TryTake(out var obj))
		{
			retrievedObjects.Add(obj);
		}

		CollectionAssert.Contains(retrievedObjects, obj1);
		CollectionAssert.Contains(retrievedObjects, obj2);
	}
}