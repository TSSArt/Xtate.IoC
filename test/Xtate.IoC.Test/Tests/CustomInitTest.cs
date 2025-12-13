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

namespace Xtate.IoC.Test;

[TestClass]
public class CustomInitTest
{
	[TestMethod]
	public async Task NoInitRequired_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(null!);
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void NoInitRequiredSync_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(null!);
		sc.AddTypeSync<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = serviceProvider.GetServiceSync<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public async Task NoInitOptional_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(null!);
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public async Task CustomInitRequired_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(new CustomInitializationHandler(false));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public async Task CustomInitOptional_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(new CustomInitializationHandler(false));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public async Task CustomAsyncInitRequired_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(new CustomInitializationHandler(true));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public async Task CustomInitRequiredSync_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(new CustomInitializationHandler(false));
		sc.AddTypeSync<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	[TestMethod]
	public void CustomAsyncInitRequiredSync_ShouldThrowException()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(new CustomInitializationHandler(true));
		sc.AddTypeSync<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act & Assert
		Assert.ThrowsExactly<DependencyInjectionException>([ExcludeFromCodeCoverage]() => serviceProvider.GetRequiredServiceSync<Temp>());
	}

	[TestMethod]
	public async Task CustomAsyncInitOptional_ShouldReturnNotNull()
	{
		// Arrange
		var sc = new ServiceCollection();
		sc.AddConstant<IInitializationHandler>(new CustomInitializationHandler(true));
		sc.AddType<Temp>();
		var serviceProvider = sc.BuildProvider();

		// Act
		var obj = await serviceProvider.GetService<Temp>();

		// Assert
		Assert.IsNotNull(obj);
	}

	private class CustomInitializationHandler(bool async) : IInitializationHandler
	{
	#region Interface IInitializationHandler

		public bool Initialize<T>(T instance) => async;

		[ExcludeFromCodeCoverage]
		public ValueTask InitializeAsync<T>(T instance) => ValueTask.CompletedTask;

	#endregion
	}

	[UsedImplicitly]
	private class Temp;
}