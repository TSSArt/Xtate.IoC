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
		dataActions.Setup(providerDataActions => providerDataActions.Event(It.IsAny<ActionsEventType>(), ref It.Ref<DataActionsContext<IServiceProviderActions, ValueTuple>>.IsAny))
				   .Callback((ActionsEventType type, ref DataActionsContext<IServiceProviderActions, ValueTuple> ctx) =>
							 {
								 Assert.IsNotNull(ctx.TypeKey);

								 if (type is ActionsEventType.FactoryCallError or ActionsEventType.ServiceRequestError)
								 {
									 Assert.IsNotNull(ctx.Exception);
								 }
								 else
								 {
									 Assert.IsNull(ctx.Exception);
								 }

								 Assert.AreEqual(expected: default, ctx.Argument);

								 if (type is ActionsEventType.FactoryCalled or ActionsEventType.ServiceRequested)
								 {
									 Assert.IsNotNull(ctx.Instance);
								 }
								 else
								 {
									 Assert.IsNull(ctx.Instance);
								 }

								 Assert.AreEqual(expected: 0, ctx.UserDataInt32);
								 Assert.IsNull(ctx.UserDataObject);
							 });

		var actions = new Mock<IServiceProviderActions>();
		actions.Setup(x => x.Event(It.IsAny<ActionsEventType>(), ref It.Ref<ActionsContext>.IsAny)).Returns(dataActions.Object);
		actions.Setup(x => x.RegisterServices(It.IsAny<int>())).Returns(dataActions.Object);

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

		dataActions.Verify(x => x.Event(It.IsAny<ActionsEventType>(), ref It.Ref<DataActionsContext<IServiceProviderActions, ValueTuple>>.IsAny), Times.Exactly(16));
		dataActions.Verify(x => x.RegisterService(It.IsAny<ServiceEntry>()), Times.Exactly(2));
		dataActions.VerifyNoOtherCalls();
	}

	[UsedImplicitly]
	[ExcludeFromCodeCoverage]
	private class ServiceProviderDataActions : IServiceProviderDataActions
	{
	#region Interface IServiceProviderDataActions

		public void RegisterService(ServiceEntry serviceEntry) => throw new NotSupportedException();

		public void Event<T, TArg>(ActionsEventType type, ref DataActionsContext<T, TArg> context) => throw new NotSupportedException(context.Argument?.ToString());

	#endregion
	}
}