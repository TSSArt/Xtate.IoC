# Xtate.IoC

A modern, async-first, attribute-light Inversion of Control (IoC) / Dependency Injection (DI) container for .NET with explicit lifetime semantics, argument-aware service resolution, rich decorator and factory patterns, and fine‑grained disposal tracking. Targets `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, and `net462`.

## Table of Contents
- Motivation
- Key Features
- Supported Frameworks
- Installation
- Quick Start
- Examples
- Core Concepts
- Service Registration API Overview
- Lifetimes & Ownership Model
- Generic / Argumented Services (`Type + TArg`)
- Decorators
- Factories
- Modules
- Scopes
- Asynchronous Initialization & Disposal
- Forwarding & Constants
- Comparison with Microsoft.Extensions.DependencyInjection
- Performance Notes
- Extensibility Points
- Error Handling
- Testing
- Roadmap / Ideas
- Contributing
- License

## Motivation
`Xtate.IoC` is built for scenarios requiring:
- Asynchronous factories and initialization flows without forcing `Task.Run` wrappers.
- Precise resource ownership & deterministic disposal (including async disposal).
- Argument-sensitive service resolution (e.g. resolve the same service type with different contextual arguments).
- Efficient decorator chains with minimal overhead.
- Lightweight module system without reflection scanning.
- Ability to share (singleton / scoped) objects or create forwarding proxies.

## Key Features
- Async + sync factory support equally first-class.
- Distinct ownership semantics: internal vs external (no double disposal).
- Rich registration DSL via extension methods (e.g. `AddType<T>()`, `AddSharedImplementation<T>()`, `AddDecorator<T>()`).
- Decorators work for all lifetimes (transient, scoped, singleton) and with argument tuples.
- Argument-aware service keys: service identity includes optional argument type allowing contextual resolution.
- Forwarding registrations for constants or computed delegations.
- Module system (`AddModule<TModule>()`) to bundle registrations.
- Explicit scoping via `IServiceScopeFactory` / `CreateScope` with optional additional service overlay.
- Async initialization integration (`IInitializationHandler`).
- Fine-grained disposal tracking (`ObjectsBin`, `SharedObjectsBin`).
- AGPL-licensed with transparent source.

## Supported Frameworks
| Target | Notes |
|--------|-------|
| net10.0 / net9.0 / net8.0 | Full feature set |
| netstandard2.0 | Compatibility layer; async interfaces via packages |
| net462 | Backport support; adds `System.ValueTuple`, async interfaces |

## Installation
NuGet:
```
dotnet add package Xtate.IoC
```
Minimal dependencies (analyzers & shared content internalized).

## Quick Start
```csharp
using Xtate.IoC;

var container = Container.Create(services =>
{
    services.AddTypeSync<MyService>();            // Transient
    services.AddSharedTypeSync<MyRepository>(SharedWithin.Container); // Singleton
    services.AddTransientDecorator<IMyLogic>((sp, inner) => new CachingLogic(inner));
});

var service = container.Get<IMyService>(); // Resolves transient instance
```
Use `Container.Create<TModule>()` to load a module class implementing `IModule`.

## Examples
A broad set of runnable usage examples (covering registration patterns, scopes, factories, decorators, async flows, disposal semantics, argumented services, etc.) is maintained inside the test project. Browse them here:

- Full examples directory: https://github.com/TSSArt/Xtate.IoC/tree/main/test/Xtate.IoC.Test/Examples

These files double as documentation-by-execution and provide concrete scenarios beyond the Quick Start.

## Core Concepts
- `ServiceCollection`: Mutable registration list.
- `ServiceEntry`: Single registration metadata (key, factory, scope).
- `TypeKey`: Internal composite key (supports generic definition keys & argument types).
- `ServiceProvider`: Resolution engine + disposal root.
- `Container`: Sealed root provider with convenience `Create` overloads.
- `ObjectsBin` / `SharedObjectsBin`: Track owned instances per scope / across scopes.

## Service Registration API Overview
Registration extension methods (grouped by intent):
- Type / Implementation: `AddType<T>()`, `AddImplementation<T>()` (both produce service instances of `T`).
- Shared (scoped/singleton): `AddSharedType<T>(SharedWithin.Scope|Container)`.
- Decorators: `AddDecorator<T>()`, `AddSharedDecorator<T>(...)`, `AddTransientDecorator<T>(...)`.
- Factories: `AddFactory<T>()` returning a delegate provider.
- Forwarding: `AddForwarding<T>(() => existing)` or `AddConstant<T>(value)`.
- Explicit lifetimes: transient vs shared (scoped / container).
Each method has sync and async variants (`AddTypeSync`, `AddSharedTypeSync`, etc.).

### Option Guards
Many methods accept `Option.IfNotRegistered` to prevent duplicate registration.

## Lifetimes & Ownership Model
Internal enum `InstanceScope` drives behavior:
- `Transient`: New instance per resolution, tracked for disposal in current provider.
- `Scoped` / `ScopedExternal`: Instance shared within a scope, owned vs externally owned.
- `Singleton` / `SingletonExternal`: Shared across container lifetime, owned vs externally owned.
- `Forwarding`: Delegates to another source or returns a constant; no new ownership.
External variants avoid disposing instances the container did not create.

## Generic / Argumented Services
Service identity may include an argument type (`TArg`). APIs have forms like `AddType<T, TArg>()` allowing resolution with contextual argument data. Under the hood, `TypeKey.ServiceKey<T, TArg>()` differentiates them.
Tuple arguments (`(TArg1, TArg2)`) are supported via helper overloads.

Example:
```csharp
services.AddTypeSync<Renderer, RenderSettings>();
var renderer = sp.Get<Renderer, RenderSettings>(settings);
```

## Decorators
Decorators wrap existing implementations in a chain. Order of registration determines wrapping sequence (last added executes outermost). Async and sync delegates supported:
```csharp
services.AddDecoratorSync<IMyLogic>((sp, inner) => new LoggingLogic(inner));
services.AddDecoratorSync<IMyLogic>((sp, inner) => new CachingLogic(inner));
// Resolution order: CachingLogic -> LoggingLogic -> Core Implementation
```
Decorators respect target lifetime; a shared decorator over a singleton yields one decorated chain instance.

## Factories
`AddFactory<T>()` exposes a factory abstraction allowing on-demand creation while preserving container construction semantics and disposal tracking.

## Modules
Implement `IModule`:
```csharp
class CoreModule : IModule
{
    public IServiceCollection Services { get; set; } = null!;
    public void AddServices()
    {
        Services.AddSharedTypeSync<ConfigProvider>(SharedWithin.Container);
        Services.AddTypeSync<Worker>();
    }
}

