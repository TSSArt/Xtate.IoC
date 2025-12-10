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

namespace Xtate.IoC.Test;

[TestClass]
public class ObjectsBinTest
{
	[TestMethod]
	public async Task AddAsync_ShouldTrackDisposable_AndDisposeOnBinDisposeAsync()
	{
		// Arrange
		var bin = new ObjectsBin();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		// Act
		await bin.AddAsync(d1);
		await bin.AddAsync(d2);
		await bin.DisposeAsync();

		// Assert
		Assert.AreEqual(expected: 1, d1.DisposeCount);
		Assert.AreEqual(expected: 1, d2.DisposeCount);
	}

	[TestMethod]
	public async Task AddSync_ShouldTrackDisposable_AndDisposeOnBinDispose()
	{
		// Arrange
		var bin = new ObjectsBin();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		// Act
		bin.AddSync(d1);
		bin.AddSync(d2);
		await bin.DisposeAsync();

		// Assert
		Assert.AreEqual(expected: 1, d1.DisposeCount);
		Assert.AreEqual(expected: 1, d2.DisposeCount);
	}

	[TestMethod]
	public async Task AddAsync_NonDisposable_ShouldNotThrow_WhenNotDisposed()
	{
		// Arrange
		var bin = new ObjectsBin();
		var nd = new NonDisposable();

		// Act
		await bin.AddAsync(nd);
	}

	[TestMethod]
	public void AddSync_NonDisposable_ShouldNotThrow_WhenNotDisposed()
	{
		// Arrange
		var bin = new ObjectsBin();
		var nd = new NonDisposable();

		// Act
		bin.AddSync(nd);
	}

	[TestMethod]
	public async Task AddAsync_AfterDisposed_ShouldEagerlyDisposeAndThrow()
	{
		// Arrange
		var bin = new ObjectsBin();
		var d = new SyncDisposable();

		await bin.DisposeAsync();

		// Act
		try
		{
			await bin.AddAsync(d);
		}
		catch (ObjectDisposedException)
		{
			// Assert
			Assert.AreEqual(expected: 1, d.DisposeCount);
		}
	}

	[TestMethod]
	public async Task AddSync_AfterDisposed_ShouldEagerlyDisposeAndThrow()
	{
		// Arrange
		var bin = new ObjectsBin();
		var d = new AsyncDisposable();

		await bin.DisposeAsync();

		// Act
		try
		{
			await bin.AddAsync(d);
			Assert.Fail("Expected ObjectDisposedException was not thrown.");
		}
		catch (ObjectDisposedException)
		{
			// Assert
			Assert.AreEqual(expected: 1, d.DisposeCount);
		}
	}

	[TestMethod]
	public async Task DisposeAsync_IsIdempotent_WhenCalledMultipleTimes()
	{
		// Arrange
		var bin = new ObjectsBin();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		await bin.AddAsync(d1);
		await bin.AddAsync(d2);

		// Act
		await bin.DisposeAsync();
		await bin.DisposeAsync();

		// Assert
		Assert.AreEqual(expected: 1, d1.DisposeCount);
		Assert.AreEqual(expected: 1, d2.DisposeCount);
	}

	[TestMethod]
	public async Task Dispose_IsIdempotent_WhenCalledMultipleTimes()
	{
		// Arrange
		var bin = new ObjectsBin();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		bin.AddSync(d1);
		bin.AddSync(d2);

		// Act
		await bin.DisposeAsync();
		await bin.DisposeAsync();

		// Assert
		Assert.AreEqual(expected: 1, d1.DisposeCount);
		Assert.AreEqual(expected: 1, d2.DisposeCount);
	}

