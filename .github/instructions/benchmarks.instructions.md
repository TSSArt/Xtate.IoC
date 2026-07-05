---
applyTo: "test/Xtate.IoC.Benchmark/**/*.cs"
---

# Benchmark project instructions

## Purpose

BenchmarkDotNet performance measurements for container resolution and lifecycle overhead.

## Follow existing patterns

- Keep benchmark setup in `[GlobalSetup]` and benchmark methods focused on a single measurable operation.
- Use lightweight benchmark fixture types and deterministic behavior to reduce noise.
- Keep BenchmarkDotNet attributes/cofiguration style consistent with existing benchmark classes.

## Implementation rules

- Separate direct-instantiation baselines from IoC-resolution benchmarks for comparable results.
- Keep benchmark code free from test-only assertions and side effects unrelated to measured behavior.

## Testing rules

- Validate that benchmark project still builds/runs with `dotnet run --project test/Xtate.IoC.Benchmark -c Release`.

## Avoid

- Do not introduce logging, randomization, or external I/O inside benchmark methods.
