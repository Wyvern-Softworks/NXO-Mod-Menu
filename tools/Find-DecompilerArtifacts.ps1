param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [switch]$Csv
)

$artifactPattern = 'goto IL_|^\s*IL_[0-9A-Fa-f]{4}:|end_IL_|while \(true\)|_003C[^\s\(\)\[\];,]*|CS_0024_003C_003E|IteratorStateMachine|AsyncStateMachine'
$methodPattern = '^\s*(?:\[[^\]]+\]\s*)*(?:(?:public|private|protected|internal|static|sealed|override|virtual|async|extern|unsafe|new|partial)\s+)+(?:[\w<>,\.\[\]\?]+\s+)+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\([^;]*\)'
$constructorPattern = '^\s*(?:\[[^\]]+\]\s*)*(?:(?:public|private|protected|internal|static|extern|unsafe)\s+)*(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\([^;]*\)'
$classPattern = '^\s*(?:(?:public|private|protected|internal|static|sealed|abstract|partial)\s+)*(?:class|struct|record)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)'

function Get-EnclosingMethod {
    param([string[]]$Lines, [int]$Index)

    $className = '<global>'
    for ($i = $Index; $i -ge 0; $i--) {
        $line = $Lines[$i]
        if ($line -match $methodPattern) {
            return "$className.$($Matches.name)"
        }
        if ($line -match $constructorPattern -and $line -notmatch '\b(if|for|foreach|while|switch|catch|using|lock)\s*\(') {
            return "$className.$($Matches.name)"
        }
        if ($line -match $classPattern) {
            $className = $Matches.name
        }
    }
    return '<unknown>'
}

$rows = New-Object System.Collections.Generic.List[object]
Get-ChildItem -LiteralPath $Root -Recurse -Filter '*.cs' |
    Where-Object { $_.FullName -notmatch '\\(bin|obj|\.vs|References\.Publicized)' } |
    ForEach-Object {
        $path = $_.FullName
        $lines = [IO.File]::ReadAllLines($path)
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match $artifactPattern) {
                $artifact = switch -Regex ($lines[$i]) {
                    'goto IL_' { 'goto' ; break }
                    '^\s*IL_[0-9A-Fa-f]{4}:' { 'label' ; break }
                    'end_IL_' { 'end-label' ; break }
                    'while \(true\)' { 'while-true' ; break }
                    '_003C.*_003Ed__' { 'iterator-class' ; break }
                    '_003C_003Ec__DisplayClass' { 'display-class' ; break }
                    '_003C[^\s\(\)\[\];,]*' { 'compiler-name' ; break }
                    'CS_0024_003C_003E' { 'compiler-local' ; break }
                    'IteratorStateMachine' { 'iterator-attribute' ; break }
                    'AsyncStateMachine' { 'async-attribute' ; break }
                    default { 'artifact' }
                }
                $rows.Add([pscustomobject]@{
                    File = $path.Substring($Root.Length).TrimStart('\')
                    Line = $i + 1
                    Method = Get-EnclosingMethod -Lines $lines -Index $i
                    Artifact = $artifact
                    Text = $lines[$i].Trim()
                })
            }
        }
    }

if ($Csv) {
    $rows | ConvertTo-Csv -NoTypeInformation
} else {
    $rows | Group-Object File, Method | Sort-Object Count -Descending | ForEach-Object {
        [pscustomobject]@{
            Count = $_.Count
            File = ($_.Group[0].File)
            Method = ($_.Group[0].Method)
        }
    } | Format-Table -AutoSize
}
