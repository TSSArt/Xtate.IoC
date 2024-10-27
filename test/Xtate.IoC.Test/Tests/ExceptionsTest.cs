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

namespace Xtate.IoC.Test;

[TestClass]
public class ExceptionsTest
{
	[TestMethod]
	public void InvalidOperationException_DefaultConstructor_ShouldHaveDefaultMessageAndNoInnerException()
	{
		// Arrange
		var ex = new InvalidOperationException();

		// Act

		// Assert
		Assert.IsNotNull(ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void InvalidOperationException_MessageConstructor_ShouldSetMessageAndHaveNoInnerException()
	{
		// Arrange
		var ex = new InvalidOperationException(@"text");

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void InvalidOperationException_MessageAndInnerExceptionConstructor_ShouldSetMessageAndInnerException()
	{
		// Arrange
		var exInner = new ApplicationException();
		var ex = new InvalidOperationException(message: @"text", exInner);

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.AreSame(exInner, ex.InnerException);
	}

	[TestMethod]
	public void DependencyInjectionException_DefaultConstructor_ShouldHaveDefaultMessageAndNoInnerException()
	{
		// Arrange
		var ex = new DependencyInjectionException();

		// Act

		// Assert
		Assert.IsNotNull(ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void DependencyInjectionException_MessageConstructor_ShouldSetMessageAndHaveNoInnerException()
	{
		// Arrange
		var ex = new DependencyInjectionException(@"text");

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void DependencyInjectionException_MessageAndInnerExceptionConstructor_ShouldSetMessageAndInnerException()
	{
		// Arrange
		var exInner = new ApplicationException();
		var ex = new DependencyInjectionException(message: @"text", exInner);

		// Act

		// Assert
		Assert.AreEqual(expected: @"text", ex.Message);
		Assert.AreSame(exInner, ex.InnerException);
	}
}