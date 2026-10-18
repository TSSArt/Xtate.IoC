# Xtate.IoC conventions

This document describes the Xtate.IoC solution rooted three directories above this file: one library, one MSTest project with executable examples, and one BenchmarkDotNet executable. It does not cover sibling solutions in the enclosing Xtate checkout. It documents conventions; it is neither a compliance audit nor an implementation backlog.

## Authority, coverage, and interpretation

Matching `.github/instructions` rules take precedence over `.agents/AGENTS.md`; explicit configuration and public contracts precede inferred patterns. Existing rule means an explicit repository requirement, Established pattern means a bounded observation worth retaining, Proposed target requires adoption, and Decision required means the affected aspect has no complete auditable target. These labels never assert that current code complies.

The historical input inventory contains 194 maintained files with SHA-256 hashes. Revision 52 repairs its normalized cohort counts and records implementation policy choices; it does not claim a fresh compliance audit. Original observations below remain historical unless explicitly updated. Generated resource accessors and generated build/package property files are configuration evidence only, not coding-style peers. Build output, IDE metadata, and benchmark output are excluded. Every criterion uses named evidence and records its eligible cohort and discovery sample; these broad cohort denominators are file counts, not counts of applicable methods or compliant implementations. No source-wide prevalence or behavioral correctness is claimed. Extra peer examination belongs to a separately requested audit.

Git CLI status was unavailable (`cannot change to T:/Projects/TSSArt/xtate.dev`). The enclosing checkout's HEAD ref was read directly as main at 06d4c326d4c6ca3d5acefc4bb76d7cc20936d95b; this is not proof of a clean worktree. File hashes identify the actual inspected inputs. Local build props import enclosing build settings, so standalone and enclosing-checkout evaluation may differ. No builds, tests, benchmark measurements, or package restores were run for this documentation-only task.

All C01–C50 criteria apply at least to part of this solution. C48 concerns resource representation, not an invented wire protocol; C49 concerns localization and deterministic tests, not a nonexistent clock service. Implementation decisions now select targets for C06, C11, C13, C15, C36, C45 and C46; the earlier observations remain historical evidence. Existing rules in those sections remain usable within their stated limits.

## Reading the records

Each record states its scope, observations, selected target, inspection conditions, and decisions. Evidence links point to original files; symbol names in the prose identify the relevant code. Configuration suggestions retain their configured severity. A reviewer must not convert a proposed target into an existing rule or an observed defect into a sanctioned exception.

<a id="c09"></a>

## C09 — Failure representation

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** GetRequiredServiceSync throws MissedServiceException on missing values; optional GetService and factory helpers preserve default/nullable results. Infra assertions represent invariant failures.

**Target — Established pattern.** Preserve the required/optional distinction: required lookup throws the existing missing-service exception; optional lookup retains its generic default semantics. Do not equate operational failure with absence.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Trace required and optional overloads through entry/factory paths, including value-type semantics; compare thrown type and optional result in corresponding tests.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs), [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs)

<a id="c10"></a>

## C10 — Nullability and absence

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 2. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Nullable is enabled in build settings. IServiceProvider explicitly allows absent initialization handler, actions array, and entry. GetService<T> exposes T? while required methods constrain T to notnull.

**Target — Existing rule.** Keep nullable annotations and generic constraints accurate. Preserve null-as-absence in existing optional contracts; do not replace nullable action arrays with empty arrays without checking callers. T? on unconstrained generics is not a universal Nullable<T> wrapper.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect signatures, annotations, null branches, and generic constraints; verify optional and required behavior separately.

**Unresolved decisions:** None.

**Evidence:** [Directory.Build.props](../../../Directory.Build.props), [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [.github/instructions/csharp.instructions.md](../../../.github/instructions/csharp.instructions.md)

<a id="c11"></a>

## C11 — Argument validation

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Infra.Assert and NotNull throw InvalidOperationException for internal invariants. Decorator factory assignability uses localized DependencyInjectionException. ServiceCollection.Add and provider constructors do not establish a universal argument guard policy.

**Target — Selected implementation policy.** Preserve existing public invalid-input exception types and timing. Callers supply non-null providers, collections, entries, keys and factory delegates unless explicitly nullable. Extension overloads that bridge existing APIs inherit their validation. New independent entry points validate at their boundary with named ArgumentNullException/ArgumentException; internal invariants keep Infra checks. Do not retrofit guards without a compatibility review.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Insufficient for a complete auditable target.

**Inspection and compliance:** Distinguish malformed caller input from broken internal assumptions; inspect entry-point tests before prescribing new guards.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs), [src/Xtate.IoC/IoC/ServiceCollection.cs](../../../src/Xtate.IoC/IoC/ServiceCollection.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [Xtate.IoC.sln.DotSettings](../../../Xtate.IoC.sln.DotSettings)

<a id="c12"></a>

## C12 — Cancellation

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** IServiceProvider exposes DisposeToken; ServiceProvider cancels its token during disposal. The older-framework CancelAsync shim schedules Cancel on TaskScheduler.Default. Arg0 resolution APIs have no cancellation token parameter.

**Target — Established pattern.** Preserve lifecycle cancellation and framework-specific CancelAsync behavior. Do not invent a token-last rule for existing tokenless resolution APIs or reinterpret cancellation as successful absence.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Trace token creation, cancellation, and disposal in both provider paths; inspect compatibility preprocessor guards and distinguish disposal cancellation from caller request cancellation.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs](../../../src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs)

<a id="c13"></a>

