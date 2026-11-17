# Builds __APP_NAME__ and installs it to %LOCALAPPDATA%\Programs\__APP_NAME__, with a Start menu shortcut.
# Build products live in %LOCALAPPDATA%\__APP_NAME__\build, outside the repo (and outside OneDrive).
#
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1            release build, install
#        ... -File build.ps1 -Debug     debug build, install (faster)
#        ... -File build.ps1 -Open      install, then stop the running copy and start the new one
param([switch]$Debug, [switch]$Open)
$ErrorActionPreference = "Stop"
$name = "__APP_NAME__"
$here = $PSScriptRoot
$config = if ($Debug) { "Debug" } else { "Release" }
$install = Join-Path $env:LOCALAPPDATA "Programs\$name"
$exe = Join-Path $install "$name.exe"

# A running copy locks its files. Stop it only when asked to.
$running = Get-Process -Name $name -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
if ($running) {
    if (-not $Open) { Write-Output "close $name first, or run build.ps1 -Open"; exit 1 }
    $running | Stop-Process -Force
    $running | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue
}

# Native tools write to stderr; keep going and judge them by their exit code.
$ErrorActionPreference = "Continue"

# The icon: Icon\AppIcon.svg -> Icon\AppIcon.ico, rebuilt when the SVG changed.
$svg = Join-Path $here "Icon\AppIcon.svg"
$ico = Join-Path $here "Icon\AppIcon.ico"
if ((Test-Path -LiteralPath $svg) -and (-not (Test-Path -LiteralPath $ico) -or (Get-Item -LiteralPath $svg).LastWriteTime -gt (Get-Item -LiteralPath $ico).LastWriteTime)) {
    dotnet run (Join-Path $here "tools\make-icon.cs") -- $svg $ico
    if ($LASTEXITCODE -ne 0) { Write-Output "build failed: icon"; exit 1 }
}

$log = [IO.Path]::GetTempFileName()
dotnet publish (Join-Path $here "src\$name\$name.csproj") -c $config -r win-x64 --self-contained -o $install -nologo > $log 2>&1
$code = $LASTEXITCODE
Get-Content -LiteralPath $log | Where-Object { $_ -match ': (error|warning) [A-Z]+[0-9]+' } | Select-Object -Unique
Remove-Item -LiteralPath $log
if ($code -ne 0 -or -not (Test-Path -LiteralPath $exe)) { Write-Output "build failed"; exit 1 }

# Start menu shortcut, so people find the app like any other.
$shortcut = Join-Path ([Environment]::GetFolderPath("Programs")) "$name.lnk"
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut)
$link.TargetPath = $exe
$link.WorkingDirectory = $install
$link.IconLocation = "$exe,0"
$link.Save()
Write-Output "installed $exe ($config)"

if ($Open) { Start-Process -FilePath $exe -WorkingDirectory $install }
exit 0
