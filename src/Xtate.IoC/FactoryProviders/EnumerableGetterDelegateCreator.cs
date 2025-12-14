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

using System.Reflection;

namespace Xtate.IoC;

internal partial class ClassFactoryProvider
{
	private class EnumerableGetterDelegateCreator : GetterDelegateCreator
	{
		private static readonly MethodInfo GetServices;

		private static readonly MethodInfo GetServicesSync;

		static EnumerableGetterDelegateCreator()
		{
			GetServices = GetMethodInfo<EnumerableGetterDelegateCreator>(nameof(GetServicesWrapper));
			GetServicesSync = GetMethodInfo<EnumerableGetterDelegateCreator>(nameof(GetServicesSyncWrapper));
		}

		public override Delegate? TryCreate(MemberBase member)
		{
			if (IsEnumerable(member.Type, out var async) is { } serviceType)
			{
				return CreateDelegate(async ? GetServices : GetServicesSync, serviceType);
			}

			return null;
		}

		private static object GetServicesWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServices<T>();

		private static object GetServicesSyncWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSync<T>();
	}
}