## C13 — Resource ownership and disposal

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** ScopedOwnerImplementationEntry registers produced values in ObjectsBin on completed and awaited paths; provider maps external entries separately. Disposer prefers sync disposal in its sync method and async disposal in its async method. DisposerTest explicitly covers incomplete async disposal not being awaited by Dispose.

**Target — Selected implementation policy.** Follow docs/lifecycle.md: bins stop on first disposer failure; provider phases and failed-construction cleanup use ordinary using/finally semantics, with the latest exception winning. No failure aggregation is introduced.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Trace creation, initialization, bin publication, and disposal for each lifetime; assert identity and disposal counts. Inspect failure after publication and incomplete async disposal separately.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [.github/instructions/implementation-entry.instructions.md](../../../.github/instructions/implementation-entry.instructions.md), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/ObjectsBin.cs](../../../src/Xtate.IoC/IoC/ObjectsBin.cs), [src/Xtate.IoC/Helpers/Disposer.cs](../../../src/Xtate.IoC/Helpers/Disposer.cs), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs](../../../test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs)

<a id="c14"></a>

## C14 — Mutation and collection contracts

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** ServiceCollection appends registrations to a List and tracks membership in a HashSet; membership deduplication does not remove list entries. Provider builds its own keyed entry chains and scope overlays.

**Target — Established pattern.** Preserve ordered registrations and distinguish IsRegistered membership from duplicate suppression. Keep parent and child scope registration behavior distinct; do not promise immutable exposed arrays or automatic collection snapshots beyond actual code.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Inspect Add, enumeration, GroupServices, AddRegistration and SourceServiceCollection; test duplicate options, chain order and scope overlays before changing collection representation.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceCollection.cs](../../../src/Xtate.IoC/IoC/ServiceCollection.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [.github/instructions/ioc-core.instructions.md](../../../.github/instructions/ioc-core.instructions.md), [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs)

<a id="c15"></a>

## C15 — Error translation at boundaries

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Infra.TypeInitHandle unwraps TypeInitializationException using ExceptionDispatchInfo. Decorator creation reports localized domain errors. ImplementationEntry.WithActions reports error events; HandleException returns a fault from a throwing error callback.

**Target — Selected implementation policy.** A throwing Error callback replaces the exception being reported in sync and async paths. The latest exception wins; no secondary-exception Data storage is part of the contract.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Insufficient for a complete auditable target.

**Inspection and compliance:** Inspect original exceptions, inner exceptions, callback exceptions and synchronous/asynchronous reporting separately; validate observable exceptions at public boundaries.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs), [src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs), [src/Xtate.IoC/Helpers/Disposer.cs](../../../src/Xtate.IoC/Helpers/Disposer.cs)

<a id="c16"></a>

## C16 — Diagnostics

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** ServiceProvider obtains optional IServiceProviderActions and sends registration notifications. ImplementationEntry emits Enter, Exit, SyncExit and Error through typed contexts; no logger-level policy follows from these hooks.

**Target — Established pattern.** Use the existing action interfaces and event/context protocol for container instrumentation. Preserve callback ordering and sync/async event distinctions; do not introduce unconditional logging into resolution.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Trace normal, incomplete, and exceptional action paths and compare their event order. A new logging adapter must define its own redaction and level policy.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs), [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs)

<a id="c17"></a>

## C17 — Contracts versus implementations

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 1. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Interfaces are organized under Interfaces but share the Xtate.IoC namespace. IServiceProvider exposes ImplementationEntry and other container abstractions; this is one library, not separate abstraction/implementation assemblies.

**Target — Existing rule.** Preserve existing contract ownership and public compatibility. Keep factory internals in their existing families; do not impose a new assembly split or hide already public types merely for naming symmetry.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect the public dependency surface and accessibility before moving declarations; distinguish internal helpers from intentionally exposed extension seams.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs), [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj), [.agents/AGENTS.md](../../../.agents/AGENTS.md), [.github/instructions/factory-providers.instructions.md](../../../.github/instructions/factory-providers.instructions.md)

<a id="c18"></a>

## C18 — Dependency direction

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Both test and benchmark projects reference the library. The library project contains package references and InternalsVisibleTo for its tests; it does not reference either consumer project.

**Target — Established pattern.** Keep tests and benchmarks dependent on the runtime library, with no reverse project dependency. Consider enclosing SubRepos imports when evaluating effective references.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Inspect all three project files and evaluated imports for cycles, test-only dependencies and consumer-specific packages.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj), [test/Xtate.IoC.Test/Xtate.IoC.Test.csproj](../../../test/Xtate.IoC.Test/Xtate.IoC.Test.csproj), [test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj](../../../test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj), [Directory.Build.props](../../../Directory.Build.props)

<a id="c19"></a>

## C19 — Dependency construction

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** ServiceProvider creates its internal cache, bins and token source directly. Registration extension methods adapt provider/argument delegates; decorator factories use centralized delegate builders.

**Target — Existing rule.** Use existing factory/delegate and registration mechanisms for user services; direct construction of container infrastructure remains valid. Do not replace factory-provider helpers with ad-hoc reflection or runtime code generation.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Follow a registration through delegate creation and runtime entry construction; verify argument/decorator delegate shapes and lifetime ownership.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs), [.github/instructions/factory-providers.instructions.md](../../../.github/instructions/factory-providers.instructions.md)

<a id="c20"></a>

## C20 — Registration and composition

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 2. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Arg0 registration overloads bridge to Empty; AddConstant uses forwarding and preserves a supplied ValueTask. The guide defines module idempotency, Option.IfNotRegistered and last-decorator-outermost semantics.

