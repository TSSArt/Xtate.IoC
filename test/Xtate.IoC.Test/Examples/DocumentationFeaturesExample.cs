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

// Reflection-constructed tutorial collaborators retain public contracts for the extracted documentation snippets.
// ReSharper disable ClassNeverInstantiated.Global, MemberCanBePrivate.Global, UnusedMember.Global, UnusedMemberInSuper.Global

#pragma warning disable IDE0200 // Explicit lambda parameters keep the extracted registration snippets readable.

using System.IO;

namespace Xtate.IoC.Test.Examples;

[TestClass]
public class DocumentationFeaturesExample
{
	[TestMethod]
	public async Task QuickStart()
	{
	#region QuickStart

		await using var container = Container.Create(services =>
													 {
														 services.AddImplementation<Greeting>().For<IGreeting>();
													 });
		var greeting = await container.GetRequiredService<IGreeting>();
		Assert.AreEqual(expected: "Hello", greeting.Text);

	#endregion
	}

	[TestMethod]
	public void SyncStart()
	{
	#region SyncStart

		using var container = Container.Create(s => s.AddTypeSync<Greeting>());
		var greeting = container.GetRequiredServiceSync<Greeting>();
		Assert.AreEqual(expected: "Hello", greeting.Text);

	#endregion
	}

	[TestMethod]
	public async Task SharedArguments()
	{
	#region SharedArguments

		await using var container = Container.Create(s =>
														 s.AddSharedTypeSync<Named, string>(SharedWithin.Container));
		var first = await container.GetRequiredService<Named, string>("alpha");
		var same = await container.GetRequiredService<Named, string>("alpha");
		var other = await container.GetRequiredService<Named, string>("beta");
		Assert.AreSame(first, same);
		Assert.AreNotSame(first, other);

	#endregion
	}

	[TestMethod]
	public async Task MultiContract()
	{
	#region MultiContract

		await using var container = Container.Create(s =>
														 s.AddSharedImplementationSync<Greeting>(SharedWithin.Container)
														  .For<IGreeting>()
														  .For<IMessage>());
		var greeting = await container.GetRequiredService<IGreeting>();
		var message = await container.GetRequiredService<IMessage>();
		Assert.AreSame<object>(greeting, message);

	#endregion
	}

	[TestMethod]
	public async Task ScopeOverlay()
	{
	#region ScopeOverlay

		await using var container = Container.Create(s => s.AddConstant("root"));
		var scopes = await container.GetRequiredService<IServiceScopeFactory>();
		await using var child = scopes.CreateScope(s => s.AddConstant("child"));
		Assert.AreEqual(expected: "child", await child.ServiceProvider.GetRequiredService<string>());
		Assert.AreEqual(expected: "root", await container.GetRequiredService<string>());

	#endregion
	}

	[TestMethod]
	public async Task RegistrationOptions()
	{
	#region RegistrationOptions

		await using var container = Container.Create(s =>
													 {
														 s.AddTypeSync<Greeting>();
														 s.AddTypeSync<Greeting>(Option.IfNotRegistered);
													 });
		var count = 0;

		await foreach (var _ in container.GetServices<Greeting>())
		{
			count ++;
		}

		Assert.AreEqual(expected: 1, count);

	#endregion
	}

	[TestMethod]
	public async Task CollectionsAreLazy()
	{
	#region CollectionsAreLazy

		var created = 0;
		await using var container = Container.Create(s =>
													 {
														 s.AddTransient<IGreeting>(_ =>
																				   {
																					   created ++;

																					   return new Greeting();
																				   });
														 s.AddTransient<IGreeting>(_ =>
																				   {
																					   created ++;

																					   return new Greeting();
																				   });
													 });
		var services = container.GetServices<IGreeting>();
		Assert.AreEqual(expected: 0, created);

		await foreach (var service in services)
		{
			Assert.AreEqual(expected: "Hello", service.Text);
		}

		Assert.AreEqual(expected: 2, created);

	#endregion
	}

	[TestMethod]
	public async Task RequiredMembers()
	{
	#region RequiredMembers

		await using var container = Container.Create(s =>
													 {
														 s.AddTypeSync<Greeting>();
														 s.AddTypeSync<Consumer>();
													 });
		var consumer = await container.GetRequiredService<Consumer>();
		Assert.AreEqual(expected: "Hello", consumer.Greeting.Text);
		Assert.IsNull(consumer.Optional);

	#endregion
	}

	[TestMethod]
	public async Task InjectedFactories()
	{
	#region InjectedFactories

		await using var container = Container.Create(s =>
													 {
														 s.AddTypeSync<Named, string>();
														 s.AddType<FactoryConsumer>();
													 });
		var consumer = await container.GetRequiredService<FactoryConsumer>();
		var item = await consumer.Create("created later");
		Assert.AreEqual(expected: "created later", item.Name);

	#endregion
	}

	[TestMethod]
	public async Task FactoryProductOwnership()
	{
	#region FactoryProductOwnership

		await using var container = Container.Create(s => s.AddFactorySync<OwnedFactory>().For<Owned>(Option.DoNotDispose));
		var owned = await container.GetRequiredService<Owned>();
		Assert.IsFalse(owned.Disposed);
		owned.Dispose();
		Assert.IsTrue(owned.Disposed);

	#endregion
	}

	[TestMethod]
	public async Task ModulesAreIdempotent()
	{
	#region ModulesAreIdempotent

		await using var container = Container.Create(s =>
													 {
														 s.AddModule<GreetingModule>();
														 s.AddModule<GreetingModule>();
														 Assert.IsTrue(s.IsModuleRegistered<GreetingModule>());
													 });
		var count = 0;

		await foreach (var _ in container.GetServices<IGreeting>())
		{
			count ++;
		}

		Assert.AreEqual(expected: 1, count);

	#endregion
	}

