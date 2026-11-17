---
name: csharp-winui
description: C# and WinUI 3 code rules for Windows apps, including the UI thread, MVVM with CommunityToolkit.Mvvm, x:Bind, SQLite, Win32 interop, and fixes for common XAML and compiler errors. Use when writing, reviewing, or debugging C# or XAML in a WinUI app, or on WMC, CS, or crash-at-start errors.
---

# C# and WinUI 3

The code is right when it builds with zero warnings and the app does what
the story says. For how it should look, call the Skill tool with
`windows-design`. For copyable view code, read
[../../design/windows-design/patterns.md](../../design/windows-design/patterns.md).

## The UI thread

WinUI has one UI thread. Every control, every `ObservableCollection` bound
to a list, and every view model property bound with `x:Bind` belongs to it.

| Work | Put it in |
| --- | --- |
| Views, view models, bound collections | The UI thread (the default in event handlers and commands) |
| File system scans, hashing, parsing, big queries | `await Task.Run(() => ...)` from the command |
| Rules and data logic | The `.Kit` library, with `ConfigureAwait(false)` on its awaits |
| A callback from another thread (a timer, a file watcher, Win32) | `dispatcherQueue.TryEnqueue(() => ...)` back to the UI thread |

- After `await` in UI code you are back on the UI thread. Do not add
  `ConfigureAwait(false)` there.
- Report progress with `Progress<T>`: created on the UI thread, its callback
  runs there. Report at most about 10 times a second, never per file.
- Add results to a bound collection in batches after the work, or a few
  hundred at a time. Adding 100,000 rows one by one freezes the window.
- A long task gets a cancel button (`[RelayCommand(IncludeCancelCommand = true)]`).

```csharp
[RelayCommand(IncludeCancelCommand = true)]
private async Task ScanAsync(string root, CancellationToken cancel)
{
    Results.Clear();
    var progress = new Progress<int>(count => Status = $"{count:N0} folders");   // runs on the UI thread
    var found = await Task.Run(() => Scanner.Scan(root, progress, cancel), cancel);
    foreach (var folder in found.Take(500)) Results.Add(folder);              // back on the UI thread
    Status = $"{found.Count:N0} folders";
}
```

This generates `ScanCommand` and `ScanCancelCommand`.

## View models (CommunityToolkit.Mvvm)

- A view model is a `sealed partial class` that derives from `ObservableObject`.
- Properties: `[ObservableProperty] public partial string Status { get; set; }`.
  Partial properties, not fields: they work with WinUI's trimming and AOT
  rules, and the analyzer warns about fields.
- React to a change with `partial void OnStatusChanged(string value)`.
- Commands: `[RelayCommand]` on a method `AddItem()` generates
  `AddItemCommand`. `CanExecute = nameof(HasSelection)` plus
  `[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]` on the property it
  depends on keeps buttons enabled and disabled at the right time.
- Every action also goes into `AppCommands` (`Navigation.cs`), so it gets a
  shortcut and the self-test sees it.

## x:Bind

- `x:Bind` is compiled and checked at build time. Prefer it to `Binding`.
- Its default mode is `OneTime`. Anything that changes needs
  `Mode=OneWay`, and an input needs `Mode=TwoWay`. A value that never
  updates on screen is almost always a missing `Mode=OneWay`.
- `bool` binds straight to `Visibility`.
- In a `DataTemplate`, set `x:DataType` to the row type. Then `{x:Bind Title}`
  reads the row's property.
- Never give an element an `x:Name` that equals a property name used by
  `x:Bind` in the same page (`x:Name="Title"` next to `{x:Bind Title}` in a
  template). The XAML compiler fails with `WMC9999` and no useful text.
- `SelectedItem` is `object`. Sync it in `SelectionChanged` instead of a
  `TwoWay` bind to a typed property.
- A row type defined in the app project and used as `x:DataType` must not
  have `init` setters: a positional `record Row(string Path)` fails with
  `CS8852` in `XamlTypeInfo.g.cs`. Give it get-only properties
  (`public string Path { get; } = Path;`), or keep row types in the Kit.

## Data (SQLite)

- The database lives in `%LOCALAPPDATA%\<App>\<App>.db`. `APP_STORE`
  overrides it, and test modes only run on a store in `%TEMP%`.
- One `ItemStore`, one connection, opened at start, WAL mode.
- Schema changes are new entries at the end of `Migrations`. Never edit an
  old one. New columns get a `DEFAULT`, so existing databases keep loading.
- Queries that can return thousands of rows run in `Task.Run`, then fill the
  collection on the UI thread.
