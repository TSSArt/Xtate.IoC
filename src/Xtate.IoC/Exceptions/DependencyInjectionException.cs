﻿// Copyright © 2019-2025 Sergii Artemenko
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
///     Represents errors that occur during dependency injection.
/// </summary>
[Serializable]
public class DependencyInjectionException : Exception
{
	/// <summary>
	///     Initializes a new instance of the <see cref="DependencyInjectionException" /> class.
	/// </summary>
	public DependencyInjectionException() { }

	/// <summary>
	///     Initializes a new instance of the <see cref="DependencyInjectionException" /> class with a specified error message.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public DependencyInjectionException(string? message) : base(message) { }

	/// <summary>
	///     Initializes a new instance of the <see cref="DependencyInjectionException" /> class with a specified error message
	///     and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="innerException">
	///     The exception that is the cause of the current exception, or a null reference if no inner
	///     exception is specified.
	/// </param>
	public DependencyInjectionException(string? message, Exception? innerException) : base(message, innerException) { }
}