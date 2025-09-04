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
public class ServiceTypeTest
{
    [TestMethod]
    public void ServiceType_WhenEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var empty = new ServiceType();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>([ExcludeFromCodeCoverage]() => _ = empty.Type);
    }

    [TestMethod]
    public void ServiceType_WhenIntType_ReturnsCorrectDefinition()
    {
        // Arrange
        var intType = ServiceType.TypeOf<int>();

        // Act
        var definition = intType.Definition;

        // Assert
        Assert.AreEqual(new ServiceType(), definition);
    }

    [TestMethod]
    public void ServiceType_WhenGenericTypes_HaveSameDefinition()
    {
        // Arrange
        var listIntType = ServiceType.TypeOf<List<int>>();
        var listLongType = ServiceType.TypeOf<List<long>>();

        // Act
        var listIntDefinition = listIntType.Definition;
        var listLongDefinition = listLongType.Definition;

        // Assert
        Assert.IsTrue(listIntDefinition.Equals(listIntDefinition));
        Assert.IsTrue(listIntDefinition.Equals(listLongDefinition));
        Assert.IsTrue(listIntDefinition.Equals((object)listIntDefinition));
        Assert.IsTrue(listIntDefinition.Equals((object)listLongDefinition));
        Assert.IsFalse(listIntDefinition.Equals(new object()));
    }

    [TestMethod]
    public void ServiceType_WhenEmpty_HasCorrectBaseMethodsBehavior()
    {
        // Arrange
        var empty = new ServiceType();

        // Act
        var equalsSelf = empty.Equals(empty);
        var toString = empty.ToString();
        var hashCode = empty.GetHashCode();

        // Assert
        Assert.IsTrue(equalsSelf);
        Assert.AreEqual(expected: "", toString);
        Assert.AreEqual(expected: 0, hashCode);
    }

    [TestMethod]
    public void ServiceType_WhenIntType_HasCorrectBaseMethodsBehavior()
    {
        // Arrange
        var intType = ServiceType.TypeOf<int>();

        // Act
        var equalsSelf = intType.Equals(intType);
        var toString = intType.ToString();
        var hashCode = intType.GetHashCode();

        // Assert
        Assert.IsTrue(equalsSelf);
        Assert.IsFalse(string.IsNullOrEmpty(toString));
        Assert.AreNotEqual(long.MaxValue, hashCode);
    }

    [TestMethod]
    public void ArgumentType_WhenEmpty_HasCorrectToString()
    {
        // Arrange
        var empty = new ArgumentType();

        // Act
        var toString = empty.ToString();

        // Assert
        Assert.AreEqual(expected: "", toString);
    }
}