# Consistency TODO

Remaining-TODO implementation (2026-10-18): 12 of the 13 previously open items are complete. CONS-017 has its suppression changes in place but remains open for ReSharper inspection verification; no inspection CLI is installed locally.

The zero-argument collection factory additions were removed by owner decision: existing enumerable APIs already defer service creation. The remaining runtime fix is the argument-aware resource key. Existing validation timing, exception precedence and post-disposal lookup behavior are preserved. Documentation and tests now reflect the owner's reviewed lifecycle decisions, superseding the original P1 implementation descriptions below.

Verification: solution test run built all six library targets. net10.0, net9.0 and net8.0 passed 718/718 tests. net11.0 and net462 each passed 717/718, with only the previously observed WeakReferenceSetLifecycleTest.AddTakeCompactionAndDisposal_LeaveNoHandlesInEitherQueue handle race failing. This pass does not change WeakReferenceSet synchronization or weaken that regression.

The historical discovery inventory is preserved. Revision 52 repairs its normalized library/test cohorts (96/60 files), records the selected policies, and marks the dependent audit stale. This is not a fresh source-wide compliance audit. Exact cohort selectors, sample/supporting-input derivation and convention digest are in DOCUMENTING.progress.json and consistency.json.

<a id="cons-012"></a>
- [x] **CONS-012 [P1] Complete async-initialization state after faults and cancellation.** Terminal failures/cancellation remain cached; LifecycleFailureTest covers current and future waiters.

<a id="cons-005"></a>
- [x] **CONS-005 [P1] Decide concurrent resolution/add/dispose guarantees (C46).** The [lifecycle contract](../../lifecycle.md) records owner-selected best-effort in-flight object disposal and post-disposal metadata lookup. Scope creation and service use require caller coordination. The known WeakReferenceSet native-handle race remains unresolved; earlier claims of complete internal handle synchronization and a provider acquisition gate are superseded.

<a id="cons-006"></a>
- [x] **CONS-006 [P1] Decide cleanup behavior after disposal or initialization failures (C13).** Bins stop at the first disposer failure. Provider cleanup releases references and attempts later phases through using/finally; the latest exception wins. Failed-construction cleanup follows the same precedence. No new aggregation contract applies.

<a id="cons-007"></a>
- [x] **CONS-007 [P1] Choose diagnostic callback exception precedence (C15).** A throwing Error callback replaces the original error. DiagnosticFailureTest covers sync and async paths; no secondary-exception Data attachment is used.

<a id="cons-001"></a>
- [x] **CONS-001 [P2] Closed without adding zero-argument collection factories.** Owner decision: GetServices<T>() / GetServicesSync<T>() and directly injected enumerables already defer service creation until iteration. Extra Func wrappers are unnecessary. CollectionEnumerationTest covers direct/injected enumeration, ordering, empty results and deferred creation.

