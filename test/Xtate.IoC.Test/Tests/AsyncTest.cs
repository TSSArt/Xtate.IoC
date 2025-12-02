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

namespace Xtate.IoC.Test;

[TestClass]
public class AsyncTest
{
	[InstantiatedByIoC]
	public class Service0;

	[InstantiatedByIoC]
	public class Service1;

	[InstantiatedByIoC]
	public class FactoryService;

	[InstantiatedByIoC]
	public class ServiceA : IAsyncInitialization
	{
		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	[InstantiatedByIoC]
	public class ServiceB : IAsyncInitialization
	{
		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	[InstantiatedByIoC]
	public class ServiceC : IAsyncInitialization
	{
		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	[InstantiatedByIoC]
	public class ServiceD : IAsyncInitialization
	{
		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	[InstantiatedByIoC]
	public class BigService1(Service0 service0, ServiceA? serviceA, ServiceB serviceB) : IAsyncInitialization
	{
		public readonly Service0 Service0 = service0;

		public readonly ServiceA? ServiceA = serviceA;

		public readonly ServiceB ServiceB = serviceB;

		[SetByIoC]
		public required Service1 Service1;

		[SetByIoC]
		public required ServiceC? ServiceC;

		[SetByIoC]
		public required ServiceD ServiceD;

		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	[InstantiatedByIoC]
	public class BigService2(Service0 service0)
	{
		[UsedImplicitly]
		public readonly Service0 Service0 = service0;

		[UsedImplicitly]
		public required Service1 Service1;
	}

	[UsedImplicitly]
	public interface IBigService3;

	[InstantiatedByIoC]
	public class BigService3 : IAsyncInitialization, IBigService3
	{
#pragma warning disable CA1822
		[UsedImplicitly]
		public FactoryService GetService() => new ();
#pragma warning restore CA1822

		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	public interface IDecor;

	[InstantiatedByIoC]
	public class BaseDecor : IDecor, IAsyncInitialization
	{
		public Task Initialization => field ??= Init();

		private static async Task Init() => await Task.Delay(1);
	}

	[InstantiatedByIoC]
	public class FactoryDecor
	{
#pragma warning disable CA1822
		[CalledByIoC]
		public IDecor? Factory() => null;
#pragma warning restore CA1822
	}

	[InstantiatedByIoC]
	public class WrapDecor(IDecor decor) : IDecor
	{
		[UsedImplicitly]
		public readonly IDecor Decor = decor;
	}

	[TestMethod]
	public async Task TestMethod0Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<Service0>();
														 services.AddType<Service1>();
														 services.AddType<ServiceA>();
														 services.AddType<ServiceB>();
														 services.AddType<ServiceC>();
														 services.AddType<ServiceD>();
														 services.AddType<BigService1>();
													 });

		var bigService1 = await container.GetRequiredService<BigService1>();
		Assert.IsNotNull(bigService1);
		Assert.IsNotNull(bigService1.Service0);
		Assert.IsNotNull(bigService1.Service1);
		Assert.IsNotNull(bigService1.ServiceA);
		Assert.IsNotNull(bigService1.ServiceB);
		Assert.IsNotNull(bigService1.ServiceC);
		Assert.IsNotNull(bigService1.ServiceD);
	}

	[TestMethod]
	public async Task TestMethod1Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<Service0>();
														 services.AddType<Service1>();
														 services.AddType<ServiceA>();
														 services.AddType<ServiceC>();
														 services.AddType<ServiceD>();
														 services.AddType<BigService1>();
													 });

		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(async () => await container.GetRequiredService<BigService1>());
	}

	[TestMethod]
	public async Task TestMethod2Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<Service0>();
														 services.AddType<Service1>();
														 services.AddType<ServiceA>();
														 services.AddType<ServiceB>();
														 services.AddType<ServiceC>();
														 services.AddType<BigService1>();
													 });

		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(async () => await container.GetRequiredService<BigService1>());
	}

	[TestMethod]
	public async Task TestMethod3Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<Service0>();
														 services.AddType<BigService2>();
													 });

		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(async () => await container.GetRequiredService<BigService2>());
	}

	[TestMethod]
	public async Task TestMethod4Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddType<Service0>();
														 services.AddType<Service1>();
														 services.AddType<ServiceA>();
														 //services.AddType<ServiceB>();
														 services.AddType<ServiceC>();
														 services.AddType<ServiceD>();
														 services.AddType<BigService1>();
													 });

		await Assert.ThrowsExactlyAsync<DependencyInjectionException>(async () => await container.GetRequiredService<BigService1>());
	}

	[TestMethod]
	public async Task TestMethod5Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddFactory<BigService3>().For<FactoryService>();
													 });

		var factoryService = await container.GetRequiredService<FactoryService>();
		Assert.IsNotNull(factoryService);
	}

	[TestMethod]
	public async Task TestMethod6Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddImplementation<BigService3>().For<IBigService3>();
													 });

		var bigService3 = await container.GetRequiredService<IBigService3>();
		Assert.IsNotNull(bigService3);
	}

	[TestMethod]
	public async Task DecorTestMethod1Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddImplementation<BaseDecor>().For<IDecor>();
														 services.AddDecorator<WrapDecor>().For<IDecor>();
													 });

		var decor = await container.GetRequiredService<IDecor>();
		Assert.IsNotNull(decor);
	}

	[TestMethod]
	public async Task DecorTestMethod2Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddDecorator<WrapDecor>().For<IDecor>();
													 });

		var decor = await container.GetService<IDecor>();
		Assert.IsNull(decor);
	}

	[TestMethod]
	public async Task DecorTestMethod3Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddFactory<FactoryDecor>().For<IDecor>();
														 services.AddDecorator<WrapDecor>().For<IDecor>();
													 });

		var decor = await container.GetService<IDecor>();
		Assert.IsNull(decor);
	}

	[TestMethod]
	public async Task DecorTestMethod4Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddImplementation<BaseDecor>().For<IDecor>();
														 services.AddDecoratorSync<WrapDecor>().For<IDecor>();
													 });

		var decor = await container.GetRequiredService<IDecor>();
		Assert.IsNotNull(decor);
	}

	[TestMethod]
	public async Task DecorTestMethod5Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddDecoratorSync<WrapDecor>().For<IDecor>();
													 });

		var decor = await container.GetService<IDecor>();
		Assert.IsNull(decor);
	}

	[TestMethod]
	public async Task DecorTestMethod6Async()
	{
		await using var container = Container.Create(services =>
													 {
														 services.AddFactory<FactoryDecor>().For<IDecor>();
														 services.AddDecoratorSync<WrapDecor>().For<IDecor>();
													 });

		var decor = await container.GetService<IDecor>();
		Assert.IsNull(decor);
	}
}