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

using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using JetBrains.Annotations;
using Microsoft.VSDiagnostics;

namespace Xtate.IoC.Benchmark;

/// <summary>
///     An empty class used to benchmark raw instantiation versus IoC resolution overhead.
/// </summary>
public class EmptyClass;

/// <summary>
///     Represents a node wrapping a <see cref="WeakReference" /> and linking to another node, forming
///     a simple singly-linked weak-reference chain for benchmark purposes.
/// </summary>
/// <param name="target">The target object to reference weakly.</param>
public class WeakReferenceNode(object target) : WeakReference(target);

/// <summary>
///     Benchmark test type that simulates an asynchronously initialized component resolved by the IoC container.
/// </summary>
[UsedImplicitly]
public class AsyncInitClass : IAsyncInitialization
{
#region Interface IAsyncInitialization

	/// <inheritdoc />
	public Task Initialization => Task.CompletedTask;

#endregion
}

/// <summary>
///     Benchmark test type implementing asynchronous disposal for IoC resolution comparison.
/// </summary>
public class AsyncDisposeClass : IAsyncDisposable
{
#region Interface IAsyncDisposable

	/// <summary>
	///     Performs asynchronous disposal. In this benchmark implementation no resources are released.
	/// </summary>
	/// <returns>A completed <see cref="ValueTask" />.</returns>
	public ValueTask DisposeAsync() => ValueTask.CompletedTask;

#endregion
}

/// <summary>
///     Contains BenchmarkDotNet benchmarks comparing direct instantiation and IoC container resolution
///     for several lightweight service types, including async initialization and disposal scenarios.
/// </summary>
[MemoryDiagnoser]
[CPUUsageDiagnoser]
public class Benchmarks
{
	/// <summary>
	///     The IoC container instance used for resolution benchmarks. Created once in <see cref="Setup" />.
	/// </summary>
	private Container _container = null!;

	/// <summary>
	///     Global setup for the benchmark suite. Registers test types in the IoC container.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		// ReSharper disable once NotDisposedResource
		_container = Container.Create(sp =>
									  {
										  sp.AddType<EmptyClass>();
										  sp.AddType<AsyncInitClass>();
										  sp.AddType<AsyncDisposeClass>();
									  });
	}

	/// <summary>
	///     Benchmarks direct instantiation of <see cref="EmptyClass" />.
	/// </summary>
	/// <returns>A new instance of <see cref="EmptyClass" />.</returns>
	[Benchmark]
	public EmptyClass CreateEmptyClass() => new();

	/// <summary>
	///     Benchmarks resolving <see cref="EmptyClass" /> through the IoC container.
	/// </summary>
	/// <returns>The resolved <see cref="EmptyClass" /> instance.</returns>
	[Benchmark]
	public EmptyClass IoCRequireEmptyClass() => _container.GetRequiredService<EmptyClass>().GetAwaiter().GetResult();

	/// <summary>
	///     Benchmarks resolving an asynchronously initialized class via the IoC container.
	/// </summary>
	/// <returns>The resolved <see cref="AsyncInitClass" /> instance.</returns>
	[Benchmark]
	public AsyncInitClass IoCRequireAsyncInitClass() => _container.GetRequiredService<AsyncInitClass>().GetAwaiter().GetResult();

	/// <summary>
	///     Benchmarks creation of a <see cref="WeakReferenceNode" /> wrapping an <see cref="AsyncDisposeClass" />.
	/// </summary>
	/// <returns>A new <see cref="WeakReferenceNode" /> instance.</returns>
	[Benchmark]
	public WeakReferenceNode WeakRefEmpty() => new(new AsyncDisposeClass());

	/// <summary>
	///     Benchmarks resolving an asynchronously disposable class via the IoC container.
	/// </summary>
	/// <returns>The resolved <see cref="AsyncDisposeClass" /> instance.</returns>
	[Benchmark]
	public AsyncDisposeClass IoCRequireAsyncDisposeClass() => _container.GetRequiredService<AsyncDisposeClass>().GetAwaiter().GetResult();
}