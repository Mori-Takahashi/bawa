# WinUI patterns for Windows apps

Copyable code for things most apps need. Every block compiles in an app made
from the `new-windows-app` template (namespace `<App>`, `using` lines as in
the template plus the ones shown).

## Contents

- Long work with progress and cancel
- Confirm, then move to the Recycle Bin
- Undo in an InfoBar
- Search with Ctrl+F
- Drag rows out as real files
- Show a file in Explorer
- Pick a folder
- Remember the window's size and place

## Long work with progress and cancel

The command runs the work in `Task.Run`, reports progress at most ten times
a second, and fills the list once at the end. `IncludeCancelCommand`
generates `ScanCancelCommand` for a Cancel button.

```csharp
public sealed partial class ScanViewModel : ObservableObject
{
    public ObservableCollection<FolderSize> Results { get; } = [];

    [ObservableProperty]
    public partial string Status { get; set; } = "";

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ScanAsync(string root, CancellationToken cancel)
    {
        Results.Clear();
        var progress = new Progress<int>(count => Status = $"{count:N0} folders");   // runs on the UI thread
        var found = await Task.Run(() => Scanner.Scan(root, progress, cancel), cancel);
        foreach (var folder in found.Take(500)) Results.Add(folder);              // back on the UI thread
        Status = $"{found.Count:N0} folders";
    }
}

public static class Scanner
{
    public static List<FolderSize> Scan(string root, IProgress<int> progress, CancellationToken cancel)
    {
        var result = new List<FolderSize>();
        var skip = new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = System.IO.FileAttributes.ReparsePoint };
        var deep = new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true, AttributesToSkip = System.IO.FileAttributes.ReparsePoint };
        var last = Stopwatch.StartNew();
        foreach (var dir in Directory.EnumerateDirectories(root, "*", skip))
        {
            cancel.ThrowIfCancellationRequested();
            var bytes = new DirectoryInfo(dir).EnumerateFiles("*", deep).Sum(f => f.Length);
            result.Add(new FolderSize(dir, bytes));
            if (last.ElapsedMilliseconds > 100) { progress.Report(result.Count); last.Restart(); }
        }
        return result.OrderByDescending(f => f.Bytes).ToList();
    }
}

// A row type for x:DataType in the app project: get-only properties, not a positional record.
public sealed record FolderSize(string Path, long Bytes)
{
    public string Path { get; } = Path;
    public long Bytes { get; } = Bytes;
}
```

`IgnoreInaccessible` skips folders the user may not read instead of
throwing. `AttributesToSkip = ReparsePoint` skips junctions and symbolic
links, which would count a folder twice or loop forever.

## Confirm, then move to the Recycle Bin

Name what goes and how much. The safe button is the default, so Enter does
not delete.

```csharp
public static async Task<bool> RecycleAsync(XamlRoot root, IReadOnlyList<string> paths, string sizeText)
{
    var dialog = new ContentDialog
    {
        XamlRoot = root,
        Title = paths.Count == 1 ? $"Move \"{Path.GetFileName(paths[0])}\" to the Recycle Bin?" : $"Move {paths.Count} items to the Recycle Bin?",
        Content = $"{sizeText} will be freed when you empty the Recycle Bin.",
        PrimaryButtonText = "Move to Recycle Bin",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Close,
    };
    if (await dialog.ShowAsync() != ContentDialogResult.Primary) return false;
    await Task.Run(() =>
    {
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            else
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
        }
    });
    return true;
}
```

`Microsoft.VisualBasic.FileIO` ships with .NET on Windows; no package
needed. Call it with `XamlRoot = this.XamlRoot` from a page.

## Undo in an InfoBar

After a bulk action, an InfoBar at the top of the page says what happened
and offers Undo. It stays until closed or replaced.

```xml
<InfoBar x:Name="Status" Grid.Row="1" IsOpen="False" IsClosable="True" />
```

```csharp
public static void OfferUndo(InfoBar bar, string message, Action undo)
{
    var button = new Button { Content = "Undo" };
    button.Click += (_, _) => { undo(); bar.IsOpen = false; };
    bar.Title = message;
    bar.Severity = InfoBarSeverity.Informational;
    bar.ActionButton = button;
    bar.IsOpen = true;
}
```

## Search with Ctrl+F

```xml
<AutoSuggestBox x:Name="Search" PlaceholderText="Search" QueryIcon="Find" Width="320"
                TextChanged="OnSearchTextChanged" AutomationProperties.Name="Search">
  <AutoSuggestBox.KeyboardAccelerators>
    <KeyboardAccelerator Key="F" Modifiers="Control" Invoked="OnFocusSearch" />
  </AutoSuggestBox.KeyboardAccelerators>
</AutoSuggestBox>
```

```csharp
private void OnFocusSearch(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
{
    Search.Focus(FocusState.Keyboard);
    args.Handled = true;
}

private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
{
    if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
    var query = sender.Text.Trim();
    Visible.Clear();
    foreach (var row in _all.Where(r => query.Length == 0 || r.Path.Contains(query, StringComparison.CurrentCultureIgnoreCase)))
        Visible.Add(row);
}
```

Add the search to `AppCommands` too, so the command audit sees Ctrl+F.

## Drag rows out as real files

```xml
<ListView SelectionMode="Extended" CanDragItems="True" DragItemsStarting="OnDragItemsStarting" />
```

```csharp
public static void OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
{
    var paths = e.Items.OfType<FolderSize>().Select(r => r.Path).Where(File.Exists).ToList();
    if (paths.Count == 0) { e.Cancel = true; return; }
    e.Data.RequestedOperation = DataPackageOperation.Copy;
    // The files are read only when the drop target asks for them.
    e.Data.SetDataProvider(StandardDataFormats.StorageItems, async request =>
    {
        var deferral = request.GetDeferral();
        var files = new List<IStorageItem>();
        foreach (var path in paths) files.Add(await StorageFile.GetFileFromPathAsync(path));
        request.SetData(files);
        deferral.Complete();
    });
}
```

`DataPackage` has no deferral of its own; async work goes in the data
provider. Needs `using Windows.ApplicationModel.DataTransfer;` and
`using Windows.Storage;` (then write `System.IO.FileAttributes` in full).

## Show a file in Explorer

```csharp
public static void ShowInExplorer(string path)
    => Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
```

## Pick a folder

```csharp
public static async Task<StorageFolder?> PickFolderAsync(Window window)
{
    var picker = new FolderPicker();
    picker.FileTypeFilter.Add("*");
    WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
    return await picker.PickSingleFolderAsync();
}
```

Without `InitializeWithWindow` the picker throws in a desktop app.

## Remember the window's size and place

Save on close, restore at start, but only when the spot is still on a
screen (a monitor may be gone).

```csharp
public static string SaveBounds(AppWindow window)
    => $"{window.Position.X},{window.Position.Y},{window.Size.Width},{window.Size.Height}";

public static void RestoreBounds(AppWindow window, string? saved)
{
    if (saved?.Split(',').Select(int.Parse).ToArray() is not [var x, var y, var w, var h]) return;
    var rect = new RectInt32(x, y, w, h);
    var area = DisplayArea.GetFromRect(rect, DisplayAreaFallback.None);
    if (area is null) return;
    window.MoveAndResize(rect);
}
```

Store the string next to the theme settings (`ThemeStore`'s JSON file).
Snapshot mode should not restore it, so snapshots keep one size.
