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
///     Specifies the scope of an instance in the IoC container.
/// </summary>
public enum InstanceScope
{
    /// <summary>
    ///     A new instance is created every time it is requested.
    /// </summary>
    Transient,

    /// <summary>
    ///     The instance is forwarded from another source and the IoC container is not responsible for its disposal.
    /// </summary>
    Forwarding,

    /// <summary>
    ///     A single instance is created and shared within a defined scope.
    /// </summary>
    Scoped,

    /// <summary>
    ///     A single instance is created and shared within a defined scope, and the IoC container is not responsible for its
    ///     disposal.
    /// </summary>
    ScopedExternal,

    /// <summary>
    ///     A single instance is created and shared across the container.
    /// </summary>
    Singleton,

    /// <summary>
    ///     A single instance is created and shared across the container, and the IoC container is not responsible for its
    ///     disposal.
    /// </summary>
    SingletonExternal
}