"""Reviewed runtime criterion dispositions for generation 2."""
import runpy, re
from pathlib import Path
D=Path(__file__).resolve().parent
api=runpy.run_path(str(D/'AUDIT.review.py')); checkpoint=api['checkpoint']; load=api['load']
structure=load('AUDIT.structure.json')
lib=[f['path'] for f in structure if f['role']=='library']
def matching(pattern):
    return [f['path'] for f in structure if f['role']=='library' and re.search(pattern,'\n'.join(m['signature']+' '+(m['body'] or '') for m in f['declarations']))]
entry='src/Xtate.IoC/ImplementationEntry/ImplementationEntry.cs'
weak='src/Xtate.IoC/Helpers/WeakReferenceSet.cs'
checkpoint('C09',matching(r'throw|FromException|GetRequired|GetService|TryGet|TryMatch'),
 'Traced required/optional lookups in all Arg0–4 and factory variants through entries: absence gives MissedServiceException for required lookup and generic default for optional lookup; operational failures propagate. Infra invariant failures and localization remain distinct. Corresponding absence, type and factory tests pass across five TFMs.')
checkpoint('C10',matching(r'\?|notnull|MaybeNull|NotNull|default'),
 'Reviewed nullable signatures, generic notnull/default constraints, null branches and attribute reflection across declaration inventory. Nullable is enabled in all evaluated projects. Optional handler/action arrays retain null; unconstrained T? preserves value-type default semantics. NullabilityHelper, required-member and value-type tests pass.')
checkpoint('C11',matching(r'public |Validate|Infra\.|IsAssignableFrom'),
 'Compared existing public boundaries to the C11 compatibility matrix: deferred factory null failures, direct callback invocation, optional generic module callbacks, nontransactional ServiceCollection.Add, specialized option/assignability validation and internal invariant exceptions remain intact. ArgumentValidationTest covers both sync/async forms; no new independent runtime entry points were introduced in the current dirty source set.')
checkpoint('C12',matching(r'CancellationToken|DisposeToken|Cancel\('),
 'Traced provider token creation/cancellation/disposal, async factory token forwarding, and NET8 compatibility CancelAsync shim using CancellationToken.None and TaskScheduler.Default. Resolution APIs remain tokenless. Failed/canceled initialization retains original exception/token for waiters; cancellation tests pass on native and compatibility TFMs.')
checkpoint('C13',matching(r'Dispos|ObjectsBin|WeakGCHandle|GCHandle|ReturnArray|RentArray'),
 'Traced owned transient/scoped/shared entries, external/forwarding entries, shared-reference increments/decrements, constructor-failure using cleanup and completed/pending bin paths. Owner registration precedes initialization; bins stop on first failure; provider finally releases shared reference. Native-handle post-drain recycling remains a deviation (CONS-018); object-level best-effort races remain explicitly permitted.',
 {weak:['CONS-018']},
 {'src/Xtate.IoC/Helpers/Disposer.cs':'Selected compatibility contract: sync Dispose starts an incomplete async-only disposer without awaiting it; it reports only already-observable failure. DisposerTest and lifecycle.md explicitly retain this behavior.',
  'src/Xtate.IoC/IoC/ObjectsBin.cs':'Selected lifecycle policy intentionally permits best-effort object disposal for in-flight additions and requires coordination of direct concurrent bin disposal. Stop-on-first-disposer-failure is intentional; misleading XML is tracked separately under C36/C37.'})
checkpoint('C14',matching(r'AddToChain|AsChain|_registrations|GroupServices|SourceServiceCollection|GetServices|IsRegistered'),
 'Inspected ServiceCollection list/membership operations, circular entry chain, GroupServices/AddRegistration/CopyEntries and SourceServiceCollection overlays. Membership does not suppress duplicates without IfNotRegistered; last decorator wraps previous registration and enumeration follows chain order. CollectionEnumerationTest and overlay/decorator tests pass.')