**Target — Existing rule.** Maintain arity 0–4 and sync/async symmetry, TypeKey identities and existing lifetime mappings. Keep Container.Create lightweight, modules explicit and idempotent, and decorators ordered. Do not add auto-scanning.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Map changed capabilities across corresponding registration/resolution overloads; assert duplicate registration, module identity, decorator output, and scope behavior.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [.github/instructions/ioc-core.instructions.md](../../../.github/instructions/ioc-core.instructions.md), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs)

<a id="c21"></a>

## C21 — Configuration organization

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 1. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Configuration is expressed by registration delegates, Option and SharedWithin rather than a host configuration binding layer; these models are explicitly protected by core instructions.

**Target — Existing rule.** Keep container options in the existing registration/lifetime model. Do not invent binding, reload or snapshot requirements for this library. Validate unsupported option combinations through the existing option model.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect public defaults and option-to-scope mapping against matching overloads; any future host adapter owns its external keys and reload semantics.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [.github/instructions/ioc-core.instructions.md](../../../.github/instructions/ioc-core.instructions.md), [.agents/AGENTS.md](../../../.agents/AGENTS.md)

<a id="c22"></a>

## C22 — Shared functionality placement

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Helpers contains Infra, Cache, Disposer and compatibility shims; factory construction remains in FactoryProviders, with shared state/entry mechanics elsewhere.

**Target — Existing rule.** Reuse helpers within their responsibility and extend existing factory-provider families. Keep target-framework shims conditional and local; avoid creating a second parallel factory abstraction.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Compare proposed helper callers and dependency direction with existing placements; require a concrete shared responsibility before moving local code.

**Unresolved decisions:** None.

**Evidence:** [.agents/AGENTS.md](../../../.agents/AGENTS.md), [.github/instructions/factory-providers.instructions.md](../../../.github/instructions/factory-providers.instructions.md), [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/Helpers/Disposer.cs](../../../src/Xtate.IoC/Helpers/Disposer.cs), [src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs](../../../src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs)

<a id="c01"></a>

## C01 — Domain terminology

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** The guide distinguishes service key, implementation entry, factory, decorator, scope, ownership and forwarding. Empty denotes the zero-argument marker, while SharedWithin selects sharing boundaries.

**Target — Existing rule.** Use TypeKey for type-plus-argument identity, ImplementationEntry for runtime lifetime behavior, and owned/external/forwarding precisely. Do not use singleton as a synonym for ownership.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Compare API terminology and docs with lifetime mappings and the Empty alias; check that ownership is stated independently of sharing.

**Unresolved decisions:** None.

**Evidence:** [.agents/AGENTS.md](../../../.agents/AGENTS.md), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/Properties/GlobalUsings.cs](../../../src/Xtate.IoC/Properties/GlobalUsings.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs)

<a id="c02"></a>

## C02 — Type naming by responsibility

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 6. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** The sample has IServiceProvider, ServiceCollection, ScopedOwnerImplementationEntry, DecoratorAsyncFactoryProvider, ResourcesExtensions and Infra. Suffixes distinguish runtime entries, provider builders and extension APIs.

**Target — Established pattern.** Keep I-prefixed interfaces and existing responsibility suffixes: ImplementationEntry, FactoryProvider and Extensions. Owner marks ownership, not merely scoped or singleton sharing. Short foundational helper names remain role-specific.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Compare new types with their immediate family rather than requiring every implementation to end in Service; verify owner naming against actual bin registration.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceCollection.cs](../../../src/Xtate.IoC/IoC/ServiceCollection.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs), [src/Xtate.IoC/Helpers/ResourcesExtensions.cs](../../../src/Xtate.IoC/Helpers/ResourcesExtensions.cs), [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs)

<a id="c03"></a>

## C03 — Method and property naming

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Arg0 asynchronous lookup is GetService/GetRequiredService; synchronous lookup uses Sync. Disposal/initialization methods follow DisposeAsync/InitializeAsync. IsRegistered is a predicate.

**Target — Established pattern.** Preserve the library's unsuffixed async resolution APIs and Sync counterparts; use existing framework Async names for lifecycle contracts. Keep Add/Get/Create and Is predicates aligned with peer operations.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Inspect paired APIs and their return types. Do not mechanically append Async to public resolution methods or rename framework contract members.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [src/Xtate.IoC/AsyncInitialization/AsyncInitializationHandler.cs](../../../src/Xtate.IoC/AsyncInitialization/AsyncInitializationHandler.cs), [src/Xtate.IoC/IoC/ObjectsBin.cs](../../../src/Xtate.IoC/IoC/ObjectsBin.cs)

<a id="c04"></a>

## C04 — Field, parameter and local naming

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** ServiceProvider and Cache use _camelCase private fields, PascalCase properties/constants, and camelCase parameters/locals. Registration lambdas use sp and discard underscore for unused Empty arguments.

**Target — Established pattern.** Match these role-specific forms; use T/TArg and descriptive generic names from the corresponding API family. Preserve public parameter names when compatibility matters; short sp and discard names are acceptable in adapters.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Compare declarations with same-role adjacent members and configured abbreviation rules; distinguish constants and public fixture fields from private instance fields.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [Xtate.IoC.sln.DotSettings](../../../Xtate.IoC.sln.DotSettings)

<a id="c05"></a>

## C05 — Units and qualifiers

**Scope and comparison unit:** library role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** DisposeToken, internalServicesCount, ConcurrencyLevel, SharedWithin and Owner convey purpose, cardinality or lifetime. The inspected core has no general time-unit naming scheme.

