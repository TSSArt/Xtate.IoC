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

using System.Linq;

namespace Xtate.IoC.Test;

[TestClass]
public class CollectionEnumerationTest
{
	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task ZeroArgumentEnumerations_PreserveOrderAndEmptyKey(bool populated)
	{
		await using var provider = Container.Create(services =>
													{
														services.AddTypeSync<Consumer>();
														services.AddForwarding<string, int>((_, _) => "argument-only");

														if (populated)
														{
															services.AddConstant("first");
															services.AddConstant("second");
														}
													});
		var expected = populated ? new[] { "first", "second" } : [];
		var consumer = provider.GetRequiredServiceSync<Consumer>();
		CollectionAssert.AreEqual(expected, provider.GetServicesSync<string>().ToArray());
		CollectionAssert.AreEqual(expected, consumer.Sync.ToArray());

		foreach (var enumerable in new[] { provider.GetServices<string>(), consumer.Async })
		{
			var values = new List<string>();

			await foreach (var value in enumerable)
			{
				values.Add(value);
			}

			CollectionAssert.AreEqual(expected, values);
		}
	}

	[TestMethod]
	public async Task Enumerations_CreateServicesOnlyWhenAdvanced()
	{
		var created = 0;
		await using var provider = Container.Create(services => services.AddTransient(_ => ++ created));
		var sync = provider.GetServicesSync<int>();
		var async = provider.GetServices<int>();
		using var syncEnumerator = sync.GetEnumerator();
		await using var asyncEnumerator = async.GetAsyncEnumerator();
		Assert.AreEqual(expected: 0, created);
		Assert.IsTrue(syncEnumerator.MoveNext());
		Assert.AreEqual(expected: 1, created);
		Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
		Assert.AreEqual(expected: 2, created);
	}

	[TestMethod]
	public async Task AsyncEnumeration_AwaitsPendingRegistration()
	{
		var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
		await using var provider = Container.Create(services => services.AddTransient<string>(_ => new ValueTask<string>(completion.Task)));
		await using var enumerator = provider.GetServices<string>().GetAsyncEnumerator();
		var pending = enumerator.MoveNextAsync();
		Assert.IsFalse(pending.IsCompleted);
		completion.SetResult("completed");
		Assert.IsTrue(await pending);
		Assert.AreEqual(expected: "completed", enumerator.Current);
		Assert.IsFalse(await enumerator.MoveNextAsync());
	}

	[UsedImplicitly] // Constructed through container reflection.
	private sealed class Consumer(IEnumerable<string> sync, IAsyncEnumerable<string> async)
	{
		public IEnumerable<string> Sync { get; } = sync;

		public IAsyncEnumerable<string> Async { get; } = async;
	}
}