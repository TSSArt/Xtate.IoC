# Xtate.IoC repository guide

Use this guide as the first source of repository context. Preserve symmetry across related APIs and inspect the corresponding tests and examples before changing container behavior.

## Project purpose

Xtate.IoC is an async-first IoC and dependency-injection container with argument-aware service keys and explicit ownership semantics.

| Path | Purpose |
| --- | --- |
| `src/Xtate.IoC/Xtate.IoC.csproj` | Multi-targeted library and NuGet package |
| `test/Xtate.IoC.Test/Xtate.IoC.Test.csproj` | MSTest behavior tests and executable examples |
| `test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj` | BenchmarkDotNet measurements |
| `Xtate.IoC.sln` | Repository solution |

The library targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, and `net462`. Tests target modern frameworks plus `net462` unless `SkipNetFrameworkTests=true`.

## Architecture

- `IoC/TypeKey.cs` identifies services by service type and optional argument type.
- `IoC/ServiceCollection.cs` and `ServiceCollectionArg0..4Extensions` define registration APIs.
- `IoC/ServiceProvider.cs` and `ServiceProviderArg0..4Extensions` define resolution APIs.
- `ImplementationEntry` contains lifetime-specific resolution, chain, initialization, and ownership behavior.
- `FactoryProviders` builds cached sync and async factory/decorator delegates.
- `IoC/ObjectsBin.cs` and `SharedObjectsBin.cs` track owned disposable instances.
- `IoC/Module.cs` and `Module<...>` compose explicit registrations without assembly scanning.

`Empty` is the zero-argument service-key marker. Arg0 APIs delegate to the generic argument-aware implementation. When a public capability changes, check all applicable argument arities, sync/async variants, registration methods, resolution methods, examples, and tests.

Lifetime behavior is contractual: owner entries dispose created instances; external and forwarding entries do not. Decorator order is also contractual: the last registered decorator is outermost.

## Code conventions and hazards

- Follow `.editorconfig`: tabs, nullable annotations, analyzer rules, and existing naming/style.
- Preserve `ValueTask`-based public APIs and required `ConfigureAwait(false)` calls.
- Maintain symmetry between sync/async paths and `ServiceCollectionArg0..4` / `ServiceProviderArg0..4` APIs.
- Preserve `TypeKey`, `InstanceScope`, `SharedWithin`, `Option.IfNotRegistered`, and module idempotency semantics.
- Keep fast completed-`ValueTask` paths and asynchronous fallbacks behaviorally identical.
- Edit `Properties/Resources.resx`, not the generated `Resources.Designer.cs`.
- Treat `Directory.Build.props` and `Global.Packages.props` as generated; keep package versions in `Directory.Packages.props`.
- Ignore `bin`, `obj`, `TestResults`, benchmark output, and IDE metadata.

Path-specific rules in `.github/instructions` take precedence for matching files.

## Build, test, and benchmark

```powershell
dotnet restore
dotnet build Xtate.IoC.sln
dotnet test Xtate.IoC.sln
```

For a focused modern target:

```powershell
dotnet test test/Xtate.IoC.Test/Xtate.IoC.Test.csproj -f net10.0
```

Run benchmarks separately and only when performance is in scope:

```powershell
dotnet run --project test/Xtate.IoC.Benchmark --configuration Release
```

## Change checklist

1. Map the change across service keys, registrations, factory providers, implementation entries, resolution, and disposal as applicable.
2. Add tests for sync and async paths, argument arities, scopes, ownership, and disposal where relevant.
3. Update an executable example when recommended public usage changes.
4. Run a focused test first, then the solution build/test command.
5. Keep generated files and unrelated existing work untouched.
