// Copyright © 2019-2026 Sergii Artemenko
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

/// <summary>Provides registration lookup, initialization and resolution callbacks for the container extensions.</summary>
public interface IServiceProvider
{
	/// <summary>Signals that provider disposal has started; it does not wait for in-flight service use.</summary>
	CancellationToken DisposeToken { get; }

	/// <summary>Gets the optional handler used to initialize resolved instances.</summary>
	IInitializationHandler? InitializationHandler { get; }

	/// <summary>Gets optional resolution callbacks. Callback exceptions can replace the failure being reported.</summary>
	IServiceProviderActions[]? Actions { get; }

	/// <summary>Gets the registration chain for a key, or null when absent. Lookup remains available after disposal.</summary>
	/// <remarks>Lookup may create cached entries; it does not create or initialize service instances.</remarks>
	ImplementationEntry? GetImplementationEntry(TypeKey typeKey);
}