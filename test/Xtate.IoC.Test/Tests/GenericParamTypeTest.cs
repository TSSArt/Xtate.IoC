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

// ReSharper disable ClassNeverInstantiated.Local

namespace Xtate.IoC.Test;

[TestClass]
public class GenericParamTypeTest
{
	[TestMethod]
	public void SubTypeInt32_ShouldReturnCorrectValue()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddTypeSync<Gen<Any>, Any>();
		var serviceProvider = services.BuildProvider();

		// Act
		var service = serviceProvider.GetRequiredServiceSync<Gen<int>, int>(6);

		// Assert
		Assert.AreEqual(expected: 6, service.Val);
	}

	[TestMethod]
	public async Task GenericParticular_ShouldReturnNonNullServices()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddConstant<IInterface<string>>(new Class<string>());
		services.AddConstant<IInterface<Version>>(new Class<Version>());
		var serviceProvider = services.BuildProvider();

		// Act
		var service1 = await serviceProvider.GetRequiredService<IInterface<string>>();
		var service2 = await serviceProvider.GetRequiredService<IInterface<Version>>();

		// Assert
		Assert.IsNotNull(service1);
		Assert.IsNotNull(service2);
	}

	private interface IInterface<[UsedImplicitly] T>;

	private class Class<T> : IInterface<T>;

	private class Gen<T>(T val)
	{
		public T Val { get; } = val;
	}
}