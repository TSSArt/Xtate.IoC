using BenchmarkDotNet.Running;

namespace Xtate.IoC.Benchmark;

/// <summary>
/// Provides the application entry point that triggers execution of all BenchmarkDotNet benchmarks
/// defined in the current assembly.
/// </summary>
/// <remarks>
/// <para>
/// BenchmarkDotNet scans the supplied assembly for benchmark classes and methods (annotated with its attributes)
/// and then executes them, producing reports (console, Markdown, HTML, etc.) as configured by benchmark settings.
/// </para>
/// <para>
/// The returned summary from BenchmarkRunner.Run() is intentionally ignored
/// because the side effects (result artifacts) are the primary output of running benchmarks.
/// </para>
/// <seealso href="https://benchmarkdotnet.org/">BenchmarkDotNet Documentation</seealso>
/// </remarks>
public class Program
{
	/// <summary>
	/// Entry point. Discovers and runs all benchmarks located in the current assembly using BenchmarkDotNet.
	/// </summary>
	/// <remarks>
	/// Invokes BenchmarkRunner.Run() with the assembly that contains this <see cref="Program"/> type.
	/// </remarks>
	public static void Main()
	{
		_ = BenchmarkRunner.Run(typeof(Program).Assembly);
	}
}