---
name: test-windows-app
description: Test a WinUI 3 Windows app at four levels, from xUnit unit tests and an in-app self-test to a command audit and UI Automation that drives the running app from outside. Use when adding logic, fixing a bug, before a release, or when the user asks for tests, end-to-end checks, or "make sure it works".
---

# Test a Windows app

Four levels, cheapest first. Each new story gets the cheapest level that
can prove its acceptance criteria. A bug fix starts with a test that fails.

| Level | Proves | Speed |
| --- | --- | --- |
| Unit tests (xUnit, `dotnet test`) | Rules, parsing, data | Seconds |
| Self-test (`--selftest`) | The app's real functions on a test store | Seconds |
| Command audit (inside the self-test) | Every action has a shortcut, none twice | Seconds |
| UI Automation (`ui-check.cs`) + computer use | Real controls, key presses, other apps | Minutes |

Snapshots (`build-and-verify` skill) prove how it looks. This skill proves
how it behaves. Below, `ps1` stands for
`powershell -NoProfile -ExecutionPolicy Bypass -File`, the way to run a
script that Windows would block by default.

## 1. Unit tests

Logic lives in the `<App>.Kit` library with no WinUI, so tests run without
the app.

```csharp
[Theory]
[InlineData("tomorrow 9am", 1, 9)]
[InlineData("next fri", 5, null)]
public void Parses_natural_dates(string phrase, int dayOffset, int? hour)
{
    var result = DateParser.Parse(phrase, now: Fixed.Monday);
    Assert.NotNull(result);
    Assert.Equal(dayOffset, result.DayOffset);
    Assert.Equal(hour, result.Hour);
}
```

```powershell
dotnet test
```

- Fix the clock (a `now` parameter) and the store path in every test. A
  test that opens a store uses its own folder from
  `Directory.CreateTempSubdirectory`, and calls
  `SqliteConnection.ClearAllPools()` before deleting it, or the file stays
  locked.
- A parser that removes text from what the user typed needs at least 20
  negative rows: real titles that contain date or time words ("Watch
  Friday Night Lights"). Add consistency rows for related phrases ("next
  week" and "next friday"), and a tested way to undo a parsed part.
- **Table tests** (`[Theory]` with `[InlineData]`) for anything with many
  cases. Keep the table in `docs/research/<topic>.md` too, and add a row
  there before changing behavior.
- Anything that writes numbers or dates to a file gets a test under
  `CultureInfo("de-DE")`: German Windows writes `800,5` unless the code
  says `CultureInfo.InvariantCulture`.

## 2. Self-test

A launch flag that runs the same functions the UI calls, prints PASS or
FAIL per step, and exits 0 or 1:

```powershell
ps1 tools\selftest.ps1
# PASS New item adds one item
# PASS the new item is selected
# ...
# SELFTEST PASS 8 checks
```

`selftest.ps1` deletes the old self-test store, sets `APP_STORE` in
`%TEMP%`, runs the installed app with `--selftest`, prints the log, and
exits with the app's exit code. The app refuses `--selftest` on any store
outside `%TEMP%`.

Add checks in `src\<App>\SelfTest.cs` with `Check("<what the user can do>", condition)`.
Run actions through the view model's commands (`vm.AddItemCommand.Execute(null)`),
the same objects the buttons and shortcuts use.

Checks worth adding to any self-test:

- For every page: New creates a row that is visible and selected on that
  page. After an action removes the selected row, the next row is selected.
- Every setting survives a restart: write it, create a new `ThemeStore` on
  the same file, read it back.
- **Smoke step**: start every background task, timer, and file watcher for
  a few seconds, then print PASS. It is the only check that catches crashes
  on background threads.

## 3. Command audit

`AppCommands` in `Navigation.cs` lists every action with its title, icon,
and shortcut, and the command bar, context menus, and accelerators are
built from it. The template's self-test already checks that every command
has a shortcut and that no shortcut is used twice. Add a command to the
list, never a button with its own accelerator, so the audit sees it.

The audit proves the table. Whether a real key press reaches the command
is level 4.

## 4. UI Automation and computer use

UI Automation is how Narrator and test tools see the app. It finds controls
by name and type and works the app from outside, with no change to the
app. Copy [scripts/ui-check.cs](scripts/ui-check.cs) to `tools\` and grow
it per story:

```powershell
$env:APP_STORE = "$env:TEMP\<App>-ui.db"
dotnet run tools\ui-check.cs -- "$env:LOCALAPPDATA\Programs\<App>\<App>.exe"
# PASS New item adds a row
# PASS Delete removes the selected row
# PASS Ctrl+N adds a row
```

- Prefer patterns (`Invoke` on a button, `SelectionItem.Select` on a row).
  They work while the app is in the background.
- Real key presses go to whichever window is in front. Windows often
  refuses to bring an app forward while someone else uses the PC (the
  foreground lock). `ui-check.cs` checks that the app has keyboard focus
  first, and prints `SKIP` instead of typing into another app.
- Wait for a condition (the `Within` helper, up to 3 seconds), never a
  fixed sleep. A fixed sleep passes on a fast run and fails on a cold start.
- A control that UI Automation cannot find by name has no name for Narrator
  either. Give it `AutomationProperties.Name`.

Computer use is the last resort, for what only a person can do: a drag from
Explorer, a dialog from another app, a UAC prompt. Tell the user to keep off
the mouse while it runs. Capture your own window, never the full screen.

## Runs that never hang

- Quit through `App.Quit(code)`: it sets `Environment.ExitCode` and calls
  `Application.Current.Exit()`. Scripts read the code from
  `Start-Process -Wait -PassThru`.
- Delete the old test store and its `-wal`, `-shm`, `.log`, and
  `.settings.json` files before a seeded run. Seed data only fills an empty
  store.
- Write output with `AppOutput.Line`: one flushed line at a time to the
  console and to `<APP_STORE>.log`, so a crash keeps the lines before it.
- A WinUI app is a GUI program: `& app.exe` in PowerShell does not wait for
  it. Use `Start-Process -Wait -PassThru`, or pipe its output
  (`& app.exe | Out-String`), which waits until it exits.

## A bridge into the running app

For behavior a fresh launch cannot reach (a tray app that allows one
instance, global hotkeys, timing), the running app can listen on a named
pipe for commands like `status`, `selftest`, or one command per action, and
write the answer back. The full recipe belongs to the `agent-cli` skill,
which is not ported to Windows yet. Until then, keep such checks in the
self-test or in `ui-check.cs`.

Done when every acceptance line of the story maps to a passing test,
self-test step, UI Automation check, or a named manual check, and the bug
fix has a test that failed before the fix.
