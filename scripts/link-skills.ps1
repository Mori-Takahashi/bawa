# Links every skill listed in .claude-plugin\plugin.json into %USERPROFILE%\.claude\skills and
# %USERPROFILE%\.agents\skills, so edits in the repo show up in Claude Code and Codex at once.
# Uses directory junctions, which need no administrator rights.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File scripts\link-skills.ps1
$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot
$plugin = Get-Content -Raw -LiteralPath (Join-Path $repo ".claude-plugin\plugin.json") | ConvertFrom-Json
$skills = $plugin.skills | ForEach-Object { Get-Item -LiteralPath (Join-Path $repo $_.TrimStart(".", "/")) }
foreach ($target in @((Join-Path $env:USERPROFILE ".claude\skills"), (Join-Path $env:USERPROFILE ".agents\skills"))) {
    New-Item -ItemType Directory -Force $target | Out-Null
    foreach ($skill in $skills) {
        $link = Join-Path $target $skill.Name
        if (Test-Path -LiteralPath $link) {
            $item = Get-Item -LiteralPath $link -Force
            if (-not ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
                Write-Output "skipped $link (a real folder, not a link)"
                continue
            }
            # Remove only the junction, never the folder it points to.
            [IO.Directory]::Delete($link)
        }
        New-Item -ItemType Junction -Path $link -Target $skill.FullName | Out-Null
        Write-Output $link
    }
}
