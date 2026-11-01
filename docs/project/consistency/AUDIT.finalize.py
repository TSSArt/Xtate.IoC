"""Validate and publish the completed audit without modifying source/conventions."""
import collections, datetime, hashlib, json, re, subprocess
from pathlib import Path
D = Path(__file__).resolve().parent
ROOT = D.parents[2]
def load(n): return json.loads((D/n).read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest().upper()
def save(n, value):
    tmp = D/(n+'.tmp')
    tmp.write_text(json.dumps(value, indent=2, ensure_ascii=False)+'\n', encoding='utf-8')
    tmp.replace(D/n)
inv = load('AUDIT.inventory.json')
p = load('TODO.progress.json')
m = load('consistency.json')
paths = {f['path'] for f in inv}
assert all(sha(ROOT/f['path']) == f['sha256'].upper() for f in inv)
actual = subprocess.check_output(['rg','--files','--hidden','-g','!bin','-g','!obj','-g','!.git','-g','!.vs','-g','!TestResults','-g','!BenchmarkDotNet.Artifacts','-g','!docs/project/consistency/**','-g','!*.DotSettings.user'],cwd=ROOT,text=True)
assert {x.replace('\\','/') for x in actual.splitlines()} == paths
assert sha(D/'CONVENTIONS.md') == p['inputs']['conventions_digest'] == m['conventions_digest']
assert sha(D/'DOCUMENTING.progress.json') == p['inputs']['document_progress_sha256']
assert m['conventions_revision'] == p['inputs']['conventions_revision'] == 52
rows = load('AUDIT.coverage.json')
s = load('AUDIT.summary.json')['criteria']
assert set(s) == {f'C{i:02}' for i in range(1,51)}
todo = (D/'TODO.md').read_text(encoding='utf-8')
tasks = re.findall(r'- \[ \] \*\*(CONS-\d+)', todo)
assert len(tasks) == len(set(tasks)) == 5
for cid, unit in s.items():
    assert len(unit['rows']) == len(paths)
    assert {r['path'] for r in unit['rows']} == paths
    for row in unit['rows']:
        assert row['disposition'] != 'Unreviewed'
        assert row['source_sha256'] == next(f['sha256'] for f in inv if f['path'] == row['path'])
        if row['disposition'] == 'Deviation': assert row['tasks'] and set(row['tasks']) <= set(tasks)
        if row['disposition'] == 'Justified exception': assert row['evidence']
for unit in p['work_units']:
    assert unit['state'] == 'complete' and not unit['remaining']
    assert set(unit['eligible_set']) == set(unit['processed']) == paths
assert len(rows) == 50*len(paths)
now = datetime.datetime.now(datetime.timezone.utc).isoformat()
logs = {n:(D/n).read_text(encoding='utf-8-sig') for n in ['AUDIT.test-current.log','AUDIT.test-all.log','AUDIT.ValueTaskProbe.log']}
assert logs['AUDIT.test-current.log'].count('Passed:   739') == 5
assert 'GetResult calls=2' in logs['AUDIT.ValueTaskProbe.log']
save('AUDIT.verification.json', {
    'updated_at':now, 'verification_date':'2026-10-30',
    'source_revision':'88d74702ce1685dd4544cfe68abbd776f6cc44a1',
    'source_identity':'Dirty worktree identified by AUDIT.inventory.json, not HEAD alone. Existing package edits and two documentation examples included.',
    'test_command':'dotnet test Xtate.IoC.sln --no-restore -m:1 -nodeReuse:false -p:UseSharedCompilation=false',
    'test_exit_code':0,
    'tests':{t:{'passed':739,'failed':0,'skipped':0} for t in ['net11.0','net10.0','net9.0','net8.0','net462']},
    'library_targets':['net11.0','net10.0','net9.0','net8.0','netstandard2.0','net462'],
    'logs':logs, 'log_hashes':{n:sha(D/n) for n in logs},
    'probe':'Earlier isolated net10.0 probe uses AUDIT.ValueTaskProbe.cs and the unchanged runtime. No-op callbacks consume a source-backed ValueTask twice. No production test was added.',
    'format':'Earlier read-only dotnet format --verify-no-changes --no-restore --severity info exited 2. Report predates the latest two example edits; it remains diagnostic evidence, not current clean-formatter certification.',
    'limits':['ReSharper CLI inspection not run; CONS-017 acceptance remains outstanding.','Syntax extraction uses default symbols; branch review and all-target builds supplement it.','Passing stress tests do not disprove a possible race.','No benchmark measurements.']})
todo = todo.replace('Restarted audit, generation 2, 2026-10-28','Completed audit, generation 2, 2026-10-30',1)
todo = todo.replace('[test log](AUDIT.test-all.log)','[verification evidence](AUDIT.verification.json)',1)
todo = todo.replace('[actual output](AUDIT.ValueTaskProbe.log)','[actual output](AUDIT.verification.json)',1)
todo = todo.replace('disables eight ReSharper inspections at file scope (including AccessToDisposedClosure and DisposeOnUsingVariable)','disables seven ReSharper inspections at file scope (including AccessToDisposedClosure and UseAwaitUsing)',1)
todo = todo.replace('disables fixture-member inspections at file scope without explaining the scope','disables fixture-member inspections and UseAwaitUsing at file scope without explaining the scope',1)
todo = todo.replace('[C29](CONVENTIONS.md#c29).','[C29](CONVENTIONS.md#c29), [C35](CONVENTIONS.md#c35), [C50](CONVENTIONS.md#c50).',1)
(D/'TODO.md').write_text(todo,encoding='utf-8')
lines = ['# Consistency audit coverage','',
    'Completed generation 2 on 2026-10-30 against convention revision 52. All 50 criteria are reviewed across the 205-file inventory. The audit and remediation plan are complete; five implementation tasks remain open: 2 P1, 2 P2 and 1 P3.','',
    'Exact per-file dispositions, hashes, evidence and task IDs: [coverage enumeration](AUDIT.coverage.json). Review methods and selectors: [summary](AUDIT.summary.json). Inputs: [inventory](AUDIT.inventory.json), [syntax declarations and bodies](AUDIT.structure.json), [checks](AUDIT.checks.json), [direct member ordering](AUDIT.members.json), and [evaluated projects](AUDIT.projects.json). The single remediation list is [TODO.md](TODO.md); completion is recorded in [TODO.progress.json](TODO.progress.json).','',
    'The comparison unit is each file and its relevant declarations/operations, or a configuration/project boundary. Each criterion partitions the same 205-file universe. Applicable = compliant + deviation + exception + unreviewed; N/A is separate. Criteria overlap: summing rows does not count unique files. Generated accessors/build inputs are configuration evidence, not style peers. Build output, vendored files, IDE metadata and this audit workspace are excluded from source comparison.','',
    '| Criterion | Applicable | Compliant | Deviation | Exception | Unreviewed | N/A | Tasks |',
    '| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |']
for cid, unit in sorted(s.items()):
    c = collections.Counter(r['disposition'] for r in unit['rows'])
    ids = sorted({t for r in unit['rows'] for t in r['tasks']})
    lines.append(f"| [{cid}](CONVENTIONS.md#{cid.lower()}) | {len(paths)-c['Not applicable']} | {c['Compliant']} | {c['Deviation']} | {c['Justified exception']} | {c['Unreviewed']} | {c['Not applicable']} | "+', '.join(f'[{t}](TODO.md#{t.lower()})' for t in ids)+' |')
lines += ['', '## Verification and limits','',
    'Current-source verification: all six library targets built; 739 tests passed on each of five test frameworks (3,695 executions). [Verification evidence](AUDIT.verification.json) preserves full console output because .log files are ignored by Git. The two changed documentation examples were reread and structural evidence refreshed before completion. No production/test source or conventions were edited by the audit.','',
    'The isolated ValueTask probe reproduces CONS-019 in the unchanged runtime. The native-handle race is supported by a concrete source interleaving and the selected lifecycle policy; this test run did not reproduce the intermittent stress failure.','',
    'Earlier read-only formatter verification exited 2. Its report predates the latest two example edits. IMPORTS identifies the two still-unsorted global lists; IDE0130 conflicts with selected namespace-folder exceptions. WHITESPACE includes unselected line-ending defaults and JetBrains alignment/unary-spacing differences. Other SDK suggestions are not automatically selected convention violations. Generated/vendor diagnostics are excluded. No clean formatter result is claimed. Initial generic-owner member-order candidates were corrected by direct syntax-span inspection in AUDIT.members.json.','',
    'ReSharper CLI inspection remains an unperformed implementation acceptance step under CONS-017. Suppression scope/rationale and analyzer configuration were reviewed from source; no successful ReSharper run is claimed. Default-symbol structural extraction is supplemented by branch review and all-target builds. No performance measurements were taken. This audit assesses the documented conventions, not exhaustive correctness.','',
    'All source hashes, file membership, convention revision/digest and documenting tracker were revalidated. Historical implementation evidence and owner decisions remain preserved; checked historical decisions do not imply that current defects are fixed.','']
(D/'COVERAGE.md').write_text('\n'.join(lines),encoding='utf-8')
for name in ['TODO.md','COVERAGE.md']:
    text = (D/name).read_text(encoding='utf-8')
    anchors = re.findall(r'<a id="([^"]+)"',text)
    assert len(anchors) == len(set(anchors))
    for target in re.findall(r'\]\(([^)]+)\)',text):
        if '://' in target: continue
        file,_,anchor = target.partition('#')
        dest = (D/file).resolve() if file else D/name
        assert dest.exists(), (name,target)
        if anchor: assert f'id="{anchor}"' in dest.read_text(encoding='utf-8-sig'), (name,target)
m['files']['historical_audit_evidence'] = ['AUDIT.reproduction.json','AUDIT.checkpoint.ps1','AUDIT.whitespace.log']
m['files']['audit_evidence'] = [f.name for f in sorted(D.glob('AUDIT.*')) if f.suffix not in ['.log','.tmp'] and f.name not in m['files']['historical_audit_evidence']] + ['format-report.json']
m.setdefault('previous_freshness',m['freshness'])
m['freshness'] = {'is_current':True,'detected_at':now,'reason':'Generation 2 audit complete against revision 52 and current source fingerprints; remediation is still open.','next_revalidation':'Recheck affected criteria after source or convention edits.'}
save('consistency.json',m)
p['inputs']['source_revision'] = '88d74702ce1685dd4544cfe68abbd776f6cc44a1'
p['inputs']['dirty_worktree'] = 'Existing package edits and added/modified DocumentationFeaturesExample.cs and DocumentationJobExample.cs included by inventory hashes; audit artifacts changed separately.'
for key,name in [('inventory','AUDIT.inventory.json'),('structure','AUDIT.structure.json')]: p['inputs'][key]['sha256'] = sha(D/name)
p['inputs']['revalidation'] = '2026-10-30: reread two changed examples, refreshed structural/suppression evidence and all criterion fingerprints, reran all test frameworks.'
p['status'] = 'complete'; p['checkpoint'] += 1; p['updated_at'] = now
p['validation'] = ['50 criteria and inventory complete; 10,250 criterion/file rows; none unreviewed.','All 205 source file hashes and membership unchanged at finalization.','All eligible/processed sets reconcile; every deviation maps to an unchecked task.','Five unique open tasks, unique anchors, Markdown links checked.','Conventions and documenting tracker fingerprints unchanged.','739 tests pass on each of five TFMs; six library TFMs built.','ReSharper verification remains a CONS-017 implementation acceptance limitation.']
p['next_step'] = 'Audit complete. Remediation is a separate action: prioritize CONS-019 and CONS-018, then CONS-020/CONS-017 and CONS-021; revalidate coverage after fixes.'
p['outputs'] = {n:{'path':n,'sha256':sha(D/n)} for n in sorted(set(m['files']['audit_evidence'])|{'TODO.md','COVERAGE.md','consistency.json'})}
save('TODO.progress.json',p)
print('Audit complete: 50 criteria, 205 source files, 10250 dispositions, five open tasks.')