checkpoint('C15',matching(r'catch|HandleException|TypeInitHandle|GetFactoryException'),
 'Reviewed every catch/translation boundary in structural inventory. Infra unwraps type initialization with ExceptionDispatchInfo; factory builders retain localized DependencyInjectionException with inner cause. Sync/completed/pending diagnostic error callbacks replace the reported exception under the selected policy; no Exception.Data aggregation introduced. DiagnosticFailureTest passes.')
checkpoint('C16',matching(r'ActionsContext|ActionsEventType|IServiceProviderActions|WithActions|GetActionsService|ServiceLogger'),
 'Inspected registration notifications, Enter/Exit/SyncExit/Error paths, data callbacks and optional debugger. Instrumentation uses the established action protocol. Completed source-backed ValueTasks are consumed twice when actions are enabled: AUDIT.ValueTaskProbe.log proves the behavioral difference; CONS-019 covers event-preserving repair.',{entry:['CONS-019']})
checkpoint('C17',lib,
 'Reviewed all library type/member accessibility in AUDIT.structure.json against project/folder roles. Public IServiceProvider exposes ImplementationEntry intentionally; helpers/factory providers retain internal families and public extension seams remain public. No new assembly split, narrowing, namespace rename or reverse consumer dependency.')
checkpoint('C19',matching(r'Factory|Delegate|Expression\.|CreateInstance|new ServiceProvider'),
 'Traced registration to cached delegate creators, centralized factory providers and runtime entries, including all sync/async provider families and arity getter creators. Reflection/expression compilation remains in construction helpers; direct construction of caches/bins/token sources is infrastructure. No added assembly scanning or alternative user-service construction path.')
checkpoint('C20',[f for f in lib if '/IoC/' in f or '/FactoryProviders/' in f or '/ImplementationEntry/' in f],
 'Compared all ten Arg0–4 declaration/body families (AUDIT.checks.json api_families), tuple forwarding/order, option/lifetime mappings, FactoryImplementation.For overloads, Container.Create and module idempotency. Sync/async pairs preserve TypeKey identities. Zero-arg collection Func wrappers remain intentionally absent under owner decision CONS-001; direct lazy enumeration is exercised by tests.',
 exceptions={'src/Xtate.IoC/IoC/ServiceProviderArg0Extensions.cs':'Owner decision CONS-001: no zero-argument collection factory wrappers; GetServices/GetServicesSync and injected enumerables are already lazy. Arguments 1–4 require delegates to supply arguments; this semantic distinction justifies the API difference.'})
checkpoint('C21',matching(r'Option|SharedWithin|InstanceScope'),
 'Reviewed all option defaults, validation masks, shared-within switch mappings and factory product ownership options. SharedWithin.Scope/Container maps to scoped/singleton; DoNotDispose chooses external/forwarding only where supported. No host configuration binding/reload layer exists; option/model tests pass.')
checkpoint('C22',lib,
 'Compared every library path/type to its responsibility: Helpers contains cache/disposal/type/compatibility utilities; FactoryProviders contains delegate builders and getter families; ImplementationEntry owns lifetime execution; IoC owns registration/resolution. Target-framework shims remain conditional and local. No competing helper/factory subsystem found.')
checkpoint('C25',matching(r'\b(public|protected|private|internal) \w+(<[^>]+>)?\('),
 'Reviewed constructor declarations and bodies/primary constructors in source inventory, including provider bins/token/cache setup before callbacks, child reference increment after successful initialization, factory-provider base initialization and owner-bin capture. No movement across side-effect/publication boundaries; constructor and lifecycle tests pass.')
checkpoint('C27',matching(r'\breadonly\b|\bprivate\b.*_[a-z]'),
 'Enumerated all fields and traced mutable runtime fields: initializer state/result and builder storage, native handles, entry chains/delegate cache, scoped instance cache, disposal token/bin sentinels, shared reference count and debugger state all have writes after construction. Fixed infrastructure/cache references are readonly. Readonly collection references are not claimed immutable; concurrency defect is separately C46.')
