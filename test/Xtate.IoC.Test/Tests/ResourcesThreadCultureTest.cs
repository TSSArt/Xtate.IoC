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
public class ResourcesThreadCultureTest
{
	private class ServiceX;

	[TestMethod]
	public void French_Resources_ShouldRespect_Thread_CurrentUICulture()
	{
		var prevUi = Thread.CurrentThread.CurrentUICulture;
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr");

		try
		{
			const double value = 1.5;

			var msg1 = Resources.Exception_ServiceMissedInContainer(typeof(ServiceX));
			var msg2 = Resources.Exception_AssertUnmatched(typeof(double), value);

			// ReSharper disable StringLiteralTypo
			Assert.AreEqual(expected: "Le service [ResourcesThreadCultureTest.ServiceX] est manquant dans le conteneur", msg1);
			Assert.AreEqual(expected: "Valeur non correspondante (double:1,5)", msg2);

			// ReSharper enable StringLiteralTypo
		}
		finally
		{
			Thread.CurrentThread.CurrentUICulture = prevUi;
		}
	}
}