- Write numbers and dates to files and the database with
  `CultureInfo.InvariantCulture` or ISO 8601 (`ToString("O")`). A German PC
  writes `800,5` otherwise.
- In tests, `SqliteConnection.ClearAllPools()` before deleting a database
  file, or it stays locked.

## No package identity

The template is unpackaged. APIs that need a package identity fail at run
time:

- `ApplicationData.Current` (LocalSettings, LocalFolder) throws. Use files in
  `%LOCALAPPDATA%\<App>\`.
- `ms-appx:///` works for files copied next to the exe (`Content` items).
- Toast notifications, share target, startup tasks: see `new-windows-app`,
  "When to switch to an MSIX package".

## Win32 and WinRT from a desktop app

- The window handle: `WinRT.Interop.WindowNative.GetWindowHandle(window)`.
- Pickers and some dialogs need it, or they throw "Invalid window handle":

  ```csharp
  var picker = new FolderPicker();
  picker.FileTypeFilter.Add("*");
  WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
  var folder = await picker.PickSingleFolderAsync();
  ```

- Win32 calls use `[LibraryImport]` in a `static partial class`, with
  `[return: MarshalAs(UnmanagedType.Bool)]` on `BOOL` results. The template
  sets `AllowUnsafeBlocks`, which `LibraryImport` needs.
- `ContentDialog` needs `XamlRoot = this.XamlRoot` (from a page) or it
  throws. Only one dialog can be open at a time.

## Names that clash

`ImplicitUsings` imports `System.IO`, `System.Threading.Tasks`, and more.
Never name your own type `File`, `Directory`, `Path`, `Task`, `Thread`,
`Timer`, `Window`, `Page`, `Application`, or `Grid`. `File` breaks every
`File.ReadAllText`. Use `ScannedFile`, `TodoTask`, `FolderPath`.

`using Windows.Storage;` next to `System.IO` makes `FileAttributes`
ambiguous: write `System.IO.FileAttributes`.

## Files and the file system

- `EnumerationOptions` skips hidden and system files unless you set
  `AttributesToSkip = 0`. A disk or backup tool that keeps the default
  misses `desktop.ini`, `pagefile.sys`, and every hidden folder.
- Skip folder junctions and symbolic links by checking
  `FileAttributes.ReparsePoint` on directories only. OneDrive files are
  reparse points too: `AttributesToSkip = ReparsePoint` drops them.
- `IgnoreInaccessible = true` also hides a folder whose own listing fails.
  To count folders you may not read, keep it false and catch
  `UnauthorizedAccessException` around the listing.
- Glyphs in C# strings are escapes: `"\uE8B7"`. The invisible character
  itself survives the compiler but not every editor or script.
- Windows PowerShell 5.1 reads UTF-8 files without a BOM as ANSI, so
  `Get-Content | Set-Content` garbles non-ASCII text, and `[IO.File]`
  resolves relative paths against the process folder. Edit with the editor
  tool, or `[IO.File]` with full paths.

## Build and run errors

| Error | Fix |
| --- | --- |
| `WMC9999` with "ErrorMessages.resources" or "keine Ressourcen gefunden" | The XAML compiler hit an error it cannot print. Read the stack in `obj\...\output.json` (`MSBuildLogEntries`). `BindingPathListener` means an `x:Bind` path: check names and `x:DataType` |
| `WMC9999` "object reference not set" right after C# errors | Fix the C# errors first; the XAML pass needs the compiled code |
| `WMC9997` "Unexpected token" | Broken XAML: a missing quote or space, or a raw `<` inside an attribute (write `&lt;`) |
| `CS0246` `ObservableObject` or `RelayCommand` not found | Add the `CommunityToolkit.Mvvm` package to the app project |
| `CS9248` partial property needs an implementation | The MVVM source generator is not running: same package fix |
| `CS0104` ambiguous reference | Two imported namespaces share a name. Use the full name |
| "The process cannot access the file" during build | The app is running: `build.ps1 -Open` |
| App closes at start, `0xC000027B` in the event log | Missing XAML in the install (`EnableMsixTooling` must stay true), or an exception in a XAML event: run the debug exe from `bin\` to see it |
| `XamlParseException` at run time | A `StaticResource` key that does not exist, or a resource defined after its first use |
| `dotnet run tool.cs` says reflection-based serialization is disabled | File-based apps default to Native AOT. Add `#:property PublishAot=false` |

Fix the first error first. Later errors often come from it.

Done when the app builds with zero warnings, `dotnet test` and
`tools\selftest.ps1` pass, and the story's acceptance checks pass.
