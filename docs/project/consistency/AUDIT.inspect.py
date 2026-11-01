"""Read-only structural audit evidence. Run from the solution root."""
import json, re, hashlib, collections, xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).resolve().parent
def read(name): return json.loads((OUT/name).read_text(encoding='utf-8-sig'))
def source_text(path):
    data=path.read_bytes()
    try: return data.decode('utf-8-sig')
    except UnicodeDecodeError: return data.decode('cp1252')
inventory, structure = read('AUDIT.inventory.json'), read('AUDIT.structure.json')
checks = {k: [] for k in ['namespaces','types','fields','usings','awaits','suppressions','headers','test_methods','timing','links','resource_keys','resource_calls','api_families','parse_errors','nested_order_candidates','field_order_candidates','comments']}
for file in structure:
    path=file['path']; source=source_text(ROOT/path); lines=source.splitlines()
    checks['parse_errors'] += [dict(path=path,error=e) for e in file['parse_errors']]
    checks['headers'].append(dict(path=path,agpl='GNU Affero General Public License' in source[:1400]))
    for i,line in enumerate(lines,1):
        if re.match(r'\s*(global )?using ',line) and not '(' in line and line.rstrip().endswith(';'): checks['usings'].append(dict(path=path,line=i,text=line.strip()))
        if re.search(r'ReSharper (disable|restore)|#pragma warning|SuppressMessage',line): checks['suppressions'].append(dict(path=path,line=i,text=line.strip()))
        if re.search(r'Thread.Sleep|Task.Delay|SpinWait|CurrentCulture|CurrentUICulture|Random\(|DateTime.Now|DateTime.UtcNow|Environment\.',line): checks['timing'].append(dict(path=path,line=i,text=line.strip()))
        if re.search(r'TODO|FIXME|placeholder|transient|thread.safe|thread safe',line,re.I) and line.lstrip().startswith('//'): checks['comments'].append(dict(path=path,line=i,text=line.strip()))
    for m in file['declarations']:
        kind=m['kind']; item=dict(path=path,line=m['line'],name=m['name'],owner=m['owner'],signature=m['signature'])
        if 'Namespace' in kind: checks['namespaces'].append(item)
        if kind in ['ClassDeclarationSyntax','InterfaceDeclarationSyntax','StructDeclarationSyntax','RecordDeclarationSyntax','EnumDeclarationSyntax']: checks['types'].append(item)
        if kind=='FieldDeclarationSyntax': checks['fields'].append(item)
        if kind=='MethodDeclarationSyntax' and re.search(r'\[(?:Data)?TestMethod',m['signature']):
            body=m['body'] or ''
            checks['test_methods'].append(dict(**item,assertion=bool(re.search(r'Assert\.|Assert[A-Z]|\.Verify\(|Capture\(|Observe\(',body)),body=body))
    for a in file['awaits']: checks['awaits'].append(dict(path=path,**a,configured='ConfigureAwait(false)' in a['expression']))
    if re.search(r'(ServiceCollection|ServiceProvider)Arg\dExtensions',path):
        checks['api_families'].append(dict(path=path,methods=[dict(name=m['name'],line=m['line'],signature=m['signature'],body=m['body']) for m in file['declarations'] if m['kind']=='MethodDeclarationSyntax']))
    owners=collections.defaultdict(list)
    for m in file['declarations']:
        if m['owner']: owners[m['owner']].append(m)
    for owner,members in owners.items():
        nested=[m for m in members if m['kind'] in ['ClassDeclarationSyntax','StructDeclarationSyntax','RecordDeclarationSyntax','InterfaceDeclarationSyntax']]
        methods=[m for m in members if m['kind'] in ['MethodDeclarationSyntax','ConstructorDeclarationSyntax','PropertyDeclarationSyntax']]
        for n in nested:
            later=[m['name'] for m in methods if m['line']>n['line']]
            if later: checks['nested_order_candidates'].append(dict(path=path,owner=owner,nested=n['name'],line=n['line'],later=later))
        fields=[m for m in members if m['kind']=='FieldDeclarationSyntax']
        for static in [True,False]:
            cohort=[m for m in fields if bool(re.search(r'\b(static|const)\b',m['signature']))==static]
            names=[re.sub(r'\s*=.*','',m['name']).strip() for m in cohort]
            if names!=sorted(names,key=str.casefold) and len(names)>1: checks['field_order_candidates'].append(dict(path=path,owner=owner,static=static,names=names,lines=[m['line'] for m in cohort]))
for file in inventory:
    path=file['path']
    if path.endswith('.md'):
        source=source_text(ROOT/path)
        for label,target in re.findall(r'\[([^\]]*)\]\(([^)]+)\)',source):
            if re.match(r'https?://|mailto:|#',target): continue
            target=target.split('#')[0].strip('<>')
            checks['links'].append(dict(path=path,label=label,target=target,exists=(ROOT/path).parent.joinpath(target).exists()))
    if path.endswith('.resx'):
        data={n.attrib['name']:n.findtext('value') or '' for n in ET.parse(ROOT/path).getroot().findall('data')}
        checks['resource_keys'].append(dict(path=path,keys={k:sorted(set(re.findall(r'(?<!\{)\{(\d+)',v))) for k,v in data.items()}))
resources=source_text(ROOT/'src/Xtate.IoC/Helpers/ResourcesExtensions.cs')
checks['resource_calls']=resources.splitlines()[18:]
(OUT/'AUDIT.checks.json').write_text(json.dumps(checks,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
summary={k:len(v) for k,v in checks.items()}
summary['roles']=dict(collections.Counter(f['role'] for f in inventory))
summary['missing_headers']=[x for x in checks['headers'] if not x['agpl']]
summary['broken_links']=[x for x in checks['links'] if not x['exists']]
summary['library_await_candidates']=[x for x in checks['awaits'] if x['path'].startswith('src/') and not x['configured']]
summary['test_assertion_candidates']=[dict(path=x['path'],name=x['name'],line=x['line']) for x in checks['test_methods'] if not x['assertion']]
print(json.dumps(summary,indent=2))

