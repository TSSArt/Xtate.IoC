# Xtate.IoC Copilot instructions

## Repository at a glance

Xtate.IoC is an async-first IoC and dependency-injection container for .NET. The production project is `src/Xtate.IoC/Xtate.IoC.csproj`; behavior tests and executable examples are in `test/Xtate.IoC.Test`; benchmarks are in `test/Xtate.IoC.Benchmark`.

Read [`.agents/AGENTS.md`](../.agents/AGENTS.md) for architecture and hazards. Apply every matching file in [`.github/instructions`](instructions); those rules are more specific than this guide.

## Working approach

1. Identify the service-key, registration, factory-provider, implementation-entry, resolution, and disposal paths affected by the task.
2. Check symmetry across sync/async variants and argument arities in `ServiceCollectionArg0..4Extensions` and `ServiceProviderArg0..4Extensions`.
3. Inspect the matching tests and an analogous executable example before changing public behavior.
4. Preserve ownership and decorator ordering semantics.
5. Run a focused test before the broader solution build/test commands.

## Build, test, and benchmark

```powershell
dotnet restore
dotnet build Xtate.IoC.sln
dotnet test Xtate.IoC.sln
```

Focused example:

```powershell
dotnet test test/Xtate.IoC.Test/Xtate.IoC.Test.csproj -f net10.0
```

Run benchmarks only when performance is in scope:

```powershell
dotnet run --project test/Xtate.IoC.Benchmark --configuration Release
```

The library targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, and `net462`. Tests target modern frameworks plus optional `net462`.

## Shared coding rules

- Follow `.editorconfig`; C# uses tabs, nullable annotations, analyzers, and preview language features.
- Match the AGPL header and current style in adjacent source files.
- Preserve `ValueTask`-based public APIs and required `ConfigureAwait(false)` calls.
- Preserve zero-argument delegation through `Empty` and the existing `TypeKey` helpers.
- Keep module registration idempotent and `Option.IfNotRegistered` behavior unchanged unless explicitly in scope.
- Maintain fast completed-`ValueTask` paths and equivalent awaited fallbacks.
- Keep package versions in `Directory.Packages.props`.
- Treat `Directory.Build.props`, `Global.Packages.props`, and `Resources.Designer.cs` as generated. Edit `Resources.resx` instead.
- Ignore `bin`, `obj`, `TestResults`, benchmark output, and IDE metadata.

## Architecture guardrails

- Service identity is the service type plus an optional argument type.
- The last registered decorator is the outermost wrapper.
- Owned transient, scoped, and singleton entries track and dispose instances.
- External and forwarding entries must not take ownership.
- Async initialization completes before a resolved instance is returned.
- Explicit modules are preferred; do not add assembly scanning or reflection-driven registration.

## Tests and examples

- Use MSTest and keep tests independent and parallel-safe.
- Cover sync and async variants when shared behavior changes.
- For lifetime changes, assert sharing, ownership, initialization, and disposal rather than only resolved type.
- Keep `test/Xtate.IoC.Test/Examples` executable and focused on recommended public APIs.
- Run benchmarks against a stable baseline and avoid unrelated work inside benchmark methods.

## Before finishing

Confirm API symmetry, ownership behavior, decorator order, focused tests, generated-file safety, compatibility targets, and task-scoped changes.
