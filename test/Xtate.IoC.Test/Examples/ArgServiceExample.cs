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

// Service with constructor argument.
// Demonstrates resolving a service that requires an argument at resolution time.

public class SingleArgService(int value)
{
	public int Argument { get; } = value;
}

// Arguments can be provided via constructor parameters or via public required properties/fields.
public class DoubleArgService(int value)
{
	public required string Name { get; init; }

	public int Value { get; } = value;
}

// There are no restrictions on amount of arguments
public class MultiArgService
{
	public required string Name { get; init; }

	public required int Value { get; init; }

	public required Guid Id { get; init; }
}

// Arguments with same types can be provided via ValueTuple
public class SameTypeArgService((string First, string Last) name)
{
	public (string First, string Last) Name { get; init; } = name;
}

// Arguments with same types can be provided via ValueTuple
public class MultiSameTypeArgService((string First, string Last) name)
{
	public (string First, string Last) Name { get; init; } = name;

	public required (int x, int y) Vector { get; init; }
}

// If arguments of the same types can't be represented through the tuple (class is immutable) instance can be created through factory delegate.
public class CustomArgService(double arg1, double arg2)
{
	public double Arg1 { get; } = arg1;

	public double Arg2 { get; } = arg2;
}

[TestClass]
public class ServiceWithArgumentsExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<SingleArgService, int>();
														 services.AddType<DoubleArgService, int, string>();
														 services.AddType<MultiArgService, (Guid, string, int)>();
														 services.AddType<SameTypeArgService, (string First, string Last)>();
														 services.AddType<MultiSameTypeArgService, (string First, string Last), (int x, int y)>();
														 services.AddTransient<CustomArgService, double, double>((_, d1, d2) => new CustomArgService(d1, d2));
													 });

		var singleArgService = await container.GetRequiredService<SingleArgService, int>(5);
		Assert.AreEqual(expected: 5, singleArgService.Argument);

		var doubleArgService = await container.GetRequiredService<DoubleArgService, int, string>(arg1: 7, arg2: "Hello");
		Assert.AreEqual(expected: 7, doubleArgService.Value);
		Assert.AreEqual(expected: "Hello", doubleArgService.Name);

		var multiArgService = await container.GetRequiredService<MultiArgService, (Guid, string, int)>((Guid.Empty, "World", 9));
		Assert.AreEqual(Guid.Empty, multiArgService.Id);
		Assert.AreEqual(expected: "World", multiArgService.Name);
		Assert.AreEqual(expected: 9, multiArgService.Value);

		var sameTypeArgService = await container.GetRequiredService<SameTypeArgService, (string First, string Last)>(("John", "Doe"));
		Assert.AreEqual(expected: "John", sameTypeArgService.Name.First);
		Assert.AreEqual(expected: "Doe", sameTypeArgService.Name.Last);

		var multiSameTypeArgService = await container.GetRequiredService<MultiSameTypeArgService, (string First, string Last), (int x, int y)>(("John", "Doe"), (1, 2));
		Assert.AreEqual(expected: "John", multiSameTypeArgService.Name.First);
		Assert.AreEqual(expected: "Doe", multiSameTypeArgService.Name.Last);
		Assert.AreEqual(expected: 1, multiSameTypeArgService.Vector.x);
		Assert.AreEqual(expected: 2, multiSameTypeArgService.Vector.y);

		var customArgService = await container.GetRequiredService<CustomArgService, double, double>(arg1: 3.14, arg2: 42);
		Assert.AreEqual(expected: 3.14, customArgService.Arg1);
		Assert.AreEqual(expected: 42, customArgService.Arg2);
	}
}