"""Publish this audit's reviewed tasks without discarding implementation history."""
from pathlib import Path
import re
D=Path(__file__).resolve().parent
p=D/'TODO.md'; old=p.read_text(encoding='utf-8-sig')
marker='## Preserved implementation history (previous generation)'
history=old.split(marker,1)[1] if marker in old else old
match=re.search(r'<a id="cons-017"></a>.*?(?=<a id="cons-|\Z)',history,re.S)
prior17=match.group(0) if match else ''
if match: history=history[:match.start()]+history[match.end():]
prior17=prior17.replace('<a id="cons-017"></a>','').replace('- [ ] **CONS-017','**Previous CONS-017 record (historical):**\n\n**CONS-017')
current='''# Consistency TODO

Restarted audit, generation 2, 2026-10-28, against convention revision 52. Scope: Xtate.IoC library, tests/examples, benchmarks, and maintained solution settings/guides. Current source fingerprints are in [AUDIT.inventory.json](AUDIT.inventory.json); dispositions and limits are in [COVERAGE.md](COVERAGE.md). This is an audit and implementation plan; no production/test source or conventions were edited.

Verification: all six library TFMs built; all 739 tests passed on each of net11.0, net10.0, net9.0, net8.0 and net462. See [test log](AUDIT.test-all.log). A passing stress run does not disprove the evidenced handle race. The isolated diagnostic probe below fails with callbacks enabled. Formatter verification returned 2; reports include external/generated files, namespace exceptions and defaults not selected by repository policy, so raw report counts are not a remediation count.

## Current audit findings

<a id="cons-019"></a>
- [ ] **CONS-019 [P1] Consume completed factory ValueTasks once when diagnostics are enabled.**
  - Criteria: [C47](CONVENTIONS.md#c47), [C16](CONVENTIONS.md#c16), [C29](CONVENTIONS.md#c29).
  - Affected: `src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs:WithActions`, its caller `GetServiceNoActions`, and `WithActionsWait`; regression destination: `test/Xtate.IoC.Test/Tests/ServiceProviderActionsTest.cs` or `DiagnosticFailureTest.cs`.
  - Evidence: the completed branch reads `valueTask.Result` into the context and returns the original valueTask. The caller then consumes it again. [Probe source](AUDIT.ValueTaskProbe.cs) and [actual output](AUDIT.ValueTaskProbe.log): no actions resolves with one GetResult call; no-op actions cause InvalidOperationException with two GetResult calls. The probe uses a completed, single-consumption IValueTaskSource-backed forwarding factory, not an incomplete operation incorrectly blocked by the caller.
  - Required: after consuming a completed result, return a fresh result-backed ValueTask; preserve action ordering and the selected callback-exception precedence. Review all completed/pending nested action paths for single consumption.
  - Dependencies: none. Risk: medium; diagnostic exception/event timing is observable. Effort: small implementation, medium regression coverage. No public signature or parameter rename.
  - Acceptance: source-backed success/fault/cancellation tests, with zero/one/multiple ordinary and data callbacks; count GetResult exactly once per supplied operation. Preserve existing callback-failure tests; run `dotnet test test/Xtate.IoC.Test/Xtate.IoC.Test.csproj -f net10.0 --filter "FullyQualifiedName~ServiceProviderActionsTest|FullyQualifiedName~DiagnosticFailureTest"`, then all supported TFMs.

<a id="cons-018"></a>
- [ ] **CONS-018 [P1] Close the native-handle recycling race after WeakReferenceSet disposal.**
  - Criteria: [C46](CONVENTIONS.md#c46), [C13](CONVENTIONS.md#c13).
  - Affected: `src/Xtate.IoC/Helpers/WeakReferenceSet.cs:TryTake`, `Clean`, `FreeHandles`, and `CleanAndThrowIfDisposed`; legacy/native handle branches in `Helpers/WeakGCHandle.cs`; regression: `test/Xtate.IoC.Test/Tests/WeakReferenceSetLifecycleTest.cs:AddTakeCompactionAndDisposal_LeaveNoHandlesInEitherQueue`.
  - Evidence: TryTake can remove an active handle, then FreeHandles marks the set disposed and drains both queues, then TryTake enqueues that still-owned handle in `_freeHandles`. Clean also recycles outside its write-lock section. Subsequent FreeHandles returns early because `_cleanerHandle` is no longer allocated. This is the known regression explicitly excluded from accepted best-effort object disposal in [lifecycle.md](../../lifecycle.md). The stress regression passed in this run; the source interleaving remains possible.
  - Required: give each active/recycled handle one owner and prevent enqueue after the final drain, or dispose handles whose recycle loses closure. Preserve best-effort instance disposal and avoid invoking user disposal callbacks under a storage lock.
  - Dependencies: none; selected policy CONS-005 is already decided. Risk: medium/high synchronization and finalizer behavior; benchmark any hot-path locking change. Effort: medium.
  - Acceptance: deterministic coverage of recycle-versus-drain and compaction-versus-drain; both queues empty and cleaner unallocated after workers finish; no lost/double-freed handles, invalid native handle access, or deadlock. Preserve the existing stress test and run it on modern native WeakGCHandle and legacy shim targets; then run the full solution tests.

<a id="cons-020"></a>
- [ ] **CONS-020 [P2] Correct ObjectsBin XML documentation to the selected lifecycle contract.**
  - Criteria: [C36](CONVENTIONS.md#c36), [C37](CONVENTIONS.md#c37).
  - Affected: `src/Xtate.IoC/IoC/ObjectsBin.cs`, class remarks at lines 24–38 and AddAsync/AddSync exception prose. Required text belongs in those existing XML comments, referring to `docs/lifecycle.md` for the full matrix.
  - Evidence: class remarks promise all members are safe concurrently and exactly-once disposal via atomic exchange. Both disposal methods use a plain read followed by `_instancesForDispose = null`; the selected contract requires coordination for direct concurrent bin disposal. Closed additions can propagate a cleanup failure rather than always ObjectDisposedException. The storage is a set implemented with queues, not the claimed weak-reference stack.
  - Required: document caller coordination, best-effort in-flight additions, stop-on-first-failure cleanup, and exception precedence accurately. Do not change runtime behavior to match the stale comment.
  - Dependencies: none; independent of CONS-018. Risk/effort: low, documentation only; no new lifetime guarantees.
  - Acceptance: compare XML against both completed/pending disposal paths and lifecycle.md; existing lifecycle tests remain unchanged and pass; no universal atomic/exactly-once claim remains on the bin.

<a id="cons-017"></a>
- [ ] **CONS-017 [P2] Finish validating narrow inspection suppressions, including new documentation examples.**
  - Criterion: [C43](CONVENTIONS.md#c43). Original implementation history is retained below. No `ReSharper disable All` remains in maintained source.
  - Current affected set: every suppression site is enumerated with exact path/line/text in [AUDIT.checks.json](AUDIT.checks.json), `suppressions`. Existing fixture-member suppressions have role rationale. Newly added `Examples/DocumentationFeaturesExample.cs` disables eight ReSharper inspections at file scope (including AccessToDisposedClosure and DisposeOnUsingVariable) and IDE0200 without a local rationale; `Examples/DocumentationJobExample.cs` disables fixture-member inspections at file scope without explaining the scope.
  - Required: explain each retained rule, scope behavior-sensitive suppressions to intentional operations, and verify the original named replacements with ReSharper. Do not reintroduce blanket suppression or rewrite reflection-constructed fixtures merely to remove unused-member suggestions.
  - Dependencies: none. Risk: low behavior risk, medium analyzer churn. Effort: small/medium. No public type renames.
  - Acceptance: ReSharper inspection run recorded with the repository settings; each suppressed finding has an explicit role/operation rationale and the narrowest practical scope; all existing tests remain green. Roslyn/formatter success is not ReSharper verification. The CLI is not available in the current environment; this acceptance step remains outstanding.

<a id="cons-021"></a>
- [ ] **CONS-021 [P3] Sort both global-using lists according to the explicit import rule.**
  - Criterion: [C40](CONVENTIONS.md#c40).
  - Exact affected set: `src/Xtate.IoC/Properties/GlobalUsings.cs` and `test/Xtate.IoC.Test/GlobalUsings.cs`. No moves or renames.
  - Evidence: System.Threading precedes System.Linq/System.Diagnostics in the library, and System.Linq.Expressions precedes System.Diagnostics in tests. `.editorconfig` requires alphabetical System-first ordering; [format-report.json](format-report.json) contains IMPORTS diagnostics for exactly these files.
  - Required: sort ordinary global imports while preserving the Empty/NotNullAttribute aliases and their separate role. Do not mix this with broad formatter-default churn.
  - Dependencies: none. Risk/effort: low/small; preserve aliases and conditional compilation.
  - Acceptance: import-order verification has no IMPORTS changes for these two files, alias bindings are unchanged, and the solution builds.

## Preserved implementation history (previous generation)

These records preserve owner decisions and verified past implementation. Old evidence paragraphs describe the earlier audit, not current defects. Current deviations are listed above. Checked policy decisions do not imply the native-handle regression was fixed.
'''
p.write_text(current+history+'\n\n### Earlier CONS-017 implementation record\n\n'+prior17,encoding='utf-8')
