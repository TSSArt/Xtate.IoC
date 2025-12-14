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
	private abstract class FuncWithArgsGetterDelegateCreator : GetterDelegateCreator
	{
		protected abstract MethodInfo GetFactory { get; }

		protected abstract MethodInfo GetSyncFactory { get; }

		protected abstract MethodInfo GetServicesFactory { get; }

		protected abstract MethodInfo GetServicesSyncFactory { get; }

		protected abstract MethodInfo GetRequiredFactory { get; }

		protected abstract MethodInfo GetRequiredSyncFactory { get; }

		protected abstract string AsyncPath { get; }

		protected abstract string SyncPath { get; }

		protected abstract (Type? Type, Type[]? ArgTypes) IsFuncN(Type type);

		public override Delegate? TryCreate(MemberBase member)
		{
			if (IsFuncN(member.Type) is { Type: { } resultType, ArgTypes: { } argTypes })
			{
				if (IsEnumerable(resultType, out var async) is { } itemServiceType)
				{
					return CreateDelegate(async ? GetServicesFactory : GetServicesSyncFactory, [itemServiceType, ..argTypes]);
				}

				if (IsValueTask(resultType) is { } serviceType)
				{
					return CreateDelegate(member.IsNotNull(AsyncPath) ? GetRequiredFactory : GetFactory, [serviceType, ..argTypes]);
				}

				return CreateDelegate(member.IsNotNull(SyncPath) ? GetRequiredSyncFactory : GetSyncFactory, [resultType, ..argTypes]);
			}

			return null;
		}
	}
}
