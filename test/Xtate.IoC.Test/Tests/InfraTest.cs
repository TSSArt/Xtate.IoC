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
public class InfraTest
{
	[TestMethod]
	public void Assert_ShouldThrowInvalidOperationException_WhenConditionIsFalse()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => Infra.Assert(false));
	}

	[TestMethod]
	public void NotNull_ShouldThrowInvalidOperationException_WhenArgumentIsNull()
	{
		// Arrange

		// Act

		// Assert
		Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => Infra.NotNull(null!));
	}

	[TestMethod]
	public void Unmatched_ShouldReturnExceptionWithMessageContainingPrimitiveValue()
	{
		// Arrange
		const int value = 456789123;

		// Act
		var ex = Infra.Unmatched(value);

		// Assert
		Assert.Contains(value.ToString(), ex.Message);
	}

	[TestMethod]
	public void Unmatched_ShouldReturnExceptionWithMessageContainingEnumValue()
	{
		// Arrange
		const UnexpectedEnumTestEnum value = UnexpectedEnumTestEnum.Val1;

		// Act
		var ex = Infra.Unmatched(value);

		// Assert
		Assert.Contains(value.ToString(), ex.Message);
	}

	[TestMethod]
	public void Unmatched_ShouldReturnExceptionWithMessageContainingDelegateType()
	{
		// Arrange
		Delegate value = [ExcludeFromCodeCoverage]() => { };

		// Act
		var ex = Infra.Unmatched(value);

		// Assert
		Assert.Contains(substring: "Delegate", ex.Message);
	}

	[TestMethod]
	public void Unmatched_ShouldReturnExceptionWithMessageContainingOtherType()
	{
		// Arrange
		var value = new Version();

		// Act
		var ex = Infra.Unmatched(value);

		// Assert
		Assert.Contains(substring: "Version", ex.Message);
	}

	private enum UnexpectedEnumTestEnum
	{
		Val1
	}
}