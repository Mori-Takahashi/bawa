# Snapshots of every page and Settings section in light and dark, with seed data, on a fresh test store.
# Writes %TEMP%\__APP_NAME__-shots\<page>-light.png, -dark.png, and a -layout.json for each.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File tools\snap.ps1
#        ... -File tools\snap.ps1 -Pages all,settings       only these pages
#        ... -File tools\snap.ps1 -Accent green             another accent color
#        ... -File tools\snap.ps1 -Exe <path to a dev build>
param(
    [string]$Exe = (Join-Path $env:LOCALAPPDATA "Programs\__APP_NAME__\__APP_NAME__.exe"),
    [string]$Pages = "",
    [string]$Accent = ""
)
$ErrorActionPreference = "Stop"
if (-not (Test-Path -LiteralPath $Exe)) { Write-Output "no app at ${Exe}: run build.ps1 first"; exit 1 }

$store = Join-Path $env:TEMP "__APP_NAME__-test.db"
$shots = Join-Path $env:TEMP "__APP_NAME__-shots"
# Seed data only fills an empty store, so start from nothing.
Get-ChildItem -Path $env:TEMP -Filter "__APP_NAME__-test.*" -ErrorAction SilentlyContinue | Remove-Item -Force
if (Test-Path -LiteralPath $shots) { Remove-Item -LiteralPath $shots -Recurse -Force }

$env:APP_STORE = $store
$env:APP_SEED = "1"
$env:APP_SNAPSHOT = $shots
if ($Pages) { $env:APP_SNAPSHOT_PAGES = $Pages } else { Remove-Item Env:APP_SNAPSHOT_PAGES -ErrorAction SilentlyContinue }
if ($Accent) { $env:APP_ACCENT = $Accent } else { Remove-Item Env:APP_ACCENT -ErrorAction SilentlyContinue }

$p = Start-Process -FilePath $Exe -Wait -PassThru
$log = "$store.log"
if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log } else { Write-Output "SNAPSHOT no output (crashed at start?)" }
exit $p.ExitCode