**Target — Established pattern.** Use count for cardinality and explicit scope/ownership qualifiers for lifetime distinctions. Preserve typed options and argument identities; do not add invented numeric limits or duration conventions.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Check name/type meaning against actual use, especially counts versus indexes and ownership versus sharing; any new duration API must explicitly state its unit.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/IoC/SharedObjectsBin.cs](../../../src/Xtate.IoC/IoC/SharedObjectsBin.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs)

<a id="c06"></a>

## C06 — Namespace and module structure

**Scope and comparison unit:** all role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 5. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Production source uses file-scoped Xtate.IoC despite organizational subfolders; project DotSettings explicitly skips these folders. All 12 example files declare Xtate.IoC.Test.Examples, while examples.instructions.md requires Xtate.IoC.Examples.

**Target — Selected implementation policy.** Preserve production namespace-folder exceptions and use Xtate.IoC.Test.Examples for executable examples. The stale example instruction is corrected; no public example types are renamed.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Insufficient for a complete auditable target.

**Inspection and compliance:** Compare file namespaces with project DotSettings and path-specific instructions. Resolve explicit instruction conflicts before claiming namespace compliance.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [src/Xtate.IoC/Xtate.IoC.csproj.DotSettings](../../../src/Xtate.IoC/Xtate.IoC.csproj.DotSettings), [test/Xtate.IoC.Test/Xtate.IoC.Test.csproj.DotSettings](../../../test/Xtate.IoC.Test/Xtate.IoC.Test.csproj.DotSettings), [.editorconfig](../../../.editorconfig), [.github/instructions/examples.instructions.md](../../../.github/instructions/examples.instructions.md), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs)

<a id="c07"></a>

## C07 — Folder responsibilities

**Scope and comparison unit:** all role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 4. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** The guide identifies runtime IoC, ImplementationEntry, FactoryProviders, AsyncInitialization, Helpers, Interfaces, Properties, test Tests/Examples and the separate benchmark project.

**Target — Existing rule.** Place new work with its responsibility and analogous peer family. Keep examples under the test project and benchmarks separate; organizational folders need not create public subnamespaces.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Compare path and responsibility against the guide; a small type is not grounds to bypass an established folder role.

**Unresolved decisions:** None.

**Evidence:** [.agents/AGENTS.md](../../../.agents/AGENTS.md), [.github/instructions/csharp.instructions.md](../../../.github/instructions/csharp.instructions.md), [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md), [.github/instructions/benchmarks.instructions.md](../../../.github/instructions/benchmarks.instructions.md)

<a id="c08"></a>

## C08 — File naming and contents

**Scope and comparison unit:** all role cohort; compare files and their relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 6. This is discovery sampling, not an exhaustive compliance audit. Supporting instructions/settings may be outside the cohort. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** ServiceProvider.cs and Cache.cs match their principal types and may contain nested support types. DecoratorExample.cs and Benchmarks.cs contain multiple related top-level fixture types. GlobalUsings and assembly attributes have dedicated files.

**Target — Established pattern.** Use principal-type file names for runtime code; allow related demonstration/benchmark fixture types in scenario files and nested implementation helpers. Keep global imports and assembly metadata in their existing dedicated locations.

**Rationale and confidence:** Preserve repository instructions and demonstrated role contracts before imposing uniformity. Moderate; based on the named discovery sample, not a prevalence census.

**Inspection and compliance:** Inspect top-level declarations by role, not a universal one-type-per-file rule. Generated Resources.Designer.cs is excluded from style comparisons.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs), [test/Xtate.IoC.Benchmark/Benchmarks.cs](../../../test/Xtate.IoC.Benchmark/Benchmarks.cs), [src/Xtate.IoC/Properties/GlobalUsings.cs](../../../src/Xtate.IoC/Properties/GlobalUsings.cs), [test/Xtate.IoC.Test/ParallelizeAttribute.cs](../../../test/Xtate.IoC.Test/ParallelizeAttribute.cs)

<a id="c32"></a>

## C32 — Test placement and naming

**Scope and comparison unit:** tests role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 60 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Tests and Examples are distinct folders in one MSTest project. DisposerTest and ObjectsBinTest use subject-based names; DecoratorExample uses Resolve.

**Target — Existing rule.** Keep behavior tests under Tests, runnable scenarios under Examples, and benchmarks in their separate project. Match peer Test/Example names.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Map production capabilities to matching tests and verify MSTest discovery; short scenario names are valid when class context is sufficient.

**Unresolved decisions:** None.

**Evidence:** [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs](../../../test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs), [test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj](../../../test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj)


<a id="c33"></a>

## C33 — Test organization

**Scope and comparison unit:** tests role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 60 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Sample tests use MSTest and async Task; the example uses ValueTask. Assembly configuration enables method-level parallelism.

**Target — Existing rule.** Keep deterministic independent tests with behavioral assertions. Await async work; no Task-only convention is established.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect setup/action/assertions and ensure expected exceptions cannot pass silently when absent. Catch-only samples do not justify missing assertions.

**Unresolved decisions:** None.

**Evidence:** [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs](../../../test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs), [test/Xtate.IoC.Test/ParallelizeAttribute.cs](../../../test/Xtate.IoC.Test/ParallelizeAttribute.cs)


<a id="c34"></a>

## C34 — Fixtures and doubles

**Scope and comparison unit:** tests role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 60 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Disposal tests have nested fakes, counters and completion signals; examples have related top-level collaborators; culture tests restore UI culture.

