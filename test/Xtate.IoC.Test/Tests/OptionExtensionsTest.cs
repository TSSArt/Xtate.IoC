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
public class OptionExtensionsTest
{
    [TestMethod]
    public void Validate_ShouldThrowArgumentException_WhenOptionContainsUnsupportedOptions()
    {
        // Arrange
        const Option option = Option.IfNotRegistered | Option.DoNotDispose;
        const Option allowedOptions = Option.IfNotRegistered;

        // Act

        // Assert
        Assert.Throws<ArgumentException>(() => option.Validate(allowedOptions));
    }

    [TestMethod]
    public void Validate_ShouldNotThrowException_WhenOptionIsValid()
    {
        // Arrange
        const Option option = Option.IfNotRegistered;
        const Option allowedOptions = Option.IfNotRegistered | Option.DoNotDispose;

        // Act
        option.Validate(allowedOptions);

        // Assert
    }

    [TestMethod]
    public void Has_ShouldReturnTrue_WhenOptionContainsToCheck()
    {
        // Arrange
        const Option option = Option.IfNotRegistered | Option.DoNotDispose;
        const Option toCheck = Option.IfNotRegistered;

        // Act
        var result = option.Has(toCheck);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Has_ShouldReturnFalse_WhenOptionDoesNotContainToCheck()
    {
        // Arrange
        const Option option = Option.IfNotRegistered;
        const Option toCheck = Option.DoNotDispose;

        // Act
        var result = option.Has(toCheck);

        // Assert
        Assert.IsFalse(result);
    }
}