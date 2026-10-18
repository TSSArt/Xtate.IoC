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
public class ArgumentValidationTest
{
	[TestMethod]
	public void NullProvider_FailsAtLookupForBothEnumerationForms()
	{
		IServiceProvider provider = null!;
		Assert.ThrowsExactly<NullReferenceException>(provider.GetServices<object>);
		Assert.ThrowsExactly<NullReferenceException>(provider.GetServicesSync<object>);
		Assert.ThrowsExactly<NullReferenceException>(() => provider.GetService<object>());
		Assert.ThrowsExactly<NullReferenceException>(provider.GetServiceSync<object>);
	}

	[TestMethod]
	public async Task NullFactory_FailsOnResolutionInBothForms()
	{
		await using var provider = Container.Create(services => services.AddTransient((Func<IServiceProvider, object>)null!));

		// The callback completes before the enclosing resource scope ends.
		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<NullReferenceException>(provider.GetRequiredServiceSync<object>);
		await Assert.ThrowsExactlyAsync<NullReferenceException>(() => provider.GetRequiredService<object>().AsTask());
	}
}