---
name: build-and-verify
description: Build a WinUI 3 Windows app from the command line, install it, and check light and dark window snapshots of every page before calling a change done. Use after any UI code change, when a build fails, or when the user asks to run, screenshot, or check the app.
---

# Build and verify

A change is done when four things are true:

1. `build.ps1` finishes with no errors.
2. You looked at the light and dark snapshots of every page you touched.
3. The snapshots match what the user story or reference screenshot asks for.
4. The grid checks pass (`layout-grid` skill).

App not made from the `new-windows-app` template (a packaged MSIX app, WPF,
WinForms)? Map the commands below with [existing-app.md](existing-app.md)
first.

Do not report a UI change as done from the code alone. XAML that compiles
can still render an empty page, clipped text, or unreadable dark mode.

Run every script with `powershell -NoProfile -ExecutionPolicy Bypass -File`,
because Windows blocks `.ps1` files by default. Below, `ps1` stands for
that prefix.

## 1. Build

```powershell
ps1 build.ps1            # release build, installs %LOCALAPPDATA%\Programs\<AppName>
ps1 build.ps1 -Debug     # faster build while iterating
ps1 build.ps1 -Open      # also restarts the app so the user sees the new build
```

Read the errors, fix them, and build again. Fix the first error first: later
errors often come from it. `build.ps1` prints only error and warning lines.
`close <AppName> first` means the installed app is running: use `-Open`.
For XAML compiler errors (`WMC....`) see `new-windows-app`, "When the
build fails".

## 2. Snapshot

The app opens with a test database, walks through every navigation page and
every Settings section in light and dark, saves one PNG per page and mode,
and quits.

```powershell
ps1 tools\snap.ps1
# SNAPSHOT C:\Users\<you>\AppData\Local\Temp\<AppName>-shots\all-light.png
# ...
# SNAPSHOT done C:\Users\<you>\AppData\Local\Temp\<AppName>-shots
```

`snap.ps1` deletes the old test store (with its `-wal`, `-shm`, log, and
settings files) first, then runs with sample data (`APP_SEED=1`). Seed data
only fills an empty store, so a kept store goes stale.

`ps1 tools\snap.ps1 -Pages all,starred` limits the pages. Add `settings`
to include the Settings sections. `-Accent green` shows another accent.
`-NoMica` shows the Windows 10 look (no Mica, solid background) on
Windows 11.

A page with more than one state (empty, working, result) needs a capture
per state. Add it in `Snapshot.cs` after the page loop: put the app in the
state with the same command the user runs (for example scan the seed
folder), capture `<page>-<state>-<mode>.png`, and reset. Then `-Pages`
takes that id too.

In every image, look for three bugs agents often miss: a focus ring
nobody asked for, a scroll bar with nothing to scroll, and an empty band
under short content. The fixes are in `windows-design`, "Windows open
calm".

Snapshots always use `APP_STORE` in `%TEMP%`. The app refuses snapshot mode
on any other store, so the user's real database in
`%LOCALAPPDATA%\<AppName>\` stays untouched.

### Seed data

Empty pages only show empty states. `src\<AppName>\Data\SeedData.cs`
fills an empty test store when `APP_SEED=1` is set. Grow it with the app:
10 to 20 items with long and short titles, dates in the past and future,
every status, and only what a person can create in the app.

### How the capture works

Snapshot mode draws the app's own window with `PrintWindow`, so the title
bar, Mica, and the navigation pane look as on screen, and nothing of the
windows behind it shows. It needs no permission. Before each capture it
restores the window if it was minimized and brings it forward.

A line that ends with `(fallback: backdrop and title bar are blank)` means
the window could not be drawn as on screen and the image came from the
XAML tree alone (`PrintWindow` returned an empty image). Run it again
with the screen unlocked and nobody minimizing the app during the run.

The built-in Settings entry of the navigation pane shows in the Windows
display language ("Einstellungen" on a German PC). That is expected.

## 3. Check the grid

```powershell
ps1 tools\grid-lint.ps1
dotnet run tools\check-layout.cs -- "$env:TEMP\<AppName>-shots\*-layout.json"
```

Both print PASS. On a FAIL, call the Skill tool with `layout-grid`: it has
the rules and a table of fixes.

## 4. Look

Open every PNG with the Read tool and check, in this order:

- Nothing is blank, clipped, or overlapping. Text fits. The navigation pane shows its items.
- Dark mode is readable. No fixed white or black surfaces. The caption buttons show in both modes.
- The page matches the story or the reference screenshot (layout, labels, order).
- The `windows-design` rules hold: Mica only as the window background, one
  accent, Segoe Fluent Icons, Fluent text styles, empty states that invite
  the first action.

Describe what you see in one or two sentences per page in your report, and
name the PNG paths so the user can open them.

## 5. Interaction checks

Snapshots show state, not behavior. Run `ps1 tools\selftest.ps1` (exit 0
when every check passes) and add a check for each new action. The
`test-windows-app` skill covers unit tests, the self-test, the command
audit, and UI Automation from outside.

### Debug output and private screens

The app has no console window. Write debug lines with `AppOutput.Line`:
they go to the console that started the app and to `<APP_STORE>.log`, one
flushed line at a time, so a crash keeps the lines before it.

Do not take full-screen captures to check your app: they show the user's
other windows. Capture your own window, or add a mode to the app that
renders the state you want to see (prefilled text, an open dialog).

## 6. Commit

When the build is clean and the snapshots look right, commit with a message
that says what the user can now do: "Add tasks from the quick-add box",
not "Update views".

Done when the four conditions at the top hold and the change is committed.
