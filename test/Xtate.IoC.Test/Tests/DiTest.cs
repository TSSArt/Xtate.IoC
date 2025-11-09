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

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedVariable
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable NotAccessedField.Global

namespace Xtate.IoC.Test;

[TestClass]
public class DiTest
{
	[TestMethod]
	public async Task CreateEmptyClass_ShouldCreateNewInstances()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class1>();
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var class1InstanceA = await serviceProvider.GetRequiredService<Class1>();
		var class1InstanceB = await serviceProvider.GetRequiredService<Class1>();

		// Assert
		Assert.IsNotNull(class1InstanceA);
		Assert.IsNotNull(class1InstanceB);
		Assert.AreNotSame(class1InstanceA, class1InstanceB);
	}

	[TestMethod]
	public async Task CreateClassWithDependency_ShouldCreateNewInstancesWithDifferentDependencies()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class1>();
		serviceCollection.AddType<Class2>();
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var class2InstanceA = await serviceProvider.GetRequiredService<Class2>();
		var class2InstanceB = await serviceProvider.GetRequiredService<Class2>();

		// Assert
		Assert.IsNotNull(class2InstanceA);
		Assert.IsNotNull(class2InstanceB);
		Assert.AreNotSame(class2InstanceA, class2InstanceB);
		Assert.AreNotSame(class2InstanceA.Class1, class2InstanceB.Class1);
	}

	[TestMethod]
	public async Task CreateSingleton_ShouldReturnSameInstanceAcrossScopes()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSharedType<SingletonClass>(SharedWithin.Container);
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var classInstanceA = await serviceProvider.GetRequiredService<SingletonClass>();
		var classInstanceB = await serviceProvider.GetRequiredService<SingletonClass>();

		var serviceScopeFactory = await serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var scopedServiceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
		var classInstanceC = await scopedServiceProvider.GetRequiredService<SingletonClass>();
		var classInstanceD = await scopedServiceProvider.GetRequiredService<SingletonClass>();

		// Assert
		Assert.IsNotNull(classInstanceA);
		Assert.IsNotNull(classInstanceB);
		Assert.IsNotNull(classInstanceC);
		Assert.IsNotNull(classInstanceD);
		Assert.AreSame(classInstanceA, classInstanceB);
		Assert.AreSame(classInstanceC, classInstanceD);
		Assert.AreSame(classInstanceA, classInstanceC);
	}

	[TestMethod]
	public async Task CreateScoped_ShouldReturnSameInstanceWithinScope()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSharedType<ScopedClass>(SharedWithin.Scope);
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var classInstanceA = await serviceProvider.GetRequiredService<ScopedClass>();
		var classInstanceB = await serviceProvider.GetRequiredService<ScopedClass>();

		var serviceScopeFactory = await serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var scopedServiceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
		var classInstanceC = await scopedServiceProvider.GetRequiredService<ScopedClass>();
		var classInstanceD = await scopedServiceProvider.GetRequiredService<ScopedClass>();

		// Assert
		Assert.IsNotNull(classInstanceA);
		Assert.IsNotNull(classInstanceB);
		Assert.IsNotNull(classInstanceC);
		Assert.IsNotNull(classInstanceD);
		Assert.AreSame(classInstanceA, classInstanceB);
		Assert.AreSame(classInstanceC, classInstanceD);
		Assert.AreNotSame(classInstanceA, classInstanceC);
	}

	[TestMethod]
	public async Task CreateDecorator_ShouldDecorateService()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementation<DecoratedClass>().For<IForDecoration>();
		serviceCollection.AddDecorator<DecoratorClass>().For<IForDecoration>();
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var classInstance = await serviceProvider.GetRequiredService<IForDecoration>();

		// Assert
		Assert.IsNotNull(classInstance);
		Assert.AreEqual(expected: @"[DecoratedClass]", classInstance.Value);
	}

	[TestMethod]
	public async Task CreateSyncDecorator_ShouldDecorateServiceSynchronously()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTransient<IForDecorationSync>(sp => new DecoratedClassSync());
		serviceCollection.AddTransientDecorator<IForDecorationSync>((sp, d) => new DecoratorClassSync(d));
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var classInstance = await serviceProvider.GetRequiredService<IForDecorationSync>();

		// Assert
		Assert.IsNotNull(classInstance);
		Assert.AreEqual(expected: @"[DecoratedClassSync]", classInstance.Value);
	}

	[TestMethod]
	public async Task CreateMultiInterface_ShouldRegisterMultipleInterfaces()
	{
		// Arrange
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementation<Class3>().For<IInterface1>().For<IInterface2>();
		var serviceProvider = serviceCollection.BuildProvider();

		// Act
		var interface1 = await serviceProvider.GetRequiredService<IInterface1>();
		var interface2 = await serviceProvider.GetRequiredService<IInterface2>();
		var class3Instance = await serviceProvider.GetService<Class3>();

		// Assert
		Assert.IsNotNull(interface1);
		Assert.IsNotNull(interface2);
		Assert.IsNull(class3Instance);
	}

	private class Class1;

	private class Class2(Class1 class1)
	{
		public Class1 Class1 { get; } = class1;
	}

	private interface IInterface1;

	private interface IInterface2;

	private class Class3 : IInterface1, IInterface2;

	private class SingletonClass;

	private class ScopedClass;

	private interface IForDecoration
	{
		string Value { get; }
	}

	private interface IForDecorationSync
	{
		string Value { get; }
	}

	private class DecoratedClass : IForDecoration
	{
	#region Interface IForDecoration

		public string Value => "DecoratedClass";

	#endregion
	}

	private class DecoratorClass(IForDecoration decorated) : IForDecoration
	{
		private IForDecoration Decorated { get; } = decorated;

	#region Interface IForDecoration

		public string Value => "[" + Decorated.Value + "]";

	#endregion
	}

	private class DecoratedClassSync : IForDecorationSync
	{
	#region Interface IForDecorationSync

		public string Value => "DecoratedClassSync";

	#endregion
	}

	private class DecoratorClassSync(IForDecorationSync decorated) : IForDecorationSync
	{
	#region Interface IForDecorationSync

		public string Value => "[" + decorated.Value + "]";

	#endregion
	}

	public sealed class DisposableClass : IDisposable
	{
		public bool Disposed;

	#region Interface IDisposable

		public void Dispose()
		{
			Disposed = true;
		}

	#endregion
	}

	public sealed class AsyncDisposableClass : IAsyncDisposable
	{
		public bool Disposed;

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Disposed = true;

			return default;
		}

	#endregion
	}
}