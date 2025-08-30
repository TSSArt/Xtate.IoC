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

using System.Collections.Immutable;
using System.Threading;

// ReSharper disable UnusedMember.Local
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

#pragma warning disable IDE0079
#pragma warning disable CA2012 // Use ValueTasks correctly
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1859

namespace Xtate.IoC.Test;

[TestClass]
public class DiTest2
{
	[TestMethod]
	public async Task CreateWithArg()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class4, int>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class4 = await serviceProvider.GetRequiredService<Class4, int>(55);

		Assert.IsNotNull(class4);
		Assert.AreEqual(expected: 55, class4.Val);
	}

	[TestMethod]
	public async Task CreateWithArg2()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class4A, int, string>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class4 = await serviceProvider.GetRequiredService<Class4A, int, string>(arg1: 556, arg2: "aa");

		Assert.IsNotNull(class4);
		Assert.AreEqual(expected: 556, class4.Val);
		Assert.AreEqual(expected: "aa", class4.ValStr);
	}

	[TestMethod]
	public async Task CreateWithArg2A()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class4A, (int, string)>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class4 = await serviceProvider.GetRequiredService<Class4A, (int, string)>((556, "aa"));

		Assert.IsNotNull(class4);
		Assert.AreEqual(expected: 556, class4.Val);
		Assert.AreEqual(expected: "aa", class4.ValStr);
	}

	[TestMethod]
	public async Task CreateClassDifferentRequiredDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class1>();
		serviceCollection.AddType<Class2>();
		serviceCollection.AddType<Class4, int>();
		serviceCollection.AddTypeSync<Class4Sync, int>();
		serviceCollection.AddType<Class4, int, int>();
		serviceCollection.AddTypeSync<Class4Sync, int, int>();
		serviceCollection.AddType<Class5, int>();
		serviceCollection.AddImplementation<DecoratedClass>().For<IForDecoration>();
		serviceCollection.AddDecorator<DecoratorClass>().For<IForDecoration>();
		serviceCollection.AddImplementationSync<DecoratedClassSync>().For<IForDecorationSync>();
		serviceCollection.AddDecoratorSync<DecoratorClassSync>().For<IForDecorationSync>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class5 = await serviceProvider.GetRequiredService<Class5, int>(22);

		Assert.IsNotNull(class5);
		Assert.IsNotNull(class5.PropClass1);
		Assert.IsNotNull(class5.PropClass2);
		Assert.IsNotNull(class5.PropClass4);
		Assert.AreEqual(expected: 22, class5.PropClass4.Val);
		Assert.HasCount(expected: 1, class5.PropClass4S);
		Assert.AreEqual(expected: 22, class5.PropClass4S[0].Val);
		Assert.HasCount(expected: 1, class5.PropClass4SSync);
		Assert.AreEqual(expected: 22, class5.PropClass4SSync[0].Val);
		Assert.HasCount(expected: 1, class5.PropClass4STwo);
		Assert.AreEqual(expected: 22, class5.PropClass4STwo[0].Val);
		Assert.HasCount(expected: 1, class5.PropClass4STwoSync);
		Assert.AreEqual(expected: 22, class5.PropClass4STwoSync[0].Val);
		Assert.HasCount(expected: 2, class5.PropForDecorations);
		Assert.HasCount(expected: 2, class5.PropForDecorationsSync);
	}

	[TestMethod]
	public async Task CreateOptionalDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class2Opt>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class2 = await serviceProvider.GetRequiredService<Class2Opt>();

		Assert.IsNotNull(class2);
	}

	[TestMethod]
	public void CreateOptionalSyncDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class2Opt>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class2 = serviceProvider.GetRequiredServiceSync<Class2Opt>();

		Assert.IsNotNull(class2);
	}

	[TestMethod]
	public async Task CreateClassDifferentOptionalDependencies()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class6, int>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class6 = await serviceProvider.GetRequiredService<Class6, int>(22);

		Assert.IsNotNull(class6);
		Assert.IsNull(class6.PropClass1);
		Assert.IsNull(class6.PropClass2);
		Assert.IsNull(class6.PropClass4);
		Assert.IsEmpty(class6.PropClass4S);
		Assert.IsEmpty(class6.PropForDecorations);
	}

	[TestMethod]
	public async Task GenericClassRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<GenericClass<Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var genTypeInt = await serviceProvider.GetRequiredService<GenericClass<int, int>>();
		var genTypeVersion = await serviceProvider.GetRequiredService<GenericClass<Version, Version>>();

		Assert.IsNotNull(genTypeInt);
		Assert.AreSame(typeof(int), genTypeInt.ArgType1);

		Assert.IsNotNull(genTypeVersion);
		Assert.AreSame(typeof(Version), genTypeVersion.ArgType1);
	}

	[TestMethod]
	public async Task GenericClassWithInterfaceRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementation<GenericClass<Any, Any>>().For<IGenericInterface<byte, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var genTypeInt = await serviceProvider.GetRequiredService<IGenericInterface<byte, int, long>>();
		var genTypeVersion = await serviceProvider.GetRequiredService<IGenericInterface<byte, Version, object>>();

		Assert.IsNotNull(genTypeInt);
		Assert.AreSame(typeof(long), genTypeInt.ArgType1);
		Assert.AreSame(typeof(int), genTypeInt.ArgType2);

		Assert.IsNotNull(genTypeVersion);
		Assert.AreSame(typeof(object), genTypeVersion.ArgType1);
		Assert.AreSame(typeof(Version), genTypeVersion.ArgType2);
	}

	[TestMethod]
	public void GenericClassWithInterfaceRegistrationSync()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddImplementationSync<GenericClass<Any, Any>>().For<IGenericInterface<byte, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var genTypeInt = serviceProvider.GetRequiredServiceSync<IGenericInterface<byte, int, long>>();
		var genTypeVersion = serviceProvider.GetRequiredServiceSync<IGenericInterface<byte, Version, object>>();

		Assert.IsNotNull(genTypeInt);
		Assert.AreSame(typeof(long), genTypeInt.ArgType1);
		Assert.AreSame(typeof(int), genTypeInt.ArgType2);

		Assert.IsNotNull(genTypeVersion);
		Assert.AreSame(typeof(object), genTypeVersion.ArgType1);
		Assert.AreSame(typeof(Version), genTypeVersion.ArgType2);
	}

	[TestMethod]
	public async Task FactoryRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddFactory<Class8>().For<Class7>();
		var serviceProvider = serviceCollection.BuildProvider();
		var class7 = await serviceProvider.GetRequiredService<Class7>();
		var class8 = await serviceProvider.GetService<Class8>();

		Assert.IsNotNull(class7);
		Assert.IsNull(class8);
	}

	[TestMethod]
	public async Task GenericFactoryRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class9<Any, Any>>();
		serviceCollection.AddFactory<Class10<Any>>().For<IInterface9<Any, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var i9 = await serviceProvider.GetRequiredService<IInterface9<string, List<object>, int>>();
		var i9A = await serviceProvider.GetRequiredService<IInterface9<Version, Uri, char>>();

		Assert.IsNotNull(i9);
		Assert.IsNotNull(i9A);
	}

	[TestMethod]
	public void GenericFactorySyncRegistration()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class9<Any, Any>>();
		serviceCollection.AddFactorySync<Class10Sync<Any>>().For<IInterface9<Any, Any, Any>>();
		var serviceProvider = serviceCollection.BuildProvider();
		var i9 = serviceProvider.GetRequiredServiceSync<IInterface9<string, List<object>, int>>();
		var i9A = serviceProvider.GetRequiredServiceSync<IInterface9<Version, Uri, char>>();

		Assert.IsNotNull(i9);
		Assert.IsNotNull(i9A);
	}

	[TestMethod]
	public void SyncTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class11>();
		serviceCollection.AddTypeSync<Class12>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class12Factory = serviceProvider.GetRequiredSyncFactory<Class12>();
		var class12 = class12Factory();
		var class11 = class12.Class11;

		Assert.IsNotNull(class11);
		Assert.IsNotNull(class12);
	}

	[TestMethod]
	public async Task MultiArgTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<Class14, int, long>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class14 = await serviceProvider.GetRequiredService<Class14, int, long>(arg1: 3, arg2: 4);

		Assert.AreEqual(expected: 7, class14.Result);
	}

	[TestMethod]
	public async Task IndirectAsyncInitTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTransient<ClassA1>(sp => new ClassAsyncInit1());
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<ClassA1>();

		Assert.IsTrue(class1.InitCalled);
	}

	[TestMethod]
	public async Task DisposableTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<DisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<DisposableClass>();

		await Disposer.DisposeAsync(serviceProvider);

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task Disposable2Test()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<DisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<DisposableClass>();

		await Disposer.DisposeAsync(serviceProvider);

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task AsyncDisposableTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<AsyncDisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<AsyncDisposableClass>();

		await Disposer.DisposeAsync(serviceProvider);

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task AsyncDisposable2Test()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddType<AsyncDisposableClass>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<AsyncDisposableClass>();

		await Disposer.DisposeAsync(serviceProvider);

		Assert.IsTrue(class1.Disposed);
	}

	[TestMethod]
	public async Task MultiArgFuncTestTest()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTypeSync<Class1, int, long>();
		serviceCollection.AddType<ClassMultiDep>();
		var serviceProvider = serviceCollection.BuildProvider();

		var class1 = await serviceProvider.GetRequiredService<ClassMultiDep>();

		Assert.IsNotNull(class1);
	}

	[ExcludeFromCodeCoverage]
	private class ClassMultiDep(Func<int, long, Class1> a)
	{
		public Func<int, long, Class1> A { get; } = a;
	}

	private class ClassA1
	{
		public bool InitCalled;
	}

	private class ClassAsyncInit1 : ClassA1, IAsyncInitialization
	{
	#region Interface IAsyncInitialization

		public Task Initialization
		{
			get
			{
				InitCalled = true;

				return Task.CompletedTask;
			}
		}

	#endregion
	}

	private class Class1;

	private class Class2(Class1 class1)
	{
		public Class1 Class1 { get; } = class1;
	}

	private class Class2Opt(Class1? class1)
	{
		public Class1? Class1 { [ExcludeFromCodeCoverage] get; } = class1;
	}

	private class Class4(int val)
	{
		public int Val { get; } = val;
	}

	private class Class4Sync(int val)
	{
		public int Val { get; } = val;
	}

	private class Class4A(string valStr, int val)
	{
		public string ValStr { get; } = valStr;

		public int Val { get; } = val;
	}

	private class Class5 : IAsyncInitialization
	{
		private readonly int _arg;

		private readonly Func<ValueTask<Class2>> _class2Factory;

		private readonly Func<int, IAsyncEnumerable<Class4>> _class4Factories;

		private readonly Func<int, IEnumerable<Class4Sync>> _class4FactoriesSync;

		private readonly Func<int, int, IAsyncEnumerable<Class4>> _class4FactoriesTwo;

		private readonly Func<int, int, IEnumerable<Class4Sync>> _class4FactoriesTwoSync;

		private readonly Func<int, ValueTask<Class4>> _class4Factory;

		private readonly IAsyncEnumerable<IForDecoration> _forDecorations;

		private readonly IEnumerable<IForDecorationSync> _forDecorationsSync;

		public Class5(Class1 class1,
					  int arg,
					  Func<ValueTask<Class2>> class2Factory,
					  Func<int, ValueTask<Class4>> class4Factory,
					  IAsyncEnumerable<IForDecoration> forDecorations,
					  IEnumerable<IForDecorationSync> forDecorationsSync,
					  Func<int, IAsyncEnumerable<Class4>> class4Factories,
					  Func<int, IEnumerable<Class4Sync>> class4FactoriesSync,
					  Func<int, int, IAsyncEnumerable<Class4>> class4FactoriesTwo,
					  Func<int, int, IEnumerable<Class4Sync>> class4FactoriesTwoSync)
		{
			PropClass1 = class1;
			_arg = arg;
			_class2Factory = class2Factory;
			_class4Factory = class4Factory;
			_forDecorations = forDecorations;
			_forDecorationsSync = forDecorationsSync;
			_class4Factories = class4Factories;
			_class4FactoriesSync = class4FactoriesSync;
			_class4FactoriesTwo = class4FactoriesTwo;
			_class4FactoriesTwoSync = class4FactoriesTwoSync;

			Initialization = Initialize();
		}

		public Class1 PropClass1 { [UsedImplicitly] get; }

		public ImmutableArray<Class4> PropClass4S { get; private set; } = [];

		public ImmutableArray<Class4Sync> PropClass4SSync { get; private set; } = [];

		public ImmutableArray<Class4> PropClass4STwo { get; private set; } = [];

		public ImmutableArray<Class4Sync> PropClass4STwoSync { get; private set; } = [];

		public ImmutableArray<IForDecoration> PropForDecorations { get; private set; } = [];

		public ImmutableArray<IForDecorationSync> PropForDecorationsSync { get; private set; } = [];

		public Class4 PropClass4 { get; private set; } = null!;

		public Class2 PropClass2 { [UsedImplicitly] get; private set; } = null!;

	#region Interface IAsyncInitialization

		public Task Initialization { get; }

	#endregion

		private async Task Initialize()
		{
			PropClass2 = await _class2Factory();
			PropClass4 = await _class4Factory(_arg);

			await foreach (var i in _forDecorations)
			{
				PropForDecorations = PropForDecorations.Add(i);
			}

			foreach (var i in _forDecorationsSync)
			{
				PropForDecorationsSync = PropForDecorationsSync.Add(i);
			}

			await foreach (var i in _class4Factories(_arg))
			{
				PropClass4S = PropClass4S.Add(i);
			}

			foreach (var i in _class4FactoriesSync(_arg))
			{
				PropClass4SSync = PropClass4SSync.Add(i);
			}

			await foreach (var i in _class4FactoriesTwo(_arg, _arg))
			{
				PropClass4STwo = PropClass4STwo.Add(i);
			}

			foreach (var i in _class4FactoriesTwoSync(_arg, _arg))
			{
				PropClass4STwoSync = PropClass4STwoSync.Add(i);
			}
		}
	}

	private class Class6 : IAsyncInitialization
	{
		private readonly int _arg;

		private readonly Func<ValueTask<Class2?>> _class2Factory;

		private readonly Func<int, IAsyncEnumerable<Class4>> _class4Factories;

		private readonly Func<int, ValueTask<Class4?>> _class4Factory;

		private readonly IAsyncEnumerable<IForDecoration> _forDecorations;

		public Class6(Class1? class1,
					  int arg,
					  Func<ValueTask<Class2?>> class2Factory,
					  Func<int, ValueTask<Class4?>> class4Factory,
					  Func<object, ValueTask<Class4?>> _,
					  IAsyncEnumerable<IForDecoration> forDecorations,
					  Func<int, IAsyncEnumerable<Class4>> class4Factories)
		{
			PropClass1 = class1;
			_arg = arg;
			_class2Factory = class2Factory;
			_class4Factory = class4Factory;
			_forDecorations = forDecorations;
			_class4Factories = class4Factories;

			Initialization = Initialize();
		}

		public Class1? PropClass1 { get; }

		public ImmutableArray<Class4> PropClass4S { get; [ExcludeFromCodeCoverage] private set; } = [];

		public ImmutableArray<IForDecoration> PropForDecorations { get; [ExcludeFromCodeCoverage] private set; } = [];

		public Class4? PropClass4 { get; private set; }

		public Class2? PropClass2 { get; private set; }

	#region Interface IAsyncInitialization

		public Task Initialization { get; }

	#endregion

		[ExcludeFromCodeCoverage]
		private async Task Initialize()
		{
			PropClass2 = await _class2Factory();
			PropClass4 = await _class4Factory(_arg);

			await foreach (var i in _forDecorations)
			{
				PropForDecorations = PropForDecorations.Add(i);
			}

			await foreach (var i in _class4Factories(_arg))
			{
				PropClass4S = PropClass4S.Add(i);
			}
		}
	}

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

	private interface IGenericInterface<TI0, TI1, TI2>
	{
		Type ArgType1 { get; }

		Type ArgType2 { get; }
	}

	private class GenericClass<TC1, TC2> : IGenericInterface<byte, TC2, TC1>, IGenericInterface<TC1, sbyte, TC2>
	{
	#region Interface IGenericInterface<byte,TC2,TC1>

		public Type ArgType1 => typeof(TC1);

		public Type ArgType2 => typeof(TC2);

	#endregion
	}

	private class Class7;

	private class Class8
	{
		public ValueTask<Class7> Factory() => new(new Class7());

		[Obsolete("For tests")]
		[ExcludeFromCodeCoverage]
		public ValueTask<Class7> Factory1() => default;
	}

	private interface IInterface9<TI1, TI2, TI3>;

	private class Class9<TC1, TC2> : IInterface9<TC2, List<TC1>, int>, IInterface9<TC2, TC1, long>, IInterface9<TC2, TC1, char>;

	private class Class10<T2>(IServiceProvider serviceProvider)
	{
		public async ValueTask<IInterface9<T2, List<T1>, int>> Factory<T1>() => await serviceProvider.GetRequiredService<Class9<T1, T2>>();

		[ExcludeFromCodeCoverage]
		public async ValueTask<IInterface9<T2, T1, long>> Factory2<T1>() => await serviceProvider.GetRequiredService<Class9<T1, T2>>();

		[UsedImplicitly]
		public IInterface9<T2, T1, char> Factory3<T1>(CancellationToken _) => serviceProvider.GetRequiredService<Class9<T1, T2>>().Result;
	}

	private class Class10Sync<T2>(IServiceProvider serviceProvider)
	{
		public IInterface9<T2, List<T1>, int> Factory<T1>() => serviceProvider.GetRequiredServiceSync<Class9<T1, T2>>();

		[ExcludeFromCodeCoverage]
		public IInterface9<T2, T1, long> Factory2<T1>() => serviceProvider.GetRequiredServiceSync<Class9<T1, T2>>();

		public IInterface9<T2, T1, char> Factory3<T1>() => serviceProvider.GetRequiredService<Class9<T1, T2>>().Result;
	}

	private class Class11;

	[ExcludeFromCodeCoverage]
	private class Class12(Func<Class11> factory, Func<Class11> _)
	{
		public Func<Class11> Unknown { get; } = _;

		public Class11 Class11 { get; } = factory();
	}

	private class Class14(int a, long b)
	{
		public long Result => a + b;
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