checkpoint('C28',matching(r'\bconst\b|static readonly|Option.Default|SharedWithin'),
 'Reviewed constants/default parameters and generic static caches across all source declarations. Defaults delegate to the common option/lifetime model; ConcurrencyLevel and reflection attribute strings remain with their owners. No tuning/performance changes or new numeric policy inferred.')
checkpoint('C29',matching(r'IsCompleted|ThrowIf|return;|return default|if \('),
 'Compared guard exits and side effects in runtime bodies: disposal capture/nulling, owner publication, provider constructor failure and fast/fallback resolution. The completed diagnostic branch returns an already-consumed source ValueTask, unlike the awaited branch; reproduced by the audit probe and mapped to CONS-019.',{entry:['CONS-019']})
checkpoint('C30',lib,
 'Reviewed method bodies and helper responsibilities across library declarations, including provider grouping/copying, factory construction phases, tuple matching and owned creation. Existing named phases clarify state and ownership; no independent numeric complexity criterion or evidence-backed extraction is imposed. Concrete lifecycle/async findings remain in C13/C46/C47.')
checkpoint('C31',lib,
 'Compared expression-bodied wrappers and block-bodied lifecycle/mutation code in the structural inventory. Arg0/tuple arities and getter wrappers directly forward; provider construction/disposal and state transitions keep explicit blocks. Direct simple assignments and interface adapter expressions retain existing family presentation.')
checkpoint('C45',lib,
 'Applied the selected qualitative rubric to type/member bodies: provider composition, entry lifetime, cached delegate construction and helpers retain distinct responsibilities. Concrete failures are tracked under CONS-018 and CONS-019; no separate size-based restructuring task or unmeasured hot-path refactor is justified.')
checkpoint('C46',matching(r'Interlocked|Volatile|lock \(|Concurrent|_delegateEntry|WeakGCHandle|GCHandle|DisposeToken'),
 'Traced initialization publication, per-entry/per-argument scoped/singleton locks, cache publication, provider disposal claim, shared references and handle recycling. Source-derived TryTake/Clean versus FreeHandles interleaving can leave recycled native handles after closure. The recorded lifecycle contract explicitly excludes that race from accepted best-effort disposal; CONS-018 remains necessary despite passing stress tests.',{weak:['CONS-018']})
checkpoint('C47',matching(r'ValueTask|await |IAsyncEnumerable|IsCompleted'),
 'Inspected every runtime await expression (all use ConfigureAwait(false)), ValueTask return/consumption paths, cached initialization failures, compatibility shims and async disposal. Probe confirms completed diagnostic callbacks double-consume a single-use source ValueTask; CONS-019. Other tested sync/async failure and initialization paths retain selected contracts.',{entry:['CONS-019']},
 {'src/Xtate.IoC/Helpers/Disposer.cs':'Sync disposal intentionally starts incomplete async-only cleanup without joining it; selected compatibility behavior, not a recommendation for async resolution.',
  'src/Xtate.IoC/Helpers/CancellationTokenSourceExtensions.cs':'Legacy CancelAsync adapter uses Task.Factory.StartNew on TaskScheduler.Default under !NET8_0_OR_GREATER; compatibility implementation retained.'})
checkpoint('C50',lib+['test/Xtate.IoC.Benchmark/Benchmarks.cs','test/Xtate.IoC.Benchmark/Program.cs'],
 'Inspected generic nested caches, factory ArrayPool rent/clear/return paths, completed/fallback ValueTasks and benchmark setup/method separation. No performance numbers or gains are claimed; no production changes made. Benchmark GetResult calls operate on its deliberately synchronously completing fixture factories, unlike general async APIs. CONS-019 takes precedence over preserving a faulty fast path.',{entry:['CONS-019']},
 {'test/Xtate.IoC.Benchmark/Benchmarks.cs':'Fixtures are explicitly completed-value factories/initializers; direct GetAwaiter().GetResult benchmarks are role-specific and do not authorize blocking arbitrary source-backed ValueTasks. Setup is outside measured methods; no timing claim made.'})
