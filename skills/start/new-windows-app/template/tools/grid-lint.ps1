# Flags spacing numbers off the 4 px grid in XAML and C#, and views placed with Canvas or a TranslateTransform.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File tools\grid-lint.ps1 [-Path src]
# Allowed numbers: 0, 1, 2, and multiples of 4. Better: a token (SpaceLg, PageMargin, Space.Lg).
# The token files (Theme.xaml, Grid.cs, and a design kit's Tokens.xaml) are skipped.
# Silence one line with a reason: <!-- grid: ok <why> --> or // grid: ok <why>
param([string]$Path = (Join-Path (Split-Path -Parent $PSScriptRoot) "src"))
$ErrorActionPreference = "Stop"

$skipNames = @("Theme.xaml", "Grid.cs", "Tokens.xaml")
$xamlAttr = '\b(Margin|Padding|Spacing|RowSpacing|ColumnSpacing|Width|Height|MinWidth|MaxWidth|MinHeight|MaxHeight|CornerRadius)="([^"{]*)"'
$csAssign = '\b(Spacing|RowSpacing|ColumnSpacing|Width|Height|MinWidth|MaxWidth|MinHeight|MaxHeight)\s*=\s*(-?\d+(?:\.\d+)?)\b'
$csNew = '\bnew\s+(?:Microsoft\.UI\.Xaml\.)?(Thickness|CornerRadius)\(([^)]*)\)'

function Test-OffGrid([string]$value) {
    $n = 0.0
    if (-not [double]::TryParse($value.Trim(), [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$n)) { return $false }
    $a = [Math]::Abs($n)
    return -not ($a -eq 0 -or $a -eq 1 -or $a -eq 2 -or ($a -eq [Math]::Floor($a) -and $a % 4 -eq 0))
}

$files = Get-ChildItem -LiteralPath $Path -Recurse -File -Include *.xaml, *.cs |
    Where-Object { $skipNames -notcontains $_.Name -and $_.FullName -notmatch '\\(obj|bin)\\' }

$count = 0
foreach ($file in $files) {
    $lines = [IO.File]::ReadAllLines($file.FullName)
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        if ($line -match 'grid: ok') { continue }
        $bad = New-Object System.Collections.Generic.List[string]
        if ($file.Extension -eq ".xaml") {
            foreach ($m in [regex]::Matches($line, $xamlAttr)) {
                foreach ($part in ($m.Groups[2].Value -split '[,\s]+')) {
                    if ($part -and $part -notmatch '\*' -and (Test-OffGrid $part)) { $bad.Add($part) }
                }
            }
            if ($line -match '\bCanvas\.(Left|Top)=') { $bad.Add("Canvas") }
            if ($line -match '<TranslateTransform\b') { $bad.Add("TranslateTransform") }
        } else {
            foreach ($m in [regex]::Matches($line, $csAssign)) {
                if (Test-OffGrid $m.Groups[2].Value) { $bad.Add($m.Groups[2].Value) }
            }
            foreach ($m in [regex]::Matches($line, $csNew)) {
                foreach ($part in ($m.Groups[2].Value -split ',')) {
                    if (Test-OffGrid $part) { $bad.Add($part.Trim()) }
                }
            }
        }
        if ($bad.Count -gt 0) {
            $count++
            Write-Output ("{0}:{1}: off grid ({2}): {3}" -f $file.FullName, ($i + 1), ($bad -join ", "), $line.Trim())
        }
    }
}

if ($count -gt 0) { Write-Output "FAIL grid-lint: $count lines off the grid"; exit 1 }
Write-Output "PASS grid-lint"
exit 0