**Target — Existing rule.** Keep doubles local/nested when practical, allow readable example collaborators, and restore environment changes. Prefer completion signals to timing assumptions.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect ownership, shared state and finally cleanup under method-level parallel execution.

**Unresolved decisions:** None.

**Evidence:** [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs](../../../test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs), [test/Xtate.IoC.Test/Tests/ResourcesThreadCultureTest.cs](../../../test/Xtate.IoC.Test/Tests/ResourcesThreadCultureTest.cs)


<a id="c35"></a>

## C35 — Failure and boundary tests

**Scope and comparison unit:** tests role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 60 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Samples cover faulted/canceled/incomplete disposal, counts, idempotency and virtual cleanup order. Instructions require affected scopes, arities and ownership.

**Target — Existing rule.** Cover affected sync/async paths, arities or a proven common path, identity, ownership, initialization, scope boundaries and decorator order.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Ensure failure assertions fail when exceptions are absent; inspect both completed/pending paths and optional/required absence. Existing tests do not prove exhaustive coverage.

**Unresolved decisions:** None.

**Evidence:** [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs](../../../test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs), [.github/instructions/implementation-entry.instructions.md](../../../.github/instructions/implementation-entry.instructions.md)


<a id="c36"></a>

## C36 — Public-contract documentation

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Owner entries have detailed XML comments; IServiceProvider and Arg0 APIs are terse. Universal XML coverage is not established.

**Target — Selected implementation policy.** Prioritize externally consumed contracts and changed APIs. Require XML documentation on new or changed public contracts, with a shared guide for repeated overload semantics; do not require boilerplate on every unchanged declaration.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Compare comments with actual signatures, generic nullability and control flow; avoid copied ownership descriptions.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/Interfaces/IServiceProvider.cs](../../../src/Xtate.IoC/Interfaces/IServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [test/Xtate.IoC.Test/Examples/DecoratorExample.cs](../../../test/Xtate.IoC.Test/Examples/DecoratorExample.cs), [README.md](../../../README.md)


<a id="c37"></a>

## C37 — Implementation comments

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Provider comments explain internal counts; Infra explains invariants; factory suppression is narrow. A scoped owner field comment describes transient instances.

**Target — Established pattern.** Explain invariants, compatibility and ownership/cache decisions; contradictory copied text is not a convention.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Compare comments to bodies/tests and inspect suppression scope rather than comment volume.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs)


<a id="c38"></a>

## C38 — Component documentation structure

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Root README, repository guide, path instructions and runnable examples supply complementary guidance; per-project READMEs are not established.

**Target — Established pattern.** Keep overview/use/build guidance central, developer rules in the existing guide/instructions, and executable usage in Examples. Link this workspace from an index without empty per-folder guides.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Validate placement independently from content and check links/snippets against current APIs.

**Unresolved decisions:** None.

**Evidence:** [README.md](../../../README.md), [.agents/AGENTS.md](../../../.agents/AGENTS.md), [.github/instructions/examples.instructions.md](../../../.github/instructions/examples.instructions.md), [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md)


<a id="c23"></a>

## C23 — Member ordering

**Scope and comparison unit:** all role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** DotSettings provides default ordering and interface regions. Special xUnit/NUnit patterns do not describe the MSTest cohort.

**Target — Existing rule.** Use configured order: public delegates, public enums, matching public Provider nested classes, static fields/constants, instance fields, constructors, properties/indexers, interface regions, remaining members, nested types. Fields sort readonly then name; constructors by static status. Preserve initialization dependencies and non-reorderable patterns.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect matcher priorities, including interface members outranking ordinary categories; do not invent event/helper slots or apply xUnit/NUnit fixture patterns to MSTest.

**Unresolved decisions:** None.

**Evidence:** [Xtate.IoC.sln.DotSettings](../../../Xtate.IoC.sln.DotSettings), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceCollection.cs](../../../src/Xtate.IoC/IoC/ServiceCollection.cs), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs)


<a id="c24"></a>

## C24 — Related-member grouping

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 5. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Provider/collection use Interface regions; Arg0 APIs use extension blocks; owner entries keep fast/wait helpers near each other.

**Target — Established pattern.** Retain protocol grouping, related overload adjacency and nearby fallback logic, subject to layout and semantic dependencies.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Compare paired overloads and preserve preprocessor and initializer boundaries.

**Unresolved decisions:** None.

**Evidence:** [Xtate.IoC.sln.DotSettings](../../../Xtate.IoC.sln.DotSettings), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceCollection.cs](../../../src/Xtate.IoC/IoC/ServiceCollection.cs), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs)


<a id="c25"></a>

## C25 — Constructor organization

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Provider initializes bins/cache/token before Initialization; owner delegates to base then stores its bin; Cache constructors assign dictionaries.

**Target — Established pattern.** Preserve prerequisite initialization before callbacks/publication. Match family parameter order and base delegation; use expressions for simple assignments and blocks for staged setup.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Trace assignment/callback order and reference ownership before moving initializers.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs)


<a id="c26"></a>

## C26 — Accessibility

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 5. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Provider/owner types have public/protected seams; Infra/Cache/factory helpers are internal. Tests receive InternalsVisibleTo.

**Target — Existing rule.** Preserve compatibility and intended extensibility. Follow explicit accessibility suggestions for non-interface members and keep helper visibility role-specific.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Check overrides, public surface and test access before sealing/narrowing; implementation responsibility does not imply internal C# visibility.

**Unresolved decisions:** None.

**Evidence:** [.editorconfig](../../../.editorconfig), [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs)


<a id="c27"></a>

