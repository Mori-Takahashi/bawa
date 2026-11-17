<div align="center">

# BAWA: Build Any Windows App

### See an app you like. Build it as your own native Windows app.

Agent skills to research, clone, design, build, optimize, and test native Windows apps with C# and WinUI 3.<br>
Local data. No Visual Studio needed.

Based on [BAMA: Build Any Mac App](https://github.com/daveebbelaar/bama) by Dave Ebbelaar.

[![Windows 10 and 11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=flat-square&logo=windows&logoColor=white)](#requirements)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)](#requirements)
[![WinUI 3](https://img.shields.io/badge/WinUI-3-0078D4?style=flat-square)](#requirements)
[![11 skills](https://img.shields.io/badge/skills-11-5E5CE6?style=flat-square)](#skills)
[![Claude Code plugin](https://img.shields.io/badge/Claude_Code-plugin-D97757?style=flat-square&logo=claude&logoColor=white)](#install)
[![Codex plugin](https://img.shields.io/badge/Codex-plugin-10A37F?style=flat-square)](#install)
[![MIT license](https://img.shields.io/badge/license-MIT-F4C430?style=flat-square)](LICENSE)

**[Quick start](#quick-start) · [How it works](#how-it-works) · [Skills](#skills) · [Specs](#specs) · [Install](#install)**

**On a Mac? Use [BAMA: Build Any Mac App](https://github.com/daveebbelaar/bama) instead.**

</div>

## Quick start

```bash
npx skills@latest add Mori-Takahashi/bawa
```

Then ask your agent:

```text
Build the app in specs/disk-space.md with BAWA.
Write the brief with me, then build milestone 1.
```

Not sure where to start? Type `/bawa`. It looks at your project and picks the next skill.

## How it works

<table>
<tr>
<td width="33%" valign="top">

**1. Plan**

Your agent writes a one-page brief with you, then user stories with checkable acceptance criteria, in milestones.

</td>
<td width="33%" valign="top">

**2. Scaffold**

It creates a WinUI 3 app from a template that already builds, tests itself, and takes screenshots of every page.

</td>
<td width="33%" valign="top">

**3. Build**

It builds one story per commit. Before each commit it snapshots every page in light and dark mode, checks the layout grid, and runs the tests.

</td>
</tr>
</table>

## Skills

<table>
<tr><th>Stage</th><th>Skill</th><th>What it does</th></tr>
<tr><td rowspan="3" valign="top"><b>Start</b></td><td nowrap><a href="skills/start/bawa/SKILL.md"><samp>bawa</samp></a></td><td>Router: finds where you are and calls the right skill</td></tr>
<tr><td nowrap><a href="skills/start/app-brief/SKILL.md"><samp>app-brief</samp></a></td><td>Turns an idea into a one-page brief</td></tr>
<tr><td nowrap><a href="skills/start/new-windows-app/SKILL.md"><samp>new-windows-app</samp></a></td><td>A working app from a template: .NET 10, WinUI 3, SQLite, build script, snapshot mode, self-test, git</td></tr>
<tr><td valign="top"><b>Plan</b></td><td nowrap><a href="skills/plan/user-stories/SKILL.md"><samp>user-stories</samp></a></td><td>A backlog with checkable acceptance criteria, in milestones</td></tr>
<tr><td valign="top"><b>Code</b></td><td nowrap><a href="skills/code/csharp-winui/SKILL.md"><samp>csharp-winui</samp></a></td><td>The UI thread, MVVM, x:Bind, SQLite, Win32 interop, and fixes for XAML and compiler errors</td></tr>
<tr><td rowspan="3" valign="top"><b>Design</b></td><td nowrap><a href="skills/design/ux-design/SKILL.md"><samp>ux-design</samp></a></td><td>Names, flow, where each feature opens (inline, flyout, dialog, pane), words, and accessibility, before code</td></tr>
<tr><td nowrap><a href="skills/design/windows-design/SKILL.md"><samp>windows-design</samp></a></td><td>Windows 11 look and feel: Mica, navigation, command bars, keyboard, type, dark mode</td></tr>
<tr><td nowrap><a href="skills/design/layout-grid/SKILL.md"><samp>layout-grid</samp></a></td><td>A 4 px grid with checks that fail on misalignment: a code lint and a layout probe in snapshot mode</td></tr>
<tr><td rowspan="3" valign="top"><b>Ship</b></td><td nowrap><a href="skills/ship/build-and-verify/SKILL.md"><samp>build-and-verify</samp></a></td><td>Build, install, snapshot every page in light and dark, look at them</td></tr>
<tr><td nowrap><a href="skills/ship/test-windows-app/SKILL.md"><samp>test-windows-app</samp></a></td><td>xUnit, an in-app self-test, a command audit, and UI Automation that drives the running app</td></tr>
<tr><td nowrap><a href="skills/ship/improve-loop/SKILL.md"><samp>improve-loop</samp></a></td><td>Works through the backlog one story per commit until a milestone is done</td></tr>
</table>

**Coming next**, ported from BAMA: `research`, `clone`, `performance`,
`os-integrations` (global hotkeys, tray icon, Explorer), `improve-architecture`,
`agent-cli`, `design-system`, `design-kit`, `design-from-app`, `app-icon`.

## Specs

Not sure what to build? [specs/](specs/) has build plans for small Windows apps. The first one is a disk space analyzer: scan a drive, see the biggest folders, files, and programs, and move what you pick to the Recycle Bin. Copy it into an empty folder and tell your agent:

```text
Build the app in disk-space.md with BAWA.
```

## Install

Pick one route per agent. Two routes install every skill twice.

<details open>
<summary><b>Claude Code</b></summary>

```text
/plugin marketplace add Mori-Takahashi/bawa
/plugin install bawa@bawa
```

To get new versions, run this in your shell, then start a new session:

```bash
claude plugin marketplace update bawa
claude plugin update bawa@bawa
```

</details>

<details>
<summary><b>Codex</b></summary>

```bash
codex plugin marketplace add Mori-Takahashi/bawa
codex plugin add bawa@bawa
```

</details>

<details>
<summary><b>Any other agent</b> (Cursor, Gemini CLI, Copilot, and more)</summary>

Uses [skills.sh](https://skills.sh). Install all skills, or one with `--skill`.

```bash
npx skills@latest add Mori-Takahashi/bawa -a <agent>
npx skills@latest add Mori-Takahashi/bawa --skill=new-windows-app
```

</details>

<details>
<summary><b>By hand</b></summary>

```powershell
git clone https://github.com/Mori-Takahashi/bawa.git
powershell -NoProfile -ExecutionPolicy Bypass -File bawa\scripts\link-skills.ps1
```

It links every skill into `%USERPROFILE%\.claude\skills` and `%USERPROFILE%\.agents\skills`.

</details>

## Requirements

- Windows 10 (version 2004, build 19041) or later; Windows 11 for Mica
- The .NET 10 SDK: `winget install Microsoft.DotNet.SDK.10`
- Git
- An agent that supports skills: Claude Code, Codex, Cursor, Gemini CLI
- No Visual Studio: everything builds with `dotnet` and PowerShell

## Credits

- BAWA is a Windows port of [BAMA: Build Any Mac App](https://github.com/daveebbelaar/bama)
  by Dave Ebbelaar. The pipeline, the skill structure, the specs format, and
  much of the text come from BAMA, under its MIT license.
- The idea and the "any" promise come from
  [REA: Reverse Engineer Anything](https://github.com/morluto/rea) by morluto.
- Skill format and writing rules follow Matt Pocock's
  [skills](https://github.com/mattpocock/skills) and his
  `writing-for-agents` skill.

## License

[MIT](LICENSE). Copyright Dave Ebbelaar (BAMA) and Lyonel Berzen (BAWA).
