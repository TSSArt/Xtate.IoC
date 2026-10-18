# Audit coverage

> Historical audit coverage: source changes and convention revision 52 invalidate these dispositions. See TODO.md for current implementation status; no fresh compliance audit is claimed.

**Historical snapshot:** P1 remediation subsequently changed the maintained inputs and selected C13/C15/C46 policies. The coverage and counts below describe the original audit only. Current implementation verification is in [IMPLEMENTATION.P1.json](IMPLEMENTATION.P1.json); task status is in [TODO.md](TODO.md). Refresh discovery/audit evidence before treating these coverage records as current.

Completed on 2026-10-18 for the **43 agreed criteria**, against convention revision 51 and its pinned SHA-256 digest. C06, C11, C13, C15, C36, C45 and C46 are **outside this audit**, with seven decision tasks in the same [TODO](TODO.md). They are not counted as compliant or audited. Completion here means the selected audit and plan are complete; none of the remediation is implemented.

The canonical workspace is `docs/project/consistency/`, rooted at the Xtate.IoC solution. The 194 maintained source/configuration/guide inputs are unchanged since documenting. Two existing convention artifacts are additionally included as C38 metadata-integrity inputs. Sibling solutions, generated build output, IDE metadata and benchmark output are outside scope. Generated resource accessors are source-generation evidence; generated shared props are effective configuration inputs rather than style peers.

## Reconstructing coverage

- [AUDIT.inventory.json](AUDIT.inventory.json): exact source paths, roles and hashes.
- [AUDIT.structure.json](AUDIT.structure.json) and [AUDIT.members.json](AUDIT.members.json): all 158 maintained C# files, declarations, constructors/primary constructors, parameters, variables, operators and member locations.
- [AUDIT.coverage.json](AUDIT.coverage.json): every criterion/file disposition, evidence, concrete exception rationale and task ID; this is the authoritative detailed enumeration behind the table.
- [AUDIT.checks.json](AUDIT.checks.json): API families, 503 attributed test bodies (641 data-expanded cases), all 45 explicit library await expressions, all six resource key/placeholder inventories, naming/import/header/explicit-whitespace checks, member-layout candidates and exact suppression locations.
- [AUDIT.reproduction.json](AUDIT.reproduction.json): bounded sequential initialization calls proving the fault/cancellation state problem (CONS-012).

A file unit includes all relevant declarations/operations in its enumerated member inventory, not just its first search hit. Semantic boundary checks trace the corresponding provider/entry/factory/helper paths and related tests. This is a static consistency audit with baseline execution, not a proof of general correctness or performance. No historical compatibility baseline was inferred from the current checkout.

`Compliant + Deviation + Justified exception + Unreviewed = Applicable`. N/A records are counted separately and include reasons. A file with several findings counts once as a deviation for that criterion and links all related tasks. The same file intentionally appears under several criteria; summing the table does **not** give a distinct source-file count. Criterion scopes follow comparable responsibilities; no maintained component was excluded for its size or unusual role.

## Reconciliation by criterion

