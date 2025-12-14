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

public static class ServiceProviderExtensions
{
	extension(IServiceProvider serviceProvider)
	{
		internal TDelegate GetServicesFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		internal TDelegate GetServicesSyncFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServicesSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		internal TDelegate GetRequiredFactoryBase<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceDelegate<T, TArg, TDelegate>() ?? throw MissedServiceException.Create<T, TArg>();

		internal TDelegate GetFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServiceDelegate<T, TArg, TDelegate>() ?? emptyDelegate;

		internal TDelegate GetRequiredSyncFactoryBase<T, TArg, TDelegate>() where T : notnull where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetRequiredServiceSyncDelegate<T, TArg, TDelegate>() ?? throw MissedServiceException.Create<T, TArg>();

		internal TDelegate GetSyncFactoryBase<T, TArg, TDelegate>(TDelegate emptyDelegate) where TDelegate : Delegate =>
			serviceProvider.GetImplementationEntry(TypeKey.ServiceKeyFast<T, TArg>())?.GetServiceSyncDelegate<T, TArg, TDelegate>() ?? emptyDelegate;
	}
}