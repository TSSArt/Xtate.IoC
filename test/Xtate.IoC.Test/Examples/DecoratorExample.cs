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

// Demonstrates registering decorators.
// Order of registration defines wrapping order: last registered decorator is outermost.
// services.AddImplementation<BaseFormatter>().For<IMessageFormatter>();
// services.AddDecorator<BracketsFormatter>().For<IMessageFormatter>();
// services.AddDecorator<UpperFormatter>().For<IMessageFormatter>();
// Resolving IMessageFormatter returns UpperFormatter(BracketsFormatter(BaseFormatter)).

public interface IMessageFormatter
{
	string Format(string message);
}

public class BaseFormatter : IMessageFormatter
{
#region Interface IMessageFormatter

	public string Format(string message) => message + "!";

#endregion
}

public class BracketsFormatter(IMessageFormatter inner) : IMessageFormatter
{
#region Interface IMessageFormatter

	public string Format(string message) => "[" + inner.Format(message) + "]";

#endregion
}

public class UpperFormatter(IMessageFormatter inner) : IMessageFormatter
{
#region Interface IMessageFormatter

	public string Format(string message) => inner.Format(message).ToUpperInvariant();

#endregion
}

// Async decorator example (simulates async initialization) -- shows that async decorator is awaited automatically.
public class AsyncSuffixFormatter : IMessageFormatter, IAsyncInitialization
{
	private readonly IMessageFormatter _inner;

	private string _suffix = string.Empty;

	public AsyncSuffixFormatter(IMessageFormatter inner)
	{
		_inner = inner;
		Initialization = Init();
	}

#region Interface IAsyncInitialization

	public Task Initialization { get; }

#endregion

#region Interface IMessageFormatter

	public string Format(string message) => _inner.Format(message) + _suffix;

#endregion

	private async Task Init()
	{
		await Task.Yield(); // simulate async work
		_suffix = "#";
	}
}

[TestClass]
public class DecoratorExample
{
	[TestMethod]
	public async ValueTask Resolve()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddImplementation<BaseFormatter>().For<IMessageFormatter>();
														 services.AddDecorator<BracketsFormatter>().For<IMessageFormatter>();
														 services.AddDecorator<UpperFormatter>().For<IMessageFormatter>();
														 services.AddDecorator<AsyncSuffixFormatter>().For<IMessageFormatter>();
													 });

		var formatter = await container.GetRequiredService<IMessageFormatter>();
		var result = formatter.Format("hello");

		// Expected layering:
		// BaseFormatter: "hello!"
		// BracketsFormatter: "[hello!]"
		// UpperFormatter: "[HELLO!]"
		// AsyncSuffixFormatter: "[HELLO!]#"
		Assert.AreEqual(expected: "[HELLO!]#", result);
	}
}