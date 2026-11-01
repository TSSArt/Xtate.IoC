$ErrorActionPreference = 'Stop'
$root = (Resolve-Path "$PSScriptRoot/../../..").Path
Set-Location -LiteralPath $root
function SaveJson($value, $name) {
    $path = Join-Path $PSScriptRoot $name
    $value | ConvertTo-Json -Depth 40 | Set-Content -LiteralPath "$path.tmp" -Encoding utf8
    Move-Item -LiteralPath "$path.tmp" -Destination $path -Force
}
$paths = @(rg --files --hidden -g '!bin' -g '!obj' -g '!.git' -g '!.vs' -g '!TestResults' -g '!BenchmarkDotNet.Artifacts' -g '!docs/project/consistency/**' -g '!*.DotSettings.user' | ForEach-Object { $_.Replace('\','/') } | Sort-Object)
$inventory = @($paths | ForEach-Object {
    $role = if ($_ -match '^src/.*\.cs$') {'library'} elseif ($_ -match '/Examples/.*\.cs$') {'example'} elseif ($_ -match '^test/Xtate.IoC.Test/.*\.cs$') {'test'} elseif ($_ -match '^test/Xtate.IoC.Benchmark/.*\.cs$') {'benchmark'} elseif ($_ -match '\.md$') {'documentation'} else {'configuration'}
    [ordered]@{path=$_; sha256=(Get-FileHash -LiteralPath $_ -Algorithm SHA256).Hash; role=$role; generated=($_ -match '(Resources.Designer.cs|^Directory.Build.props|^Global.Packages.props)$'); bytes=(Get-Item -LiteralPath $_).Length}
})
SaveJson $inventory 'AUDIT.inventory.json'
$structure = @()
foreach ($file in $inventory | Where-Object {$_.path -like '*.cs' -and !$_.generated}) {
    $source = [IO.File]::ReadAllText((Join-Path $root $file.path))
    $options = [Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion([Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Preview)
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source, $options)
    $syntax = $tree.GetRoot()
    $members = @($syntax.DescendantNodes() | Where-Object {$_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax]} | ForEach-Object {
        $node=$_; $kind=$node.GetType().Name
        $name = if ($node.PSObject.Properties['Identifier']) {$node.Identifier.ValueText} elseif ($node.PSObject.Properties['Declaration']) {$node.Declaration.Variables.ToString()} elseif ($node.PSObject.Properties['Name']) {$node.Name.ToString()} else {$kind}
        $body = if ($node.PSObject.Properties['Body'] -and $node.Body) {$node.Body.ToString()} elseif ($node.PSObject.Properties['ExpressionBody'] -and $node.ExpressionBody) {$node.ExpressionBody.ToString()} else {$null}
        $signature = $node.ToString()
        if ($body) {$signature=$signature.Substring(0,$signature.IndexOf($body)).Trim()}
        elseif ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax] -or $node -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseNamespaceDeclarationSyntax]) {$signature=($signature -split "`n")[0].Trim()}
        $parent=$node.Parent; $owners=@()
        while ($parent) {if ($parent -is [Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax]) {$owners=@($parent.Identifier.ValueText)+$owners}; $parent=$parent.Parent}
        [ordered]@{kind=$kind;name=$name;owner=($owners -join '.');line=($tree.GetLineSpan($node.Span).StartLinePosition.Line+1);signature=$signature;body=$body;documentation=$node.GetLeadingTrivia().ToFullString().Trim()}
    })
    $awaits=@($syntax.DescendantNodes() | Where-Object {$_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AwaitExpressionSyntax]} | ForEach-Object {@{line=($tree.GetLineSpan($_.Span).StartLinePosition.Line+1);expression=$_.Expression.ToString()}})
    $structure += [ordered]@{path=$file.path;role=$file.role;lines=($source -split "`n").Count;declarations=$members;awaits=$awaits;parse_errors=@($tree.GetDiagnostics()|Where-Object Severity -eq Error|ForEach-Object {$_.ToString()});directives=@($source -split "`n"|Where-Object {$_ -match '^\s*#(if|elif|else|endif)'})}
}
SaveJson $structure 'AUDIT.structure.json'
$p = Get-Content "$PSScriptRoot/TODO.progress.json" -Raw | ConvertFrom-Json
$p.inputs | Add-Member inventory @{path='AUDIT.inventory.json';sha256=(Get-FileHash "$PSScriptRoot/AUDIT.inventory.json").Hash} -Force
$p.inputs | Add-Member structure @{path='AUDIT.structure.json';sha256=(Get-FileHash "$PSScriptRoot/AUDIT.structure.json").Hash;tool=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree].Assembly.FullName;configuration='Preview, default preprocessor symbols; directives retained for branch inspection, builds check configured targets'} -Force
$p.work_units[0].state='complete';$p.work_units[0].eligible_set=@($inventory.path);$p.work_units[0].processed=@($inventory.path);$p.work_units[0].remaining=@()
$p.work_units = @($p.work_units[0]) + @(1..50 | ForEach-Object {@{id=('C{0:d2}' -f $_);state='pending';eligible_set=@($inventory.path);processed=@();remaining=@($inventory.path);evidence=@();comparison_unit='File and all relevant declarations/operations, or project/configuration boundary; per-file applicability recorded separately'}})
$p.checkpoint++;$p.updated_at=[DateTimeOffset]::Now.ToString('o');$p.next_step='Inspect all current criterion/file cohorts; generate explicit applicability and dispositions. No historical dispositions carried forward.'
SaveJson $p 'TODO.progress.json'
$inventory | Group-Object role | Select-Object Name,Count
@{files=$inventory.Count;code_files=$structure.Count;declarations=@($structure.declarations).Count;parse_errors=@($structure.parse_errors).Count;code_lines=($structure.lines|Measure-Object -Sum).Sum}|ConvertTo-Json

