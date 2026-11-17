# Where a feature opens

Code for each row of the surface table in [SKILL.md](SKILL.md). It compiles
in an app made from the `new-windows-app` template (.NET 10, WinUI 3,
Windows App SDK 2.6).

## Contents

- Inline
- Details pane
- Flyout
- TeachingTip
- ContentDialog
- Confirmation dialog
- New window
- Navigation page
- Status instead of a dialog
- Unsaved changes

## Inline

Use for one value on something already on screen. Nothing opens, so nothing
needs closing. Enter saves, Escape puts the old text back, leaving the box
saves.

```xml
<TextBox x:Name="TitleBox" PlaceholderText="Task title" AutomationProperties.Name="Task title"
         KeyDown="OnTitleKeyDown" LostFocus="OnTitleLostFocus" />
```

```csharp
private void OnTitleKeyDown(object sender, KeyRoutedEventArgs e)
{
    if (e.Key == VirtualKey.Enter) { Save(); e.Handled = true; }
    else if (e.Key == VirtualKey.Escape) { TitleBox.Text = _saved; e.Handled = true; }
}
private void OnTitleLostFocus(object sender, RoutedEventArgs e) => Save();
```

## Details pane

Use for details of the selection while people keep working in the list. It
stays open across selections and shows a short empty state with nothing
selected. Layout rules: `windows-design`, app-shell.md.

```xml
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*" />
    <ColumnDefinition x:Name="DetailsColumn" Width="320" />
  </Grid.ColumnDefinitions>
  <!-- the list in column 0 -->
  <Border Grid.Column="1" BorderThickness="1,0,0,0" BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
          Padding="{StaticResource PageMargin}">
    <TextBlock Text="No selection" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
  </Border>
</Grid>
```

## Flyout

Use for a few related choices tied to one control. It closes when people
click outside it or press Escape, and changes apply at once.

```xml
<Button Content="Due date">
  <Button.Flyout>
    <Flyout Placement="Bottom">
      <StackPanel Spacing="{StaticResource SpaceSm}">
        <CalendarView x:Name="DueCalendar" SelectionMode="Single" />
        <Button Content="Clear due date" Click="OnClearDue" />
      </StackPanel>
    </Flyout>
  </Button.Flyout>
</Button>
```

A list of commands is a `MenuFlyout`, not a `Flyout`.

## TeachingTip

Use once, to point at a control people have not found yet. Save that it
was shown, and never show it again.

```xml
<Button x:Name="ScanButton" Content="Scan" />
<TeachingTip x:Name="ScanTip" Target="{x:Bind ScanButton}" Title="Start with a scan"
             Subtitle="Scan a drive to see what takes the most space." IsLightDismissEnabled="True" />
```

```csharp
public void ShowTipOnce(bool seen) { if (!seen) ScanTip.IsOpen = true; }
```

## ContentDialog

Use for one short task that must finish or cancel before going on. The title
names the task, the primary button names the action, and the box is
selected so typing replaces the name.

```csharp
public async Task<string?> AskNewNameAsync(string current)
{
    var box = new TextBox { Text = current };
    AutomationProperties.SetName(box, "Name");
    box.SelectAll();
    var dialog = new ContentDialog
    {
        XamlRoot = XamlRoot,
        Title = "Rename project",
        Content = box,
        PrimaryButtonText = "Rename",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Primary,
    };
    return await dialog.ShowAsync() == ContentDialogResult.Primary ? box.Text.Trim() : null;
}
```

- `XamlRoot` is required, and only one dialog can be open at a time.
- `AutomationProperties.Name` is an attached property: set it with
  `AutomationProperties.SetName(control, "...")` in code.

## Confirmation dialog

Only for rare actions that cannot be undone, or that remove a lot. The
title is a question with the verb, the button repeats the verb, and Cancel
is the default so Enter is safe. Code: `windows-design`, patterns.md,
"Confirm, then move to the Recycle Bin".

## New window

Use for a long or self-contained task people keep open beside the main
window, like a document or a log viewer.

```csharp
var window = new Window { Title = "Scan log" };
window.Content = new LogPage();
window.Activate();
```

A second window needs its own title bar setup and backdrop; copy them from
`MainWindow`. Most features belong on a page instead.

## Navigation page

Use for a place people come back to: a list, a collection, a report. Add a
case to `SidebarPage`, its row in `NavPages` (title, glyph, page class), and
the page. The navigation pane and snapshot mode pick it up from there.

## Status instead of a dialog

Information that needs no decision goes in an `InfoBar` at the top of the
page. It does not block the work, and people close it when they are done.

```xml
<InfoBar x:Name="Status" IsOpen="True" Severity="Warning" IsClosable="True"
         Title="12 folders could not be read" Message="They need administrator rights. Their sizes are left out." />
```

Severities: `Informational`, `Success`, `Warning`, `Error`. An InfoBar can
carry one action button, like Undo or Retry.

## Unsaved changes

Ask on close only when people would lose what they typed. Everything else
saves as it goes.

```csharp
public static void GuardClose(Window window, Func<bool> hasUnsavedText, Func<Task<bool>> askDiscard)
{
    window.AppWindow.Closing += async (AppWindow sender, AppWindowClosingEventArgs args) =>
    {
        if (!hasUnsavedText()) return;
        args.Cancel = true;
        if (await askDiscard()) { sender.Destroy(); }
    };
}
```

The dialog asks "Save changes to "Q3 report"?" with "Save", "Don't save",
and "Cancel".
