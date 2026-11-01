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

// Container reflection constructs these public collaborators; the same contracts form the extracted tutorial.
// ReSharper disable ClassNeverInstantiated.Global, UnusedMember.Global

using System.IO;

namespace Xtate.IoC.Test.Examples;

// This exact tutorial source is also compiled into website/ioc/examples/Jobs.

#region JobTutorial

public static class DocumentationJobs
{
	public static async Task Run(TextWriter output)
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddConstant(output);
														 services.AddSharedType<JobSession>(SharedWithin.Scope);
														 services.AddImplementation<JobHandler, Job>().For<IJobHandler>();
														 services.AddDecorator<LoggedJobHandler, Job>().For<IJobHandler>();
													 });

		var scopes = await container.GetRequiredService<IServiceScopeFactory>();

		// The configured overload enables initialization/actions in this checkout.
		await using (var scope = scopes.CreateScope(_ => { }))
		{
			var handler = await scope.ServiceProvider.GetRequiredService<IJobHandler, Job>(new Job("report"));
			await handler.Run();
		}

		await output.WriteLineAsync("Scope finished");
	}
}

public sealed record Job(string Name);

public interface IJobHandler
{
	ValueTask Run();
}

public sealed class JobSession(TextWriter output) : IAsyncInitialization, IAsyncDisposable
{
	private readonly AsyncInit<JobSession> _initialization = new(async instance =>
																 {
																	 await instance.Output.WriteLineAsync("Session ready").ConfigureAwait(false);
																 });

	private TextWriter Output { get; } = output;

#region Interface IAsyncDisposable

	public async ValueTask DisposeAsync() => await Output.WriteLineAsync("Session disposed").ConfigureAwait(false);

#endregion

#region Interface IAsyncInitialization

	public ValueTask InitializeAsync() => AsyncInit.For(this).Run(_initialization);

#endregion
}

public sealed class JobHandler(Job job, JobSession session, TextWriter output) : IJobHandler
{
	// Keeping this dependency alive also matters for the container's weak disposal tracking.
	public JobSession Session { get; } = session;

#region Interface IJobHandler

	public async ValueTask Run() => await output.WriteLineAsync("Processed " + job.Name).ConfigureAwait(false);

#endregion
}

public sealed class LoggedJobHandler(IJobHandler inner, TextWriter output) : IJobHandler
{
#region Interface IJobHandler

	public async ValueTask Run()
	{
		await output.WriteLineAsync("Starting job").ConfigureAwait(false);
		await inner.Run().ConfigureAwait(false);
		await output.WriteLineAsync("Finished job").ConfigureAwait(false);
	}

#endregion
}

#endregion

#if !DOCS_CONSOLE
[TestClass]
public class DocumentationJobExample
{
	[TestMethod]
	public async Task TutorialRunsAndCleansUp()
	{
		// StringWriter cleanup is synchronous; no asynchronous resource is owned here.
		// ReSharper disable once UseAwaitUsing
		using var output = new StringWriter();
		await DocumentationJobs.Run(output);
		Assert.AreEqual(
			string.Join(
				Environment.NewLine,
				"Session ready", "Starting job", "Processed report", "Finished job", "Session disposed", "Scope finished", ""), output.ToString());
	}
}
#endif