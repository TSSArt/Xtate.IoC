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
public class MissedServiceExceptionTest
{
    [TestMethod]
    public void MissedServiceException_DefaultConstructor_ShouldInitializeCorrectly()
    {
        // Act
        var exception = new MissedServiceException { Service = typeof(void) };

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(typeof(void), exception.Service);
        Assert.IsNull(exception.Argument);
    }

    [TestMethod]
    public void MissedServiceException_MessageConstructor_ShouldInitializeCorrectly()
    {
        // Arrange
        const string message = "Service not found";

        // Act
        var exception = new MissedServiceException(message) { Service = typeof(void) };

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(typeof(void), exception.Service);
        Assert.IsNull(exception.Argument);
    }

    [TestMethod]
    public void MissedServiceException_MessageAndInnerExceptionConstructor_ShouldInitializeCorrectly()
    {
        // Arrange
        const string message = "Service not found";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new MissedServiceException(message, innerException) { Service = typeof(void) };

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(message, exception.Message);
        Assert.AreEqual(innerException, exception.InnerException);
        Assert.AreEqual(typeof(void), exception.Service);
        Assert.IsNull(exception.Argument);
    }

    [TestMethod]
    public void MissedServiceException_Create_ShouldInitializeCorrectly()
    {
        // Act
        var exception = MissedServiceException.Create<string>();

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(typeof(string), exception.Service);
        Assert.IsNull(exception.Argument);
    }

    [TestMethod]
    public void MissedServiceException_CreateWithArgument_ShouldInitializeCorrectly()
    {
        // Act
        var exception = MissedServiceException.Create<string, int>();

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual(typeof(string), exception.Service);
        Assert.AreEqual(typeof(int), exception.Argument);
    }
}