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

using System.Threading.Tasks.Sources;

namespace Xtate.IoC.Test;

[TestClass]
public class DiagnosticValueTaskTest
{
	[TestMethod]
	[DataRow(0, false, false)]
	[DataRow(1, false, false)]
	[DataRow(2, false, false)]
	[DataRow(0, true, false)]
	[DataRow(1, true, false)]
	[DataRow(2, true, false)]
	[DataRow(0, false, true)]
	[DataRow(1, false, true)]
	[DataRow(2, false, true)]
	[DataRow(0, true, true)]
	[DataRow(1, true, true)]
	[DataRow(2, true, true)]
	public async Task SourceBackedFactory_IsConsumedOnce(int actionCount, bool withData, bool pending)
	{
		// All arities share ImplementationEntry.WithActions<T, TArg>.
		foreach (var outcome in new[] { 0, 1, 2 })
		{
			var source = new SingleUseSource();
			Exception? error = outcome switch
							   {
								   1 => new InvalidOperationException("factory failed"),
								   2 => new OperationCanceledException("factory canceled"),
								   _ => null
							   };

			if (!pending)
			{
				source.Complete(error);
			}

			await using var provider = Container.Create(services =>
														{
															for (var i = 0; i < actionCount; i ++)
															{
																services.AddConstant<IServiceProviderActions>(new NoOpActions(withData));
															}

															services.AddForwarding<string>(_ => source.Operation);
														});

			var request = provider.GetRequiredService<string>();

			if (pending)
			{
				Assert.IsFalse(request.IsCompleted);
				source.Complete(error);
			}

			if (error is OperationCanceledException)
			{
				Assert.AreSame(error, await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await request));
			}
			else if (error is not null)
			{
				Assert.AreSame(error, await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await request));
			}
			else
			{
				Assert.AreEqual(expected: "resolved", await request);
			}

			Assert.AreEqual(expected: 1, source.Calls);
		}
	}

	private sealed class SingleUseSource : IValueTaskSource<string>
	{
		private ManualResetValueTaskSourceCore<string> _core = new() { RunContinuationsAsynchronously = true };

		public int Calls { get; private set; }

		public ValueTask<string> Operation => new(this, _core.Version);

	#region Interface IValueTaskSource<string>

		public string GetResult(short token)
		{
			if (++ Calls != 1)
			{
				throw new InvalidOperationException("Operation consumed twice");
			}

			return _core.GetResult(token);
		}

		public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

		public void OnCompleted(Action<object?> continuation,
								object? state,
								short token,
								ValueTaskSourceOnCompletedFlags flags) =>
			_core.OnCompleted(continuation, state, token, flags);

	#endregion

		public void Complete(Exception? error)
		{
			if (error is null)
			{
				_core.SetResult("resolved");
			}
			else
			{
				_core.SetException(error);
			}
		}
	}

	private sealed class NoOpActions(bool withData) : IServiceProviderActions, IServiceProviderDataActions
	{
	#region Interface IServiceProviderActions

		public IServiceProviderDataActions? RegisterServices(int count) => withData ? this : null;

		public IServiceProviderDataActions? Event(ActionsEventType type, ref ActionsContext context) => withData ? this : null;

	#endregion

	#region Interface IServiceProviderDataActions

		public void RegisterService(ServiceEntry serviceEntry) { }

		public void Event<T, TArg>(ActionsEventType eventType, ref DataActionsContext<T, TArg> context) { }

	#endregion
	}
}