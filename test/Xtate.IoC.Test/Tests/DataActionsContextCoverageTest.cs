// Copyright © 2019-2026 Sergii Artemenko
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
public class DataActionsContextCoverageTest
{
	[TestMethod]
	public void ContextRoundTripsUserDataAndDataActionsConsumeEventType()
	{
		var context = new DataActionsContext<object, int>(42) { UserDataObject = "state" };
		IServiceProviderDataActions actions = new CapturingDataActions();

		actions.Event(ActionsEventType.FactoryCalling, ref context);

		Assert.AreEqual(expected: "state", context.UserDataObject);
		Assert.AreEqual((int)ActionsEventType.FactoryCalling, context.UserDataInt32);
	}

	private sealed class CapturingDataActions : IServiceProviderDataActions
	{
	#region Interface IServiceProviderDataActions

		public void RegisterService(ServiceEntry serviceEntry) => _ = serviceEntry;

		public void Event<T, TArg>(ActionsEventType eventType, ref DataActionsContext<T, TArg> context) => context.UserDataInt32 = (int)eventType;

	#endregion
	}
}