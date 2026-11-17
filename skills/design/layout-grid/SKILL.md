---
name: layout-grid
description: Grid and alignment rules for a WinUI 3 Windows app, with two checks that catch misalignment (a lint for off-grid numbers and a layout probe that measures real frames in snapshot mode). Use when building or reviewing a custom page, card, row, form, or dialog, after every build that touched layout, or when the user says something looks misaligned, crooked, cramped, or unevenly spaced.
---

# Layout grid

Agents guess numbers. A guessed `Margin="13"` looks almost right and makes
the whole app feel slightly off. This skill replaces guessing with one
scale and two checks that fail when something is off.

The template has everything in place: `UI/Theme.xaml`, `UI/Grid.cs`, and
`tools\`. For an app made before this skill, copy those files and
`src\<App>.Kit\Layout\` from the `new-windows-app` skill's template, and
the layout file line from its `Snapshot.cs`.

Below, `ps1` stands for `powershell -NoProfile -ExecutionPolicy Bypass -File`.

## The rules

1. Every margin, padding, spacing, and size comes from the scale, in
   effective pixels: `Xs 4`, `Sm 8`, `Md 12`, `Lg 16`, `Xl 20`, `Xxl 24`,
   `Section 32`. In XAML the keys are `SpaceSm`, `SpaceLg`, and so on, plus
   `PageMargin`, `PageHeaderMargin`, `CardPadding`, `RowPadding`. In C#,
   `Space.Lg`. Raw numbers are 0, 1, 2, or a multiple of 4.
2. Pages have a 24 px margin (`PageMargin`). Cards pad 16 (`CardPadding`).
   Related controls sit 8 apart, groups 16 to 24, sections 32. Cards in a
   stack sit 4 apart, like the Windows Settings app.
3. Each column has one leading edge. Titles, text, cards, and buttons in a
   column start on the same x. Indent on purpose, by a step of the scale.
4. Let containers do the aligning:
   - Rows and columns: `Grid` with `RowSpacing` and `ColumnSpacing`.
   - A stack: `StackPanel` with `Spacing`.
   - A label with a control on the right: a card `Grid` with a `*` column
     and an `Auto` column (`SettingsPage.xaml`).
   - Numbers in a column: `TextAlignment="Right"` in a fixed-width column,
     so units and digits line up.
   - An icon with text: a horizontal `StackPanel` with `Spacing="{StaticResource SpaceSm}"`.
5. Know the controls' own insets. A `ListViewItem` pads its content 16 px
   on the left, so a list under a title at 24 px gets `Padding="8,..."`.
   A `SelectorBar` is 47 px tall: give it `MinHeight="48"`.
6. Text that must line up with other views uses a style with a fixed line
   height (`PageTitleTextStyle`, `RowTextStyle`: `LineHeight` plus
   `LineStackingStrategy="BlockLineHeight"`). Without it a text box is a
   fraction of a pixel tall, like 37.24, and every gap below it is off.
7. Dialogs are a `ContentDialog`, which keeps itself between 320 and 548 px
   wide. Controls take their height from their style, never from a fixed
   `Height`.
8. Nested corners are concentric: inner radius = outer radius - padding.
9. `Canvas.Left`, `Canvas.Top`, and `TranslateTransform` move a view after
   layout, so nothing lines up with it. Use them only for motion and drag
   previews, with `<!-- grid: ok <why> -->` on the line.
10. Long text truncates (`TextTrimming="CharacterEllipsis"`) with its full
    value in a tooltip. Nothing clips.

## Check 1: lint the code

Run it after every build that touched views:

```powershell
ps1 tools\grid-lint.ps1
# ...\Pages\TaskRow.xaml:14: off grid (10): <Border Padding="10,8" />
# FAIL grid-lint: 1 lines off the grid
```

It reads `Margin`, `Padding`, `Spacing`, `RowSpacing`, `ColumnSpacing`,
widths, heights, and `CornerRadius` in XAML, and the same properties plus
`new Thickness(...)` and `new CornerRadius(...)` in C#. Fix each line with
a token. The token files (`Theme.xaml`, `Grid.cs`, a design kit's
`Tokens.xaml`) are skipped.

## Check 2: probe the real layout

Mark what you lay out yourself: the page, its title, cards, the first row
of a list, dialog content. The navigation pane, command bar, and title bar
align themselves and need no probes. Your own content beside them does.

A check only sees what has probes. A page with two probes passes while
everything else is off, so read the count in the PASS line.

```xml
<Grid ui:Layout.Area="page">
  <TextBlock Text="Today" Style="{StaticResource PageTitleTextStyle}"
             ui:Layout.Probe="title" ui:Layout.ProbeArea="page" />
  <Border Style="{StaticResource CardStyle}"
          ui:Layout.Area="card" ui:Layout.Inset="16"
          ui:Layout.Probe="card" ui:Layout.ProbeArea="page" />
