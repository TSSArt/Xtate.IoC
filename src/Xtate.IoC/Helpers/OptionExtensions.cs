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

internal static class OptionExtensions
{
    /// <summary>
    ///     Validates the specified option against the allowed options.
    /// </summary>
    /// <param name="option">The option to validate.</param>
    /// <param name="allowedOptions">The allowed options.</param>
    /// <exception cref="ArgumentException">Thrown when the option contains unsupported options.</exception>
    public static void Validate(this Option option, Option allowedOptions)
    {
        if ((option & ~allowedOptions) is var notSupportedOptions && notSupportedOptions != 0)
        {
            throw new ArgumentException(Res.Format(Resources.Exception_OptionDoesNotSupport, notSupportedOptions));
        }
    }

    /// <summary>
    ///     Determines whether the specified option contains the option to check.
    /// </summary>
    /// <param name="option">The option to check.</param>
    /// <param name="toCheck">The option to check for.</param>
    /// <returns><c>true</c> if the specified option contains the option to check; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(this Option option, Option toCheck) => (option & toCheck) == toCheck;
}