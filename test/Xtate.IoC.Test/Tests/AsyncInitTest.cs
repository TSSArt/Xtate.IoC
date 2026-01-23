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
public class AsyncInitTest
{
	[TestMethod]
	public async Task InitializeAsync_ShouldSetValue()
	{
		// Arrange
		var instance = new AsyncInitClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: 3.14, instance.ValB);
	}

	[TestMethod]
	public async Task DerivedClass_InitializeAsync_ShouldSetBaseAndDerivedValues()
	{
		// Arrange
		var instance = new DerivedAsyncInitClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: 3.14, instance.ValB);
		Assert.AreEqual(expected: 3.15, instance.ValD);
	}

	[TestMethod]
	public void ThrowIfNotInitialized_ShouldThrow()
	{
		// Arrange
		var instance = new AsyncInitClass();

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>(() => _ = instance.ValB);
	}

	[TestMethod]
	public void EnsureInitialized_ShouldThrow()
	{
		// Arrange
		var instance = new ClassWithEnsureInitialized();

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>(() => instance.EnsureInit());
	}

	[TestMethod]
	public async Task EnsureInitialized_AfterInit_ShouldNotThrow()
	{
		// Arrange
		var instance = new ClassWithEnsureInitialized();

		// Act
		await instance.InitializeAsync();

		// Assert
		instance.EnsureInit();
	}

	[TestMethod]
	public async Task ConcurrentInitialization_ShouldInitializeOnce()
	{
		// Arrange
		var instance = new CounterAsyncInitClass();
		var tasks = new Task[10];

		// Act
		for (var i = 0; i < tasks.Length; i ++)
		{
			tasks[i] = Task.Run(async () => await instance.InitializeAsync(), CancellationToken.None);
		}

		await Task.WhenAll(tasks);

		// Assert
		Assert.AreEqual(expected: 1, instance.InitCount);
	}

	[TestMethod]
	public async Task MultipleAsyncInit_ShouldRunAllInOrder()
	{
		// Arrange
		var instance = new MultipleAsyncInitClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: "12345", instance.Result);
	}

	[TestMethod]
	public async Task AsyncInitWithNonCompletedTask_ShouldComplete()
	{
		// Arrange
		var instance = new DelayedAsyncInitClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: 42, instance.Value);
	}

	[TestMethod]
	public async Task AsyncInitWithResult_ImplicitConversion_ShouldWork()
	{
		// Arrange
		var instance = new AsyncInitClass();

		// Act
		await instance.InitializeAsync();
		var result = instance.ValB;

		// Assert
		Assert.AreEqual(expected: 3.14, result);
	}

	[TestMethod]
	public void BuilderWithoutBaseInit_ForDerivedClass_ShouldThrow()
	{
		// Arrange
		var instance = new InvalidDerivedClass();

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>(() => instance.InitializeAsync().AsTask());
	}

	[TestMethod]
	public async Task BuilderWithBaseInit_NotCompleted_ShouldAwait()
	{
		// Arrange
		var instance = new DerivedWithDelayedBase();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: 3.14, instance.ValB);
		Assert.AreEqual(expected: 100, instance.ValD);
	}

	[TestMethod]
	public async Task MultipleRunners_MoreThan4_ShouldGrowArray()
	{
		// Arrange
		var instance = new ManyInitializersClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: "0123456789", instance.Result);
	}

	[TestMethod]
	public async Task AsyncInitWithResultAndDelay_ShouldCompleteCorrectly()
	{
		// Arrange
		var instance = new DelayedResultAsyncInitClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: 999, instance.Val);
	}

	[TestMethod]
	public async Task AsyncInitNoValueClass_ShouldWork()
	{
		// Arrange
		var instance = new AsyncInitNoValueClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		instance.Ensure();
	}

	[TestMethod]
	public async Task SyncInitValueClass_ShouldWork()
	{
		// Arrange
		var instance = new SyncInitValueClass();

		// Act
		await instance.InitializeAsync();

		// Assert
		Assert.AreEqual(expected: 42, instance.Value);
	}

	private class AsyncInitClass : IAsyncInitialization
	{
		private readonly AsyncInit<AsyncInitClass, double> _init = new(async _ =>
																	   {
																		   await Task.Delay(1);

																		   return 3.14;
																	   });

		public double ValB => _init;

	#region Interface IAsyncInitialization

		public virtual ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion
	}

	private class DerivedAsyncInitClass : AsyncInitClass
	{
		private readonly AsyncInit<DerivedAsyncInitClass, double> _init = new(async _ =>
																			  {
																				  await Task.Delay(1);

																				  return 3.15;
																			  });

		public double ValD => _init;

		public override ValueTask InitializeAsync() => AsyncInit.For(this, base.InitializeAsync()).Run(_init);
	}

	private class ClassWithEnsureInitialized : IAsyncInitialization
	{
		private readonly AsyncInit<ClassWithEnsureInitialized> _init = new(_ => ValueTask.CompletedTask);

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion

		public void EnsureInit() => _init.EnsureInitialized();
	}

	private class CounterAsyncInitClass : IAsyncInitialization
	{
		private readonly AsyncInit<CounterAsyncInitClass, int> _init;

		private int _initCount;

		public CounterAsyncInitClass()
		{
			_init = new AsyncInit<CounterAsyncInitClass, int>(async _ =>
															  {
																  await Task.Delay(10);

																  return Interlocked.Increment(ref _initCount);
															  });
		}

		public int InitCount => _initCount;

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion
	}

	private class MultipleAsyncInitClass : IAsyncInitialization
	{
		private readonly AsyncInit<MultipleAsyncInitClass> _init1;

		private readonly AsyncInit<MultipleAsyncInitClass> _init2;

		private readonly AsyncInit<MultipleAsyncInitClass> _init3;

		private readonly AsyncInit<MultipleAsyncInitClass> _init4;

		private readonly AsyncInit<MultipleAsyncInitClass> _init5;

		public MultipleAsyncInitClass()
		{
			_init1 = new AsyncInit<MultipleAsyncInitClass>(_ =>
														   {
															   Result += "1";

															   return ValueTask.CompletedTask;
														   });
			_init2 = new AsyncInit<MultipleAsyncInitClass>(_ =>
														   {
															   Result += "2";

															   return ValueTask.CompletedTask;
														   });
			_init3 = new AsyncInit<MultipleAsyncInitClass>(_ =>
														   {
															   Result += "3";

															   return ValueTask.CompletedTask;
														   });
			_init4 = new AsyncInit<MultipleAsyncInitClass>(_ =>
														   {
															   Result += "4";

															   return ValueTask.CompletedTask;
														   });
			_init5 = new AsyncInit<MultipleAsyncInitClass>(_ =>
														   {
															   Result += "5";

															   return ValueTask.CompletedTask;
														   });
		}

		public string Result { get; private set; } = string.Empty;

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() =>
			AsyncInit.For(this)
					 .Run(_init1)
					 .Run(_init2)
					 .Run(_init3)
					 .Run(_init4)
					 .Run(_init5);

	#endregion
	}

	private class DelayedAsyncInitClass : IAsyncInitialization
	{
		private readonly AsyncInit<DelayedAsyncInitClass, int> _init = new(async _ =>
																		   {
																			   await Task.Delay(5);

																			   return 42;
																		   });

		public int Value => _init;

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion
	}

	private class BaseAsyncInitClass : IAsyncInitialization
	{
		private readonly AsyncInit<BaseAsyncInitClass, double> _init = new(async _ =>
																		   {
																			   await Task.Delay(1);

																			   return 3.14;
																		   });

		public double ValB => _init;

	#region Interface IAsyncInitialization

		public virtual ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion
	}

	private class InvalidDerivedClass : BaseAsyncInitClass
	{
		private readonly AsyncInit<InvalidDerivedClass> _init = new(_ => ValueTask.CompletedTask);

		public override ValueTask InitializeAsync() =>

			// This should throw because base class implements IAsyncInitialization,
			// but we're not passing base.InitializeAsync()
			AsyncInit.For(this).Run(_init);
	}

	private class DerivedWithDelayedBase : BaseAsyncInitClass
	{
		private readonly AsyncInit<DerivedWithDelayedBase, int> _init = new(async _ =>
																			{
																				await Task.Delay(5);

																				return 100;
																			});

		public int ValD => _init;

		public override ValueTask InitializeAsync() => AsyncInit.For(this, base.InitializeAsync()).Run(_init);
	}

	private class ManyInitializersClass : IAsyncInitialization
	{
		private readonly AsyncInit<ManyInitializersClass> _init0;

		private readonly AsyncInit<ManyInitializersClass> _init1;

		private readonly AsyncInit<ManyInitializersClass> _init2;

		private readonly AsyncInit<ManyInitializersClass> _init3;

		private readonly AsyncInit<ManyInitializersClass> _init4;

		private readonly AsyncInit<ManyInitializersClass> _init5;

		private readonly AsyncInit<ManyInitializersClass> _init6;

		private readonly AsyncInit<ManyInitializersClass> _init7;

		private readonly AsyncInit<ManyInitializersClass> _init8;

		private readonly AsyncInit<ManyInitializersClass> _init9;

		public ManyInitializersClass()
		{
			_init0 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "0";

															  return ValueTask.CompletedTask;
														  });
			_init1 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "1";

															  return ValueTask.CompletedTask;
														  });
			_init2 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "2";

															  return ValueTask.CompletedTask;
														  });
			_init3 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "3";

															  return ValueTask.CompletedTask;
														  });
			_init4 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "4";

															  return ValueTask.CompletedTask;
														  });
			_init5 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "5";

															  return ValueTask.CompletedTask;
														  });
			_init6 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "6";

															  return ValueTask.CompletedTask;
														  });
			_init7 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "7";

															  return ValueTask.CompletedTask;
														  });
			_init8 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "8";

															  return ValueTask.CompletedTask;
														  });
			_init9 = new AsyncInit<ManyInitializersClass>(_ =>
														  {
															  Result += "9";

															  return ValueTask.CompletedTask;
														  });
		}

		public string Result { get; private set; } = string.Empty;

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() =>
			AsyncInit.For(this)
					 .Run(_init0)
					 .Run(_init1)
					 .Run(_init2)
					 .Run(_init3)
					 .Run(_init4)
					 .Run(_init5)
					 .Run(_init6)
					 .Run(_init7)
					 .Run(_init8)
					 .Run(_init9);

	#endregion
	}

	private class DelayedResultAsyncInitClass : IAsyncInitialization
	{
		private readonly AsyncInit<DelayedResultAsyncInitClass, int> _init = new(async _ =>
																				 {
																					 await Task.Delay(10);

																					 return 999;
																				 });

		public int Val => _init;

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion
	}

	private class AsyncInitNoValueClass : IAsyncInitialization
	{
		private readonly AsyncInit<AsyncInitNoValueClass> _init = new(async _ => { await Task.Delay(10); });

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion

		public void Ensure() => _init.EnsureInitialized();
	}

	private class SyncInitValueClass : IAsyncInitialization
	{
		private readonly AsyncInit<SyncInitValueClass, int> _init = new(_ => new ValueTask<int>(Task.FromResult(42)));

		public int Value => _init.Value;

	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_init);

	#endregion
	}
}