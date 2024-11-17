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

using Moq;

namespace Xtate.IoC.Test;

[TestClass]
public class ServiceProviderActionsTest
{
	[TestMethod]
	public async Task RegisterServiceProviderActions_ShouldRegisterAndResolveServicesCorrectly()
	{
		// Arrange
		var dataActions = new Mock<IServiceProviderDataActions>();

		var actions = new Mock<IServiceProviderActions>();
		actions.Setup(x => x.ServiceRequesting(It.IsAny<TypeKey>())).Returns(dataActions.Object);
		actions.Setup(x => x.ServiceRequested(It.IsAny<TypeKey>())).Returns(dataActions.Object);
		actions.Setup(x => x.FactoryCalling(It.IsAny<TypeKey>())).Returns(dataActions.Object);
		actions.Setup(x => x.FactoryCalled(It.IsAny<TypeKey>())).Returns(dataActions.Object);

		var sc = new ServiceCollection();
		sc.AddForwarding<IServiceProviderActions>(_ => actions.Object);
		sc.AddType<ServiceProviderActionsTest>();
		var sp = sc.BuildProvider();

		// Act
		var rService = await sp.GetRequiredService<IServiceProviderActions>();
		var oService = await sp.GetService<IServiceProviderActions>();
		var rServiceSync = sp.GetRequiredServiceSync<IServiceProviderActions>();
		var oServiceSync = sp.GetServiceSync<IServiceProviderActions>();

		// Assert
		Assert.AreSame(rService, actions.Object);
		Assert.AreSame(oService, actions.Object);
		Assert.AreSame(rServiceSync, actions.Object);
		Assert.AreSame(oServiceSync, actions.Object);

		dataActions.Verify(x => x.ServiceRequesting<IServiceProviderActions, ValueTuple>(It.IsAny<ValueTuple>()), Times.Exactly(4));
		dataActions.Verify(x => x.ServiceRequested<IServiceProviderActions, ValueTuple>(It.IsAny<IServiceProviderActions>()), Times.Exactly(4));
		dataActions.Verify(x => x.FactoryCalling<IServiceProviderActions, ValueTuple>(It.IsAny<ValueTuple>()), Times.Exactly(4));
		dataActions.Verify(x => x.FactoryCalled<IServiceProviderActions, ValueTuple>(It.IsAny<IServiceProviderActions>()), Times.Exactly(4));
	}

	[UsedImplicitly]
	[ExcludeFromCodeCoverage]
	private class ServiceProviderDataActions : IServiceProviderDataActions
	{
	#region Interface IServiceProviderDataActions

		public void RegisterService(ServiceEntry serviceEntry) => throw new NotSupportedException();

		public void ServiceRequesting<T, TArg>(TArg argument) => throw new NotSupportedException(argument?.ToString());

		public void ServiceRequested<T, TArg>(T? instance) => throw new NotSupportedException(instance?.ToString());

		public void FactoryCalling<T, TArg>(TArg argument) => throw new NotSupportedException(argument?.ToString());

		public void FactoryCalled<T, TArg>(T? instance) => throw new NotSupportedException(instance?.ToString());

	#endregion
	}
}