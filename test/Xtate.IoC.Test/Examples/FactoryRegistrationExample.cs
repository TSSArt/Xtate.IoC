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

// Demonstrates factory registration via AddFactory<TFactory>().For<TService...>()
// The IoC container creates the factory type and calls its method returning the service.
// Method name is discovered automatically; it must have a compatible signature and return the service interface.
// Example shows: no-arg factory and multi-arg factory.

public interface IMessageProvider
{
	string Message { get; }
}

public class HelloMessageProvider : IMessageProvider
{
#region Interface IMessageProvider

	public string Message => "Hello";

#endregion
}

public class MessageFactory // factory with method that creates the service
{
	public virtual IMessageProvider CreateService() => new HelloMessageProvider();
}

public interface ISumCalculator
{
	int Result { get; }
}

public class SumCalculator(int a, int b) : ISumCalculator
{
#region Interface ISumCalculator

	public int Result => a + b;

#endregion
}

public class SumCalculatorFactory // factory with arguments
{
	public virtual ISumCalculator CreateService((int a, int b) args) => new SumCalculator(args.a, args.b);
}

[TestClass]
public class FactoryRegistrationExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddFactory<MessageFactory>().For<IMessageProvider>();
														 services.AddFactory<SumCalculatorFactory>().For<ISumCalculator, int, int>();
													 });

		var provider = await container.GetRequiredService<IMessageProvider>();
		Assert.AreEqual(expected: "Hello", provider.Message);

		var calc = await container.GetRequiredService<ISumCalculator, int, int>(arg1: 3, arg2: 4);
		Assert.AreEqual(expected: 7, calc.Result);
	}
}