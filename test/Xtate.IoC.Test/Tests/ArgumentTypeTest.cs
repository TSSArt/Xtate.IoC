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
public class ArgumentTypeTest
{
    [TestMethod]
    public void ToString_ShouldReturnEmptyString_WhenTypeIsNull()
    {
        // Arrange
        var argumentType = new ArgumentType();

        // Act
        var result = argumentType.ToString(format: null, formatProvider: null);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFriendlyName_WhenFormatIsNullOrEmpty()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<int>();

        // Act
        var result = argumentType.ToString(format: null, formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "int", result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFormattedString_WhenFormatIsValid()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<ValueTuple<int, string>>();

        // Act
        var result = argumentType.ToString(format: "; |arg0", formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "int arg0; string arg1", result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFormattedString_WhenFormatIsEmpty()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<ValueTuple<int, string>>();

        // Act
        var result = argumentType.ToString(format: "; |", formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "(int, string)", result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFormattedString_WhenFormatStartWith0()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<ValueTuple<int, string>>();

        // Act
        var result = argumentType.ToString(format: "; |0we", formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "(int, string)", result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFormattedString_WhenFormatStartWith1()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<ValueTuple<int, string>>();

        // Act
        var result = argumentType.ToString(format: "; |1f", formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "(int, string)", result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFormattedString_WhenFormatContainsWrongChars()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<ValueTuple<int, string>>();

        // Act
        var result = argumentType.ToString(format: "; |@23", formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "(int, string)", result);
    }

    [TestMethod]
    public void ToString_ShouldReturnFormattedString_WhenFormatContainsNoDigits()
    {
        // Arrange
        var argumentType = ArgumentType.TypeOf<ValueTuple<int, string>>();

        // Act
        var result = argumentType.ToString(format: "; |arg", formatProvider: null);

        // Assert
        Assert.AreEqual(expected: "int arg; string arg", result);
    }
}