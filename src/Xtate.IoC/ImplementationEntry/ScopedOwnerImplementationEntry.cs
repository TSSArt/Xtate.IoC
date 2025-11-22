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
///     Represents an entry for a scoped implementation in the IoC container whose produced scoped
///     instances are owned by the container (tracked for disposal).
///     <para>
///         Extends <see cref="ScopedImplementationEntry" /> by registering every successfully created instance
///         (if disposable) into the owning <see cref="ServiceProvider" />'s <see cref="ObjectsBin" /> so its lifetime
///         is tied to the scope/provider.
///     </para>
///     <para>
///         Thread-safety: Instances of this type are used concurrently. Registration into
///         <see cref="ObjectsBin" /> is performed using its thread-safe API.
///     </para>
/// </summary>
public class ScopedOwnerImplementationEntry : ScopedImplementationEntry
{
	/// <summary>
	///     Bin that tracks created transient instances for disposal when the owning <see cref="ServiceProvider" /> is
	///     disposed.
	/// </summary>
	/// <remarks>
	///     Non‑disposable instances are ignored by the bin. Disposable instances are registered so their lifetime is tied to
	///     the service provider.
	/// </remarks>
	private readonly ObjectsBin _objectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="ScopedOwnerImplementationEntry" /> class with a factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The service provider owning this entry (scope root).</param>
	/// <param name="factory">The factory delegate used to create (or decorate) service instances.</param>
	public ScopedOwnerImplementationEntry(ServiceProvider serviceProvider, Delegate factory) : base(serviceProvider, factory) => _objectsBin = serviceProvider.ObjectsBin;

	/// <summary>
	///     Initializes a new instance of the <see cref="ScopedOwnerImplementationEntry" /> class by copying the factory
	///     from a source entry (used when creating a scoped copy for a new <see cref="ServiceProvider" />).
	/// </summary>
	/// <param name="serviceProvider">The target service provider owning the new scoped entry.</param>
	/// <param name="sourceEntry">The source implementation entry whose factory is reused.</param>
	protected ScopedOwnerImplementationEntry(ServiceProvider serviceProvider, ImplementationEntry sourceEntry) : base(serviceProvider, sourceEntry) => _objectsBin = serviceProvider.ObjectsBin;

	/// <summary>
	///     Creates a new scoped owner entry bound to another <see cref="ServiceProvider" /> preserving the current factory.
	/// </summary>
	/// <param name="serviceProvider">The target service provider.</param>
	/// <returns>A new <see cref="ScopedOwnerImplementationEntry" /> instance.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider) => new ScopedOwnerImplementationEntry(serviceProvider, this);

	/// <summary>
	///     Creates a new scoped owner entry bound to another <see cref="ServiceProvider" /> using a new factory delegate.
	/// </summary>
	/// <param name="serviceProvider">The target service provider.</param>
	/// <param name="factory">Replacement factory delegate.</param>
	/// <returns>A new <see cref="ScopedOwnerImplementationEntry" /> instance.</returns>
	public override ImplementationEntry CreateNew(ServiceProvider serviceProvider, Delegate factory) => new ScopedOwnerImplementationEntry(serviceProvider, factory);

	/// <summary>
	///     Executes the underlying factory and registers the produced instance in the <see cref="ObjectsBin" /> for
	///     aggregated disposal (if the instance is disposable). Handles both already-completed and asynchronously
	///     completing <see cref="ValueTask{TResult}" /> paths without extra allocations when possible.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <typeparam name="TArg">Factory argument type.</typeparam>
	/// <param name="argument">Argument passed to the factory.</param>
	/// <returns>
	///     A <see cref="ValueTask{TResult}" /> that yields the created instance (or <c>null</c>).
	///     If the factory produced <c>null</c>, nothing is registered.
	/// </returns>
	/// <remarks>
	///     Fast path:
	///     <list type="bullet">
	///         <item>
	///             If the factory completes synchronously and returns a non-null instance, the registration attempt is
	///             performed immediately.
	///         </item>
	///         <item>If registration also completes synchronously, a completed <see cref="ValueTask{TResult}" /> is returned.</item>
	///         <item>Otherwise an async continuation awaits registration without capturing the context.</item>
	///     </list>
	///     Slow path:
	///     <list type="bullet">
	///         <item>
	///             If the factory returns an incomplete <see cref="ValueTask{TResult}" />, it is awaited, then the resulting
	///             instance is registered asynchronously.
	///         </item>
	///     </list>
	/// </remarks>
	protected override ValueTask<T?> ExecuteFactoryBase<T, TArg>(TArg argument) where T : default
	{
		var valueTask = base.ExecuteFactoryBase<T, TArg>(argument);

		if (!valueTask.IsCompletedSuccessfully)
		{
			return ExecuteFactoryWait(valueTask);
		}

		if (valueTask.Result is not { } instance)
		{
			return new ValueTask<T?>(default(T?));
		}

		var addValueTask = _objectsBin.AddAsync(instance);

		return addValueTask.IsCompletedSuccessfully ? new ValueTask<T?>(instance) : Wait(addValueTask, instance);

		static async ValueTask<T?> Wait(ValueTask valueTask, T value)
		{
			await valueTask.ConfigureAwait(false);

			return value;
		}
	}

	/// <summary>
	///     Await continuation for the factory invocation when its <see cref="ValueTask{TResult}" /> was not completed
	///     synchronously. Once the instance is produced it is registered in the <see cref="ObjectsBin" />.
	/// </summary>
	/// <typeparam name="T">Instance type.</typeparam>
	/// <param name="valueTask">The pending factory result task.</param>
	/// <returns>The produced instance (possibly <c>null</c>).</returns>
	/// <remarks>
	///     Registration gracefully ignores non-disposable instances. Disposal state violations propagate via
	///     <see cref="ObjectsBin.AddAsync{T}" />.
	/// </remarks>
	private async ValueTask<T?> ExecuteFactoryWait<T>(ValueTask<T?> valueTask)
	{
		var instance = await valueTask.ConfigureAwait(false);

		await _objectsBin.AddAsync(instance).ConfigureAwait(false);

		return instance;
	}
}