## C27 — Field mutability

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Caches/bin references are readonly; provider token source is exchanged atomically; shared bins have mutable reference counts.

**Target — Established pattern.** Use readonly for fixed references while retaining necessary synchronized lifecycle sentinels/counters. Readonly does not imply immutable contents.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Inspect write sites, ownership and publication independently of field syntax.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/SharedObjectsBin.cs](../../../src/Xtate.IoC/IoC/SharedObjectsBin.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs)


<a id="c28"></a>

## C28 — Constants and defaults

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Cache has local ConcurrencyLevel, provider names internalServicesCount, and registrations use Option.Default/SharedWithin.

**Target — Established pattern.** Keep semantic constants near owners and reuse lifetime/default models; avoid centralizing unrelated literals or changing tuning without measurements.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Trace constants through overloads, initialization and compatibility guards; preserve public defaults.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs), [.github/instructions/ioc-core.instructions.md](../../../.github/instructions/ioc-core.instructions.md)


<a id="c29"></a>

## C29 — Guards and main-path layout

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Owner execution checks incomplete completion, null result and bin registration; bin disposal returns early for missing state and cleans late additions.

**Target — Established pattern.** Use clear guards preserving creation/publication/cleanup order; fast/fallback equivalence is semantic.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Compare exits, exceptions and side effects, including late disposal and incomplete completions.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/IoC/ObjectsBin.cs](../../../src/Xtate.IoC/IoC/ObjectsBin.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [.github/instructions/implementation-entry.instructions.md](../../../.github/instructions/implementation-entry.instructions.md)


<a id="c30"></a>

## C30 — Expression decomposition

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Provider uses GroupServices/AddRegistration/CopyEntries/CreateEntries; owner code names tasks and uses wait helpers.

**Target — Established pattern.** Use named phases that clarify state/ownership, retaining compact single-purpose expressions. No numeric complexity limit is established.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Check helper responsibility, evaluation/exception timing and generic allocation implications before extraction.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs)


<a id="c31"></a>

## C31 — Property and method presentation

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 5. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Arg0/Cache forwarding uses expression bodies; provider/disposal procedures use blocks.

**Target — Established pattern.** Use expressions for direct forwarding/results and blocks for multi-step lifecycle/mutation. Preserve overload delegation.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Compare same-role members; do not compress ownership transitions just to reduce lines.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceCollectionArg0Extensions.cs)


<a id="c39"></a>

## C39 — Formatting

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Editorconfig specifies tabs width 4, newline catch/else/finally, compact initializer members and preserved single-line blocks; source rules require AGPL headers.

**Target — Existing rule.** Follow explicit local settings and header policy. No universal local encoding/line-ending rule is stated; preserve file conventions and inspect inherited settings.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Check effective path settings, retain suggestion severity and avoid unrelated formatting churn.

**Unresolved decisions:** None.

**Evidence:** [.editorconfig](../../../.editorconfig), [.github/instructions/csharp.instructions.md](../../../.github/instructions/csharp.instructions.md), [Xtate.IoC.sln.DotSettings](../../../Xtate.IoC.sln.DotSettings)


<a id="c40"></a>

## C40 — Imports and usings

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Library/test GlobalUsings centralize imports; library aliases Empty/NotNullAttribute. Cache/Infra use specialty imports. System-first is configured; current global lists are not fully sorted.

**Target — Existing rule.** Keep shared/global and specialty/local roles; preserve aliases and explicit System-first ordering. Existing order does not override configuration.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect global/local/conditional imports together and verify alias semantics; use tooling for unused imports on code changes.

**Unresolved decisions:** None.

**Evidence:** [.editorconfig](../../../.editorconfig), [src/Xtate.IoC/Properties/GlobalUsings.cs](../../../src/Xtate.IoC/Properties/GlobalUsings.cs), [test/Xtate.IoC.Test/GlobalUsings.cs](../../../test/Xtate.IoC.Test/GlobalUsings.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/Helpers/Infra.cs](../../../src/Xtate.IoC/Helpers/Infra.cs)


<a id="c41"></a>

## C41 — Equivalent language syntax

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 6. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Editorconfig suggests var/default/inferred tuples/object initializers/keywords/reduced this. Build props select preview; code uses extension blocks and collection expressions.

**Target — Existing rule.** Follow suggestion-level preferences when semantically equivalent and compatible with every target; new syntax does not authorize new runtime APIs.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Check language version, guards, evaluation order and nullability before normalization.

**Unresolved decisions:** None.

**Evidence:** [.editorconfig](../../../.editorconfig), [Directory.Build.props](../../../Directory.Build.props), [.github/instructions/csharp.instructions.md](../../../.github/instructions/csharp.instructions.md), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [src/Xtate.IoC/IoC/ServiceCollection.cs](../../../src/Xtate.IoC/IoC/ServiceCollection.cs), [src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs](../../../src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs)


<a id="c42"></a>

## C42 — Project and build organization

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 5. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Projects separate a six-target package library, multi-target tests with optional net462, and net10.0 benchmark executable. Generated local props import enclosing settings.

**Target — Existing rule.** Preserve target/packability roles and compatibility branches. Treat generated files as inputs and use repository restore/build/test entry points; benchmarks remain separate.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect projects plus enclosing imports; code changes need focused/applicable compatibility checks. This documentation task claims no build result.

**Unresolved decisions:** None.

**Evidence:** [.agents/AGENTS.md](../../../.agents/AGENTS.md), [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj), [test/Xtate.IoC.Test/Xtate.IoC.Test.csproj](../../../test/Xtate.IoC.Test/Xtate.IoC.Test.csproj), [test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj](../../../test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj), [Directory.Build.props](../../../Directory.Build.props)


