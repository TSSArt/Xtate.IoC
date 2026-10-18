# Lifecycle and failure contract

This document records the owner-selected behavior after review of the P1 changes. It supersedes the earlier implementation reports, including their aggregation, primary-error preservation, and closed-lookup claims.

## Initialization

`AsyncInit<T>` and `AsyncInit<T, TResult>` invoke their callback once. Synchronous throws, faulted operations and cancellation are terminal: every current and subsequent caller receives the cached failure. Cancellation preserves the original cancellation token. There is no implicit retry. Value access and `EnsureInitialized()` remain guarded after failure.

Successful synchronous initialization keeps its completed `ValueTask` path. Concurrent waiters are signaled after the value or failure is published. Callback reentrancy that waits for its own initialization is unsupported.

## Concurrent operations

| Operations | Contract |
| --- | --- |
| Registration changes with registration, provider construction, or resolution | Unsupported. Finish configuring collections before use. Scope overlays are separate collections. |
| Concurrent service resolution | Supported for container bookkeeping. Scoped and singleton factories share their existing per-entry/per-argument creation protocols. User factories, services, initialization handlers and action callbacks must support their own concurrent use. |
| Bin additions with disposal | Best effort for in-flight additions: an instance may miss disposal. An addition observing a closed bin attempts immediate cleanup and then throws. |
| Repeated provider disposal | The provider claims disposal once; later calls do not join or retry cleanup. Direct concurrent bin disposal requires caller coordination. |
| Resolution or use racing with disposal | Caller coordination is required. GetImplementationEntry remains available during and after disposal. Async resolution of registered entries checks the disposal token; this is not a universal synchronous-resolution guard. |
| Child-scope acquisition racing with parent disposal | Unsupported; coordinate scope creation with disposal. No acquisition gate is implemented. |
| Existing child with a disposed ancestor | The child remains usable, including lazy lookup of inherited registrations. Shared objects are released by the last provider reference. Create children before disposing their parent. |

An accepted in-flight resolution can still return a service that disposal has begun cleaning. Object disposal racing with additions is best effort. A known WeakReferenceSet disposal race can leave a recycled handle queued; its regression test is retained. A shared singleton can remain owned while another child is alive. No service-use lease, wait-for-factories or wait-for-all-disposers guarantee is added.

Bins retain weak references and track additions, not unique object identities. Keep required instances alive; do not register the same disposable instance under several independent owners. Directly disposing the public bins independently of their provider requires caller coordination.

## Cleanup and ownership after failure

Each bin stops on its first disposer failure and does not retry remaining entries on repeated disposal. Provider cleanup still attempts subsequent phases and releases its shared reference. Async provider cleanup retains that reference until scoped cleanup finishes, including failure. Ordering between individual entries is not guaranteed.

The latest exception wins through ordinary using/finally unwinding. Failures are not collected into a new aggregate and no secondary-exception data is attached.

`DisposeAsync()` awaits asynchronous disposers. `Dispose()` prefers `IDisposable` and initiates async-only disposal without waiting for incomplete operations, preserving existing compatibility behavior. It can report only failures observable during that call. A later `DisposeAsync()` does not recover asynchronous work already started by `Dispose()`.

An instance returned by a factory is tracked by its owner entry before initialization. If initialization fails, that entry retains its ownership until provider cleanup; it does not immediately dispose and then dispose again later. Forwarding and external entries never acquire ownership. A factory that throws before returning an instance remains responsible for its own partial construction.

Provider construction failure attempts local cleanup; a failed child does not acquire a shared reference. Cleanup exceptions replace earlier failures. Public async disposal invokes the synchronous hook only after successful async cleanup. Custom overrides must preserve the base cleanup protocol.

An addition that observes a closed bin before capturing storage attempts immediate cleanup. If that cleanup succeeds, the caller receives `ObjectDisposedException`; if cleanup itself fails, its exception is propagated. This does not extend to an in-flight addition rejected by its captured storage. Synchronous additions retain the same async-only completion limitation as synchronous disposal.

`SharedObjectsBin` uses atomic increments/decrements. ObjectsBin has no outer lock. WeakReferenceSet currently has partial internal synchronization; the known handle race is not a stronger object-disposal guarantee.

## Diagnostic failures

When service resolution or factory execution fails, a throwing Error callback replaces the exception being reported. The latest callback failure wins in synchronous, completed and pending asynchronous paths, including data-action callbacks. No Exception.Data convention is used. Enter, Exit and SyncExit failures also propagate normally.

## Verification

[LifecycleFailureTest](../test/Xtate.IoC.Test/Tests/LifecycleFailureTest.cs) covers failed/canceled waiters, ownership, pending cleanup, shared-reference transitions, in-flight factories and deterministic race boundaries. [DiagnosticFailureTest](../test/Xtate.IoC.Test/Tests/DiagnosticFailureTest.cs) covers callback exception precedence. ServiceProviderConstructionTest covers failed construction. All argument arities use the same generic implementation-entry and bin protocols; their API shapes are unchanged.

Historical commands, outcomes and source fingerprints (predating owner-directed reversions) are recorded in [P1 implementation evidence](project/consistency/IMPLEMENTATION.P1.json).

The later synchronization review and revised contract are recorded in [weak-reference review evidence](project/consistency/IMPLEMENTATION.WeakReferenceSet.json).
