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

using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class DisposerTest
{
	[TestMethod]
	public void Dispose_ShouldDisposeObject()
	{
		// Arrange
		var obj = new object();

		// Act
		Disposer.Dispose(obj);

		// Assert
		// Add assertions here if needed
	}

	[TestMethod]
	public void DisposeAsync_ShouldDisposeObjectAsync()
	{
		// Arrange
		var obj = new object();

		// Act
		_ = Disposer.DisposeAsync(obj).AsTask();

		// Assert
		// Add assertions here if needed
	}

	[TestMethod]
	public void IsDisposable_ShouldReturnFalse_ForNonDisposable()
	{
		// Arrange
		var obj = new object();

		// Act
		var result = Disposer.IsDisposable(obj);

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsDisposable_ShouldReturnTrue_ForIDisposable()
	{
		// Arrange
		var obj = new TestDisposable();

		// Act
		var result = Disposer.IsDisposable(obj);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsDisposable_ShouldReturnTrue_ForIAsyncDisposable()
	{
		// Arrange
		var obj = new TestAsyncDisposable();

		// Act
		var result = Disposer.IsDisposable(obj);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Dispose_ShouldInvokeIDisposableDispose()
	{
		// Arrange
		var obj = new TestDisposable();

		// Act
		Disposer.Dispose(obj);

		// Assert
		Assert.IsTrue(obj.Disposed);
	}

	[TestMethod]
	public void Dispose_ShouldInvokeIAsyncDisposableDisposeSynchronously_WhenCompleted()
	{
		// Arrange
		var obj = new TestAsyncDisposable();

		// Act
		Disposer.Dispose(obj);

		// Assert
		Assert.IsTrue(obj.Disposed);
	}

	[TestMethod]
	public async Task DisposeAsync_ShouldInvokeIAsyncDisposableDisposeAsync()
	{
		// Arrange
		var obj = new TestAsyncDisposable();

		// Act
		await Disposer.DisposeAsync(obj);

		// Assert
		Assert.IsTrue(obj.Disposed);
	}

	[TestMethod]
	public async Task DisposeAsync_ShouldInvokeIDisposableDispose_WhenNoAsync()
	{
		// Arrange
		var obj = new TestDisposable();

		// Act
		await Disposer.DisposeAsync(obj);

		// Assert
		Assert.IsTrue(obj.Disposed);
	}

	[TestMethod]
	public void Dispose_ShouldDoNothing_ForNonDisposable()
	{
		// Arrange
		var obj = new object();

		// Act
		Disposer.Dispose(obj);

		// Assert
		Assert.IsFalse(Disposer.IsDisposable(obj));
	}

	[TestMethod]
	public async Task DisposeAsync_ShouldComplete_ForNonDisposable()
	{
		// Arrange
		var obj = new object();

		// Act
		await Disposer.DisposeAsync(obj);

		// Assert
		Assert.IsFalse(Disposer.IsDisposable(obj));
	}

	[TestMethod]
	public void Dispose_ShouldThrow_ForFaultedIAsyncDisposable()
	{
		var obj = new FaultedAsyncDisposable();
		var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Disposer.Dispose(obj));
		Assert.AreEqual(expected: "Fault", ex.Message);
		Assert.IsTrue(obj.DisposedStarted);
	}

	[TestMethod]
	public void Dispose_ShouldThrow_ForCanceledIAsyncDisposable()
	{
		var obj = new CanceledAsyncDisposable();
		Assert.Throws<OperationCanceledException>(() => Disposer.Dispose(obj));
		Assert.IsTrue(obj.DisposedStarted);
	}

	[TestMethod]
	public async Task Dispose_ShouldNotAwaitIncompleteIAsyncDisposableDispose()
	{
		var obj = new DelayedAsyncDisposable();

		// ReSharper disable once MethodHasAsyncOverload
		Disposer.Dispose(obj);
		Assert.IsFalse(obj.Disposed);
		obj.Complete();
		await obj.DisposeCompletion;
		Assert.IsTrue(obj.Disposed);
	}

	private sealed class TestDisposable : IDisposable
	{
		public bool Disposed;

	#region Interface IDisposable

		public void Dispose()
		{
			Disposed = true;
		}

	#endregion
	}

	private sealed class TestAsyncDisposable : IAsyncDisposable
	{
		public bool Disposed;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Disposed = true;

			return ValueTask.CompletedTask;
		}

	#endregion
	}

	private sealed class FaultedAsyncDisposable : IAsyncDisposable
	{
		public bool DisposedStarted;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			DisposedStarted = true;

			return new ValueTask(Task.FromException(new InvalidOperationException("Fault")));
		}

	#endregion
	}

	private sealed class CanceledAsyncDisposable : IAsyncDisposable
	{
		public bool DisposedStarted;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			DisposedStarted = true;
			var cts = new CancellationTokenSource();
			cts.Cancel();

			return new ValueTask(Task.FromCanceled(cts.Token));
		}

	#endregion
	}

	private sealed class DelayedAsyncDisposable : IAsyncDisposable
	{
		private readonly TaskCompletionSource<bool> _tcs = new();

		private readonly TaskCompletionSource<bool> _tcs2 = new();

		public bool Disposed;

		public Task DisposeCompletion => _tcs2.Task;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			_ = Task.Run(async () =>
						 {
							 await _tcs.Task;
							 Disposed = true;
							 _tcs2.SetResult(true);
						 });

			return new ValueTask(DisposeCompletion);
		}

	#endregion

		public void Complete() => _tcs.TrySetResult(true);
	}
}