</Grid>
```

(`xmlns:ui="using:<App>.UI"` on the page.) `Layout.Area` marks a container,
with `Layout.Inset` (default 24). `Layout.Probe` marks a view in the area
named by `Layout.ProbeArea`. Probes with the same `Layout.Edge` (default
`lead`) must share one leading edge. `Layout.Edge=""` for views that sit
side by side. In a list template, only the first row's probe counts.

Snapshot mode writes a `<page>-<mode>-layout.json` next to every PNG.
Check them:

```powershell
ps1 tools\snap.ps1
dotnet run tools\check-layout.cs -- "$env:TEMP\<App>-shots\*-layout.json"
# PASS all-light-layout.json: 2 probes in 1 areas
# FAIL starred-light-layout.json
#   - edge page/lead has 2 leading edges: title 345, row 349
```

It checks that every probe:

1. Starts on a whole or half pixel, so its edges stay sharp.
2. Starts a multiple of 4 px from its area's edge.
3. Stays inside its area's inset on both sides.
4. Shares one leading edge with the others on its edge.
5. Sits a multiple of 4 px below the probe above it.
6. Never overlaps another probe in its area.

`--unit 8` checks against another step. A design kit measured from another
app passes that app's step, or `0.5` to check only edges, insets, and
overlaps.

### Line up across areas

Rules 2 to 6 stay inside one area. Views in different areas, like a list in
a scroll viewer and a toolbar outside it, need an edge that starts with `=`:

```xml
<ListView ui:Layout.Probe="list" ui:Layout.Edge="=content&lt;" />
<CommandBar ui:Layout.Probe="actions" ui:Layout.Edge="=content&lt;" />
```

| Edge | Must match |
| --- | --- |
| `=name` | left and right |
| `=name<` | left |
| `=name>` | right |
| `=name\|` | center |
| `=name_` | bottom |

These probes skip rules 2 to 6, so a view off the 4 px grid can still match
a system edge. In XAML, write `<` as `&lt;`: a raw `<` inside an attribute
breaks the file (`WMC9997`).

## Fix a failure

| Message | Usual cause | Fix |
| --- | --- | --- |
| starts 28 px into page | a control's own inset on top of your padding | subtract it from your padding (rule 5) |
| 2 leading edges | one child with extra margin, or a centered panel | `HorizontalAlignment="Stretch"`, remove the extra margin |
| gap 28.76 px | a text box with a fractional height | a text style with a fixed line height (rule 6) |
| gap 10 px | `Spacing="10"`, or a margin on one child | a scale token |
| inside its 24 px inset | a missing page margin, or a fixed width wider than the parent | `PageMargin`, `MaxWidth` instead of `Width` |
| overlap | a `TranslateTransform`, a negative margin, or a fixed height | remove it, let the panel size the view |
| x between pixels | a fractional number, or a centered view of odd width | a whole number from the scale |
| `=name` has different left edges | a scroll bar that takes width, or a pane's inset on one side | line both up on the pane's content edge |

Done when `tools\grid-lint.ps1` passes, `tools\check-layout.cs` passes on
the layout files of every page you touched, and every custom page and card
has a `Layout.Area` with probes on its main parts.
