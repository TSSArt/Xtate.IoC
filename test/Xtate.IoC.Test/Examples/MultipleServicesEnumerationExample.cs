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

// Demonstrates resolving multiple services via GetServices<T>() and GetServices<T,TArg>(arg)
// Each registration creates a new entry; enumeration executes each factory and yields non-null results.

public interface IProcessor
{
	string Id { get; }
}

public class AlphaProcessor : IProcessor
{
#region Interface IProcessor

	public string Id => "Alpha";

#endregion
}

public class BetaProcessor : IProcessor
{
#region Interface IProcessor

	public string Id => "Beta";

#endregion
}

// Argument-based service
public class ValueTaggedService(int value)
{
	public int Value { get; } = value;
}

[TestClass]
public class MultipleServicesEnumerationExample
{
	[TestMethod]
	public async ValueTask Enumerate()
	{
		await using var container = Container.Create(services =>
													 {
														 // Register two implementations for the same interface
														 services.AddImplementation<AlphaProcessor>().For<IProcessor>();
														 services.AddImplementation<BetaProcessor>().For<IProcessor>();

														 // Register three argument-based services (same concrete type, same argument type)
														 services.AddType<ValueTaggedService, int>();
														 services.AddType<ValueTaggedService, int>();
														 services.AddType<ValueTaggedService, int>();
													 });

		// Enumerate all IProcessor instances
		var processorIds = new List<string>();

		await foreach (var p in container.GetServices<IProcessor>())
		{
			processorIds.Add(p.Id);
		}

		Assert.HasCount(expected: 2, processorIds);
		CollectionAssert.AreEquivalent(new List<string> { "Alpha", "Beta" }, processorIds);

		// Enumerate all ValueTaggedService instances for argument 42
		var valueInstances = new List<int>();

		await foreach (var svc in container.GetServices<ValueTaggedService, int>(42))
		{
			valueInstances.Add(svc.Value);
		}

		Assert.HasCount(expected: 3, valueInstances);
		Assert.IsTrue(valueInstances.TrueForAll(v => v == 42));
	}
}