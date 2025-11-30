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

#pragma warning disable CA1822 // Mark members as static

namespace Xtate.IoC.Test;

[TestClass]
public class ImplementationTypeTest
{
	[TestMethod]
	public void TypeOf_Interface_ThrowsArgumentException()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => ImplementationType.TypeOf<IInterface>());
	}

	[TestMethod]
	public void AddImplementation_Interface_ThrowsArgumentException()
	{
		// Arrange
		var sc = new ServiceCollection();

		// Act

		// Assert
		Assert.ThrowsExactly<ArgumentException>([ExcludeFromCodeCoverage]() => sc.AddImplementation<IInterface>());
	}

	[TestMethod]
	public void EmptyImplementationType_AccessType_ThrowsInvalidOperationException()
	{
		// Arrange
		var empty = new ImplementationType();

		// Act

		// Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => _ = empty.Type);
	}

	[TestMethod]
	public void TypeOf_Int_ReturnsCorrectDefinition()
	{
		// Arrange
		var intType = ImplementationType.TypeOf<int>();

		// Act

		// Assert
		Assert.AreEqual(new ImplementationType(), intType.Definition);
	}

	[TestMethod]
	public void TypeOf_GenericList_ReturnsCorrectDefinition()
	{
		// Arrange
		var listIntType = ImplementationType.TypeOf<List<int>>();
		var listLongType = ImplementationType.TypeOf<List<long>>();

		// Act

		// Assert
		Assert.IsTrue(listIntType.Definition.Equals(listIntType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals(listLongType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals((object) listIntType.Definition));
		Assert.IsTrue(listIntType.Definition.Equals((object) listLongType.Definition));
		Assert.IsFalse(listIntType.Definition.Equals(new object()));
	}

	[TestMethod]
	public void EmptyImplementationType_BaseMethods_ReturnExpectedResults()
	{
		// Arrange
		var empty = new ImplementationType();

		// Act

		// Assert
		Assert.IsTrue(empty.Equals(empty));
		Assert.AreEqual(expected: "", empty.ToString());
		Assert.AreEqual(expected: 0, empty.GetHashCode());
	}

	[TestMethod]
	public void TypeOf_Int_BaseMethods_ReturnExpectedResults()
	{
		// Arrange
		var intType = ImplementationType.TypeOf<int>();

		// Act

		// Assert
		Assert.IsTrue(intType.Equals(intType));
		Assert.IsFalse(string.IsNullOrEmpty(intType.ToString()));
		Assert.AreNotEqual(long.MaxValue, intType.GetHashCode());
	}

	[TestMethod]
	public void TryConstruct_BaseClass_ReturnsTrueAndCorrectType()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Service<Any>>();
		var srvType = ServiceType.TypeOf<BaseBaseService<int>>();

		// Act
		var result = implType.TryConstruct(srvType, out var newImplType);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(typeof(Service<int>), newImplType.Type);
	}

	[TestMethod]
	public void TryConstruct_Interface_ReturnsTrueAndCorrectType()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Service<Any>>();
		var srvType = ServiceType.TypeOf<IService<sbyte>>();

		// Act
		var result = implType.TryConstruct(srvType, out var newImplType);

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(typeof(Service<sbyte>), newImplType.Type);
	}

	[TestMethod]
	public void GetMethodInfo_NotResolvedTypeSync_ThrowsDependencyInjectionException()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<Any>>();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<int>, int>(true));
	}

	[TestMethod]
	public void GetMethodInfo_NotResolvedType_ThrowsDependencyInjectionException()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<Any>>();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<int>, int>(false));
	}

	[TestMethod]
	public void GetMethodInfo_NotResolvedMethod_ThrowsDependencyInjectionException()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory2>();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<Any>, int>(false));
	}

	[TestMethod]
	public void GetMethodInfo_ResolvedMethod_ReturnsMethodInfo()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<object>>();

		// Act
		var method = implType.GetMethodInfo<IService<int>, int>(false);

		// Assert
		Assert.IsNotNull(method);
	}

	[TestMethod]
	public void GetMethodInfo_MultipleObsoleteMethods_ReturnsMethodInfo()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<FactoryObsolete>();

		// Act
		var method = implType.GetMethodInfo<IService<int>, int>(false);

		// Assert
		Assert.IsNotNull(method);
	}

	[TestMethod]
	public void GetMethodInfo_MultipleActualMethods_ThrowsDependencyInjectionException()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<FactoryMultiActual>();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<int>, int>(false));
	}

	[TestMethod]
	public void GetMethodInfo_InvalidParameters_ThrowsDependencyInjectionException()
	{
		// Arrange
		var implType = ImplementationType.TypeOf<Factory<int>>();

		// Act

		// Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => implType.GetMethodInfo<IService<string>, int>(false));
	}

	// ReSharper disable All
	public interface IInterface { }

	public class BaseBaseService<T> { }

	public class BaseService<T> : BaseBaseService<T> { }

	public class Service<T> : BaseService<T>, IService2<T>, IService<T> { }

	public interface IService<T> { }

	public interface IService2<T> { }

	public class Factory<T>
	{
		[ExcludeFromCodeCoverage]
		public IService<int> M1() => default!;

		[ExcludeFromCodeCoverage]
		public IService<string> M2(ref int _) => default!;
	}

	public class FactoryObsolete
	{
		[ExcludeFromCodeCoverage]
		[Obsolete("For test")]
		public IService<int> M1() => default!;

		[ExcludeFromCodeCoverage]
		[Obsolete("For test")]
		public IService<int> M2() => default!;

		[ExcludeFromCodeCoverage]
		public IService<int> M3() => default!;
	}

	public class FactoryMultiActual
	{
		[ExcludeFromCodeCoverage]
		[Obsolete("For test")]
		public IService<int> M1() => default!;

		[ExcludeFromCodeCoverage]
		public IService<int> M2() => default!;

		[ExcludeFromCodeCoverage]
		public IService<int> M3() => default!;
	}

	public class Factory2
	{
		[ExcludeFromCodeCoverage]
		public IService<TM> M1<TM>() => default!;
	}

	// ReSharper restore All
}