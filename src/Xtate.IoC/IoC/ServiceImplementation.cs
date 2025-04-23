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

public readonly struct ServiceImplementation<TImplementation, TArg> where TImplementation : notnull
{
	private readonly IServiceCollection _serviceCollection;

	private readonly bool _synchronous;

	public ServiceImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope, bool synchronous)
	{
		_serviceCollection = serviceCollection;
		_synchronous = synchronous;

		var factory = _synchronous
			? ImplementationSyncFactoryProvider<TImplementation, TArg>.Delegate()
			: ImplementationAsyncFactoryProvider<TImplementation, TArg>.Delegate();

		serviceCollection.Add(new ServiceEntry(TypeKey.ImplementationKey<TImplementation, TArg>(), instanceScope, factory));
	}

	public ServiceImplementation<TImplementation, TArg> For<TService>(Option option = Option.Default)
	{
		option.Validate(Option.IfNotRegistered);

		var key = TypeKey.ServiceKey<TService, TArg>();

		if (option.Has(Option.IfNotRegistered) && _serviceCollection.IsRegistered(key))
		{
			return this;
		}

		var factory = _synchronous
			? ForwardSyncFactoryProvider<TImplementation, TService, TArg>.Delegate()
			: ForwardAsyncFactoryProvider<TImplementation, TService, TArg>.Delegate();

		_serviceCollection.Add(new ServiceEntry(key, InstanceScope.Forwarding, factory));

		return this;
	}
}