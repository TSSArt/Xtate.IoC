﻿// Copyright © 2019-2024 Sergii Artemenko
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
public class ServiceCollectionTest
{
	[TestMethod]
	public async Task NewServiceCollectionTest()
	{
		// Arrange
		var sc = new ServiceCollectionNew();
		var sp = sc.BuildProvider();

		// Act
		var serviceScope = await sp.GetRequiredService<IServiceScopeFactory>();

		// Assert
		Assert.IsNotNull(serviceScope);
	}

	private class ServiceProviderNew(ServiceCollectionNew services) : ServiceProvider(services)
	{
		protected override void Dispose(bool disposing) { }

		protected override ValueTask DisposeAsyncCore() => default;
	}

	private class ServiceCollectionNew : ServiceCollection
	{
		public override IServiceProvider BuildProvider() => new ServiceProviderNew(this);
	}
}