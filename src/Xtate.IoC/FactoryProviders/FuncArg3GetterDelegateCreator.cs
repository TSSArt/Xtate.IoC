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
	[ExcludeFromCodeCoverage]
	private class FuncArg3GetterDelegateCreator : FuncWithArgsGetterDelegateCreator
	{
		protected override MethodInfo GetFactory { get; } = GetMethodInfo<FuncArg3GetterDelegateCreator>(nameof(GetFactoryWrapper));

		protected override MethodInfo GetSyncFactory { get; } = GetMethodInfo<FuncArg3GetterDelegateCreator>(nameof(GetSyncFactoryWrapper));

		protected override MethodInfo GetServicesFactory { get; } = GetMethodInfo<FuncArg3GetterDelegateCreator>(nameof(GetServicesFactoryWrapper));

		protected override MethodInfo GetServicesSyncFactory { get; } = GetMethodInfo<FuncArg3GetterDelegateCreator>(nameof(GetServicesSyncFactoryWrapper));

		protected override MethodInfo GetRequiredFactory { get; } = GetMethodInfo<FuncArg3GetterDelegateCreator>(nameof(GetRequiredFactoryWrapper));

		protected override MethodInfo GetRequiredSyncFactory { get; } = GetMethodInfo<FuncArg3GetterDelegateCreator>(nameof(GetRequiredSyncFactoryWrapper));

		protected override string AsyncPath => @"30";

		protected override string SyncPath => @"3";

		protected override (Type? Type, Type[]? ArgTypes) IsFuncN(Type type) =>
			type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Func<,,,>) && type.GetGenericArguments() is { } args ? (args[3], [args[0], args[1], args[2]]) : default;

		private static object GetRequiredFactoryWrapper<T, TArg1, TArg2, TArg3>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredFactory<T, TArg1, TArg2, TArg3>();

		private static object GetFactoryWrapper<T, TArg1, TArg2, TArg3>(IServiceProvider serviceProvider) => serviceProvider.GetFactory<T, TArg1, TArg2, TArg3>();

		private static object GetRequiredSyncFactoryWrapper<T, TArg1, TArg2, TArg3>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetRequiredSyncFactory<T, TArg1, TArg2, TArg3>();

		private static object GetSyncFactoryWrapper<T, TArg1, TArg2, TArg3>(IServiceProvider serviceProvider) => serviceProvider.GetSyncFactory<T, TArg1, TArg2, TArg3>();

		private static object GetServicesFactoryWrapper<T, TArg1, TArg2, TArg3>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesFactory<T, TArg1, TArg2, TArg3>();

		private static object GetServicesSyncFactoryWrapper<T, TArg1, TArg2, TArg3>(IServiceProvider serviceProvider) where T : notnull => serviceProvider.GetServicesSyncFactory<T, TArg1, TArg2, TArg3>();

	}
}
