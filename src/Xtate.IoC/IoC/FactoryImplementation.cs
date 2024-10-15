// Copyright © 2019-2024 Sergii Artemenko
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

	private readonly bool _synchronous;

	public FactoryImplementation(IServiceCollection serviceCollection, InstanceScope instanceScope, bool synchronous)
	{
		_serviceCollection = serviceCollection;
		_synchronous = synchronous;

		var factory = _synchronous
			? ImplementationSyncFactoryProvider<TImplementation, Empty>.Delegate()
			: ImplementationAsyncFactoryProvider<TImplementation, Empty>.Delegate();

		serviceCollection.Add(new ServiceEntry(TypeKey.ImplementationKey<TImplementation, Empty>(), instanceScope, factory));
	}

	public FactoryImplementation<TImplementation> For<TService>(Option option = Option.Default) => Register<TService, Empty>(default, option);

	public FactoryImplementation<TImplementation> For<TService, TArg>(Option option = Option.Default) => Register<TService, TArg>(default, Option.Default);

	public FactoryImplementation<TImplementation> For<TService, TArg1, TArg2>(Option option = Option.Default) => Register<TService, (TArg1, TArg2)>(default, option);

	public FactoryImplementation<TImplementation> For<TService>(SharedWithin sharedWithin, Option option = Option.Default) => Register<TService, Empty>(sharedWithin, option);

	public FactoryImplementation<TImplementation> For<TService, TArg>(SharedWithin sharedWithin, Option option = Option.Default) => Register<TService, TArg>(sharedWithin, Option.Default);

	public FactoryImplementation<TImplementation> For<TService, TArg1, TArg2>(SharedWithin sharedWithin, Option option = Option.Default) => Register<TService, (TArg1, TArg2)>(sharedWithin, option);

	private FactoryImplementation<TImplementation> Register<TService, TArg>(SharedWithin? sharedWithin, Option option)
	{
		option.Validate(sharedWithin is null ? Option.IfNotRegistered | Option.DoNotDispose : Option.IfNotRegistered);

		var key = TypeKey.ServiceKey<TService, TArg>();

		if (!option.Has(Option.IfNotRegistered) || !_serviceCollection.IsRegistered(key))
		{
			var factory = _synchronous
				? FactorySyncFactoryProvider<TImplementation, TService, TArg>.Delegate()
				: FactoryAsyncFactoryProvider<TImplementation, TService, TArg>.Delegate();

			var scope = sharedWithin switch
						{
							null                   => option.Has(Option.DoNotDispose) ? InstanceScope.Forwarding : InstanceScope.Transient,
							SharedWithin.Container => InstanceScope.Singleton,
							SharedWithin.Scope     => InstanceScope.Scoped,
							_                      => throw Infra.Unmatched(sharedWithin)
						};

			_serviceCollection.Add(new ServiceEntry(key, scope, factory));
		}

		return this;
	}
}