<a id="c43"></a>

## C43 — Analyzers and suppressions

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 5. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Build props enable analyzers/style; library uses warnings-as-errors and private ConfigureAwaitChecker. Factory suppresses StaticMemberInGenericType locally.

**Target — Existing rule.** Retain configuration and narrow justified suppressions; distinguish suggestions from errors and avoid global disablement for one cache pattern.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect effective properties and suppression scope/rationale; do not suppress ownership/compatibility issues as cosmetic exceptions.

**Unresolved decisions:** None.

**Evidence:** [Directory.Build.props](../../../Directory.Build.props), [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj), [.editorconfig](../../../.editorconfig), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs), [Xtate.IoC.sln.DotSettings](../../../Xtate.IoC.sln.DotSettings)


<a id="c44"></a>

## C44 — Dependency declarations

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 6. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Central management supplies local benchmark and generated shared versions. All three projects omit inline PackageReference versions and use role/framework scopes.

**Target — Existing rule.** Maintain versions in Directory.Packages.props, preserve generated inputs, omit inline versions and retain PrivateAssets/target conditions.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect items and imports for duplicate/incompatible versions; do not infer an unobserved lockfile/audit policy.

**Unresolved decisions:** None.

**Evidence:** [Directory.Packages.props](../../../Directory.Packages.props), [Global.Packages.props](../../../Global.Packages.props), [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj), [test/Xtate.IoC.Test/Xtate.IoC.Test.csproj](../../../test/Xtate.IoC.Test/Xtate.IoC.Test.csproj), [test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj](../../../test/Xtate.IoC.Benchmark/Xtate.IoC.Benchmark.csproj), [.github/instructions/csharp.instructions.md](../../../.github/instructions/csharp.instructions.md)


<a id="c45"></a>

## C45 — Complexity and responsibility

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 2. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Provider decomposes orchestration; entries isolate lifetimes. Reviewed settings provide no numeric threshold.

**Target — Selected implementation policy.** Review cohesion, state ownership, dependency direction, failure transitions and completed/awaited path symmetry. Require a concrete defect or readability gain before extraction; numeric size thresholds alone do not justify changes. Measure performance-sensitive refactors independently.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Moderate; discovery sample, not a prevalence census.

**Inspection and compliance:** Require concrete readability/correctness evidence and measure performance-sensitive changes separately.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [.github/instructions/factory-providers.instructions.md](../../../.github/instructions/factory-providers.instructions.md), [.github/instructions/implementation-entry.instructions.md](../../../.github/instructions/implementation-entry.instructions.md)


<a id="c46"></a>

## C46 — Concurrency and synchronization

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 5. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Cache uses ConcurrentDictionary, provider exchanges its token source, shared bins use Interlocked, but ObjectsBin outer nullable state transitions are plain reads/writes.

**Target — Selected implementation policy.** Concurrent resolution bookkeeping is supported. Coordinate service use and scope creation with disposal. GetImplementationEntry stays available after disposal. In-flight bin additions have best-effort object disposal. The known native-handle cleanup race remains a separate regression, not an accepted guarantee.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. Insufficient for a full auditable target.

**Inspection and compliance:** Trace publication, races and reentrancy with deterministic tests; an inner concurrent collection cannot prove outer lifecycle safety.

**Unresolved decisions:** None for the selected policy. Current implementation compliance requires a separate audit.

**Evidence:** [src/Xtate.IoC/IoC/ServiceProvider.cs](../../../src/Xtate.IoC/IoC/ServiceProvider.cs), [src/Xtate.IoC/IoC/ObjectsBin.cs](../../../src/Xtate.IoC/IoC/ObjectsBin.cs), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [src/Xtate.IoC/IoC/SharedObjectsBin.cs](../../../src/Xtate.IoC/IoC/SharedObjectsBin.cs), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs)


<a id="c47"></a>

## C47 — Asynchronous execution

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Resolution returns ValueTask; owner checks successful completion then awaits with ConfigureAwait(false). Sync Disposer intentionally does not await incomplete async disposal, covered by a named test.

**Target — Existing rule.** Preserve ValueTask APIs, required ConfigureAwait(false), equivalent paths and completed initialization before successful async resolution. Retain tested sync disposal compatibility without spreading background work.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Trace consumption, completion, cancellation and errors; benchmark GetResult calls do not justify blocking library async paths.

**Unresolved decisions:** None.

**Evidence:** [.github/instructions/csharp.instructions.md](../../../.github/instructions/csharp.instructions.md), [.github/instructions/implementation-entry.instructions.md](../../../.github/instructions/implementation-entry.instructions.md), [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/AsyncInitialization/AsyncInitializationHandler.cs](../../../src/Xtate.IoC/AsyncInitialization/AsyncInitializationHandler.cs), [src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs](../../../src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs), [src/Xtate.IoC/Helpers/Disposer.cs](../../../src/Xtate.IoC/Helpers/Disposer.cs), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Benchmark/Benchmarks.cs](../../../test/Xtate.IoC.Benchmark/Benchmarks.cs)


<a id="c48"></a>

## C48 — Serialization and external representation

**Scope and comparison unit:** all role cohort (configuration and its source consumers); compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 194 files; directly inspected evidence in this cohort: 4. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Localized resources/accessors provide representation; no wire serialization layer is documented. Instructions protect resource keys/placeholders.

