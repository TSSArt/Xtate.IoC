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

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceProviderDebuggerTest
{
	[TestMethod]
	public async Task RegisterServiceProviderDebuggerTest()
	{
		// Arrange
		var dbg = new Actions();
		var sc = new ServiceCollection();
		sc.AddTransient<IServiceProviderActions>(_ => dbg);
		sc.AddType<ServiceProviderDebuggerTest>();
		var sp = sc.BuildProvider();

		// Act
		var rService = await sp.GetRequiredService<IServiceProviderActions>();
		var oService = await sp.GetService<IServiceProviderActions>();
		var rServiceSync = sp.GetRequiredServiceSync<IServiceProviderActions>();
		var oServiceSync = sp.GetServiceSync<IServiceProviderActions>();

		// Assert
		Assert.AreSame(rService, dbg);
		Assert.AreSame(oService, dbg);
		Assert.AreSame(rServiceSync, dbg);
		Assert.AreSame(oServiceSync, dbg);
	}

	private class Actions : IServiceProviderActions, IServiceProviderDataActions
	{
	#region Interface IServiceProviderActions

		public IServiceProviderDataActions RegisterServices() => this;

		public IServiceProviderDataActions ServiceRequesting(TypeKey typeKey) => this;

		public IServiceProviderDataActions ServiceRequested(TypeKey typeKey) => this;

		public IServiceProviderDataActions FactoryCalling(TypeKey typeKey) => this;

		public IServiceProviderDataActions FactoryCalled(TypeKey typeKey) => this;

	#endregion

	#region Interface IServiceProviderDataActions

		public void RegisterService(ServiceEntry serviceEntry) { }

		public void ServiceRequesting<T, TArg>(TArg argument) { }

		public void ServiceRequested<T, TArg>(T? instance) { }

		public void FactoryCalling<T, TArg>(TArg argument) { }

		public void FactoryCalled<T, TArg>(T? instance) { }

	#endregion
	}
}