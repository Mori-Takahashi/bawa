# __APP_NAME__

A native Windows app in C# and WinUI 3. It runs on this PC only, with a local
SQLite database and no server.

## Layout

- `src/__APP_NAME__/` the app. `App.xaml.cs` picks the database and starts the window; `MainWindow` has the navigation pane, title bar, and Mica.
- `src/__APP_NAME__.Kit/` rules and data logic with no WinUI (`ItemStore`, `StorePath`, `LayoutRules`). Tests in `tests/__APP_NAME__.Kit.Tests/`.
- `__APP_NAME__.slnx` the solution. There is no Visual Studio project to open; everything builds with `dotnet`.
- `build.ps1` builds, installs `%LOCALAPPDATA%\Programs\__APP_NAME__\__APP_NAME__.exe`, and adds a Start menu shortcut.
- `Icon/AppIcon.svg` the app icon. `build.ps1` renders it to `Icon/AppIcon.ico` with `tools/make-icon.cs`.
- `docs/` the brief, the screen index, and the user stories, when they exist.

## Build and check

Scripts are PowerShell. Run them with
`powershell -NoProfile -ExecutionPolicy Bypass -File <script>`, because
Windows blocks `.ps1` files by default.

- Build and install: `build.ps1`. Add `-Open` to restart the app with the new build, `-Debug` for a faster build. The build prints warnings; fix them.
- Unit tests: `dotnet test`.
- Self-test (the app's real functions on a test store in %TEMP%): `tools\selftest.ps1`. Exit 0 when every check passes. Add a check for every story with logic in `SelfTest.cs`.
- After every UI change, take snapshots with sample data and look at them: `tools\snap.ps1`.
  It writes `%TEMP%\__APP_NAME__-shots\<page>-light.png` and `-dark.png` for every navigation page, and `settings-<section>-light.png` and `-dark.png` for every Settings section, then quits. `tools\snap.ps1 -Pages all,settings` limits the pages.
  For a dialog, flyout, or panel, add a capture to `Snapshot.cs` like the Settings loop.
- Snapshots, seed data, and the self-test refuse to run without `APP_STORE` in %TEMP%.
- Never test against the real database in `%LOCALAPPDATA%\__APP_NAME__\`.
- Then check the grid: `tools\grid-lint.ps1` and `dotnet run tools\check-layout.cs -- "$env:TEMP\__APP_NAME__-shots\*-layout.json"`. Both print PASS.
- A change is done when the build has no errors, the grid checks pass, and you looked at the light and dark snapshots.

## Design rules

- Use WinUI's controls and Fluent styles. Only the accent color is custom: `UI/ThemeStore.cs`. People change it, and light or dark mode, in Settings > Appearance.
- Views use theme resources (`AccentFillColorDefaultBrush`, `TextFillColorPrimaryBrush`, `CardBackgroundFillColorDefaultBrush`) and the tokens in `UI/Theme.xaml`. Never hard-code colors, radii, or durations in views.
- Compare accents: `tools\snap.ps1 -Accent green`.
- Spacing comes from `UI/Theme.xaml` (`SpaceSm`, `PageMargin`) and `Space` in `UI/Grid.cs`, on a 4 px grid. Mark pages and cards with `ui:Layout.Area` and `ui:Layout.Probe`.
- Text that lines up with other views uses a style with a fixed line height (`PageTitleTextStyle`, `RowTextStyle`), so its box is whole pixels tall.
- Windows open calm: nothing shows a focus ring on open (`MainWindow.FocusQuietly`). System scroll bars.
- Settings: one `SelectorBar` section per topic in `SettingsPage.xaml`, cards inside. Add a case to `SettingsSection` for a new section.
- Window chrome (title bar, caption buttons, navigation pane) stays system. Buttons use system styles, like `AccentButtonStyle` for the one main action.
- Every action lives in `AppCommands` with a keyboard shortcut, and shows in a command bar or a context menu.
- Icons from Segoe Fluent Icons (`FontIcon Glyph`). Every icon-only button has a tooltip and `AutomationProperties.Name`.
- An empty page shows what it is for and its first action.
- Mica is the window background only. Content sits on cards or directly on the page, never on extra acrylic.

## Skills

The bawa plugin has a skill for each step: `windows-design` (design rules),
`layout-grid`, `ux-design`, `csharp-winui`, `build-and-verify`,
`test-windows-app`, `user-stories`, and `improve-loop`. Use them.

## Git

- Commit after each working change, with a message that says what the user can now do.
