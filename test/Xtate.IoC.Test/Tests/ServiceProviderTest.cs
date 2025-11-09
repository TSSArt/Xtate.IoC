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

public static class AsyncEnumExt
{
	public static int Count<T>(this IAsyncEnumerable<T> en)
	{
		return InternalCount().Result;

		async Task<int> InternalCount()
		{
			var count = 0;

			await foreach (var _ in en)
			{
				count ++;
			}

			return count;
		}
	}
}

[TestClass]
public class ServiceProviderTest
{
	[TestMethod]
	public void IsRegistered_Should_Return_True_When_Service_Is_Registered()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) sp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsTrue(isRegistered);
	}

	[TestMethod]
	public void IsRegistered_Should_Return_False_When_Service_Is_Not_Registered()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) sp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsFalse(isRegistered);
	}

	[TestMethod]
	public void IsRegistered_Should_Return_True_When_Service_Is_Registered_In_SourceProvider()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) sp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsTrue(isRegistered);
	}

	[TestMethod]
	public void IsRegistered_Should_Return_True_When_Service_Is_Registered_In_ParentProvider()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var childSp = sp.GetRequiredServiceSync<IServiceScopeFactory>().CreateScope().ServiceProvider;
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) childSp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsTrue(isRegistered);
	}

	[TestMethod]
	public void SourceServiceCollection_Should_Register_Service()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) sp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsTrue(isRegistered);
	}

	[TestMethod]
	public void SourceServiceCollection_Should_Not_Register_Service()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) sp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsFalse(isRegistered);
	}

	[TestMethod]
	public void SourceServiceCollection_Should_Register_Service_From_SourceProvider()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) sp);

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsTrue(isRegistered);
	}

	[TestMethod]
	public void SourceServiceCollection_Should_Register_Service_From_Parent_Provider()
	{
		// Arrange
		var sc = new ServiceCollection();
		var sp = sc.BuildProvider();
		var childSp = sp.GetRequiredServiceSync<IServiceScopeFactory>().CreateScope().ServiceProvider;
		var sourceServiceCollection = new ServiceProvider.SourceServiceCollection((ServiceProvider) childSp);
		sourceServiceCollection.AddType<Class>();

		// Act
		var isRegistered = ((IServiceCollection) sourceServiceCollection).IsRegistered(TypeKey.ServiceKeyFast<Class, ValueTuple>());

		// Assert
		Assert.IsTrue(isRegistered);
	}

	[TestMethod]
	public async Task Should_Create_New_Scope()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<Class>();
		var sp = sc.BuildProvider();
		var ssf = await sp.GetRequiredService<IServiceScopeFactory>();
		var sp2 = ssf.CreateScope(sc2 =>
								  {
									  sc2.AddType<Class>();
									  sc2.AddType<Class2>();
									  sc2.AddType<Class2>();
								  })
					 .ServiceProvider;

		// Act
		var s1 = await sp2.GetRequiredService<Class>();
		var s2 = await sp2.GetRequiredService<Class2>();

		// Assert
		Assert.IsNotNull(s1);
		Assert.IsNotNull(s2);
	}

	[TestMethod]
	public async Task Should_Self_Register()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddTransient<object>(sp => sp);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<object>();
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.AreSame(service, sp);
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task Should_Self_Register_Singleton()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared<object>(SharedWithin.Container, sp => sp);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<object>();
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.AreSame(service, sp);
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public async Task Should_Self_Register_Scope()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddShared<object>(SharedWithin.Scope, sp => sp);
		var sp = sc.BuildProvider();

		// Act
		var service = await sp.GetRequiredService<object>();
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.AreSame(service, sp);
		Assert.IsNotNull(service);
	}

	[TestMethod]
	public void Should_Throw_Exception_For_Incorrect_Type()
	{
		// Arrange
		var sc = new ServiceCollection();

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => sc.AddShared<object>((SharedWithin) (-99), [ExcludeFromCodeCoverage](sp) => sp));
	}

	[TestMethod]
	public async Task Should_Handle_Multiple_Generics()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<GenericClass<int>>();
		sc.AddType<GenericClass<byte>>();
		sc.AddType<GenericClass<long>>();
		var sp = sc.BuildProvider();

		// Act
		var service1 = await sp.GetRequiredService<GenericClass<int>>();
		var service2 = await sp.GetRequiredService<GenericClass<byte>>();
		var service3 = await sp.GetRequiredService<GenericClass<long>>();

		// Assert
		Assert.IsNotNull(service1);
		Assert.IsNotNull(service2);
		Assert.IsNotNull(service3);
	}

	[TestMethod]
	public async Task Should_Handle_Empty_Init_Async()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(null!);
		sc.AddType<Class>();
		var sp = sc.BuildProvider();

		// Act
		var service1 = await sp.GetRequiredService<Class>();

		// Assert
		Assert.IsNotNull(service1);
	}

	[TestMethod]
	public async Task Should_Propagate_Scope_For_Generics()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddType<GenericClass<int>>();
		sc.AddType<GenericClass<long>>();
		var sp = sc.BuildProvider();
		var ssf = await sp.GetRequiredService<IServiceScopeFactory>();
		var sp2 = ssf.CreateScope(sc2 =>
								  {
									  sc2.AddType<GenericClass<int>>();
									  sc2.AddType<GenericClass<long>>();
								  })
					 .ServiceProvider;

		// Act
		var s1 = sp2.GetServices<GenericClass<int>>().Count();
		var s2 = sp2.GetServices<GenericClass<long>>().Count();

		// Assert
		Assert.AreEqual(expected: 2, s1);
		Assert.AreEqual(expected: 2, s2);
	}

	[TestMethod]
	public void Should_Throw_Exception_For_Wrong_Instance_Scope()
	{
		// Arrange
		var sc = new ServiceCollection { new ServiceEntry(TypeKey.ServiceKey<int, int>(), (InstanceScope) 456456456, Factory) };

		// Act & Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => sc.BuildProvider());

		return;

		[ExcludeFromCodeCoverage]
		static int Factory() => 33;
	}

	[TestMethod]
	public void Should_Dispose_Singleton_Sync()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedTypeSync<DisposableClass>(SharedWithin.Container);
		var sp = sc.BuildProvider();
		var service = sp.GetRequiredServiceSync<DisposableClass>();

		// Act
		Disposer.Dispose(sp);

		// Assert
		Assert.IsTrue(service.Disposed);
	}

	[TestMethod]
	public async Task Should_Dispose_Singleton_Async()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddSharedType<DisposableClass>(SharedWithin.Container);
		var sp = sc.BuildProvider();
		var service = await sp.GetRequiredService<DisposableClass>();

		// Act
		await Disposer.DisposeAsync(sp);

		// Assert
		Assert.IsTrue(service.Disposed);
	}

	// ReSharper disable All
	private class Class { }

	private class Class2 { }

	public class GenericClass<T> { }

	public class DisposableClass : IDisposable
	{
		public bool Disposed;

	#region Interface IDisposable

		public void Dispose()
		{
			Disposed = true;
			GC.SuppressFinalize(this);
		}

	#endregion
	}

	// ReSharper restore All
}