var container = Container.Create<CoreModule>();
```
Modules auto-register themselves as `IModule` (once) via `AddModule<TModule>()`.

## Scopes
Create nested scopes via `IServiceScopeFactory`:
```csharp
using var container = Container.Create(s => s.AddSharedTypeSync<Context>(SharedWithin.Scope));
using var scope = container.CreateScope(scopeServices =>
{
    scopeServices.AddTypeSync<RequestHandler>();
});
var handler = scope.ServiceProvider.Get<RequestHandler>();
```
Scope overlay services can add or decorate without mutating parent.

## Asynchronous Initialization & Disposal
Async factories (`Func<IServiceProvider, TArg, ValueTask<T>>`) supported everywhere. Disposal is async-aware (`IAsyncDisposable` respected). `IAsyncInitializationHandler` (exposed internally as `IInitializationHandler`) may perform container-level initialization steps.

## Forwarding & Constants
Forwarding allows exposing existing provider values without new lifetime ownership:
```csharp
services.AddConstant(new AppVersion("1.2.3"));
services.AddForwarding<IRuntimeEnv>(sp => sp.Get<RuntimeEnv>()); // adapter
```

## Comparison with Microsoft.Extensions.DependencyInjection
| Aspect | Xtate.IoC | MS.DI |
|--------|-----------|-------|
| Async factories | Native ValueTask & overload symmetry | Requires manual `Task` patterns |
| Argumented resolution | Built-in typed argument key | Typically manual factories | 
| Decorator pattern | First-class chain semantics | External libs / manual | 
| Ownership tracking | Explicit bins per scope | Implicit; dispose root only | 
| External vs owned | Separate scopes (`ScopedExternal`, etc.) | Not distinguished | 
| Module abstraction | Simple `IModule` contract | Extension methods / custom | 
| Forwarding constants | Dedicated `AddConstant` | `AddSingleton(value)` (owned) |
| Licensing | AGPLv3 | MIT |

## Performance Notes
- Avoid heavy decorators on hot paths; prefer minimal wrappers.
- Use sync registrations (`*Sync`) when no async work occurs to eliminate state machine overhead.
- Minimize scope depth; disposal cost is proportional to owned object count.
- Argumented generic keys cache resolved chains; repeated argument usage benefits from internal caching.

## Extensibility Points
- Implement custom `IModule` collections.
- Provide `IServiceProviderActions` to inspect registration data (see internal calls in `ServiceProvider`).
- Extend via factories returning higher-order delegates (`DelegateFactory`).

## Error Handling
- Missing service resolution triggers `MissedServiceException`.
- Lifetime violations or invalid options throw `DependencyInjectionException` variants.
- Disposal after use guarded by `ObjectDisposedException.ThrowIf` calls.

## Testing
For tests, construct minimal `ServiceCollection`, then `new ServiceProvider(services)` or use `Container.Create(...)`. Register fakes via `AddTransient` / `AddConstant`.

## Roadmap / Ideas
- Source generators for module auto-discovery (opt-in).
- Diagnostics hooks for resolution tracing.
- Benchmark suite publishing.
- Optional integration layer for `IHostedService` style apps.

## Contributing
1. Fork the repository.
2. Create feature branch.
3. Add tests (target latest framework if feature-specific).
4. Ensure build passes with warnings treated as errors.
5. Submit PR with concise description.
By contributing you agree to license your changes under AGPLv3.

## License
GNU Affero General Public License v3. See `LICENSE` file.
If AGPL is unsuitable for your closed-network use case, contact the author for alternative licensing.

## Attribution
Copyright © 2019-2025 Sergii Artemenko.
Project site: https://xtate.net/

## Disclaimer
Provided "AS IS" without warranty; evaluate suitability before production use.
