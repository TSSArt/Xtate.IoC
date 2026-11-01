$ErrorActionPreference='Stop'
$root=(Resolve-Path "$PSScriptRoot/../../..").Path
$inventory=Get-Content "$PSScriptRoot/AUDIT.inventory.json" -Raw|ConvertFrom-Json
$records=@();$candidates=@()
foreach($file in $inventory|Where-Object {$_.path -like '*.cs' -and !$_.generated}){
 $source=[IO.File]::ReadAllText((Join-Path $root $file.path))
 $tree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source,[Microsoft.CodeAnalysis.CSharp.CSharpParseOptions]::Default.WithLanguageVersion([Microsoft.CodeAnalysis.CSharp.LanguageVersion]::Preview))
 foreach($type in $tree.GetRoot().DescendantNodes()|Where-Object {$_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax]}){
  $members=@();$seenNested=$false
  foreach($m in $type.Members){
   $kind=$m.GetType().Name;$line=$tree.GetLineSpan($m.Span).StartLinePosition.Line+1
   $name=if($m.PSObject.Properties['Identifier']){$m.Identifier.ValueText}elseif($m -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax]){($m.Declaration.Variables.Identifier.ValueText -join ',')}else{$kind}
   $nested=$m -is [Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax]
   $special=$nested -and $m.Modifiers.ToString() -match 'public' -and $name -match 'Provider'
   if($seenNested -and !$nested -and $kind -ne 'EnumDeclarationSyntax'){$candidates+=@{path=$file.path;type=$type.Identifier.ValueText;type_line=$tree.GetLineSpan($type.Span).StartLinePosition.Line+1;line=$line;name=$name;reason='member after nested type'}}
   if($nested -and !$special){$seenNested=$true}
   $members+=@{name=$name;kind=$kind;line=$line;modifiers=$m.Modifiers.ToString()}
  }
  foreach($readonly in @($true,$false)){
   $fields=@($members|Where-Object {$_.kind -eq 'FieldDeclarationSyntax' -and $_.modifiers -notmatch '\b(static|const)\b' -and ($_.modifiers -match '\breadonly\b') -eq $readonly})
   $names=@($fields.name);$sorted=@($names|Sort-Object)
   if(($names -join '|') -ne ($sorted -join '|')){$candidates+=@{path=$file.path;type=$type.Identifier.ValueText;line=$tree.GetLineSpan($type.Span).StartLinePosition.Line+1;names=$names;reason='instance field name ordering within readonly/mutable cohort'}}
  }
  $records+=@{path=$file.path;type=$type.Identifier.ValueText;line=$tree.GetLineSpan($type.Span).StartLinePosition.Line+1;members=$members}
 }
}
@{method='Direct type members by syntax span; generic arities and nested declarations separated. Static fields are not alphabetized by the configured matcher. Candidates require semantic review.';types=$records;candidates=$candidates}|ConvertTo-Json -Depth 12|Set-Content "$PSScriptRoot/AUDIT.members.json" -Encoding utf8
$candidates|ConvertTo-Json -Depth 6
