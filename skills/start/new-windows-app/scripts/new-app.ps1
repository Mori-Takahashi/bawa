# Creates a new Windows app from the template next to this script.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File new-app.ps1 <AppName> [parent-folder]
#   new-app.ps1 Tasks "$env:USERPROFILE\Documents"
# Creates <parent-folder>\<AppName>. If that folder exists, it must not have a
# <AppName>.slnx yet; files already there (like docs\BRIEF.md) are kept.
# AppName must be one word in UpperCamelCase (letters and digits only).
param(
    [Parameter(Mandatory = $true)][string]$Name,
    [string]$Parent = (Get-Location).Path
)
$ErrorActionPreference = "Stop"
if ($Name -cnotmatch '^[A-Z][A-Za-z0-9]*$') {
    Write-Output "AppName must be UpperCamelCase letters and digits, like Tasks or DailyLog"
    exit 1
}
$template = Join-Path (Split-Path -Parent $PSScriptRoot) "template"
$dest = Join-Path ([IO.Path]::GetFullPath($Parent)) $Name
if (Test-Path -LiteralPath (Join-Path $dest "$Name.slnx")) {
    Write-Output "$dest already has a $Name.slnx"
    exit 1
}
# build.ps1 installs to %LOCALAPPDATA%\Programs\<AppName>. Never take over another program's folder:
# a BAWA app leaves a .bawa-app marker there; anything else with that name is someone else's.
$installed = Join-Path $env:LOCALAPPDATA "Programs\$Name"
if ((Test-Path -LiteralPath $installed) -and -not (Test-Path -LiteralPath (Join-Path $installed ".bawa-app"))) {
    Write-Output "another program is installed at ${installed}: pick another name"
    exit 1
}
New-Item -ItemType Directory -Force $dest | Out-Null

$binary = @(".ico", ".png", ".jpg", ".gif")
$utf8 = New-Object System.Text.UTF8Encoding($false)
foreach ($file in Get-ChildItem -LiteralPath $template -Recurse -Force -File) {
    $relative = $file.FullName.Substring($template.Length + 1).Replace("__APP_NAME__", $Name)
    $target = Join-Path $dest $relative
    $isGitIgnore = $relative -eq ".gitignore"
    # Keep files that are already there. A kept .gitignore gets the template lines it lacks.
    if ((Test-Path -LiteralPath $target) -and -not $isGitIgnore) { continue }
    New-Item -ItemType Directory -Force (Split-Path -Parent $target) | Out-Null
    if ($binary -contains $file.Extension.ToLowerInvariant()) {
        Copy-Item -LiteralPath $file.FullName -Destination $target
        continue
    }
    $text = [IO.File]::ReadAllText($file.FullName).Replace("__APP_NAME__", $Name)
    if ($isGitIgnore -and (Test-Path -LiteralPath $target)) {
        $have = [IO.File]::ReadAllLines($target)
        $missing = $text -split "`r?`n" | Where-Object { $_ -and ($have -notcontains $_) }
        if ($missing) { [IO.File]::AppendAllText($target, "`r`n" + ($missing -join "`r`n") + "`r`n", $utf8) }
        continue
    }
    [IO.File]::WriteAllText($target, $text, $utf8)
}

# The copied SVG is newer than the copied icon; mark the icon current so the first build skips rendering it.
$icon = Join-Path $dest "Icon\AppIcon.ico"
if (Test-Path -LiteralPath $icon) { (Get-Item -LiteralPath $icon).LastWriteTime = Get-Date }

$left = Get-ChildItem -LiteralPath $dest -Recurse -Force -File |
    Where-Object { $_.FullName -notmatch '\\\.git\\' -and $_.Name -match '__APP_NAME__' }
$left += Get-ChildItem -LiteralPath $dest -Recurse -Force -File -Include *.cs, *.xaml, *.csproj, *.slnx, *.ps1, *.md, *.props, *.json, *.manifest |
    Where-Object { $_.FullName -notmatch '\\\.git\\' -and (Select-String -LiteralPath $_.FullName -Pattern '__APP_NAME__' -SimpleMatch -Quiet) }
if ($left) {
    Write-Output "could not replace the placeholder in: $($left[0].FullName)"
    exit 1
}

$ErrorActionPreference = "Continue"
if (-not (Test-Path -LiteralPath (Join-Path $dest ".git"))) { git -C $dest init -q }
git -C $dest add -A
git -C $dest commit -q -m "Create $Name from the Windows app template"
if ($LASTEXITCODE -ne 0) { Write-Output "git commit failed: set git user.name and user.email, then commit"; exit 1 }
Write-Output "created $dest"
Write-Output "next: cd `"$dest`"; powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1 -Open"
exit 0
