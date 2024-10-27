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
public class AsyncInitializationHandlerTest
{
	[TestMethod]
	public async Task Initialize_ShouldReturnFalse_WhenObjectIsNotAsyncInitializable()
	{
		// Arrange
		var obj = new Class();

		// Act
		var init = AsyncInitializationHandler.Instance.Initialize(obj);
		await AsyncInitializationHandler.Instance.InitializeAsync(obj);

		// Assert
		Assert.IsFalse(init);
	}

	[TestMethod]
	public async Task Initialize_ShouldReturnTrue_WhenObjectIsAsyncInitializable()
	{
		// Arrange
		var objAsyncInit = new ClassAsyncInit();

		// Act
		var init = AsyncInitializationHandler.Instance.Initialize(objAsyncInit);
		await AsyncInitializationHandler.Instance.InitializeAsync(objAsyncInit);

		// Assert
		Assert.IsTrue(init);
		Assert.IsTrue(objAsyncInit.Init);
	}

	[TestMethod]
	public async Task Initialize_ShouldReturnFalse_WhenObjectIsNull()
	{
		// Arrange
		ClassAsyncInit? nullObjAsyncInit = null;

		// Act
		var init = AsyncInitializationHandler.Instance.Initialize(nullObjAsyncInit);
		await AsyncInitializationHandler.Instance.InitializeAsync(nullObjAsyncInit);

		// Assert
		Assert.IsFalse(init);
	}

	// ReSharper disable All
	public class Class { }

	public class ClassAsyncInit : IAsyncInitialization
	{
		public bool Init;

	#region Interface IAsyncInitialization

		public Task Initialization
		{
			get
			{
				Init = true;

				return Task.CompletedTask;
			}
		}

	#endregion
	}

	// ReSharper restore All
}