$ErrorActionPreference='Stop'
$dir=Join-Path (Get-Location) 'docs/project/consistency'
function Save-AuditJson($value,$path){$value|ConvertTo-Json -Depth 22|Set-Content -LiteralPath "$path.tmp" -Encoding utf8;Move-Item -LiteralPath "$path.tmp" -Destination $path -Force}
function Complete-Criterion([string]$id,[string]$method,[string[]]$applicable,[hashtable]$deviations,[hashtable]$exceptions){
 $p=Get-Content "$dir/TODO.progress.json" -Raw|ConvertFrom-Json
 $coverage=@(Get-Content "$dir/AUDIT.coverage.json" -Raw|ConvertFrom-Json)
 $u=$p.work_units|Where-Object id -eq $id;$u.state='in_progress';Save-AuditJson $p "$dir/TODO.progress.json"
 foreach($r in $coverage|Where-Object criterion -eq $id){
  $r.disposition=if($r.path -notin $applicable){'Not applicable'}elseif($deviations.ContainsKey($r.path)){'Deviation'}elseif($exceptions.ContainsKey($r.path)){'Justified exception'}else{'Compliant'}
  $r.evidence=@($method,'All relevant declarations for this path: AUDIT.structure.json; source fingerprint: AUDIT.inventory.json')
  $r.tasks=@()
  if($deviations.ContainsKey($r.path)){$r.tasks=@($deviations[$r.path])}
  if($exceptions.ContainsKey($r.path)){$r.evidence+=@($exceptions[$r.path])}
  if($r.disposition -eq 'Not applicable'){$r.evidence=@('No construct/boundary for the stated criterion in this file; applicability determined by full declaration inventory and role-specific checks, not file size.')}
 }
 Save-AuditJson $coverage "$dir/AUDIT.coverage.json"
 $u.state='complete';$u.processed=$u.eligible_set;$u.remaining=@();$u|Add-Member evidence @($method,'AUDIT.coverage.json criterion '+$id) -Force
 $p.checkpoint++;$p.updated_at=[DateTimeOffset]::Now.ToString('o');$p.outputs=@{todo=@{path='TODO.md';sha256=(Get-FileHash "$dir/TODO.md").Hash};coverage_data=@{path='AUDIT.coverage.json';sha256=(Get-FileHash "$dir/AUDIT.coverage.json").Hash}};$p.next_step='Complete remaining criterion/file dispositions and validate reconciliation';Save-AuditJson $p "$dir/TODO.progress.json"
}
$inventory=@(Get-Content "$dir/AUDIT.inventory.json" -Raw|ConvertFrom-Json)
$structure=@(Get-Content "$dir/AUDIT.structure.json" -Raw|ConvertFrom-Json)
$checks=Get-Content "$dir/AUDIT.checks.json" -Raw|ConvertFrom-Json
$library=@($structure|Where-Object role -eq library|Select-Object -ExpandProperty path)
$code=@($structure.path)
$tests=@($structure|Where-Object {$_.role -in @('test','example')}|Select-Object -ExpandProperty path)
$config=@($inventory|Where-Object {$_.path -match '(\.csproj$|\.props$|\.sln$|NuGet.Config$|\.editorconfig$|\.DotSettings$|\.github/workflows/)'}|Select-Object -ExpandProperty path)
