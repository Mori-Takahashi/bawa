---
name: windows-design
description: Design rules that make a WinUI 3 app look and feel like a native Windows 11 app, from Microsoft's five design principles to Mica, navigation, command bars, context menus, keyboard, type, color, and dark mode. Use when building or reviewing any window, page, command bar, dialog, flyout, or settings page, or when the user asks for a more native, Windows-like, or better designed app.
---

# Windows design

Windows already designed your app. Pick the right WinUI controls, arrange
them, and stay out of their way. Use the system first. Add custom design
only where the system has nothing.

- The full checklist with sources: [design-rules.md](design-rules.md).
  Use it for reviews.
- Copyable C# and XAML: [patterns.md](patterns.md).
- The Settings page: [settings.md](settings.md).
- Command bars, context menus, and shortcuts: [commands.md](commands.md).
- The window: title bar, navigation pane, page header, and an optional
  right pane: [app-shell.md](app-shell.md).
- Microsoft's current text on one topic, when a rule here is not enough:
  `powershell -NoProfile -ExecutionPolicy Bypass -File "${CLAUDE_SKILL_DIR}\scripts\design-doc.ps1" <topic>`
  (`typography`, `materials`, `commanding`, `titlebar`, or a path like
  `controls/buttons`). It prints the page as plain text.
- How a feature works and what it says (names, flow, dialogs, errors,
  accessibility): call the Skill tool with `ux-design`.
- Spacing, alignment, and the checks that catch misalignment: call the
  Skill tool with `layout-grid`.

## Microsoft's five principles

Use them as the review lens for every screen (Windows 11 design principles):

1. **Effortless**: fast and intuitive. It is easy to do what I want, with focus and precision.
2. **Calm**: soft and decluttered. The app fades into the background and keeps me focused.
3. **Personal**: it adapts to how I work: light or dark, accent color, text size, window size.
4. **Familiar**: standard controls, standard shortcuts, standard places. No learning curve.
5. **Complete and coherent**: every surface, state, and size looks like one app and like Windows.

Every screen answers: Where am I? What can I do? Where can I go?

## Before you write pages

Write these five under `## Design plan` in `docs/BRIEF.md`, or in
`docs/DESIGN.md` when the brief would pass one page:

1. **Archetype**: a window with a navigation pane, a single-page tool
   window, a tray icon with a flyout, or a quick panel opened by a global
   hotkey. Most personal apps are a navigation window plus one quick entry
   point.
2. **Information architecture**: navigation pages, at most two levels.
3. **Command map**: every action, where it shows (command bar, context
   menu, page), and its shortcut. It becomes `AppCommands`.
4. **Empty states**: what each page shows before there is data.
5. **Feeling**: the one feeling the app aims for (calm, quick, confident).
   Motion, words, and color all reinforce it.

## Layout

- `NavigationView` with `PaneDisplayMode="Left"`: pages at the top,
  Settings at the bottom (built in). No navigation pane when the app has one
  page.
- A page header: the page title (`PageTitleTextStyle`, 28 px semibold) on
  the left, the page's `CommandBar` on the right.
- Details of the selection in a right pane on wide windows (see
  [app-shell.md](app-shell.md)), or on a page of their own.
- The UI frames the user's content. Ask what the least UI is that lets it
  stand out.
- An empty page shows what it is for and its first action. Filters and
  sorting appear once there is content.
- Spacing on the 4 px grid: 24 px page margins, 16 px card padding, 8 px
  between related controls (`layout-grid`).
- The template's `PaneDisplayMode="Left"` keeps the pane open at every
  width. When the app must work in narrow windows (snapped to a third of
  the screen), use `Auto`: the pane shrinks to icons below 1008 px and to a
  menu button below 641 px. Pages wrap or stack at those widths.

## Materials

- **Mica** is the window background, behind the title bar and navigation
  pane. The template sets it. Content sits directly on it, or on cards
  (`CardStyle`).
- **Acrylic** belongs to transient surfaces: flyouts, menus, and tooltips
  get it from their own styles. Never put acrylic behind content.
- Never stack materials. No custom background behind the navigation pane or
  title bar.
- On Windows 10, Mica is missing and the window uses the plain theme
  background. Check that the app still reads well (the self-test checks the
  fallback exists).

## Commands and shortcuts

- Every action lives in `AppCommands` with a title, a Segoe Fluent Icons
  glyph, and a shortcut. The command bar and context menus are built from
  it. Full rules: [commands.md](commands.md).
- The page's `CommandBar` holds its actions, most important first: about
  four labeled primary buttons stay visible on narrow windows, the rest go
  to `SecondaryCommands` (the `...` overflow menu).
