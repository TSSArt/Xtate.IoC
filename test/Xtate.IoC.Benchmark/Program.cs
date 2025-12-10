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

using BenchmarkDotNet.Running;

namespace Xtate.IoC.Benchmark;

/// <summary>
///     Provides the application entry point that triggers execution of all BenchmarkDotNet benchmarks
///     defined in the current assembly.
/// </summary>
/// <remarks>
///     <para>
///         BenchmarkDotNet scans the supplied assembly for benchmark classes and methods (annotated with its attributes)
///         and then executes them, producing reports (console, Markdown, HTML, etc.) as configured by benchmark settings.
///     </para>
///     <para>
///         The returned summary from BenchmarkRunner.Run() is intentionally ignored
///         because the side effects (result artifacts) are the primary output of running benchmarks.
///     </para>
///     <seealso href="https://benchmarkdotnet.org/">BenchmarkDotNet Documentation</seealso>
/// </remarks>
public class Program
{
	/// <summary>
	///     Entry point. Discovers and runs all benchmarks located in the current assembly using BenchmarkDotNet.
	/// </summary>
	/// <remarks>
	///     Invokes BenchmarkRunner.Run() with the assembly that contains this <see cref="Program" /> type.
	/// </remarks>
	public static void Main()
	{
		_ = BenchmarkRunner.Run(typeof(Program).Assembly);
	}
}