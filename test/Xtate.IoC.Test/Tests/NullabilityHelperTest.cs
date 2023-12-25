// Copyright © 2019-2023 Sergii Artemenko
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
using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedParameter.Local

namespace Xtate.IoC.Test;

[TestClass]
public class NullabilityHelperTest
{
	[ExcludeFromCodeCoverage]
	private static ParameterInfo GetParameterInfo([CallerMemberName] string? member = default) =>
		typeof(NullabilityHelperTest)
			.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
			.Single(m => m.Name != member && m.Name.Contains(member!))
			.GetParameters()
			.Single();

	[TestMethod]
	public void SimpleTypeNotFoundTest()
	{
#pragma warning disable CS8321 // Local function is declared but never used
		[ExcludeFromCodeCoverage]
		static void Method(int _) { }
#pragma warning restore CS8321 // Local function is declared but never used

		// Arrange
		var parameterInfo = GetParameterInfo();

		// Act
		var result = NullabilityHelper.IsNullable(parameterInfo, path: "1");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GenericTypeNotFoundTest()
	{
#pragma warning disable CS8321 // Local function is declared but never used
		[ExcludeFromCodeCoverage]
		static void Method(List<int> _) { }
#pragma warning restore CS8321 // Local function is declared but never used

		// Arrange
		var parameterInfo = GetParameterInfo();

		// Act
		var result = NullabilityHelper.IsNullable(parameterInfo, path: "1");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GenericNullableTypeNotFoundTest()
	{
#pragma warning disable CS8321 // Local function is declared but never used
		[ExcludeFromCodeCoverage]
		static void Method(int? _) { }
#pragma warning restore CS8321 // Local function is declared but never used

		// Arrange
		var parameterInfo = GetParameterInfo();

		// Act
		var result = NullabilityHelper.IsNullable(parameterInfo, path: "1");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void NullableIntTest()
	{
#pragma warning disable CS8321 // Local function is declared but never used
		[ExcludeFromCodeCoverage]
		static void Method(int? _) { }
#pragma warning restore CS8321 // Local function is declared but never used

		// Arrange
		var parameterInfo = GetParameterInfo();

		// Act
		var result = NullabilityHelper.IsNullable(parameterInfo, path: "");

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void NestedTest()
	{
#pragma warning disable CS8321 // Local function is declared but never used
		[ExcludeFromCodeCoverage]

		// ReSharper disable once UnusedParameter.Local
		static void Method(List<List<int>> _) { }
#pragma warning restore CS8321 // Local function is declared but never used

		// Arrange
		var parameterInfo = GetParameterInfo();

		// Act
		var result = NullabilityHelper.IsNullable(parameterInfo, path: "11");

		// Assert
		Assert.IsFalse(result);
	}
#nullable disable
	[TestMethod]
	public void GenericNullabilityDisabledTest()
	{
#pragma warning disable CS8321 // Local function is declared but never used
		[ExcludeFromCodeCoverage]
		static void Method(List<List<int>> _) { }
#pragma warning restore CS8321 // Local function is declared but never used

		// Arrange
		var parameterInfo = GetParameterInfo();

		// Act
		var result = NullabilityHelper.IsNullable(parameterInfo, path: "");

		// Assert
		Assert.IsFalse(result);
	}
}