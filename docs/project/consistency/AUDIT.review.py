"""Persist explicit review decisions, with a checkpoint for each criterion."""
import json, hashlib, datetime, fnmatch
from pathlib import Path
D=Path(__file__).resolve().parent
ROOT=D.parents[2]
def load(name): return json.loads((D/name).read_text(encoding='utf-8-sig'))
def save(name,obj):
    tmp=D/(name+'.tmp'); tmp.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8'); tmp.replace(D/name)
def sha(name): return hashlib.sha256((D/name).read_bytes()).hexdigest().upper()
def checkpoint(cid, patterns, method, deviations=None, exceptions=None, unreviewed=None):
    deviations=deviations or {}; exceptions=exceptions or {}; unreviewed=unreviewed or {}
    p=load('TODO.progress.json'); inv=load('AUDIT.inventory.json')
    u=next(u for u in p['work_units'] if u['id']==cid)
    u['state']='in_progress'; u['evidence']=[method]; save('TODO.progress.json',p)
    prior=D/'AUDIT.summary.json'
    summary=load('AUDIT.summary.json') if prior.exists() else {}
    if not isinstance(summary,dict) or summary.get('generation')!=2: summary={'generation':2,'criteria':{}}
    rows=[]
    for f in inv:
        path=f['path']; applies=any(fnmatch.fnmatchcase(path,x) for x in patterns)
        disposition='Compliant' if applies else 'Not applicable'
        evidence=method if applies else 'No relevant construct/role in this file for the explicit criterion scope; see patterns in AUDIT.summary.json.'
        tasks=[]
        if path in exceptions: disposition='Justified exception'; evidence=exceptions[path]
        if path in deviations: disposition='Deviation'; tasks=deviations[path]
        if path in unreviewed: disposition='Unreviewed'; evidence=unreviewed[path]
        rows.append(dict(criterion=cid,path=path,disposition=disposition,evidence=evidence,tasks=tasks,source_sha256=f['sha256']))
    summary['criteria'][cid]={'method':method,'patterns':patterns,'rows':rows}
    save('AUDIT.summary.json',summary)
    coverage=[r for v in summary['criteria'].values() for r in v['rows']]
    save('AUDIT.coverage.json',coverage)
    u['processed']=[r['path'] for r in rows if r['disposition']!='Unreviewed']
    u['remaining']=[r['path'] for r in rows if r['disposition']=='Unreviewed']
    u['state']='blocked' if u['remaining'] else 'complete'
    u['evidence']=[method,'AUDIT.coverage.json: '+cid,'AUDIT.structure.json: all declarations in each source path; AUDIT.inventory.json: fingerprints']
    p['checkpoint']+=1;p['updated_at']=datetime.datetime.now(datetime.timezone.utc).isoformat()
    p['outputs']={n:{'path':n,'sha256':sha(n)} for n in ['TODO.md','AUDIT.inventory.json','AUDIT.structure.json','AUDIT.checks.json','AUDIT.projects.json','AUDIT.summary.json','AUDIT.coverage.json']}
    p['next_step']='Review remaining units: '+', '.join(x['id'] for x in p['work_units'] if x['state'] not in ['complete','not_applicable'])
    save('TODO.progress.json',p)
    print(cid,u['state'])

if __name__=='__main__':
    config=['*.csproj','*.props','*.sln','NuGet.Config','.github/workflows/*']
    checkpoint('C18',config,'Read all three project files and enclosing imports; AUDIT.projects.json evaluates references: test/benchmark depend on the runtime, runtime has no reverse project dependency. Six-target library built; no project cycle or runtime dependency on test packages.')
    checkpoint('C42',config,'Evaluated target/packability/build roles and imports in AUDIT.projects.json. AUDIT.test-all.log: six library TFMs compile, 739 tests pass on each of net11/net10/net9/net8/net462. Benchmark remains a separate net10 executable; release jobs preserve build-test-pack and Windows dependency ordering.')
    checkpoint('C44',config,'Read all PackageReference/PackageVersion declarations and evaluated project items in AUDIT.projects.json. Central version management enabled, no inline PackageReference versions, analyzer PrivateAssets and framework-specific compatibility references retained. Generated package props remain input evidence.')
    checks=load('AUDIT.checks.json'); resources=checks['resource_keys']; assert len(resources)==6 and all(r['keys']==resources[0]['keys'] for r in resources)
    checkpoint('C48',['src/Xtate.IoC/Properties/Resources*','src/Xtate.IoC/Helpers/ResourcesExtensions.cs','src/Xtate.IoC/IoC/ArgumentType.cs','src/Xtate.IoC/Exceptions/MissedServiceException.cs','test/Xtate.IoC.Test/Tests/*Resources*','test/Xtate.IoC.Test/Tests/MissedServiceExceptionTest.cs'],'Compared all 28 keys and placeholder indexes across all six resx locales; reviewed every ResourcesExtensions formatter and missing-service call site. Argument-aware formatter uses the matching two-placeholder resource. Localization and missing-service tests pass on all five test TFMs. Generated accessor is representation evidence, not a style peer.')

