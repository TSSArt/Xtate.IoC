﻿#region Copyright © 2019-2023 Sergii Artemenko

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

#endregion

namespace Xtate.IoC;

public class ServiceCollection : IServiceCollection
{
	private readonly List<ServiceEntry> _registrations = [];

#region Interface IEnumerable

	IEnumerator IEnumerable.GetEnumerator() => _registrations.GetEnumerator();

#endregion

#region Interface IEnumerable<ServiceEntry>

	IEnumerator<ServiceEntry> IEnumerable<ServiceEntry>.GetEnumerator() => _registrations.GetEnumerator();

#endregion

#region Interface IReadOnlyCollection<ServiceEntry>

	int IReadOnlyCollection<ServiceEntry>.Count => _registrations.Count;

#endregion

#region Interface IServiceCollection

	public void Add(ServiceEntry serviceEntry) => _registrations.Add(serviceEntry);

#endregion

	public List<ServiceEntry>.Enumerator GetEnumerator() => _registrations.GetEnumerator();

	public virtual IServiceProvider BuildProvider() => new ServiceProvider(this);
}