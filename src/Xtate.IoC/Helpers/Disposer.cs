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

namespace Xtate.IoC;

/// <summary>
///     Provides methods to dispose of objects that implement <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />
///     .
/// </summary>
internal static class Disposer
{
	/// <summary>
	///     Determines whether the specified instance is disposable.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <param name="instance">The instance to check.</param>
	/// <returns>
	///     <c>true</c> if the instance is <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />; otherwise,
	///     <c>false</c>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDisposable<T>([NotNullWhen(true)] T instance) => instance is IDisposable or IAsyncDisposable;

	/// <summary>
	///     Disposes the specified instance if it implements <see cref="IDisposable" /> or <see cref="IAsyncDisposable" />.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <param name="instance">The instance to dispose.</param>
	public static void Dispose<T>(T instance)
	{
		if (instance is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	/// <summary>
	///     Asynchronously disposes the specified instance if it implements <see cref="IAsyncDisposable" /> or
	///     <see cref="IDisposable" />.
	/// </summary>
	/// <typeparam name="T">The type of the instance.</typeparam>
	/// <param name="instance">The instance to dispose asynchronously.</param>
	/// <returns>A <see cref="ValueTask" /> representing the asynchronous dispose operation.</returns>
	public static ValueTask DisposeAsync<T>(T instance)
	{
		switch (instance)
		{
			case IAsyncDisposable asyncDisposable:
				return asyncDisposable.DisposeAsync();

			case IDisposable disposable:
				disposable.Dispose();

				return default;

			default:
				return default;
		}
	}
}