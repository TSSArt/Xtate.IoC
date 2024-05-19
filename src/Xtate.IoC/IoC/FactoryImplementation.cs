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

public readonly struct FactoryImplementation<TImplementation> where TImplementation : notnull
{
	private readonly IServiceCollection _serviceCollection;
	private readonly bool               _synchronous;

	public FactoryImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope, bool synchronous)
	{
		Infra.Requires(serviceCollection);

		_serviceCollection = serviceCollection;
		_synchronous = synchronous;

		var factory = _synchronous
			? ImplementationSyncFactoryProvider<TImplementation, Empty>.Delegate()
			: ImplementationAsyncFactoryProvider<TImplementation, Empty>.Delegate();

		serviceCollection.Add(new ServiceEntry(TypeKey.ImplementationKey<TImplementation, Empty>(), instanceScope, factory));
	}

	public FactoryImplementation<TImplementation> For<TService>() where TService : class => For<TService, Empty>();

	public FactoryImplementation<TImplementation> For<TService, TArg>() where TService : class
	{
		var factory = _synchronous
			? FactorySyncFactoryProvider<TImplementation, TService, TArg>.Delegate()
			: FactoryAsyncFactoryProvider<TImplementation, TService, TArg>.Delegate();

		_serviceCollection.Add(new ServiceEntry(TypeKey.ServiceKey<TService, TArg>(), InstanceScope.Forwarding, factory));

		return this;
	}

	public FactoryImplementation<TImplementation> For<TService, TArg1, TArg2>() where TService : class => For<TService, (TArg1, TArg2)>();
}