| Criterion | Applicable | Compliant | Deviation | Justified exception | Unreviewed | N/A | Tasks |
|---|---:|---:|---:|---:|---:|---:|---|
| [C01](CONVENTIONS.md#c01) | 116 | 112 | 4 | 0 | 0 | 0 | [CONS-002](TODO.md#cons-002), [CONS-013](TODO.md#cons-013), [CONS-014](TODO.md#cons-014) |
| [C02](CONVENTIONS.md#c02) | 95 | 95 | 0 | 0 | 0 | 1 |  |
| [C03](CONVENTIONS.md#c03) | 90 | 90 | 0 | 0 | 0 | 6 |  |
| [C04](CONVENTIONS.md#c04) | 90 | 90 | 0 | 0 | 0 | 6 |  |
| [C05](CONVENTIONS.md#c05) | 96 | 96 | 0 | 0 | 0 | 0 |  |
| [C07](CONVENTIONS.md#c07) | 158 | 158 | 0 | 0 | 0 | 0 |  |
| [C08](CONVENTIONS.md#c08) | 158 | 148 | 0 | 10 | 0 | 0 |  |
| [C09](CONVENTIONS.md#c09) | 17 | 17 | 0 | 0 | 0 | 79 |  |
| [C10](CONVENTIONS.md#c10) | 96 | 96 | 0 | 0 | 0 | 0 |  |
| [C12](CONVENTIONS.md#c12) | 13 | 12 | 0 | 1 | 0 | 83 |  |
| [C14](CONVENTIONS.md#c14) | 96 | 96 | 0 | 0 | 0 | 0 |  |
| [C16](CONVENTIONS.md#c16) | 9 | 9 | 0 | 0 | 0 | 87 |  |
| [C17](CONVENTIONS.md#c17) | 95 | 95 | 0 | 0 | 0 | 1 |  |
| [C18](CONVENTIONS.md#c18) | 8 | 8 | 0 | 0 | 0 | 7 |  |
| [C19](CONVENTIONS.md#c19) | 96 | 96 | 0 | 0 | 0 | 0 |  |
| [C20](CONVENTIONS.md#c20) | 23 | 21 | 2 | 0 | 0 | 73 | [CONS-001](TODO.md#cons-001) |
| [C21](CONVENTIONS.md#c21) | 15 | 15 | 0 | 0 | 0 | 81 |  |
| [C22](CONVENTIONS.md#c22) | 96 | 96 | 0 | 0 | 0 | 0 |  |
| [C23](CONVENTIONS.md#c23) | 151 | 149 | 2 | 0 | 0 | 7 | [CONS-016](TODO.md#cons-016) |
| [C24](CONVENTIONS.md#c24) | 90 | 90 | 0 | 0 | 0 | 6 |  |
| [C25](CONVENTIONS.md#c25) | 32 | 32 | 0 | 0 | 0 | 64 |  |
| [C26](CONVENTIONS.md#c26) | 95 | 95 | 0 | 0 | 0 | 1 |  |
| [C27](CONVENTIONS.md#c27) | 87 | 87 | 0 | 0 | 0 | 9 |  |
| [C28](CONVENTIONS.md#c28) | 96 | 96 | 0 | 0 | 0 | 0 |  |
| [C29](CONVENTIONS.md#c29) | 96 | 95 | 1 | 0 | 0 | 0 | [CONS-012](TODO.md#cons-012) |
| [C30](CONVENTIONS.md#c30) | 90 | 90 | 0 | 0 | 0 | 6 |  |
| [C31](CONVENTIONS.md#c31) | 90 | 90 | 0 | 0 | 0 | 6 |  |
| [C32](CONVENTIONS.md#c32) | 60 | 58 | 0 | 2 | 0 | 0 |  |
| [C33](CONVENTIONS.md#c33) | 58 | 51 | 3 | 4 | 0 | 2 | [CONS-003](TODO.md#cons-003), [CONS-015](TODO.md#cons-015) |
| [C34](CONVENTIONS.md#c34) | 60 | 60 | 0 | 0 | 0 | 0 |  |
| [C35](CONVENTIONS.md#c35) | 58 | 50 | 4 | 4 | 0 | 2 | [CONS-003](TODO.md#cons-003), [CONS-012](TODO.md#cons-012), [CONS-015](TODO.md#cons-015) |
| [C37](CONVENTIONS.md#c37) | 96 | 94 | 2 | 0 | 0 | 0 | [CONS-014](TODO.md#cons-014) |
| [C38](CONVENTIONS.md#c38) | 17 | 14 | 3 | 0 | 0 | 0 | [CONS-004](TODO.md#cons-004), [CONS-013](TODO.md#cons-013) |
| [C39](CONVENTIONS.md#c39) | 158 | 94 | 0 | 64 | 0 | 0 |  |
| [C40](CONVENTIONS.md#c40) | 158 | 158 | 0 | 0 | 0 | 0 |  |
| [C41](CONVENTIONS.md#c41) | 158 | 158 | 0 | 0 | 0 | 0 |  |
| [C42](CONVENTIONS.md#c42) | 15 | 15 | 0 | 0 | 0 | 0 |  |
| [C43](CONVENTIONS.md#c43) | 173 | 154 | 19 | 0 | 0 | 0 | [CONS-017](TODO.md#cons-017) |
| [C44](CONVENTIONS.md#c44) | 8 | 8 | 0 | 0 | 0 | 7 |  |
| [C47](CONVENTIONS.md#c47) | 96 | 94 | 1 | 1 | 0 | 0 | [CONS-012](TODO.md#cons-012) |
| [C48](CONVENTIONS.md#c48) | 9 | 8 | 1 | 0 | 0 | 0 | [CONS-002](TODO.md#cons-002) |
| [C49](CONVENTIONS.md#c49) | 63 | 63 | 0 | 0 | 0 | 0 |  |
| [C50](CONVENTIONS.md#c50) | 98 | 98 | 0 | 0 | 0 | 0 |  |

## Verification and limits

- Baseline test command: `dotnet test test/Xtate.IoC.Test/Xtate.IoC.Test.csproj -f net10.0 --no-restore --verbosity minimal`: 641 passed, zero failures/skips.
- Baseline library build: `dotnet build src/Xtate.IoC/Xtate.IoC.csproj --no-restore --verbosity minimal -m:1 -nodeReuse:false -p:UseSharedCompilation=false`, with telemetry and MSBuild server disabled for this environment: all six targets passed, zero warnings/errors. Earlier attempts failed on sandbox registry/named-pipe access and compiler-server cancellation; the in-process build resolved that tooling limitation.
- No compatibility-target **test** run or benchmark measurement is claimed. Compatibility library branches were inspected and compiled; default-symbol structural extraction includes the legacy shims, and the alternate pooled-array clear branch was inspected directly.
- Full MSBuild-based `dotnet format --verify-no-changes` could not connect to its build-host named pipe in the sandbox. [AUDIT.format.log](AUDIT.format.log) records the environment failure; it is not a repository build defect or a formatter success.
- Folder-mode whitespace verification completed with exit code 2: 4,096 advisory edits across 64 files, recorded in [format-report.json](format-report.json) and [AUDIT.whitespace.log](AUDIT.whitespace.log). Roslyn does not consume explicit DotSettings choices such as `SPACE_NEAR_POSTFIX_AND_PREFIX_OP=True`, `INDENT_PREPROCESSOR_REGION=OUTDENT`, aligned multiline arguments and preserved IDE layouts. Generic formatter output therefore does not itself establish a convention violation. Explicit tab/header/newline/cast/parenthesis checks have no candidates; the affected C39 file records explain this concrete configuration exception. No mass formatting task was inferred from conflicting tool defaults.
- Git CLI still reports `cannot change to T:/Projects/TSSArt/xtate.dev`. Source revision is read directly from the enclosing checkout ref where available, and actual source/configuration plus enclosing-input hashes are pinned. A clean Git worktree is not claimed.
- Earlier convention cohort counts were malformed (CONS-004). The current audit uses freshly normalized exact path/member sets; the convention text and documenting tracker remain unchanged. Refreshing them later must advance the convention revision and invalidate dependent coverage appropriately.

## Criterion evidence notes

### C01

Compared source type/lifetime/option/argument terminology with all maintained repository guides and resource forms. Verified resource argument omission, README name/accessibility/registration drift, and copied owner lifetime text against actual public declarations and mappings; related tasks preserve ownership versus sharing distinctions.

### C02

Enumerated every library type including nested generic families; interface I-prefix check has no candidate. Compared entry/provider/extension/helper suffixes with actual responsibilities, keeping foundational Any/Infra/Cache names and ownership-specific Owner suffixes.

### C03

Inspected all library method/property names and signatures in declaration inventory, then compared each async/sync registration/resolution family and framework lifecycle names. Unsuffixed ValueTask resolution and Sync variants are deliberate; framework InitializeAsync/DisposeAsync stay distinct.

### C04

Inspected all private instance fields (underscore/camelCase scan has no candidate), static fields/constants, parameters/generic parameters and local declarations in member inventory. sp/underscore adapters and reflection-expression stage names are role-appropriate; no public parameter rename is prescribed.

### C05

Reviewed count/index/capacity and lifetime/ownership identifiers against uses in provider, cache, tuple helpers and bins; remaining library declarations have no hidden external time-unit contract. Typed SharedWithin/Option/Owner qualifiers distinguish sharing from ownership.

### C07

Enumerated all maintained production/test/example/benchmark paths and compared every principal/partial declaration responsibility with the documented folder roles. No misplaced maintained component found; small/compatibility helpers were included.

### C08

Compared file basenames with every top-level/nested declaration, including operators/primary constructors in AUDIT.members.json. Related Module/TypeKey families, examples and benchmark fixtures are valid within the selected target; partial creator contributions and metadata/import files have concrete role explanations.

### C09

Traced all required/optional lookup and factory adapters, core entry guards, exception creation and nullability/delegate selection. Required lookups retain missing-service exceptions and operational faults; optional singular/factory paths preserve generic defaults and empty enumeration. Baseline required/value-type/nullability tests pass; exception-precedence decisions remain outside C15 scope.

### C10

Reviewed every nullable/generic signature and absence branch in contracts, lookup/entry/factory/delegate paths and NullabilityHelper attribute traversal. Nullable build settings are enabled; required notnull and optional T? maintain distinct value-type semantics. No immutable-array or empty-actions substitution is assumed. Six library target builds and value/nullability tests pass.

### C12

Enumerated every CancellationToken/DisposeToken occurrence, traced provider token creation/cancel/dispose, async factory token expression and signature validation, and legacy CancelAsync shim. Empty async enumeration has no pending work and intentionally ignores token. Existing tokenless resolution contracts remain unchanged.

### C14

Traced ServiceCollection list versus membership set, Option.IfNotRegistered gating, provider grouping/chain copies and child overlays, typed-key equality and per-argument shared caches; reviewed remaining collection-bearing helper structures for their roles. Ordered list duplicates and public mutable diagnostic context/array references are not silently treated as immutable snapshots. Scope/identity/ordering tests pass.

### C16

Enumerated all ActionsEventType values and action/context contracts; traced provider registration notifications and ImplementationEntry sync/completed/pending Enter/Exit/SyncExit/Error flow, plus debugger adapter dispatch. Existing action count/context tests pass. Callback-failure exception precedence remains the explicit excluded C15 decision; no new logger/redaction policy is invented.

### C17

Reviewed all interface/public/implementation declarations and the single-library project boundary. IServiceProvider intentionally exposes ImplementationEntry; helpers/providers remain internal where designed. No new assembly split, narrowed public type or adapter dependency is present.

### C18

Inspected all three project references plus shared/local props; dependency direction is test/benchmark to library, no reverse edge or solution-internal cycle. Enclosing imports recorded; net10.0 build/test succeeded.

### C19

Traced all direct construction sites, constructor/static factories and delegate creators: public service composition flows through registrations, internal caches/bins/tokens use direct construction, expression/reflection code is centralized in existing provider/helpers. Reflection-based user-module scanning was not found. Factory/delegate tests and examples pass.

### C20

Compared all ten public arity families and all enumerable/singular factory getter creators. Arg0 public collection factories and zero-argument collection-factory injection lack the corresponding arity 1-4 paths. Verified Empty/tuple forwarding, scope/option/module guards and decorator order; same root cause is deduplicated into CONS-001.

### C21

Traced every Option/SharedWithin declaration/default/use, option validation and mapping to InstanceScope in registration and fluent factory APIs. Scope overlays/modules own composition; no binding/reload/snapshot layer exists to standardize. Unsupported options remain distinct from missing services.

### C22

Reviewed responsibility and callers for every helper, factory/provider family and IoC file; shared conversion/nullability/tuple infrastructure is centralized while lifetime-specific behavior stays in entries. Conditional framework shims remain local. No parallel ad-hoc construction layer found.

### C23

Enumerated direct members of every class/struct in all 158 maintained C# files, compared field/nested-type phases and inspected the actual default and framework-specific layout patterns. Verified two private fixture types precede methods without a matching exception. Extension blocks remain related units; no unsupported semantic reorder is prescribed.

### C24

Compared interface regions, related overload groups and fast/wait helpers across all library declarations. Extension blocks and partial delegate-creator families keep conceptual groups; conditional shims preserve boundaries. No conflicting grouping found.

### C25

Inspected all explicit constructors plus primary constructors from AUDIT.members.json, including provider cache/token/bin order, owner base delegation, factory delegate/parameter/member initialization and readonly wrapper state. Prerequisite initialization precedes dependent registration/callbacks in the audited paths.

### C26

Enumerated all access modifiers, protected/virtual overrides, internal helpers, friend assembly settings and public contracts. Public owner/provider seams and internal factory infrastructure follow their documented roles; no blanket sealing/narrowing is proposed. All six library targets compile.

### C27

Reviewed every field declaration and mutable-state write role: caches/owner references are readonly; disposal sentinels, reference counts, initialization state, pooled arrays and reflection mapping buffers have necessary mutation. Thread-safety guarantees remain decision-only C46.

### C28

Enumerated constant/default declarations and public optional values; traced Option.Default, SharedWithin mappings, local counts, pool/cache capacities and static initialization dependencies. No unapproved default/lifetime replacement or invented numeric threshold found.

### C29

Traced guards in registration, provider lookup, owner entries, factory parameter/member filling and async initialization. Early-return layouts preserve phases except AsyncInit has no terminal failure transition (CONS-012). Broader disposal/concurrency guarantees remain excluded C13/C46 decisions.

### C30

Reviewed staged helpers in provider, class factory/parameter/member filling, type/tuple mapping and ownership execution alongside every method declaration/body in structural inventory. Helpers preserve named phases; no numeric complexity threshold is applied. Larger generic mapping responsibilities are policy-only C45.

### C31

Compared expression versus block bodies for every library declaration, including expression forwarding and multi-stage mutable/lifecycle methods. Dense generic tuple adapters are direct forwarding; multi-step creation/cleanup retains blocks and named waits.

### C32

Enumerated all 60 MSTest/example C# files and their attributes/names against production subjects and scenario roles; GlobalUsings/Parallelize are infrastructure, not missing tests. Benchmark code remains separate. Executable examples participate in the passing net10.0 run.

### C33

Reconciled all test/assertion/catch/discard candidates with their named contract and inspected helper assertions. Correct operation names and awaited completion are required for CONS-003/015. Explicit no-throw compatibility cases are justified individually, not rejected solely for lacking Assert. 503 attributed methods expand to 641 passing net10.0 cases.

### C34

Inspected nested/top-level fixture declarations, setup methods, singleton/shared test state, completion sources and culture restoration across all test/example files; compared distinct example role with behavior-test fixtures. Static test collection in AncestorTest is concurrent and used for coordinated collection tests, not an order-dependent fixture; culture mutations restore in finally. Delays simulate async work and are awaited rather than used as readiness assertions.

### C35

Reconciled boundary-test subjects with implementation paths and all attributed/data-row methods; verified missing AsyncInit failure/cancellation termination checks and ineffective disposal/constant tests. Intentional no-throw empty-struct/handle/option/key tests are not blanket failures. Broader undecided lifetime/concurrency guarantees are outside C13/C46 audit scope.

### C37

Compared source XML/implementation comments with declared roles, owner bin assignments, flags, initialization and cache protocols. Scoped/singleton owner fields contain copied transient descriptions, verified against actual mappings; comments do not establish unapproved concurrency/cleanup guarantees. Resource/localized text is checked separately under C48.

### C38

Inspected all maintained README/guide/instruction files and three project roles, checked expected centralized versus colocated guides and executable example links, and compared README code/name/target claims with compiled declarations/examples. Missing convention index and stale API snippets map to CONS-013; earlier convention coverage metadata inconsistency maps to CONS-004. No empty per-component README is demanded.

### C39

Ran explicit AST checks on all maintained C# files for catch/else/finally newlines, cast spacing and argument/parameter parenthesis spacing, plus header/tab checks; no direct candidate. Folder-mode whitespace verification suggested 4096 edits in 64 files under Roslyn defaults; these are not automatically normative because explicit repository DotSettings differs. Compared actual IDE settings and retained role-specific alignment/regions/operators. Full MSBuild formatter could not connect to its sandboxed named pipe; no semantic formatter pass is claimed.

### C40

Inspected every root/global/local using block and alias in all maintained C# files; no System directive follows an unrelated non-alias import. Preserved Empty/NotNull and ValueTuple aliases, global versus specialty import roles, and conditional compatibility namespaces.

### C41

Reviewed language-form choices from all declaration/method bodies and effective preview settings: var/default/patterns/initializers/qualification preferences stay suggestions, explicit base-typed expression variables and compatibility shims are semantically justified. All six library targets compile.

### C42

Compared all three project roles, six library TFMs, modern/optional-net462 tests, net10 benchmark, solution membership, generated imports, NuGet sources and workflow build/test/pack ordering. Effective net10.0 build succeeded; no performance claim or compatibility test claim.

### C43

Inspected all shared/local analyzer properties and every suppression directive in maintained source/tests/examples. Scoped generic static, compatibility, fixture and deliberate sync-path suppressions are role-supported; 19 blanket All suppressions are broader than those reasons and map to CONS-017. Source-wide .NET analyzers compiled on all six library TFMs; IDE-only inspection results are not inferred from build success.

### C44

Parsed all PackageReference/PackageVersion declarations and imports: central versions, no inline PackageReference versions, framework-specific compatibility packages and private analyzer/shared-content assets. Roles and project-reference direction checked.

### C47

Inspected every library await expression (45, all ConfigureAwait(false)), all fast-path/fallback sites and initialization/delegate ownership. Traced AsyncInit terminal-state control flow and reproduced three sequential failure modes; success-only tests do not cover this defect. Deliberate sync disposal behavior remains a compatibility exception, not a general blocking/background-work rule.

### C48

Compared every key and placeholder set across all six resx files; no missing/extra key or placeholder mismatch. Traced all ResourcesExtensions format calls and missing-service exception entry points; one wrong-key formatter verified. Localized error tests passed but omit this argument assertion.

### C49

Inspected culture/time/random/static-state/delay uses in all test/example/benchmark files and resource formatter. Both culture tests restore prior UI culture in finally; fake delays are awaited before behavioral assertions and do not define a readiness deadline. Benchmark methods have no randomization/logging/external I/O. No UTC clock service or invariant localized messages is prescribed.

### C50

Traced completed-ValueTask guards/fallbacks, static generic delegate/type caches and Cache wrapper across runtime code; inspected ClassFactoryProvider argument ArrayPool rent/clear/return and async continuation finally paths. Scoped/singleton Task caching has a distinct sharing role. Benchmark setup and direct-instantiation baseline stay separate; no timing/allocation improvement is claimed and no benchmark run was required for unchanged code.

