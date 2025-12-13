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

#if !NET10_0_OR_GREATER
namespace System.Runtime.InteropServices;

internal struct WeakGCHandle<T>(T target) where T : class
{
	private GCHandle _handle = GCHandle.Alloc(target, GCHandleType.Weak);

	public void Dispose()
	{
		if (!_handle.IsAllocated)
		{
			return;
		}

		try
		{
			_handle.Free();
		}
		catch (InvalidOperationException)
		{
			// already freed
		}
	}

	public readonly bool TryGetTarget([NotNullWhen(true)] out T? target)
	{
		if (_handle.Target is T val)
		{
			target = val;

			return true;
		}

		target = null;

		return false;
	}

	public void SetTarget(T target) => _handle.Target = target;
}

#endif