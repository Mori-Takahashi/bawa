# End-to-end check of the template: creates an app in a folder with a space in its
# name, then runs every check the skills promise. Prints TEMPLATE PASS or the first failing step.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File scripts\check-template.ps1 [-Keep]
# -Keep leaves the app, its build output, and its install in place for a look.
param([switch]$Keep)
$ErrorActionPreference = "Stop"
$name = "CheckApp"
$parent = Join-Path $env:TEMP "bawa check"
$dest = Join-Path $parent $name
$install = Join-Path $env:LOCALAPPDATA "Programs\$name"
$data = Join-Path $env:LOCALAPPDATA $name
$build = Join-Path $env:LOCALAPPDATA "$name-build"
$shortcut = Join-Path ([Environment]::GetFolderPath("Programs")) "$name.lnk"

function Remove-CheckApp {
    Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force
    foreach ($path in @($parent, $install, $data, $build, $shortcut)) {
        if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Recurse -Force }
    }
}

function Step([string]$title, [scriptblock]$run) {
    Write-Output "== $title"
    # Native tools write warnings to stderr; in PowerShell 5.1 "Stop" would turn those into errors.
    $ErrorActionPreference = "Continue"
    $global:LASTEXITCODE = 0
    $output = & $run 2>&1 | ForEach-Object { "$_" }
    $code = $LASTEXITCODE
    $output | Select-Object -Last 12 | ForEach-Object { Write-Output "   $_" }
    if ($code -ne 0) {
        Write-Output "TEMPLATE FAIL at: $title (exit $code)"
        exit 1
    }
}

Remove-CheckApp
New-Item -ItemType Directory -Force $parent | Out-Null
$ps = @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File")

Step "new-app.ps1" { powershell @ps (Join-Path $PSScriptRoot "new-app.ps1") $name $parent }
Set-Location -LiteralPath $dest
Step "dotnet test" { dotnet test }
Step "build.ps1" { powershell @ps (Join-Path $dest "build.ps1") }
Step "selftest.ps1" { powershell @ps (Join-Path $dest "tools\selftest.ps1") }
Step "snap.ps1" { powershell @ps (Join-Path $dest "tools\snap.ps1") }
Step "grid-lint.ps1" { powershell @ps (Join-Path $dest "tools\grid-lint.ps1") }
Step "check-layout.cs" { dotnet run (Join-Path $dest "tools\check-layout.cs") -- (Join-Path $env:TEMP "$name-shots\*-layout.json") }

Set-Location -LiteralPath $env:TEMP
if (-not $Keep) { Remove-CheckApp }
Write-Output "TEMPLATE PASS"
exit 0
