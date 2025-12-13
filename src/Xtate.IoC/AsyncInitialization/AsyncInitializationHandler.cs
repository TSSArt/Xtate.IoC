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

namespace Xtate.IoC;

/// <summary>
///     Default <see cref="IInitializationHandler" /> implementation for asynchronous initialization.
///     Determines whether an instance requires asynchronous initialization (implements <see cref="IAsyncInitialization" />
///     ) and provides access to its initialization <see cref="ValueTask" />.
/// </summary>
internal class AsyncInitializationHandler : IInitializationHandler
{
	/// <summary>
	///     Singleton instance of the handler.
	/// </summary>
	public static readonly IInitializationHandler Instance = new AsyncInitializationHandler();

#region Interface IInitializationHandler

	/// <summary>
	///     Executes the synchronous phase of initialization.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <param name="instance">Instance to inspect for asynchronous initialization support.</param>
	/// <returns>
	///     <see langword="true" /> if the instance implements <see cref="IAsyncInitialization" /> and requires an asynchronous
	///     phase; otherwise <see langword="false" />.
	/// </returns>
	bool IInitializationHandler.Initialize<T>(T instance) => Initialize(instance);

	/// <summary>
	///     Executes the asynchronous phase of initialization (if required).
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <param name="instance">Instance to initialize asynchronously.</param>
	/// <returns>
	///     A <see cref="ValueTask" /> representing the asynchronous initialization operation; a completed task if no
	///     asynchronous
	///     initialization is required.
	/// </returns>
	ValueTask IInitializationHandler.InitializeAsync<T>(T instance) => InitializeAsync(instance);

#endregion

	/// <summary>
	///     Determines whether the specified instance requires asynchronous initialization.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <param name="instance">Instance to inspect.</param>
	/// <returns>
	///     <see langword="true" /> if the instance implements <see cref="IAsyncInitialization" />; otherwise
	///     <see langword="false" />.
	/// </returns>
	public static bool Initialize<T>(T instance) => instance is IAsyncInitialization;

	/// <summary>
	///     Returns a <see cref="ValueTask" /> representing the asynchronous initialization for the specified instance,
	///     or a completed task if the instance does not support asynchronous initialization.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <param name="instance">Instance to initialize.</param>
	/// <returns>
	///     A <see cref="ValueTask" /> wrapping the <see cref="IAsyncInitialization.Initialization" /> task if supported;
	///     otherwise <see cref="ValueTask.CompletedTask" />.
	/// </returns>
	public static ValueTask InitializeAsync<T>(T instance) => instance is IAsyncInitialization init ? new ValueTask(init.Initialization) : ValueTask.CompletedTask;
}