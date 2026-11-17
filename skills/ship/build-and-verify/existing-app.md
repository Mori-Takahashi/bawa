# Existing apps

The ship and code skills assume an app made from the `new-windows-app`
template: an unpackaged WinUI 3 app, `build.ps1`, `APP_SNAPSHOT`,
`APP_STORE`, a `.Kit` library with xUnit tests. An older app, a packaged
(MSIX) app, or a WPF or WinForms app has none of these. Map each piece
first, then follow the skills.

## Contents

- Map the template pieces
- Snapshots in an app without snapshot mode
- Unit tests for an app without a Kit project
- Self-test in a running app

## Map the template pieces

| Template piece | Packaged WinUI (MSIX) | WPF | WinForms |
| --- | --- | --- | --- |
| `build.ps1` | `dotnet build -c Release -p:Platform=x64`, then `Add-AppxPackage -Register <bin>\AppxManifest.xml` | `dotnet publish -c Release -r win-x64 -o <install folder>` | same as WPF |
| `build.ps1 -Open` | `Stop-Process`, register again, start with `explorer.exe shell:AppsFolder\<PackageFamilyName>!App` | `Stop-Process`, publish, `Start-Process` | same as WPF |
| `APP_SNAPSHOT` mode | Copy `Snapshot.cs` and `WindowCapture.cs` from the template | `RenderTargetBitmap` of the window (below) | `Control.DrawToBitmap` (below) |
| `APP_STORE` in %TEMP% | A data folder override read at start | same | same |
| `dotnet test` | A class library with the logic plus an xUnit project | same | same |
| `--selftest` | Copy `SelfTest.cs` and `AppOutput.cs` | A `--selftest` argument in `App.OnStartup` | in `Program.Main` |

Write the mapping into the app's `CLAUDE.md`, and create the file if it is
missing.

A packaged app's data lives in
`%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState`, not in
`%LOCALAPPDATA%\<AppName>`. Point the test store override at `%TEMP%`
all the same.

## Snapshots in an app without snapshot mode

For a WinUI app, copy `WindowCapture.cs` from the template: `PrintWindow`
with `PW_RENDERFULLCONTENT` shows the window as on screen and nothing behind
it.

For WPF, draw the window's content:

```csharp
var size = new Size(window.ActualWidth, window.ActualHeight);
var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
bitmap.Render((Visual)window.Content);
var encoder = new PngBitmapEncoder();
encoder.Frames.Add(BitmapFrame.Create(bitmap));
using var file = File.Create(path);
encoder.Save(file);
```

It leaves out the title bar. That is fine for checking content.

For WinForms, `form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size))`
draws the form, title bar included, into a `System.Drawing.Bitmap`.

In every case: switch light and dark in the app (not in Windows), capture
each page, and quit with an exit code, like the template's `Snapshot.cs`.

## Unit tests for an app without a Kit project

Move rules and data logic into a class library with no UI references
(`net10.0`), reference it from the app, and add an xUnit project for it.
Keep the template's `global.json` with `"test": { "runner":
"Microsoft.Testing.Platform" }` when the tests use xunit.v3: the .NET 10
SDK refuses the old VSTest runner for it.

## Self-test in a running app

An app that allows only one instance (a tray utility) cannot start a
second copy with `--selftest`. Run the self-test as a command the running
instance receives instead: a named pipe that accepts `selftest` and writes
the PASS and FAIL lines back. The `test-windows-app` skill has the shape.
