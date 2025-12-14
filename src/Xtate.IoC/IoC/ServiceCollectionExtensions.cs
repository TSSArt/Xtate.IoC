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

public static class ServiceCollectionExtensions
{
	extension(SharedWithin sharedWithin)
	{
		internal InstanceScope GetScope() =>
			sharedWithin switch
			{
				SharedWithin.Container => InstanceScope.Singleton,
				SharedWithin.Scope     => InstanceScope.Scoped,
				_                      => throw Infra.Unmatched(sharedWithin)
			};
	}

	extension(IServiceCollection services)
	{
		internal void AddEntry(TypeKey serviceKey, InstanceScope instanceScope, Delegate factory) => services.Add(new ServiceEntry(serviceKey, instanceScope, factory));

		internal void AddEntry(TypeKey serviceKey,
							   InstanceScope instanceScope,
							   Option option,
							   Delegate factory)
		{
			option.Validate(Option.IfNotRegistered);

			if (!option.Has(Option.IfNotRegistered) || !services.IsRegistered(serviceKey))
			{
				services.Add(new ServiceEntry(serviceKey, instanceScope, factory));
			}
		}

		public void AddModule<TModule>() where TModule : IModule, new()
		{
			if (!services.IsRegistered(TypeKey.ImplementationKeyFast<TModule, Empty>()))
			{
				new TModule { Services = services }.AddServices();

				services.AddImplementationSync<TModule>().For<IModule>();
			}
		}

		public FactoryImplementation<T> AddFactory<T>() where T : notnull => new(services, InstanceScope.Transient, synchronous: false);

		public FactoryImplementation<T> AddFactorySync<T>() where T : notnull => new(services, InstanceScope.Transient, synchronous: true);

		public FactoryImplementation<T> AddSharedFactory<T>(SharedWithin sharedWithin) where T : notnull => new(services, sharedWithin.GetScope(), synchronous: false);

		public FactoryImplementation<T> AddSharedFactorySync<T>(SharedWithin sharedWithin) where T : notnull => new(services, sharedWithin.GetScope(), synchronous: true);
	}
}