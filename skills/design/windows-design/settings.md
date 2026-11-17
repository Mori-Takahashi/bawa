# Settings page

Windows apps keep settings on a page, opened from the gear at the bottom of
the navigation pane (or Ctrl+, in the template). It looks like the Windows
Settings app: a title, sections, and one card per setting.

## Contents

- Structure
- Sections
- Cards
- Size and scrolling
- Focus
- Check

## Structure

- `SettingsPage.xaml`: the page title "Settings", a `SelectorBar` with one
  item per section, and one panel per section in a `ScrollViewer`.
- `SettingsSection` in `Navigation.cs` lists the sections. Snapshot mode
  captures each one, so a new section is a new enum case plus a panel.
- Settings apply at once. No Save or Apply button. A setting that needs a
  restart says so in its description.
- Settings are saved per user next to the database
  (`<store>.settings.json`, written by `ThemeStore`). Add new settings to
  the same file; give each a default.

## Sections

- **Appearance** first in most apps: App theme (Use system setting,
  Light, Dark) and the accent color.
- One section per topic, named with a noun: General, Appearance,
  Shortcuts, Storage, About.
- **About** last: app name, version, and links (help, licenses).
- Two to five sections. One section with two settings needs no
  `SelectorBar`.

## Cards

Each setting is a card (`CardStyle`): the title and a one-line description
on the left, the control on the right.

```xml
<Border Style="{StaticResource CardStyle}">
  <Grid ColumnSpacing="{StaticResource SpaceLg}">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*" />
      <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>
    <StackPanel VerticalAlignment="Center">
      <TextBlock Text="App theme" />
      <TextBlock Text="Light, dark, or the same as Windows" Style="{StaticResource CaptionTextBlockStyle}"
                 Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
    </StackPanel>
    <ComboBox Grid.Column="1" VerticalAlignment="Center" AutomationProperties.Name="App theme" />
  </Grid>
</Border>
```

| Setting | Control |
| --- | --- |
| On or off | `ToggleSwitch` (no On/Off label text of your own) |
| One of a few named choices | `ComboBox` |
| A number in a range | `NumberBox` with `SpinButtonPlacementMode="Compact"`, or a `Slider` with a value label |
| A folder | The path as text plus a "Browse" button (patterns.md, "Pick a folder") |
| A shortcut | A button that shows the keys and records new ones |
| A destructive action (reset, clear data) | A button in its own card, with a confirmation dialog |

- Cards in a section sit 4 px apart (`StackPanel Spacing="{StaticResource SpaceXs}"`).
- Group related cards under a subtitle (`BodyStrongTextBlockStyle`) with 32
  px above it.
- Titles in sentence case, descriptions as one short sentence without a
  period.
- Every control has `AutomationProperties.Name` equal to its card title.

## Size and scrolling

- The content keeps a maximum width (`SettingsMaxWidth`, 1000 px), so cards
  do not stretch across a wide screen.
- The page scrolls only when the content is taller than the window.

## Focus

- Opening Settings moves no focus ring onto a control. The selected
  navigation item keeps focus.
- Undo applies here too: "Reset to defaults" is undoable or confirmed.

## Check

`tools\snap.ps1 -Pages settings` captures every section in light and dark.
Each card's control sits at the right edge, the titles share one left edge
(the layout probes `theme-card` and `accent-card` check it), and no section
scrolls with nothing to scroll.