	[TestMethod]
	public async Task Diagnostics()
	{
	#region Diagnostics

		// StringWriter has no asynchronous cleanup; the scenario demonstrates debugger ownership.
		// ReSharper disable once UseAwaitUsing
		using var writer = new StringWriter();
		using var debugger = new ServiceProviderDebugger(writer, leaveOpen: true);
		await using var container = Container.Create(s =>
													 {
														 // Registration resolves before debugger leaves its enclosing using scope.
														 // ReSharper disable once AccessToDisposedClosure
														 s.AddConstant<IServiceProviderActions>(debugger);
														 s.AddTypeSync<Greeting>();
													 });
		await container.GetRequiredService<Greeting>();
		Assert.IsTrue(writer.ToString().Contains(nameof(Greeting)));

	#endregion
	}

	[TestMethod]
	public async Task LowLevelRegistration()
	{
	#region LowLevelRegistration

		ServiceCollection services =
		[
			new ServiceEntry(
				TypeKey.ServiceKey<Named, string>(), InstanceScope.Transient,
				new Func<IServiceProvider, string, Named>((_, name) => new Named(name)))
		];
		await using var container = new Container(services);
		var value = await container.GetRequiredService<Named, string>("explicit");
		Assert.AreEqual(expected: "explicit", value.Name);

	#endregion
	}

	[TestMethod]
	public async Task DecoratorEnumerationIncludesEachEntry()
	{
	#region DecoratorEnumeration

		await using var container = Container.Create(s =>
													 {
														 s.AddConstant("core");
														 s.AddTransientDecorator<string>((_, inner) => "[" + inner + "]");
													 });
		var values = new List<string>();

		await foreach (var value in container.GetServices<string>())
		{
			values.Add(value);
		}

		CollectionAssert.AreEqual(new[] { "core", "[core]" }, values);
		Assert.AreEqual(expected: "[core]", await container.GetRequiredService<string>());

	#endregion
	}

	[TestMethod]
	public void CompletedAsyncConstantIsStillAsyncRegistration()
	{
		using var container = Container.Create(s => s.AddConstant(new ValueTask<string>("ready")));

		// The assertion invokes the delegate before the container is disposed.
		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<DependencyInjectionException>(container.GetRequiredServiceSync<string>);
	}

	[TestMethod]
	public void SyncResolutionRejectsInitializationInterface()
	{
		using var container = Container.Create(s => s.AddTypeSync<Prepared>());

		// The assertion invokes the delegate before the container is disposed.
		// ReSharper disable once AccessToDisposedClosure
		Assert.ThrowsExactly<DependencyInjectionException>(container.GetRequiredServiceSync<Prepared>);
	}

	[TestMethod]
	public async Task ScopeOverloadsHaveDifferentHandlerSetup()
	{
		await using var container = Container.Create(_ => { });
		var factory = await container.GetRequiredService<IServiceScopeFactory>();
		await using var plain = factory.CreateScope();
		await using var configured = factory.CreateScope(_ => { });
		Assert.IsNull(plain.ServiceProvider.InitializationHandler);
		Assert.IsNotNull(configured.ServiceProvider.InitializationHandler);
	}

	[TestMethod]
	public async Task SingletonUsesFirstResolutionContext()
	{
		await using var container = Container.Create(s =>
													 {
														 s.AddConstant("root");
														 s.AddSharedTypeSync<Named>(SharedWithin.Container);
													 });
		var factory = await container.GetRequiredService<IServiceScopeFactory>();
		await using var child = factory.CreateScope(s => s.AddConstant("child"));
		var first = await child.ServiceProvider.GetRequiredService<Named>();
		var root = await container.GetRequiredService<Named>();
		Assert.AreSame(first, root);
		Assert.AreEqual(expected: "child", root.Name);
	}

	[TestMethod]
	public async Task RepeatedArgumentTypesMatchFirstElement()
	{
		await using var container = Container.Create(s => s.AddTypeSync<Pair, int, int>());
		var pair = await container.GetRequiredService<Pair, int, int>(arg1: 3, arg2: 7);
		Assert.AreEqual(expected: 3, pair.First);
		Assert.AreEqual(expected: 3, pair.Second);
	}

	public sealed class Prepared : IAsyncInitialization
	{
	#region Interface IAsyncInitialization

		public ValueTask InitializeAsync() => default;

	#endregion
	}

	public sealed class Pair(int first, int second)
	{
		public int First { get; } = first;

		public int Second { get; } = second;
	}

	public interface IGreeting
	{
		string Text { get; }
	}

	public interface IMessage
	{
		string Text { get; }
	}

	public sealed class Greeting : IGreeting, IMessage
	{
	#region Interface IGreeting

		public string Text => "Hello";

	#endregion
	}

	public sealed class Named(string name)
	{
		public string Name { get; } = name;
	}

	public sealed class Missing;

	public sealed class Consumer
	{
		public required Greeting Greeting { get; init; }

		public required Missing? Optional { get; init; }
	}

	public sealed class FactoryConsumer(Func<string, ValueTask<Named>> create)
	{
		public Func<string, ValueTask<Named>> Create { get; } = create;
	}

	public sealed class Owned : IDisposable
	{
		public bool Disposed { get; private set; }

	#region Interface IDisposable

		public void Dispose() => Disposed = true;

	#endregion
	}

	public sealed class OwnedFactory
	{
		public Owned Create() => new();
	}

	public sealed class GreetingModule : Module
	{
		protected override void AddServices() => Services.AddImplementationSync<Greeting>().For<IGreeting>();
	}
}