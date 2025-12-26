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

using System.Runtime.InteropServices;

namespace Xtate.IoC.Test;

[TestClass]
public class WeakGCHandleTest
{
	[TestMethod]
	public void TryGetTarget_ShouldReturnTrue_ForAliveTarget()
	{
		// Arrange
		var obj = new object();
		var handle = CreateWeakHandle(obj);

		// Act
		var result = handle.TryGetTarget(out var target);

		// Assert
		Assert.IsTrue(result);
		Assert.AreSame(obj, target);
	}

	[TestMethod]
	public void TryGetTarget_ShouldReturnFalse_WhenTargetCollected()
	{
		// Arrange
		var handle = CreateWeakHandle(CreateCollectibleObject());

		// Force GC
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		// Act
		var result = handle.TryGetTarget(out var target);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNull(target);
	}

	[TestMethod]
	public void SetTarget_ShouldReplaceTarget()
	{
		// Arrange
		var obj1 = new object();
		var obj2 = new object();
		var handle = CreateWeakHandle(obj1);

		// Act
		handle.SetTarget(obj2);

		// Assert
		Assert.IsTrue(handle.TryGetTarget(out var target));
		Assert.AreSame(obj2, target);
	}

	[TestMethod]
	public void Dispose_ShouldBeIdempotent_AndNotThrow()
	{
		// Arrange
		var obj = new object();
		var handle = CreateWeakHandle(obj);

		// Act & Assert
		handle.Dispose(); // first dispose
		handle.Dispose(); // second dispose
	}

	[TestMethod]
	public void Dispose_ShouldCatch_InvalidOperationException_WhenFreeingInvalidHandle()
	{
		// Arrange
		var obj = new object();
		var handle = CreateWeakHandle(obj);

		// Dispose the valid handle first
		handle.Dispose();

		// Act & Assert: Dispose should not throw (it catches InvalidOperationException)
		handle.Dispose();
	}

	private static object CreateCollectibleObject() => new();

	private static WeakGCHandle<object> CreateWeakHandle(object target) => new(target);
}