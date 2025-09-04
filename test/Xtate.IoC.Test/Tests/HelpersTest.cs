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

using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class HelpersTest
{
    [TestMethod]
    public void ThrowIf_ObjectDisposedException_ThrownWhenConditionIsTrue()
    {
        // Arrange & Act & Assert
        Assert.ThrowsExactly<ObjectDisposedException>([ExcludeFromCodeCoverage]() => XtateObjectDisposedException.ThrowIf(condition: true, instance: "44"));
    }

    [TestMethod]
    public async Task GetAsyncEnumerator_EmptyAsyncEnumerable_CurrentIsDefault()
    {
        // Arrange
        var asyncEnum = IAsyncEnumerable<int>.Empty;

        // Act
        await using var asyncEnumerator = asyncEnum.GetAsyncEnumerator(CancellationToken.None);
        var current = asyncEnumerator.Current;

        // Assert
        Assert.AreEqual(expected: 0, current);
    }
}