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
	private class FuncNoArgsGetterDelegateCreator : GetterDelegateCreator
	{
		private static readonly MethodInfo GetFactory;

		private static readonly MethodInfo GetSyncFactory;

		private static readonly MethodInfo GetRequiredFactory;

		private static readonly MethodInfo GetRequiredSyncFactory;

		static FuncNoArgsGetterDelegateCreator()
		{
			GetFactory = GetMethodInfo<FuncNoArgsGetterDelegateCreator>(nameof(GetFactoryWrapper));
			GetRequiredFactory = GetMethodInfo<FuncNoArgsGetterDelegateCreator>(nameof(GetRequiredFactoryWrapper));
			GetSyncFactory = GetMethodInfo<FuncNoArgsGetterDelegateCreator>(nameof(GetSyncFactoryWrapper));
			GetRequiredSyncFactory = GetMethodInfo<FuncNoArgsGetterDelegateCreator>(nameof(GetRequiredSyncFactoryWrapper));
		}

		public override Delegate? TryCreate(MemberBase member)
		{
			if (IsFunc(member.Type) is { } resultType)
			{
				if (IsValueTask(resultType) is { } serviceType)
				{
					return CreateDelegate(member.IsNotNull(@"00") ? GetRequiredFactory : GetFactory, serviceType);
				}

				return CreateDelegate(member.IsNotNull(@"0") ? GetRequiredSyncFactory : GetSyncFactory, resultType);
			}

			return null;
		}

		private static object GetRequiredFactoryWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T>();

		private static object GetFactoryWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetFactory<T>();

		private static object GetRequiredSyncFactoryWrapper<T>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T>();

		private static object GetSyncFactoryWrapper<T>(IServiceProvider serviceProvider) => serviceProvider.GetSyncFactory<T>();
	}
}
