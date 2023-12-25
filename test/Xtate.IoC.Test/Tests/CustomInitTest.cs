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
public class CustomInitTest
{
	[TestMethod]
	public void CustomInitTest_NoInitRequiredTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => null!);
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetRequiredService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_NoInitRequiredSyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => null!);
		sc.AddTypeSync<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetRequiredServiceSync<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_NoInitOptionalTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => null!);
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetOptionalService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_CustomInitRequiredTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => new CustomInitializationHandler(false));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetRequiredService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_CustomInitOptionalTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => new CustomInitializationHandler(false));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetOptionalService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_CustomAsyncInitRequiredTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => new CustomInitializationHandler(true));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetRequiredService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_CustomInitRequiredSyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => new CustomInitializationHandler(false));
		sc.AddTypeSync<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetRequiredService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomInitTest_CustomAsyncInitRequiredSyncTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => new CustomInitializationHandler(true));
		sc.AddTypeSync<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act

		// Assert
		Assert.ThrowsException<DependencyInjectionException>([ExcludeFromCodeCoverage]() => serviceProvider.GetRequiredServiceSync<Temp>());
	}

	[TestMethod]
	public void CustomInitTest_CustomAsyncInitOptionalTest()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddForwarding<IInitializationHandler>(_ => new CustomInitializationHandler(true));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetOptionalService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	private class CustomInitializationHandler(bool async) : IInitializationHandler
	{
	#region Interface IInitializationHandler

		public bool Initialize<T>(T instance) => async;

		[ExcludeFromCodeCoverage]
		public Task InitializeAsync<T>(T instance) => Task.CompletedTask;

	#endregion
	}

	[UsedImplicitly]
	private class Temp;
}