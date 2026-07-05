# Copilot Instructions for Xtate.IoC

## Repository Overview

**Xtate.IoC** is an async-first, attribute-light Inversion of Control (IoC) / Dependency Injection (DI) container for .NET. It is developed as part of the broader [Xtate](https://xtate.net/) state machine framework project. The library is published on NuGet as `Xtate.IoC` and is licensed under AGPL-3.0.

### Key Distinguishing Features
- **Async-first**: All resolution APIs return `ValueTask<T>`; sync variants (`*Sync` suffix) are opt-in for hot paths.
- **Argument-aware resolution**: Service identity includes an optional `TArg` type, enabling contextual resolution like `GetRequiredService<Renderer, RenderSettings>(settings)`.
- **Explicit ownership tracking**: Separate "owned" vs "external" lifetimes (`Scoped` vs `ScopedExternal`, `Singleton` vs `SingletonExternal`). The container only disposes what it created.
- **First-class decorator chains**: Decorators wrap registrations in order; last registered is outermost.
- **Lightweight module system**: `IModule` / `Module<…>` base classes bundle registrations without reflection scanning.
- **No reflection-based scanning**: All registrations are explicit via a fluent API.

---

## Repository Layout

```
/
├── src/
│   └── Xtate.IoC/               # Main library (C# project)
│       ├── AsyncInitialization/ # IAsyncInitialization, AsyncInitializationHandler
│       ├── Exceptions/          # DependencyInjectionException, MissedServiceException
│       ├── FactoryProviders/    # Internal delegate-chain builders (sync & async)
│       ├── Helpers/             # Utilities: Cache, Disposer, Infra, WeakReferenceSet, etc.
│       ├── ImplementationEntry/ # Per-lifetime entry types (Transient, Scoped, Singleton, Forwarding)
│       ├── Interfaces/          # Public interfaces: IModule, IServiceCollection, IServiceProvider, etc.
│       ├── IoC/                 # Core types: ServiceCollection, ServiceProvider, Container, TypeKey,
│       │                        #   InstanceScope, Module, ObjectsBin, ServiceEntry,
│       │                        #   ServiceCollectionArg{0-4}Extensions, ServiceProviderArg{0-4}Extensions
│       └── Properties/          # Resources.resx / Resources.Designer.cs (error messages)
├── test/
│   ├── Xtate.IoC.Test/          # MSTest unit + integration test project
│   │   ├── Examples/            # Runnable usage examples (also serve as documentation)
│   │   └── Tests/               # Unit tests for every public class/feature
│   └── Xtate.IoC.Benchmark/     # BenchmarkDotNet benchmark project
├── Directory.Build.props        # Shared MSBuild properties (LangVersion=preview, Nullable=enable, etc.)
├── Directory.Packages.props     # Centralized NuGet version management
├── Xtate.IoC.sln                # Solution file
└── .github/workflows/
    ├── publish.yml              # Build → Test → Pack → Push to NuGet.org & GitHub Packages
    └── codeql.yml               # CodeQL security scanning
```

---

## Build, Test, and Lint

```bash
# Restore dependencies
dotnet restore

# Build (warnings treated as errors)
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run benchmarks (from benchmark project directory)
dotnet run --project test/Xtate.IoC.Benchmark -c Release
```

### Important Build Facts
- **`LangVersion=preview`** is set globally — latest C# preview features are in use (e.g., C# 14 extension members with `extension(...)` blocks).
- **`TreatWarningsAsErrors=true`** — every compiler warning is a build failure. Do not suppress warnings casually.
- **`Nullable=enable`** — full nullable reference types are required everywhere.
- **`ConfigureAwaitChecker.Analyzer`** — every `await` must use `.ConfigureAwait(false)`. The analyzer enforces this; missing it is a build error.
- Target frameworks: `net11.0`, `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, `net462` (library); test project targets `net11.0`, `net10.0`, `net9.0`, `net8.0`, `net462`.
- NuGet package versions are centrally managed in `Directory.Packages.props`.
- `InternalsVisibleTo` is configured so the test assembly can access internal types in the library.

---

## Coding Conventions

### Style
- **Tabs** for indentation (not spaces) — enforced via `.editorconfig`.
- `var` preferred everywhere unless type is ambiguous.
- `this.` qualification is avoided for fields, methods, and properties.
- System.* `using` directives sorted first.
- Interface region blocks: `#region Interface IFoo … #endregion` pattern is used throughout (see any existing file).
- Copyright header on every `.cs` file (AGPL-3.0 boilerplate, see any existing file for the template).
- Namespace `Xtate.IoC` for library; `Xtate.IoC.Test` for tests; `Xtate.IoC.Examples` for example files.
- `dotnet_style_namespace_match_folder = false` — namespaces do **not** need to match folder structure.

### C# Language Features in Use
- C# 14 **extension members** (`extension(IServiceCollection services) { … }`): All `ServiceCollectionArg*Extensions` and `ServiceProviderArg*Extensions` and `ServiceCollectionExtensions` use this syntax instead of traditional static extension methods.
- Primary constructors on classes (e.g., `Container(IServiceCollection services)`).
- `ValueTask` / `ValueTask<T>` throughout — never `Task<T>` in the public API.
- `IAsyncDisposable` is the primary disposal interface; `IDisposable` is also implemented with `GC.SuppressFinalize`.
- `Interlocked.Exchange` for thread-safe disposal guards.
- `[ExcludeFromCodeCoverage]` on trivial/unreachable paths.
- `[MustDisposeResource]` JetBrains annotation on `Container` and its factory methods.

---

## Core Architecture

### Service Identity: `TypeKey`
Every registered service is identified by a `TypeKey` — an abstract class with two concrete variants:
- `SimpleTypeKey` — for non-generic or closed-generic services.
- `GenericTypeKey` — for open-generic service definitions; resolved lazily on first use.

Service keys are created via:
```csharp
TypeKey.ServiceKey<TService, TArg>()        // standard service
TypeKey.ImplementationKey<TImpl, TArg>()    // implementation (used internally for module tracking)
TypeKey.ServiceKeyFast<T, TArg>()           // unchecked fast path (internal)
```

When `TArg` is `Empty` (a library-internal struct), the service has no argument.

### Lifetimes: `InstanceScope`
| Enum Value | Behavior |
|---|---|
| `Transient` | New instance per resolution; tracked in `ObjectsBin` for disposal |
| `Forwarding` | No new instance; delegates to a factory/constant; not disposed by container |
| `Scoped` | Shared within a scope; container owns and disposes |
| `ScopedExternal` | Shared within a scope; container does NOT dispose (externally owned) |
| `Singleton` | Shared across container lifetime; container owns and disposes |
| `SingletonExternal` | Shared across container lifetime; container does NOT dispose |

`SharedWithin.Container` maps to `Singleton`; `SharedWithin.Scope` maps to `Scoped`.

### Registration DSL
All registration goes through `IServiceCollection`. The fluent builder pattern uses method chaining:

```csharp
// Simple transient
services.AddType<MyService>();                     // async factory
services.AddTypeSync<MyService>();                 // sync factory

// Singleton / Scoped
services.AddSharedType<MyService>(SharedWithin.Container);
services.AddSharedTypeSync<MyService>(SharedWithin.Scope);

// Implementation registered for an interface
services.AddImplementation<MyImpl>().For<IMyService>();
services.AddImplementationSync<MyImpl>().For<IMyService>().For<IAlsoThis>();

// Decorator chain (last registered = outermost)
services.AddDecorator<LoggingImpl>().For<IMyService>();

// Argument-aware service
services.AddType<Renderer, RenderSettings>();
// → resolved with: sp.GetRequiredService<Renderer, RenderSettings>(settings)

// Manual factory delegate
services.AddTransient<IFoo>(sp => new Foo(sp.GetRequiredServiceSync<IBar>()));
services.AddShared<IFoo>(SharedWithin.Container, async sp => new Foo(await sp.GetRequiredService<IBar>()));

// Forwarding / constants
services.AddForwarding<IConfig>(sp => sp.GetRequiredServiceSync<AppConfig>());
services.AddConstant<IVersion>(new AppVersion("1.0.0"));

// Guard against double-registration
services.AddType<MyService>(Option.IfNotRegistered);

// Modules
services.AddModule<CoreModule>();
```

### Resolution API
```csharp
// Async (preferred)
var svc = await sp.GetRequiredService<IMyService>();       // throws if missing
var svc = await sp.GetService<IMyService>();               // returns null if missing

// Sync
var svc = sp.GetRequiredServiceSync<IMyService>();
var svc = sp.GetServiceSync<IMyService>();

// Enumerate all registrations for a type
await foreach (var svc in sp.GetServices<IPlugin>()) { … }

// With argument
var renderer = await sp.GetRequiredService<Renderer, RenderSettings>(settings);

// Factory delegates (deferred creation)
var factory = sp.GetRequiredFactory<IMyService>();         // Func<ValueTask<IMyService>>
var factory = sp.GetRequiredFactory<Renderer, RenderSettings>(); // Func<RenderSettings, ValueTask<Renderer>>
```

### `Container` vs `ServiceProvider`
- `Container` is the sealed public entry point; always use `Container.Create(…)` or `Container.Create<TModule>(…)` to build a root container.
- `ServiceProvider` is the base class; can be subclassed or instantiated directly with a `ServiceCollection` for testing.
- `ServiceCollection.BuildProvider()` returns `new ServiceProvider(this)` — useful in tests.

### Modules
Extend `Module` (or `Module<TDep1, …>` for dependency modules):
```csharp
class CoreModule : Module
{
    protected override void AddServices()
    {
        Services.AddSharedType<Config>(SharedWithin.Container);
        Services.AddType<Worker>();
    }
}

await using var container = Container.Create<CoreModule>();
```

Modules with typed dependencies automatically call `Services.AddModule<TDep>()` before `AddServices()`.

### Async Initialization
If a resolved class implements `IAsyncInitialization`, the container awaits `InitializeAsync()` before returning the instance. Use this for async constructor patterns:
```csharp
class MyService : IAsyncInitialization
{
    public async ValueTask InitializeAsync() { … }
}
```

### Scopes
```csharp
var factory = await container.GetRequiredService<IServiceScopeFactory>();
using var scope = factory.CreateScope(scopeServices =>
{
    scopeServices.AddType<RequestHandler>();
});
var handler = await scope.ServiceProvider.GetRequiredService<RequestHandler>();
```

---

## Testing Patterns

- Test framework: **MSTest** with `[TestClass]` / `[TestMethod]`.
- Mocking: **Moq**.
- Test methods are `async Task` or `async ValueTask`.
- Use `Container.Create(…)` or `new ServiceCollection()` + `BuildProvider()` for integration-style tests.
- The `Examples/` directory files are real test classes — they compile and run as part of the test suite and serve as living documentation.
- Use `Assert.IsInstanceOfType<T>(obj)`, `Assert.AreEqual`, `Assert.AreSame` / `Assert.AreNotSame` for assertions.
- The `GlobalUsings.cs` in the test project contains common `using` directives shared across all test files.

---

## Common Pitfalls

1. **Missing `.ConfigureAwait(false)`**: The `ConfigureAwaitChecker.Analyzer` enforces this on every `await`. Every `await` in library code **must** be followed by `.ConfigureAwait(false)`. Test code is exempt.
2. **Extension member syntax**: Registration extension methods use the C# 14 `extension(IServiceCollection services) { … }` block syntax, not traditional `static … (this IServiceCollection services, …)`. Match this style when adding new extension methods.
3. **`Empty` arg type**: When a service has no argument, `TArg` is `Empty` (internal struct). The shorthand zero-arg APIs in `ServiceCollectionArg0Extensions` delegate to the one-arg forms with `Empty`.
4. **`AddType<T>` vs `AddImplementation<T>()`**: Both register `T` as transient. `AddType` registers it directly as `T`; `AddImplementation` is used with `.For<IService>()` to register it for an interface. Do not confuse them.
5. **Decorator wrapping order**: Decorators are applied in registration order — last registered is outermost (wraps all previously registered decorators).
6. **Ownership semantics**: Only use `ScopedExternal` / `SingletonExternal` when the instance lifecycle is managed outside the container. Using the non-external variant on an externally-owned object causes double disposal.
7. **`Option.IfNotRegistered`**: Use this guard when registering default/fallback services that modules should be able to override.

---

## CI / Publishing

- **CodeQL**: Runs automatically on push/PR for security scanning.
- **Publish workflow**: Triggered on `v*` tags or manual dispatch. Uses GitVersion for semantic versioning. Steps: restore → build → test → pack (with symbols) → push to NuGet.org and GitHub Packages.
- Mono is installed in CI because `net462` support requires it on Ubuntu runners.
- Versioning: `Version.props` holds `0.0.0` as a placeholder; the actual version is injected by GitVersion during CI builds.
