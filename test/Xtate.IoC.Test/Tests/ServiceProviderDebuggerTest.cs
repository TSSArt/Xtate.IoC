// Copyright © 2019-2023 Sergii Artemenko
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
		var dbg = new Debugger();
		var sc = new ServiceCollection();
		sc.AddTransient<IServiceProviderDebugger>(_ => dbg);
		sc.AddType<ServiceProviderDebuggerTest>();
		var sp = sc.BuildProvider();

		// Act
		var rService = await sp.GetRequiredService<IServiceProviderDebugger>();
		var oService = await sp.GetOptionalService<IServiceProviderDebugger>();
		var rServiceSync = sp.GetRequiredServiceSync<IServiceProviderDebugger>();
		var oServiceSync = sp.GetOptionalServiceSync<IServiceProviderDebugger>();

		// Assert
		Assert.AreSame(rService, dbg);
		Assert.AreSame(oService, dbg);
		Assert.AreSame(rServiceSync, dbg);
		Assert.AreSame(oServiceSync, dbg);
	}

	private class Debugger : IServiceProviderDebugger
	{
	#region Interface IServiceProviderDebugger

		public void AfterFactory(TypeKey serviceKey) { }

		public void BeforeFactory(TypeKey serviceKey) { }

		public void FactoryCalled(TypeKey serviceKey) { }

		public void RegisterService(ServiceEntry serviceEntry) { }

	#endregion
	}
}