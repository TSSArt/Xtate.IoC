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

using System.Threading;

namespace Xtate.IoC.Test;

[TestClass]
public class DiagnosticFailureTest
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
	public async Task ThrowingErrorCallback_ReplacesOriginalExceptionAcrossAllPaths(int path, bool dataCallback, bool canceled)
	{
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();
		Exception primary = canceled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("factory");
		var secondary = new InvalidOperationException("diagnostic");
		var requestFailure = new InvalidOperationException("request diagnostic");
		var actions = new ThrowingActions(secondary, dataCallback, requestFailure);
		var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
		var services = new ServiceCollection();
		services.AddConstant<IServiceProviderActions>(actions);

		if (path == 0)
		{
			services.AddTransient(_ => ThrowFactory(primary));
		}
		else
		{
			services.AddTransient(_ => new ValueTask<object>(completion.Task));
			if (path == 1) completion.SetException(primary);
		}

		await using var provider = new ServiceProvider(services);
		var request = Observe(async () =>
							  {
								  // The callback completes before the enclosing resource scope ends.
								  // ReSharper disable once AccessToDisposedClosure
								  if (path == 0) { _ = provider.GetRequiredServiceSync<object>(); }

								  // The callback completes before the enclosing resource scope ends.
								  // ReSharper disable once AccessToDisposedClosure
								  else { _ = await provider.GetRequiredService<object>(); }
							  });

		if (path == 2)
		{
			Assert.IsFalse(request.IsCompleted);
			completion.SetException(primary);
		}

		using var timeout = new CancellationTokenSource();
		Assert.AreSame(request, await Task.WhenAny(request, Task.Delay(millisecondsDelay: 10000, timeout.Token)));
		await timeout.CancelAsync();
		Assert.AreSame(requestFailure, await request, message: "The outer request Error callback determines the observable failure.");
		Assert.HasCount(expected: 2, actions.Errors);
		Assert.AreSame(primary, actions.Errors[0], message: "The factory Error callback receives the factory failure.");
		Assert.AreSame(secondary, actions.Errors[1], message: "The request Error callback receives the exception thrown by the factory Error callback.");
		StringAssert.Contains(requestFailure.StackTrace!, nameof(ThrowingActions));
		if (canceled) Assert.AreEqual(cancellation.Token, ((OperationCanceledException)primary).CancellationToken);
	}

	[TestMethod]
	[DataRow(false)]
	[DataRow(true)]
	public async Task ThrowingErrorCallback_ReplacesFailureWithoutUsingOriginalExceptionData(bool throwingData)
	{
		Exception primary = throwingData ? new ThrowingDataException() : new InvalidOperationException("factory");
		if (!throwingData) primary.Data["CallerData"] = "reserved by caller";
		var callbackFailure = new InvalidOperationException("diagnostic");
		var actions = new ThrowingActions(callbackFailure, dataCallback: false);
		var services = new ServiceCollection();
		services.AddConstant<IServiceProviderActions>(actions);
		services.AddTransient(_ => ThrowFactory(primary));
		await using var provider = new ServiceProvider(services);
		Assert.AreSame(
			callbackFailure, await Observe(() =>
										   {
											   provider.GetRequiredServiceSync<object>();

											   return ValueTask.CompletedTask;
										   }));
		Assert.AreSame(primary, actions.Errors[0]);
		if (!throwingData) Assert.AreEqual(expected: "reserved by caller", primary.Data["CallerData"]);
	}

	private static object ThrowFactory(Exception exception) => throw exception;

	private static async Task<Exception> Observe(Func<ValueTask> operation)
	{
		try { await operation(); }
		catch (Exception exception) { return exception; }

		Assert.Fail("Expected an exception.");

		throw new InvalidOperationException();
	}

	private sealed class ThrowingActions(Exception callbackFailure, bool dataCallback, Exception? requestFailure = null) : IServiceProviderActions, IServiceProviderDataActions
	{
		public List<Exception?> Errors { get; } = [];

	#region Interface IServiceProviderActions

		public IServiceProviderDataActions? RegisterServices(int count) => null;

		public IServiceProviderDataActions? Event(ActionsEventType type, ref ActionsContext context)
		{
			if (!dataCallback && (type & ActionsEventType.Error) != 0) Fail(type, context.Exception);

			return dataCallback ? this : null;
		}

	#endregion

	#region Interface IServiceProviderDataActions

		public void RegisterService(ServiceEntry serviceEntry) { }

		public void Event<T, TArg>(ActionsEventType type, ref DataActionsContext<T, TArg> context)
		{
			if ((type & ActionsEventType.Error) != 0) Fail(type, context.Exception);
		}

	#endregion

		private void Fail(ActionsEventType type, Exception? exception)
		{
			Errors.Add(exception);

			throw type == ActionsEventType.ServiceRequestError ? requestFailure ?? callbackFailure : callbackFailure;
		}
	}

	private sealed class ThrowingDataException : Exception
	{
		public override IDictionary Data => throw new InvalidOperationException("Data unavailable");
	}
}