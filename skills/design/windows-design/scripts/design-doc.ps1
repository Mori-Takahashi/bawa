# Prints Microsoft's current Windows design guidance on one topic as plain text.
# Usage: powershell -NoProfile -ExecutionPolicy Bypass -File design-doc.ps1 <topic>
#   topics: principles, guidelines, accessibility, layout, navigation, titlebar, color,
#           materials, typography, icons, app-icons, motion, commanding, usability
#   or a path under learn.microsoft.com/windows/apps/design/, like controls/buttons
#   or controls/dialogs-and-flyouts/dialogs
param([Parameter(Mandatory = $true)][string]$Topic)
$ErrorActionPreference = "Stop"
$base = "https://learn.microsoft.com/en-us/windows/apps/design/"
$topics = @{
    "principles"    = "design-principles"
    "guidelines"    = "guidelines-overview"
    "accessibility" = "accessibility/accessibility-overview"
    "layout"        = "layout/"
    "navigation"    = "basics/navigation-basics"
    "titlebar"      = "basics/titlebar-design"
    "color"         = "signature-experiences/color"
    "materials"     = "signature-experiences/materials"
    "typography"    = "signature-experiences/typography"
    "icons"         = "iconography/"
    "app-icons"     = "iconography/app-icons"
    "motion"        = "signature-experiences/motion"
    "commanding"    = "basics/commanding-basics"
    "usability"     = "usability/"
}
$path = if ($topics.ContainsKey($Topic)) { $topics[$Topic] } else { $Topic.TrimStart("/") }
$url = $base + $path
try { $page = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30 }
catch { Write-Output "could not load ${url}: $($_.Exception.Message)"; exit 1 }

$html = $page.Content
$main = [regex]::Match($html, '<main[\s\S]*?</main>')
if ($main.Success) { $html = $main.Value }
$html = [regex]::Replace($html, '<(script|style)[^>]*>[\s\S]*?</\1>', '')
$html = [regex]::Replace($html, '<(h[1-4])[^>]*>', "`n## ")
$html = [regex]::Replace($html, '<li[^>]*>', "`n- ")
$html = [regex]::Replace($html, '<(p|tr|br|div)[^>]*>', "`n")
$text = [System.Net.WebUtility]::HtmlDecode([regex]::Replace($html, '<[^>]+>', ''))
$lines = @($text -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -and $_ -ne "-" -and $_ -ne "##" })
# Skip the page chrome (reading mode, sign-in notes) before the first heading.
$start = 0
for ($i = 0; $i -lt $lines.Count; $i++) { if ($lines[$i].StartsWith("## ")) { $start = $i; break } }
Write-Output "Source: $url"
$lines[$start..($lines.Count - 1)] | Where-Object { $_ -notmatch '^(Feedback|Summarize this article for me|In this article)$' }
