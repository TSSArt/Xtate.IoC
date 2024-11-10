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
public class HelpersTest
{
	[TestMethod]
	public void ThrowIf_ObjectDisposedException_ThrownWhenConditionIsTrue()
	{
		// Arrange & Act & Assert
		Assert.ThrowsException<ObjectDisposedException>([ExcludeFromCodeCoverage]() => XtateObjectDisposedException.ThrowIf(condition: true, instance: "44"));
	}

	[TestMethod]
	public async Task GetAsyncEnumerator_EmptyAsyncEnumerable_CurrentIsDefault()
	{
		// Arrange
		var asyncEnum = AsyncEnumerable.Empty<int>();

		// Act
		await using var asyncEnumerator = asyncEnum.GetAsyncEnumerator();
		var current = asyncEnumerator.Current;

		// Assert
		Assert.AreEqual(expected: 0, current);
	}

	private sealed class AsyncDisposableClass : IAsyncDisposable
	{
		public bool Disposed { get; private set; }

	#region Interface IAsyncDisposable

		public ValueTask DisposeAsync()
		{
			Disposed = true;

			return default;
		}

	#endregion
	}
}