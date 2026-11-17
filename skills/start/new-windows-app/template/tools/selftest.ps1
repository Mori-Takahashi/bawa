# Runs the in-app self-test against a fresh store in %TEMP%. Exit code 0 when every check passed.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File tools\selftest.ps1
#        ... -File tools\selftest.ps1 -Exe <path to a dev build>
param([string]$Exe = (Join-Path $env:LOCALAPPDATA "Programs\__APP_NAME__\__APP_NAME__.exe"))
$ErrorActionPreference = "Stop"
if (-not (Test-Path -LiteralPath $Exe)) { Write-Output "no app at ${Exe}: run build.ps1 first"; exit 1 }

$store = Join-Path $env:TEMP "__APP_NAME__-self.db"
Get-ChildItem -Path $env:TEMP -Filter "__APP_NAME__-self.*" -ErrorAction SilentlyContinue | Remove-Item -Force
$env:APP_STORE = $store
Remove-Item Env:APP_SNAPSHOT, Env:APP_SEED -ErrorAction SilentlyContinue

$p = Start-Process -FilePath $Exe -ArgumentList "--selftest" -Wait -PassThru
$log = "$store.log"
if (Test-Path -LiteralPath $log) { Get-Content -LiteralPath $log } else { Write-Output "SELFTEST no output (crashed before the first check?)" }
exit $p.ExitCode
