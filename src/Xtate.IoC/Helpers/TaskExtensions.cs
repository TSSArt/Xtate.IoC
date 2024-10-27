﻿// Copyright © 2019-2024 Sergii Artemenko
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

internal static class TaskExtensions
{
	/// <summary>
	///     Synchronously waits for the completion of the provided <see cref="ValueTask" />.
	/// </summary>
	/// <param name="valueTask">The <see cref="ValueTask" /> to wait for.</param>
	/// <exception cref="AggregateException">Thrown if the <see cref="ValueTask" /> completes in a faulted state.</exception>
	public static void SynchronousWait(this ValueTask valueTask)
	{
		if (valueTask.IsCompleted)
		{
			valueTask.GetAwaiter().GetResult();
		}
		else
		{
			valueTask.AsTask().GetAwaiter().GetResult();
		}
	}
}