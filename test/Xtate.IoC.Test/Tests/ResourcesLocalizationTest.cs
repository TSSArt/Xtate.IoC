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

using System.Globalization;
using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class ResourcesLocalizationTest
{
	private class ServiceX;

	[TestMethod]
	public void French_Resources_ShouldFormatExpectedStrings()
	{
		var previous = GetAndSetCulture(new CultureInfo("fr"));

		try
		{
			const double value = 1.5;

			var msg1 = Resources.Exception_ServiceMissedInContainer(typeof(ServiceX));
			var msg2 = Resources.Exception_AssertUnmatched(typeof(double), value);

			// ReSharper disable StringLiteralTypo
			Assert.AreEqual(expected: "Le service [ResourcesLocalizationTest.ServiceX] est manquant dans le conteneur", msg1);
			Assert.AreEqual(expected: "Valeur non correspondante (double:1,5)", msg2);

			// ReSharper enable StringLiteralTypo
		}
		finally
		{
			Thread.CurrentThread.CurrentUICulture = previous;
		}
	}

	[TestMethod]
	public void French_IoC_ShouldThrowLocalizedException_ForMissingService()
	{
		var previous = GetAndSetCulture(new CultureInfo("fr"));

		try
		{
			var container = new TestContainer();

			var ex = Assert.ThrowsExactly<InvalidOperationException>(() => container.Resolve(typeof(ServiceX)));
			var expected = Resources.Exception_ServiceMissedInContainer(typeof(ServiceX));

			Assert.AreEqual(expected, ex.Message);
		}
		finally
		{
			Thread.CurrentThread.CurrentUICulture = previous;
		}
	}

	private static CultureInfo GetAndSetCulture(CultureInfo culture)
	{
		var prevUi = Thread.CurrentThread.CurrentUICulture;
		Thread.CurrentThread.CurrentUICulture = culture;

		return prevUi;
	}

	private sealed class TestContainer
	{
		// ReSharper disable once MemberCanBeMadeStatic.Local
#pragma warning disable CA1822
		public object Resolve(Type type) => throw new InvalidOperationException(Resources.Exception_ServiceMissedInContainer(type));
#pragma warning restore CA1822
	}
}