**Target — Existing rule.** Preserve resource keys/accessors and placeholder meaning across languages; edit resx, not generated code. Do not infer network-format conventions.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Compare resource keys/placeholders and call sites; resource changes require localization tests, and future wire formats require explicit contracts.

**Unresolved decisions:** None.

**Evidence:** [.github/instructions/resources.instructions.md](../../../.github/instructions/resources.instructions.md), [src/Xtate.IoC/Helpers/ResourcesExtensions.cs](../../../src/Xtate.IoC/Helpers/ResourcesExtensions.cs), [test/Xtate.IoC.Test/Tests/ResourcesThreadCultureTest.cs](../../../test/Xtate.IoC.Test/Tests/ResourcesThreadCultureTest.cs), [src/Xtate.IoC/Xtate.IoC.csproj](../../../src/Xtate.IoC/Xtate.IoC.csproj)


<a id="c49"></a>

## C49 — Time, culture and determinism

**Scope and comparison unit:** tests role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 60 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Messages use Resources.Culture or CurrentUICulture; French culture tests restore UI culture. Benchmark rules prohibit randomization/logging/unrelated I/O.

**Target — Existing rule.** Keep localization, environment restoration and deterministic tests/benchmarks. Do not impose invariant culture on localized errors or invent clock-service requirements.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Inspect restoration under parallel tests and use completion signals rather than timing assumptions.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/Helpers/ResourcesExtensions.cs](../../../src/Xtate.IoC/Helpers/ResourcesExtensions.cs), [test/Xtate.IoC.Test/Tests/ResourcesThreadCultureTest.cs](../../../test/Xtate.IoC.Test/Tests/ResourcesThreadCultureTest.cs), [.github/instructions/tests.instructions.md](../../../.github/instructions/tests.instructions.md), [.github/instructions/benchmarks.instructions.md](../../../.github/instructions/benchmarks.instructions.md), [test/Xtate.IoC.Test/Tests/DisposerTest.cs](../../../test/Xtate.IoC.Test/Tests/DisposerTest.cs), [test/Xtate.IoC.Test/ParallelizeAttribute.cs](../../../test/Xtate.IoC.Test/ParallelizeAttribute.cs)


<a id="c50"></a>

## C50 — Allocation and performance conventions

**Scope and comparison unit:** library role cohort; compare files and relevant declarations/operations within the same responsibility. Eligible inventory: 96 files; directly inspected evidence in this cohort: 3. Discovery sampling, not an exhaustive compliance audit. Exact sets or normalized INVENTORY.json selectors are in the tracker.

**Observed:** Owner fast paths avoid async state machines; delegates use nested static caches; benchmarks separate direct creation and IoC with memory/CPU diagnosers.

**Target — Existing rule.** Preserve established ValueTask paths and generic caching with equivalent behavior. Measure relevant changes; no undocumented pooling/allocation budget is selected.

**Rationale and confidence:** Preserve explicit instructions and demonstrated role contracts before imposing uniformity. High for the explicit rule; implementation compliance is not assessed.

**Inspection and compliance:** Verify ownership/initialization first, then controlled baseline measurements; keep setup outside measured work and do not claim gains from syntax alone.

**Unresolved decisions:** None.

**Evidence:** [src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs](../../../src/Xtate.IoC/ImplementationEntry/ScopedOwnerImplementationEntry.cs), [src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs](../../../src/Xtate.IoC/FactoryProviders/Asynchronous/DecoratorAsyncFactoryProvider.cs), [.github/instructions/factory-providers.instructions.md](../../../.github/instructions/factory-providers.instructions.md), [.github/instructions/implementation-entry.instructions.md](../../../.github/instructions/implementation-entry.instructions.md), [src/Xtate.IoC/Helpers/Cache.cs](../../../src/Xtate.IoC/Helpers/Cache.cs), [test/Xtate.IoC.Benchmark/Benchmarks.cs](../../../test/Xtate.IoC.Benchmark/Benchmarks.cs), [.github/instructions/benchmarks.instructions.md](../../../.github/instructions/benchmarks.instructions.md)



## Public contract content matrix (C36)

| Surface | Required content |
| --- | --- |
| IServiceProvider | Lookup absence, disposal token meaning, optional initialization/actions and lookup after disposal. |
| New/changed Arg0–4 APIs | Service/argument key semantics, required versus optional absence, sync/async completion and collection ordering. Shared overload prose may live in the README. |
| Owner entries and scope APIs | Who owns instances, provider/shared lifetime and partial-construction cleanup. |
| Factories and callbacks | Deferred invocation, exception precedence and whether async work must complete before use. |
| Unchanged public declarations | Correct misleading comments when touched; no blanket XML retrofit. |

### Invalid-input compatibility evidence (C11)

| Family | Current behavior / guard ownership |
| --- | --- |
| Provider extension receiver | Null fails when lookup is attempted (NullReferenceException); both collection enumeration forms preserve the same timing. |
| Registration factory delegates | Null may register successfully and fail when invoked; sync and async resolution are characterized by ArgumentValidationTest. |
| ServiceCollection.Add | Requires a non-null entry; it appends before accessing Key. It is not a transactional public validation boundary. |
| Container.Create callback | Non-generic overload invokes its callback directly; generic module overloads explicitly allow a null optional callback. |
| Provider constructor | Non-null collection is the declared contract; internal nullable overlay handling is not a promise that all null constructor inputs fail uniformly. |
| Options and factory assignability | Retain specialized existing validation and localized failures; do not replace Infra invariant errors mechanically. |

Review new contracts against these distinctions and test the affected boundary, rather than imposing a new universal exception on existing overloads.