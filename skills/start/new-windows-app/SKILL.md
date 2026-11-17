---
name: new-windows-app
description: Native Windows app from a working WinUI 3 template (.NET 10, SQLite, build script, snapshot mode, self-test, git), built from the command line. Use when the user wants to start a new Windows app or scaffold a WinUI project.
argument-hint: "<AppName> [parent-folder]"
---

# New Windows app

The template builds and runs as is. The first build proves the toolchain
works before you write feature code.

Scripts are PowerShell. Run every `.ps1` with
`powershell -NoProfile -ExecutionPolicy Bypass -File <script>`: Windows
blocks scripts by default, and this runs one script without changing that.

## What the template is

A .NET solution with three projects, plus PowerShell scripts that build,
install, test, and photograph the app. An agent can read and edit every
file. No Visual Studio needed.

```text
<AppName>/
  <AppName>.slnx            the solution
  global.json               dotnet test uses Microsoft Testing Platform
  Directory.Build.props     build output in %LOCALAPPDATA%\<AppName>\build, outside the repo
  build.ps1                 dotnet publish -> %LOCALAPPDATA%\Programs\<AppName> + Start menu shortcut
  CLAUDE.md                 build, check, and design rules for agents
  Icon/AppIcon.svg          the app icon source; AppIcon.ico is made from it
  src/<AppName>/
    App.xaml.cs             database location, seed data, self-test and snapshot start
    MainWindow.xaml(.cs)    NavigationView, own title bar, Mica, caption button colors
    Navigation.cs           SidebarPage, SettingsSection, AppCommands (every action and shortcut)
    ViewModels/             AppViewModel (CommunityToolkit.Mvvm)
    Pages/                  ItemsPage (list, command bar, empty state), SettingsPage (sections)
    Data/SeedData.cs        sample data for snapshots
    UI/Theme.xaml           tokens: spacing, margins, radii, text styles, card style
    UI/ThemeStore.cs        light, dark, or system, and the accent color, saved per user
    UI/Grid.cs              the 4 px scale (Space) and layout probes (Layout.Area, Layout.Probe)
    SelfTest.cs             --selftest: PASS or FAIL per check, exit 0 or 1
    Snapshot.cs             APP_SNAPSHOT: PNG and layout file per page and Settings section, light and dark
    WindowCapture.cs        PrintWindow capture of the app's own window
  src/<AppName>.Kit/        logic with no WinUI: ItemStore (SQLite), StorePath, LayoutRules
  tests/<AppName>.Kit.Tests/  xUnit tests for the Kit
  tools/
    selftest.ps1  snap.ps1  grid-lint.ps1
    check-layout.cs         dotnet run tools\check-layout.cs -- <layout files>
    make-icon.cs            Icon\AppIcon.svg -> Icon\AppIcon.ico
```

## Steps

1. Check the toolchain. Expected: a `10.` line from the first command.

   ```powershell
   dotnet --list-sdks
   git --version
   ```

   No .NET 10 SDK: `winget install Microsoft.DotNet.SDK.10`.

2. Pick the name: one UpperCamelCase word, like `Tasks` or `DiskSpace`. If
   `docs/BRIEF.md` exists, use its name.

3. Run the script with the parent folder. It copies the template, fills in
   the name, runs `git init`, and commits. It keeps a folder that holds only
   a brief. Expected: `created <folder>`.

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File "${CLAUDE_SKILL_DIR}\scripts\new-app.ps1" Tasks "$env:USERPROFILE\Documents"
   ```

4. Build, install, and open it. The first build downloads packages and takes
   a few minutes. Expected: `installed ...\<AppName>.exe (Release)` and a
   window with All, Starred, and Settings.

   ```powershell
   cd "$env:USERPROFILE\Documents\Tasks"
   powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1 -Open
   ```

5. Run the checks the template ships with, then look at the snapshots
   (`build-and-verify` skill). Expected: tests pass, `SELFTEST PASS`,
   `SNAPSHOT done`, `PASS grid-lint`, and `PASS` for every layout file.

   ```powershell
   dotnet test
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\selftest.ps1
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\snap.ps1
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\grid-lint.ps1
   dotnet run tools\check-layout.cs -- "$env:TEMP\Tasks-shots\*-layout.json"
   ```

6. If `docs/BRIEF.md` exists and its main data is a list people create
   (tasks, notes, recipes), rename `Item` to that type and the pages in
   `SidebarPage` to the brief's pages. When the data is something else (scan
   results, system state, a feed), leave the template as it is: the first M1
   story replaces `ItemsPage`, `Item`, and `ItemStore`. Either way, nothing
   else: features go through `improve-loop`, one story at a time.

## When to switch to an MSIX package

The unpackaged app covers most personal apps. Switch to a packaged app
(`WindowsPackageType` MSIX, a `Package.appxmanifest`, and a signing
certificate) when the app needs package identity: toast notifications that
open the app, a share target, a startup task, file type associations
registered by Windows, or the Microsoft Store.

## A CLI for agents

The `agent-cli` skill (letting Claude Code, Codex, or scripts drive the
running app) is not ported to Windows yet. Until then, the self-test is the
way to run the app's real functions from a script.

## When the build fails

- `WMC9999` with "ErrorMessages.resources" or "keine Ressourcen gefunden": the
  XAML compiler hit a real error and cannot print its text. Open
  `%LOCALAPPDATA%\<AppName>\build\obj\<AppName>\<config>\...\output.json` and
  read the stack trace in `MSBuildLogEntries`. `BindingPathListener` means an
  `x:Bind` path. A common cause: an `x:Name` that equals a property name used
  by `x:Bind` in a `DataTemplate` (`x:Name="Title"` next to `{x:Bind Title}`).
- `WMC9997` "Unexpected token": broken XAML on the line it names.
- `close <AppName> first`: the installed app is running. Use `build.ps1 -Open`.
- The installed app closes at once with `0xC000027B` in the event log: the
  XAML is missing from the install. Keep `EnableMsixTooling` true in the
  `.csproj`.
- "not enough space on the disk": a WinUI build with its runtime needs
  about 250 MB per configuration. Free space, then build again.
- More compiler and runtime errors: call the Skill tool with `csharp-winui`.

Done when the app opens from the Start menu, the snapshots show every page
in light and dark, the checks in step 5 pass, and the first commit exists.