<a id="cons-002"></a>
- [x] **CONS-002 [P2] Use the argument-aware missing-service resource key.**
  - Implementation: Uses Exception_ServiceArgMissedInContainer. Regression assertions cover all six resource cultures and preserve both service and argument names.
  - Criteria: [C48](CONVENTIONS.md#c48), [C01](CONVENTIONS.md#c01).
  - Affected: `src/Xtate.IoC/Helpers/ResourcesExtensions.cs:Exception_ServiceArgMissedInContainer`; all six `src/Xtate.IoC/Properties/Resources*.resx` as verification inputs; missing-service/localization tests.
  - Evidence: the formatter currently passes the one-placeholder `Resources.Exception_ServiceMissedInContainer` with both service and argument values. All locale files define `Exception_ServiceArgMissedInContainer` with argument-specific content; the argument is lost by the current call.
  - Required: select the matching argument resource and retain placeholder meaning in every locale; add assertions that the argument identity survives formatting.
  - Dependencies: none. Risk: low implementation risk, medium observable compatibility risk because exception message text changes. Preserve exception types and resource keys. Effort: small.
  - Acceptance: localized argument-aware missing-service assertions pass; resource key/placeholder comparisons pass; run `--filter "FullyQualifiedName~MissedServiceExceptionTest|FullyQualifiedName~Resources"` on net10.0.

<a id="cons-003"></a>
- [x] **CONS-003 [P2] Make the post-disposal tests verify the operation and failure they name.**
  - Implementation: Async addition now uses an awaited exception assertion; synchronous addition and idempotency tests call their named synchronous operations.
  - Criteria: [C33](CONVENTIONS.md#c33), [C35](CONVENTIONS.md#c35).
  - Affected: `test/Xtate.IoC.Test/Tests/ObjectsBinTest.cs:AddAsync_AfterDisposed_ShouldEagerlyDisposeAndThrow`, `AddSync_AfterDisposed_ShouldEagerlyDisposeAndThrow`, `Dispose_IsIdempotent_WhenCalledMultipleTimes`.
  - Evidence: the first test catches the exception without failing if no exception occurs; the Sync-named test calls `AddAsync`; the synchronous-idempotency test calls `DisposeAsync` twice.
  - Required: awaited exception assertion plus disposal-count assertion on the async path; exercise `AddSync` and repeated `Dispose` on the sync paths. Keep existing async coverage separately.
  - Dependencies: C13 ownership policy decision applies to future failure aggregation, not these already named observable behaviors. Risk: low, tests only. Effort: small.
  - Acceptance: each test fails when its named behavior is intentionally broken; no catch-only false positive; run net10.0 `--filter FullyQualifiedName~ObjectsBinTest`.

<a id="cons-004"></a>
- [x] **CONS-004 [P2] Repair convention discovery coverage metadata.**
  - Implementation: Recomputed historical discovery sets from normalized inventory selectors, reconciled sample/supporting inputs, selected policy overrides, advanced convention revision/digest and invalidated the dependent audit. The historical source inventory is intentionally retained; no fresh compliance audit is claimed.
  - Criteria: [C38](CONVENTIONS.md#c38), integrity of audit prerequisites.
  - Affected: `docs/project/consistency/CONVENTIONS.md`, `DOCUMENTING.progress.json`, original `INVENTORY.json`, manifest and dependent audit freshness metadata.
  - Evidence: early criterion records report zero eligible library files while citing library evidence; the actual normalized inventory contains 96 maintained library C# files. The earlier script filtered slash-separated patterns against backslash paths. No source input changed since that run.
  - Required: rerun the document action with normalized paths, correct exact cohort sets and sample counts, preserve target decisions, and invalidate affected audit records when the convention digest changes. This TODO action keeps the convention set read-only.
  - Dependencies: use the document workflow, preserving the shared workspace and decision ownership. Risk: low code risk; audit freshness must be handled. Effort: medium.
  - Acceptance: every convention count reconciles with its exact eligible/sample sets; no evidence file falls outside its stated cohort without an explicit supporting-input role; document and audit hashes reconcile.

<a id="cons-008"></a>
- [x] **CONS-008 [P2] Resolve example namespace instruction conflict (C06).**
  - Implementation: Selected Xtate.IoC.Test.Examples and corrected the stale instruction; existing example type identities are preserved.
  - Target: [C06](CONVENTIONS.md#c06). Affected: `.github/instructions/examples.instructions.md` and all 12 `test/Xtate.IoC.Test/Examples/*.cs` files, project namespace settings, callers and documentation references.
  - Alternatives: correct the instruction to `Xtate.IoC.Test.Examples`, or migrate example namespaces to `Xtate.IoC.Examples`. Recommend correcting the stale instruction because all examples consistently use the test-project namespace and path.
  - Owner: repository maintainer. Blocks example-namespace C06 audit. Risk: low for instruction correction; medium for public example-type renames. Effort: small.
  - Acceptance: one approved namespace, instructions and all example declarations agree, examples discover/run, references and links remain valid.

<a id="cons-009"></a>
- [x] **CONS-009 [P2] Select the public argument-validation contract (C11).**
  - Implementation: Selected compatibility-preserving boundary validation in C11. Documented family-specific null/deferred behavior; ArgumentValidationTest characterizes sync/async factory invocation and lookup timing. Existing internal invariant errors remain unchanged.
  - Target: [C11](CONVENTIONS.md#c11). Affected: public registration/provider constructors and extension entry points, `Infra` invariant helpers, factory validation, corresponding tests.
  - Alternatives: validate at public entry points with named parameter exceptions, or retain current deferred/internal failures with explicit documented contracts. Recommend new-entry-point validation while preserving existing exception compatibility until explicitly approved.
  - Next check: enumerate invalid-input paths and current exception types/timing across public API families. Owner: repository maintainer. Blocks universal C11 audit; do not mechanically convert internal invariant failures.
  - Risk: medium/high exception and timing compatibility; effort medium. Acceptance: approved guard ownership/timing/parameter policy and sync/async invalid-input tests.

<a id="cons-010"></a>
- [x] **CONS-010 [P2] Adopt public-contract documentation coverage (C36).**
  - Implementation: Selected prioritized public-contract coverage and a content matrix in C36. Documented IServiceProvider and the added collection APIs; corrected owner lifecycle text.
  - Target: [C36](CONVENTIONS.md#c36). Affected: public library declarations, especially `IServiceProvider`, Arg0–4 APIs and owner entries; executable examples.
  - Alternatives: require XML documentation for every public declaration, or prioritize externally consumed contracts and changed APIs. Recommend the latter initially, requiring absence/failure/lifetime/completion content where relevant.
  - Owner: repository maintainer. Blocks C36 audit. Risk: low code risk; avoid inventing guarantees or copying incorrect lifecycle text. Effort: medium.
  - Acceptance: approved required surface/content matrix and XML/signature/example validation; then refresh the documented target.

<a id="cons-011"></a>
- [x] **CONS-011 [P2] Adopt qualitative responsibility review criteria (C45).**
  - Implementation: Selected qualitative cohesion, ownership, dependencies and state-transition review in C45; no numeric thresholds or hot-path refactors.
  - Target: [C45](CONVENTIONS.md#c45). Affected: `ServiceProvider`, `ImplementationEntry`, `ClassFactoryProvider`, generic delegate helpers and related tests/benchmarks.
  - Alternatives: qualitative cohesion/state-transition checks, or an evidence-backed numeric threshold. Recommend qualitative checks; existing averages do not establish useful limits.
  - Owner: repository maintainer. Blocks C45 audit. Risk: medium if used to justify hot-path refactors; effort small for policy, separate estimates for changes.
  - Acceptance: approved review rubric tied to concrete ownership/dependency clarity; performance-sensitive refactors require independent baseline measurements.

<a id="cons-013"></a>
- [x] **CONS-013 [P2] Bring README examples and terminology into agreement with the executable API.**
  - Implementation: Corrected API calls, module inheritance, registration mapping, framework list, ownership/error guidance and links. ReadmeExample compiles/runs the self-contained Quick Start; other application-dependent snippets explicitly state their required context.
  - Criteria: [C01](CONVENTIONS.md#c01), [C03](CONVENTIONS.md#c03), [C38](CONVENTIONS.md#c38).
  - Affected: `README.md` introduction, Quick Start, Generic/Argumented Services, Modules, Forwarding, Async Initialization and Lifetimes sections; matching executable examples under `test/Xtate.IoC.Test/Examples`. Add an index link to `docs/project/consistency/CONVENTIONS.md`.
  - Evidence: snippets call nonexistent `Get<...>` methods; `class CoreModule : IModule` uses `Services` without implementing that interface member; Quick Start registers `MyService` then resolves `IMyService` without mapping them; `IAsyncInitializationHandler` is not the existing `IInitializationHandler`; `InstanceScope` is described as internal though declared public; the introductory framework list omits net11.0 despite the project/table including it.
  - Required: derive snippets from runnable examples using matching registrations, awaited/Sync resolution and a valid Module/IModule implementation; correct names/accessibility and keep one framework list consistent with the csproj. Do not rename production APIs to match stale prose.
  - Dependencies: CONS-008 for the eventual example namespace choice; fixes using existing public calls can proceed independently. Risk: low code risk, medium user guidance impact. Effort: medium.
  - Acceptance: every self-contained C# snippet compiles/runs in an example harness; illustrative partial snippets explicitly state required context; local links resolve and stated targets match project files.

<a id="cons-014"></a>
- [x] **CONS-014 [P2] Correct copied ownership descriptions and misleading example comments.**
  - Implementation: Corrected scoped/singleton field descriptions and the shared-lambda disposal comment.
  - Criteria: [C01](CONVENTIONS.md#c01), [C37](CONVENTIONS.md#c37).
  - Affected: `ScopedOwnerImplementationEntry.cs:_objectsBin` and `SingletonOwnerImplementationEntry.cs:_objectsBin`; `test/Xtate.IoC.Test/Examples/DisposalBehaviorExample.cs:Disposal`.
  - Evidence: both owner fields describe transient instances; singleton comments say instances are transient despite shared-bin tracking. The disposal example's initial comment says Shared lambda registrations are not owned, while its assertions prove the scoped/container lambdas are disposed.
  - Required: describe scoped versus singleton ownership and actual bin/provider lifetime accurately; distinguish unowned constant/forwarding from owned shared/transient lambda registration. Qualify any concurrency claims only after CONS-005.
  - Dependencies: none for correcting clear lifetime text; CONS-005 for unsupported thread-safety promises. Risk: low; comments only, no contract expansion. Effort: small.
  - Acceptance: comments agree with scope mappings and existing identity/disposal assertions; run the disposal example if executable documentation changes.

<a id="cons-015"></a>
- [x] **CONS-015 [P2] Replace placeholder tests with observable outcomes and await async disposal checks.**
  - Implementation: Removed duplicate placeholder smoke tests already covered by observable disposer tests. The ValueTask constant test now awaits and asserts the non-default int value on repeated resolutions.
  - Criteria: [C33](CONVENTIONS.md#c33), [C35](CONVENTIONS.md#c35), [C37](CONVENTIONS.md#c37).
  - Affected: `DisposerTest.Dispose_ShouldDisposeObject`, `DisposeAsync_ShouldDisposeObjectAsync`; `ModuleTest.AddConstant_ShouldAddValueTask`.
  - Evidence: disposal tests use a nondisposable plain object and contain placeholder assertion comments; the async-named test discards its task. AddConstant never resolves the registered value and says no assertion is needed.
  - Required: consolidate redundant no-throw smoke tests or make them assert meaningful disposal/value behavior; await every async operation; verify the constant can be resolved and the supplied non-default constant survives repeated resolution. Preserve intentional no-throw tests whose asserted contract is explicitly absence of an exception.
  - Dependencies: CONS-003 addresses separate post-disposal tests; CONS-012 adds missing initialization failure coverage. Risk: low, tests only. Effort: small/medium.
  - Acceptance: tests fail if the asserted operation is removed/broken; no placeholder assertion text or discarded disposal task remains in this exact set. Run the corresponding test classes on net10.0.

<a id="cons-017"></a>
- [ ] **CONS-017 [P2] Replace blanket inspection suppression with specific justified rules.**
  - Implemented: replaced all 19 blanket directives with named fixture-member inspections and rationale, retaining fixture-section boundaries. Pending: run ReSharper inspections to validate the exact rule set; no local CLI is installed. Roslyn builds and tests are not substitutes for that verification.
  - Criterion: [C43](CONVENTIONS.md#c43).
  - Affected: all 12 `test/Xtate.IoC.Test/Examples/*.cs`; and `Tests/AsyncInitializationHandlerTest.cs`, `FactoryProviderTest.cs`, `ImplementationEntryTest.cs`, `ImplementationTypeTest.cs`, `ServiceCollectionExtensionsTest.cs`, `ServiceProviderTest.cs`, `StubTypeTest.cs` under the same test project. Exact suppression locations are recorded in `AUDIT.checks.json:suppressions` (filter `ReSharper disable All`).
  - Evidence: these 19 maintained files have blanket `ReSharper disable All` directives, sometimes around fixture sections, which mask unrelated inspections as well as expected demonstration/test-double warnings.
  - Required: retain only named suppressions needed for public demonstration collaborators, reflection-created fixtures or deliberate test constructs; scope each narrowly and explain its reason. Do not rewrite useful examples merely to silence unused-member suggestions.
  - Dependencies: none; preserve the example namespace decision CONS-008. Risk: low behavior risk, medium analyzer churn. Effort: medium.
  - Acceptance: no blanket suppression remains in the exact affected set; documented narrow exceptions are reviewed, relevant IDE inspections run, and all existing net10.0 tests still pass. Roslyn build success alone cannot validate ReSharper inspections.

<a id="cons-016"></a>
- [x] **CONS-016 [P3] Move local culture fixture types after test members.**
  - Implementation: Moved both ServiceX fixtures to the end of their test classes without changing type identities.
  - Criterion: [C23](CONVENTIONS.md#c23).
  - Affected: `ResourcesLocalizationTest.cs:ServiceX`, `ResourcesThreadCultureTest.cs:ServiceX`; destination: nested-type block at the end of each containing test class.
  - Evidence: both classes place private ServiceX before their test methods; the active default member layout places nested types last. MSTest does not match the xUnit/NUnit-specific layout patterns.
  - Required: move only these declarations; preserve nested names, access, culture strings and test behavior. No namespace or public type rename is needed.
  - Dependencies: none. Risk: low; inspect metadata-sensitive strings and preserve type identity. Effort: small.
  - Acceptance: configured default ordering applies and localized expected type names remain unchanged; run both Resources test classes.

