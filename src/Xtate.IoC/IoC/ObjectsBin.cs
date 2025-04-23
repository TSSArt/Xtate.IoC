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

public class ObjectsBin
{
	private WeakReferenceStack? _instancesForDispose = new();

	internal async ValueTask DisposeAsync()
	{
		if (Interlocked.CompareExchange(ref _instancesForDispose, value: null, _instancesForDispose) is { } instancesForDispose)
		{
			while (instancesForDispose.TryPop(out var instance))
			{
				await Disposer.DisposeAsync(instance).ConfigureAwait(false);
			}
		}
	}

	internal void Dispose()
	{
		if (Interlocked.CompareExchange(ref _instancesForDispose, value: null, _instancesForDispose) is { } instancesForDispose)
		{
			while (instancesForDispose.TryPop(out var instance))
			{
				Disposer.Dispose(instance);
			}
		}
	}

	public ValueTask AddAsync<T>(T instance)
	{
		if (!Disposer.IsDisposable(instance))
		{
			XtateObjectDisposedException.ThrowIf(_instancesForDispose is null, this);

			return ValueTaskExt.CompletedTask;
		}

		if (_instancesForDispose is { } instancesForDispose)
		{
			instancesForDispose.Push(instance);

			return ValueTaskExt.CompletedTask;
		}

		return DisposeAndThrow();

		async ValueTask DisposeAndThrow()
		{
			await Disposer.DisposeAsync(instance).ConfigureAwait(false);

			XtateObjectDisposedException.ThrowIf(condition: true, this);
		}
	}

	public void AddSync<T>(T instance)
	{
		if (!Disposer.IsDisposable(instance))
		{
			XtateObjectDisposedException.ThrowIf(_instancesForDispose is null, this);

			return;
		}

		if (_instancesForDispose is { } instancesForDispose)
		{
			instancesForDispose.Push(instance);

			return;
		}

		Disposer.Dispose(instance);

		XtateObjectDisposedException.ThrowIf(condition: true, this);
	}
}