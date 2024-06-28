﻿// Copyright © 2019-2024 Sergii Artemenko
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
public class WeakReferenceCollectionTest
{
	[TestMethod]
	public void BasicTest()
	{
		var wrc = new WeakReferenceCollection();
		var objects = Enumerable.Range(start: 0, count: 16).Select(_ => new object()).ToList();

		foreach (var o in objects)
		{
			wrc.Put(o);
		}

		while (wrc.TryTake(out var obj))
		{
			objects.Remove(obj);
		}

		Assert.AreEqual(expected: 0, objects.Count);
	}

	private static void AddObjects(WeakReferenceCollection wrc, int count)
	{
		for (var i = 0; i < count; i ++)
		{
			wrc.Put(new object());
		}
	}

	private static void PurgeUntil(WeakReferenceCollection wrc, int count)
	{
		var i = 0;

		while (wrc.Purge() != count)
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
			GC.AddMemoryPressure(20 * 1048576);
			GC.WaitForPendingFinalizers();

			if (i ++ == 1000)
			{
				Assert.Fail($"Collection can't be purged. Still {wrc.Purge()} elements are present");
			}
		}
	}

	[DataTestMethod]
	[DataRow(0)]
	[DataRow(1)]
	[DataRow(8)]
	[DataRow(16)]
	public void CollectAllTest(int n)
	{
		var wrc = new WeakReferenceCollection();

		AddObjects(wrc, n);

		PurgeUntil(wrc, 0);
		
		var result = wrc.TryTake(out _);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CollectSomeTest()
	{
		var wrc = new WeakReferenceCollection();
		var list = new List<object>();

		FillList(wrc, list, 8);

		NewMethod(list);

		PurgeUntil(wrc, 3);

		AddObjects(wrc, 1);

		var count = 0;
		while (wrc.TryTake(out _))
		{
			count ++;
		}

		Assert.AreEqual(4, count);
	}

	private static void NewMethod(List<object> list)
	{
		list[0] = null!;
		list[1] = null!;
		list[4] = null!;
		list[5] = null!;
		list[7] = null!;
	}

	private static void FillList(WeakReferenceCollection wrc, List<object> list, int count)
	{
		for (var i = 0; i < count; i ++)
		{
			var item = new object();
			wrc.Put(item);
			list.Add(item);
		}
	}

	[TestMethod]
	public void IfPutNullShouldRaiseExceptionTest()
	{
		var wrc = new WeakReferenceCollection();

		Assert.ThrowsException<ArgumentNullException>([ExcludeFromCodeCoverage]() => wrc.Put(default!));
	}

	[TestMethod]
	public void MultiThreadTest()
	{
		var wrc = new WeakReferenceCollection();

		var thread1 = new Thread(o => Put((WeakReferenceCollection) o!));
		var thread2 = new Thread(o => Put((WeakReferenceCollection) o!));
		var thread3 = new Thread(o => Take((WeakReferenceCollection) o!));
		var thread4 = new Thread(o => Take((WeakReferenceCollection) o!));

		thread1.Start(wrc);
		thread2.Start(wrc);
		thread3.Start(wrc);
		thread4.Start(wrc);

		thread1.Join();
		thread2.Join();
		thread3.Join();
		thread4.Join();

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
}