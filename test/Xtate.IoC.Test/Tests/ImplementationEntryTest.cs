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

namespace Xtate.IoC.Test;

[TestClass]
public class ImplementationEntryTest
{
	[TestMethod]
	public async Task CustomInitRequiredServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant(CustomAsyncInit.Instance);
		sc.AddType<Class>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<Class>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task NullRequiredServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<Class>(null!);
		var sp = sc.BuildProvider();

		// Act

		// Assert
		await Assert.ThrowsExactlyAsync<MissedServiceException>([ExcludeFromCodeCoverage] async () => await sp.GetRequiredService<Class>());
	}

	[TestMethod]
	public async Task CustomInitOptionalServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant(CustomAsyncInit.Instance);
		sc.AddType<Class>();
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetService<Class>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task NullOptionalServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<Class>(null!);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetService<Class>();

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public void CustomInitRequiredSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant(CustomSyncInit.Instance);
		sc.AddTypeSync<Class>();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetRequiredSyncFactory<Class>()();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void NullRequiredSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<Class>(null!);
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<MissedServiceException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public void CustomInitOptionalSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant(CustomSyncInit.Instance);
		sc.AddTypeSync<Class>();
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetSyncFactory<Class>()();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void NullOptionalSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<Class>(null!);
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetSyncFactory<Class>()();

		// Assert
		Assert.IsNull(service);
	}

	[TestMethod]
	public async Task NullRequiredServiceArgTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<Class, int>((_, _) => null!);
		var sp = sc.BuildProvider();

		// Act

		// Assert
		await Assert.ThrowsExactlyAsync<MissedServiceException>([ExcludeFromCodeCoverage] async () => await sp.GetRequiredService<Class, int>(4));
	}

	[TestMethod]
	public void AsyncForCustomInitRequiredSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant(CustomAsyncInit.Instance);
		sc.AddTypeSync<Class>();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public void AsyncForRequiredSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<ClassAsyncInit>();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<ClassAsyncInit>()());
	}

	[TestMethod]
	public void AsyncForCustomInitOptionalSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant(CustomAsyncInit.Instance);
		sc.AddTypeSync<Class>();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetSyncFactory<Class>()());
	}

	[TestMethod]
	public void AsyncForOptionalSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<ClassAsyncInit>();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetSyncFactory<ClassAsyncInit>()());
	}

	[TestMethod]
	public async Task WrongAsyncDelegateTypeTest()
	{
		// Arrange
		var sc = new ServiceCollection
				 {
					 new ServiceEntry(TypeKey.ServiceKey<Class, ValueTuple>(), InstanceScope.Transient, [ExcludeFromCodeCoverage]() => 5)
				 };
		var sp = sc.BuildProvider();

		// Act

		// Assert
		await Assert.ThrowsExactlyAsync<InvalidOperationException>([ExcludeFromCodeCoverage] async () => await sp.GetRequiredService<Class>());
	}

	[TestMethod]
	public void WrongSyncDelegateTypeTest()
	{
		// Arrange
		var sc = new ServiceCollection
				 {
					 new ServiceEntry(TypeKey.ServiceKey<Class, ValueTuple>(), InstanceScope.Transient, [ExcludeFromCodeCoverage]() => 5)
				 };
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public void WrongSharedSyncDelegateTypeTest()
	{
		// Arrange
		var sc = new ServiceCollection
				 {
					 new ServiceEntry(TypeKey.ServiceKey<Class, ValueTuple>(), InstanceScope.Singleton, [ExcludeFromCodeCoverage]() => 5)
				 };
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public void IncompatibleSyncDelegateType1Test()
	{
		// Arrange
		var sc = new ServiceCollection
				 {
					 new ServiceEntry(
						 TypeKey.ServiceKey<Class, ValueTuple>(), InstanceScope.Forwarding, [ExcludeFromCodeCoverage](IServiceProvider sp, ValueTuple _) => new ValueTask<Class>(new Class()))
				 };
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public void IncompatibleSyncDelegateType2Test()
	{
		// Arrange
		var sc = new ServiceCollection
				 {
					 new ServiceEntry(TypeKey.ServiceKey<Class, ValueTuple>(), InstanceScope.Forwarding, [ExcludeFromCodeCoverage](IServiceProvider sp, Class _, ValueTuple _) => new Class())
				 };
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<MissedServiceException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public void IncompatibleSyncDelegateType3Test()
	{
		// Arrange
		var sc = new ServiceCollection
				 {
					 new ServiceEntry(
						 TypeKey.ServiceKey<Class, ValueTuple>(), InstanceScope.Forwarding, [ExcludeFromCodeCoverage](IServiceProvider sp, Class _, ValueTuple _) => new ValueTask<Class>(new Class()))
				 };
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public async Task MissedRegistrationRequiredServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		await Assert.ThrowsExactlyAsync<MissedServiceException>([ExcludeFromCodeCoverage] async () => await sp.GetRequiredService<Class>());
	}

	[TestMethod]
	public void MissedRegistrationRequiredSyncServiceTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<MissedServiceException>([ExcludeFromCodeCoverage]() => sp.GetRequiredServiceSync<Class>());
	}

	[TestMethod]
	public void MissedRegistrationRequiredFactoryTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<MissedServiceException>([ExcludeFromCodeCoverage]() => sp.GetRequiredFactory<Class>());
	}

	[TestMethod]
	public void DelegateCachingTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTypeSync<Class, int>();
		var sp = sc.BuildProvider();

		// Act
		var factory0A = sp.GetServicesFactory<Class, int>();
		var factory1A = sp.GetServicesSyncFactory<Class, int>();
		var factory2A = sp.GetRequiredFactory<Class, int>();
		var factory3A = sp.GetFactory<Class, int>();
		var factory4A = sp.GetRequiredSyncFactory<Class, int>();
		var factory5A = sp.GetSyncFactory<Class, int>();

		var factory0B = sp.GetServicesFactory<Class, int>();
		var factory1B = sp.GetServicesSyncFactory<Class, int>();
		var factory2B = sp.GetRequiredFactory<Class, int>();
		var factory3B = sp.GetFactory<Class, int>();
		var factory4B = sp.GetRequiredSyncFactory<Class, int>();
		var factory5B = sp.GetSyncFactory<Class, int>();

		// Assert
		Assert.AreSame(factory0A, factory0B);
		Assert.AreSame(factory1A, factory1B);
		Assert.AreSame(factory2A, factory2B);
		Assert.AreSame(factory3A, factory3B);
		Assert.AreSame(factory4A, factory4B);
		Assert.AreSame(factory5A, factory5B);
	}

	[TestMethod]
	public void ChainTest()
	{
		// Arrange
		ImplementationEntry node1 = new ForwardingImplementationEntry(null!, ChainTest);
		ImplementationEntry node2 = new ForwardingImplementationEntry(null!, ChainTest);
		node1.AddToChain(ref node2);
		IEnumerable<ImplementationEntry> chain1 = new ImplementationEntry.Chain(node1);
		IEnumerable chain2 = new ImplementationEntry.Chain(node1);

		// Act
		var list1 = chain1.ToList();
		var list2 = chain2.Cast<ImplementationEntry>().ToList();

		var list3 = new List<object?>();
		var list4 = new List<object?>();
		var enumerator = chain2.GetEnumerator();

		while (enumerator.MoveNext())
		{
			list3.Add(enumerator.Current);
		}

		enumerator.Reset();

		while (enumerator.MoveNext())
		{
			list4.Add(enumerator.Current);
		}

		((IDisposable) enumerator).Dispose();

		// Assert
		Assert.AreEqual(expected: 2, list1.Count);
		Assert.AreEqual(expected: 2, list2.Count);
		Assert.AreEqual(expected: 2, list3.Count);
		Assert.AreEqual(expected: 2, list4.Count);
	}

	[TestMethod]
	public async Task TransientImplementationEntryAsyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTransient(_ => new AsyncDisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<AsyncDisposableClass, ValueTuple>());
		var inst = await entry!.GetRequiredService<AsyncDisposableClass, ValueTuple>(default);
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.IsTrue(inst.Disposed);
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () => await entry.GetRequiredService<AsyncDisposableClass, ValueTuple>(default));
	}

	[TestMethod]
	public void TransientImplementationEntrySyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTransient(_ => new DisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<DisposableClass, ValueTuple>());
		var inst = entry!.GetRequiredServiceSyncDelegate<DisposableClass, ValueTuple, Func<ValueTuple, DisposableClass>>()(default);
		Disposer.Dispose(sp);

		// Assert
		Assert.IsTrue(inst.Disposed);
		Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage]() => entry.GetRequiredServiceSyncDelegate<DisposableClass, ValueTuple, Func<ValueTuple, DisposableClass>>()(default));
	}

	[TestMethod]
	public async Task SingletonImplementationEntryAsyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Container, _ => new DisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<DisposableClass, ValueTuple>());
		await Disposer.DisposeAsync(sp);

		// Assert
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () => await entry!.GetRequiredService<DisposableClass, ValueTuple>(default));
	}

	[TestMethod]
	public async Task SingletonImplementationEntryAsync2Test()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Container, _ => new DisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<DisposableClass, ValueTuple>());
		var inst = await entry!.GetRequiredService<DisposableClass, ValueTuple>(default);
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.IsTrue(inst.Disposed);
	}

	[TestMethod]
	public async Task SingletonImplementationEntryForResolveTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<ClassGeneric<Any>>(SharedWithin.Container);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<ClassGeneric<int>>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void SingletonImplementationEntrySyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<Class>(SharedWithin.Container);
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	[TestMethod]
	public async Task ScopedImplementationEntryAsyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Scope, _ => new DisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<DisposableClass, ValueTuple>());
		await Disposer.DisposeAsync(sp);

		// Assert
		await Assert.ThrowsExactlyAsync<ObjectDisposedException>([ExcludeFromCodeCoverage] async () => await entry!.GetRequiredService<DisposableClass, ValueTuple>(default));
	}

	[TestMethod]
	public async Task ScopedImplementationEntryAsync2Test()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Scope, _ => new DisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<DisposableClass, ValueTuple>());
		var inst = await entry!.GetRequiredService<DisposableClass, ValueTuple>(default);
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.IsTrue(inst.Disposed);
	}

	[TestMethod]
	public void ScopedImplementationEntrySyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared(SharedWithin.Scope, _ => new DisposableClass());
		var sp = sc.BuildProvider();

		// Act
		var entry = sp.GetImplementationEntry(TypeKey.ServiceKey<DisposableClass, ValueTuple>());
		var inst = entry!.GetRequiredServiceSync<DisposableClass, ValueTuple>(default);
		Disposer.Dispose(sp);

		// Assert
		Assert.IsTrue(inst.Disposed);
	}

	[TestMethod]
	public async Task ScopedImplementationEntryForResolveTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<ClassGeneric<Any>>(SharedWithin.Scope);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<ClassGeneric<int>>();

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task ScopedImplementationEntryWithArgTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<Class, int>(SharedWithin.Scope);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<Class, int>(4);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void ScopedImplementationEntrySyncWithArgTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedTypeSync<Class, int>(SharedWithin.Scope);
		var sp = sc.BuildProvider();

		// Act
		var service = sp.GetRequiredServiceSync<Class, int>(4);

		// Assert
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void InvalidScopedImplementationEntrySyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<Class>(SharedWithin.Scope);
		var sp = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => sp.GetRequiredSyncFactory<Class>()());
	}

	// ReSharper disable All
	public class Class { }

	public class ClassAsyncInit : IAsyncInitialization
	{
	#region Interface IAsyncInitialization

		[ExcludeFromCodeCoverage]
		public Task Initialization => Task.CompletedTask;

	#endregion
	}

	public class CustomAsyncInit : IInitializationHandler
	{
		public static readonly IInitializationHandler Instance = new CustomAsyncInit();

	#region Interface IInitializationHandler

		public bool Initialize<T>(T instance) => true;

		public Task InitializeAsync<T>(T instance) => Task.CompletedTask;

	#endregion
	}

	public class CustomSyncInit : IInitializationHandler
	{
		public static readonly IInitializationHandler Instance = new CustomSyncInit();

	#region Interface IInitializationHandler

		public bool Initialize<T>(T instance) => false;

		[ExcludeFromCodeCoverage]
		public Task InitializeAsync<T>(T instance) => Task.CompletedTask;

	#endregion
	}

	public class DisposableClass : IDisposable
	{
		public bool Disposed { get; private set; }

	#region Interface IDisposable

		public void Dispose()
		{
			Disposed = true;

			GC.SuppressFinalize(this);
		}

	#endregion
	}

	public class AsyncDisposableClass : IAsyncDisposable
	{
		public bool Disposed { get; private set; }

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Disposed = true;

			GC.SuppressFinalize(this);

			return default;
		}

	#endregion
	}

	public class ClassGeneric<T> { }

	// ReSharper restore All
}