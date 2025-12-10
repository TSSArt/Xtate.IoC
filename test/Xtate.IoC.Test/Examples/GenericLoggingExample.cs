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

// ReSharper disable All

namespace Xtate.IoC.Examples;

// Demonstrates open generic registration and resolution.
// Pattern: register implementation with placeholder generic parameter `Any` then forward to matching service interface.
// services.AddImplementation<Log<Any>>().For<ILog<Any>>();
// Resolving ILog<MyType> gives Log<MyType> with MyType substituted in place of Any.
// This mirrors typical scenarios like ILogger<TCategory> / Log<TSource>.

public interface ILog<TSource>
{
	string Category { get; }

	string Format(string message);
}

public class Log<TSource> : ILog<TSource>
{
#region Interface ILog<TSource>

	public string Format(string message) => Category + ": " + message;

	public string Category => typeof(TSource).Name;

#endregion
}

public class Alpha;

public class Beta;

public class Gamma;

[TestClass]
public class GenericLoggingExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 // Open generic registration
														 services.AddImplementation<Log<Any>>().For<ILog<Any>>();
													 });

		var alphaLogger = await container.GetRequiredService<ILog<Alpha>>();
		var betaLogger = await container.GetRequiredService<ILog<Beta>>();
		var gammaLogger = await container.GetRequiredService<ILog<Gamma>>();

		Assert.AreEqual(expected: "Alpha", alphaLogger.Category);
		Assert.AreEqual(expected: "Beta", betaLogger.Category);
		Assert.AreEqual(expected: "Gamma", gammaLogger.Category);
		Assert.AreEqual(expected: "Alpha: message", alphaLogger.Format("message"));
		Assert.AreEqual(expected: "Beta: test", betaLogger.Format("test"));
		Assert.AreEqual(expected: "Gamma: ping", gammaLogger.Format("ping"));
	}
}