	[TestMethod]
	public async Task DisposeAsync_ShouldInvokeVirtualOverrides_InOrder_Once()
	{
		// Arrange
		var bin = new TestObjectsBin();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		await bin.AddAsync(d1);
		await bin.AddAsync(d2);

		// Act
		await bin.DisposeAsync();

		// Assert
		Assert.AreEqual(expected: 1, d1.DisposeCount);
		Assert.AreEqual(expected: 1, d2.DisposeCount);
		Assert.AreEqual(expected: 1, bin.DisposeAsyncCoreCallCount);
		Assert.AreEqual(expected: 1, bin.DisposeBoolCallCount);
		CollectionAssert.AreEqual(new List<string> { "DisposeAsyncCore", "Dispose(false)" }, bin.CallOrder);
	}

	[TestMethod]
	public void Dispose_ShouldInvokeVirtualDisposeBool_Once()
	{
		// Arrange
		var bin = new TestObjectsBin();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		bin.AddSync(d1);
		bin.AddSync(d2);

		// Act
		bin.Dispose();

		// Assert
		Assert.AreEqual(expected: 1, d1.DisposeCount);
		Assert.AreEqual(expected: 1, d2.DisposeCount);
		Assert.AreEqual(expected: 1, bin.DisposeBoolCallCount);
		CollectionAssert.AreEqual(new List<string> { "Dispose(true)" }, bin.CallOrder);
	}

	[TestMethod]
	public async Task DisposeAsync_OverrideWithoutBaseCall_ShouldStillAllowCustomBehavior()
	{
		// Arrange
		var bin = new TestObjectsBin_NoBase();
		var d1 = new SyncDisposable();
		var d2 = new AsyncDisposable();

		await bin.AddAsync(d1);
		await bin.AddAsync(d2);

		// Act
		await bin.DisposeAsync();

		// Assert
		// No base disposal, so tracked disposables remain undisposed.
		Assert.AreEqual(expected: 0, d1.DisposeCount);
		Assert.AreEqual(expected: 0, d2.DisposeCount);
		Assert.AreEqual(expected: 1, bin.DisposeAsyncCoreCallCount);
		Assert.AreEqual(expected: 1, bin.DisposeBoolCallCount);
		CollectionAssert.AreEqual(new List<string> { "DisposeAsyncCore", "Dispose(false)" }, bin.CallOrder);
	}

	private sealed class SyncDisposable : IDisposable
	{
		public int DisposeCount;

	#region Interface IDisposable

		public void Dispose() => DisposeCount ++;

	#endregion
	}

	private sealed class AsyncDisposable : IAsyncDisposable
	{
		public int DisposeCount;

	#region Interface IAsyncDisposable

		public async ValueTask DisposeAsync()
		{
			DisposeCount ++;

			await Task.Delay(1);
		}

	#endregion
	}

	private sealed class NonDisposable;

	private sealed class TestObjectsBin : ObjectsBin
	{
		public int DisposeAsyncCoreCallCount;

		public int DisposeBoolCallCount;

		public List<string> CallOrder { get; } = [];

		protected override ValueTask DisposeAsyncCore()
		{
			DisposeAsyncCoreCallCount ++;
			CallOrder.Add("DisposeAsyncCore");

			return base.DisposeAsyncCore();
		}

		protected override void Dispose(bool disposing)
		{
			DisposeBoolCallCount ++;
			CallOrder.Add(disposing ? "Dispose(true)" : "Dispose(false)");
			base.Dispose(disposing);
		}
	}

	private sealed class TestObjectsBin_NoBase : ObjectsBin
	{
		public int DisposeAsyncCoreCallCount;

		public int DisposeBoolCallCount;

		public List<string> CallOrder { get; } = [];

		protected override ValueTask DisposeAsyncCore()
		{
			DisposeAsyncCoreCallCount ++;
			CallOrder.Add("DisposeAsyncCore");

			// Skip base: simulate custom disposal strategy (no disposal of tracked instances).
			return ValueTask.CompletedTask;
		}

		protected override void Dispose(bool disposing)
		{
			DisposeBoolCallCount ++;
			CallOrder.Add(disposing ? "Dispose(true)" : "Dispose(false)");

			// Skip base: simulate custom finalization logic.
		}
	}
}