- At most one accent button per view, for the action that completes a task
  (Save, Send, Done). Most pages have none.
- Every row a user can act on has a context menu (right-click, Shift+F10,
  and the Menu key all open it).
- Standard shortcuts keep their meaning: Ctrl+N, O, S, Z, Y, C, X, V, A, F,
  W, F2 rename, F5 refresh, Delete, Escape.
- Disable commands that do not apply; keep them visible.
- The window title names the app. The page header names the page.

## Windows open calm

- No focus ring on open. The template gives the selected navigation item
  pointer focus (`MainWindow.FocusQuietly`), so arrow keys work at once and
  the focus ring appears only after Tab. The exception is a window whose
  only job is typing, like a search or quick-add box: it focuses its box.
- Dialogs, flyouts, and Settings sections size to their content. A page
  scrolls only when its content is taller than the window.
- System scroll bars. They hide until the pointer moves, as Windows sets
  them. Do not restyle them.

## Keyboard first

- Every frequent action has a shortcut, and every shortcut shows its result
  at once.
- Tab moves between areas, arrows move inside a list, Enter opens or runs
  the default action, Escape cancels or closes, Delete removes, F2 renames.
- Alt shows access keys (`AccessKey="N"`) on the command bar.
- New items are selected and in view, so the user can act on them at once.
- Tooltips name the shortcut: `New item (Ctrl+N)`. The template's
  `CommandUi.Bind` writes them.

## Color, type, dark mode

- Text uses the theme brushes: `TextFillColorPrimaryBrush`,
  `TextFillColorSecondaryBrush`, `TextFillColorTertiaryBrush`. Surfaces use
  `CardBackgroundFillColorDefaultBrush` and friends. No fixed white, black,
  or gray.
- One accent color. Status color always comes with an icon or a word.
- Segoe UI Variable, the default. The type ramp: Caption 12/16, Body 14/20,
  Body Strong 14/20 semibold, Subtitle 20/28 semibold, Title 28/36
  semibold. Use the XAML styles (`CaptionTextBlockStyle`,
  `BodyTextBlockStyle`, `SubtitleTextBlockStyle`, `TitleTextBlockStyle`).
  Semibold for emphasis, never Bold or Italic. Sentence case for all UI
  text, titles included.
- Lines of reading text are 20 to 60 characters wide.
- Dark mode is its own palette. Check both in every snapshot. High contrast
  themes replace your colors: use theme brushes and they follow.
- Accent-colored text uses `AccentTextFillColorPrimaryBrush`, which is
  readable in both modes. Text on an accent fill uses
  `TextOnAccentFillColorPrimaryBrush`.
- Icons: Segoe Fluent Icons through `FontIcon Glyph`. The command bar sizes
  them (20 px, 16 px in the overflow menu); in lists and menus use 16 px.

## Interaction

- Controls are 32 px tall (buttons, text boxes, combo boxes) from their
  own styles. Never set a fixed height.
- Hover and press states come with the controls. Custom controls get both.
- Drag and drop in and out where it fits. A drag out of a file list gives
  real files to Explorer ([patterns.md](patterns.md)).
- Relaunch brings back the window size and place, the page, and the
  selection. Closing never asks unless typed work would be lost.
- Undo for every destructive or bulk action. Deleting files moves them to
  the Recycle Bin, after a confirmation that names what and how much.
- `ContentDialog` only for a decision that blocks the work. Status goes in
  an `InfoBar`, a one-time hint in a `TeachingTip`.
- Respect "Animation effects" and "Transparency effects" in Windows
  settings: the built-in controls do. Custom motion checks
  `new Windows.UI.ViewManagement.UISettings().AnimationsEnabled`.
- Every icon-only button has a tooltip and `AutomationProperties.Name`.

## When to customize

Custom UI is fine when it makes the app clearly better at its one job, like
a size treemap in a disk tool or a calendar grid. Write why in a code
comment, and match the system: 4 px corners on controls and cards, 8 px on
dialogs and flyouts, theme brushes, the type ramp, the spacing scale.

## Review set

Snapshot every surface in light and dark: each page, each Settings section,
each dialog, flyout, and panel. Look for focus rings, scroll bars with
nothing to scroll, empty bands under short content, text that clips, and
caption buttons that vanish in one mode. Capture with the window active: a
snapshot of an inactive window shows dimmed Mica and does not count as
reviewed.

Done when every rule in [design-rules.md](design-rules.md) holds on the
light and dark snapshots of every surface, or has a comment saying why not.
