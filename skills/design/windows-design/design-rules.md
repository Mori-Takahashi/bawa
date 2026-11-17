# Design rules with sources

The checklist for reviews. Check each rule on the light and dark snapshots
of every surface. A rule that does not hold gets a fix or a code comment
saying why not. Sources are at the end; `design-doc.ps1 <topic>` prints the
current text.

## Structure

1. One window with a `NavigationView`, or one page when the app has one job. (navigation)
2. At most two levels of navigation. Settings is the built-in item at the bottom of the pane. (navigation)
3. Every page has a header: the page name in `PageTitleTextStyle`, actions on the right. (layout)
4. An empty page says what it is for and offers its first action. (usability)
5. The window title is the app's name, shown in the custom title bar with the app icon. (titlebar)

## Materials

6. Mica only as the window background, under the title bar and navigation pane. (materials)
7. No acrylic behind content. Flyouts, menus, and tooltips bring their own. (materials)
8. No material on top of another material, and no custom background behind the navigation pane. (materials)
9. On Windows 10 the plain background still reads well. (materials)

## Commands

10. Every action is in `AppCommands`, with a title, a glyph, and a shortcut. (commanding)
11. A page's command bar lists actions by importance; about four stay visible on narrow windows, the rest go to the overflow menu. (commanding)
12. At most one accent button per view, for the action that completes a task. (buttons)
13. Rows a user can act on have a context menu; it opens with right-click, Shift+F10, and the Menu key. (commanding)
14. Commands that do not apply are disabled, not hidden, in the command bar. Context menus leave them out. (commanding)
15. Standard shortcuts keep their Windows meaning (commands.md). (keyboard)

## Keyboard and focus

16. No focus ring on any window, page, dialog, or flyout when it opens. (focus)
17. Tab reaches every control in reading order. Arrows move inside lists. (keyboard)
18. Enter runs the default action, Escape cancels or closes, Delete removes, F2 renames. (keyboard)
19. Tooltips on command buttons name the shortcut. (commanding)
20. Alt shows access keys on the main commands. (keyboard)

## Type and color

21. Segoe UI Variable and the type ramp: Caption 12/16, Body 14/20, Subtitle 20/28, Title 28/36. (typography)
22. Semibold for emphasis and titles; no Bold, no Italic. (typography)
23. Sentence case for all UI text. (typography)
24. Text and surfaces use theme brushes; no fixed white, black, or gray. (color)
25. One accent color. Status color comes with an icon or a word. (color)
26. Accent text uses `AccentTextFillColorPrimaryBrush`; text on accent fills uses `TextOnAccentFillColorPrimaryBrush`. (color)
27. Icons are Segoe Fluent Icons: 20 px in the command bar (automatic), 16 px in menus and lists. (iconography)
28. Light, dark, and high contrast all read well. (color, accessibility)

## Shape and spacing

29. Controls and cards have 4 px corners; windows, dialogs, and flyouts 8 px. (geometry)
30. Spacing is on the 4 px grid: `tools\grid-lint.ps1` and `check-layout.cs` pass. (layout)
31. Controls take their height from their style (32 px standard). (layout)
32. Long text truncates with a tooltip showing all of it. Nothing clips. (typography)

## Interaction

33. Destructive actions have Undo, or a confirmation that names what and how much. Files go to the Recycle Bin. (usability)
34. `ContentDialog` only for decisions that block the work. Status uses `InfoBar`. (dialogs)
35. Long work shows progress in place and can be canceled. The window never freezes. (usability)
36. Relaunch restores the window size and place, the page, and the selection. (usability)
37. Closing the window never asks, unless typed work would be lost. (usability)
38. Custom motion respects the "Animation effects" setting. (motion)

## Accessibility

39. Every icon-only control has `AutomationProperties.Name` and a tooltip. (accessibility)
40. Text scales with Windows' text size setting without clipping (try 150% in Settings > Accessibility > Text size). (accessibility)
41. Contrast is at least 4.5:1 for text, which the theme brushes give. (accessibility)

## Sources

Microsoft Learn, "Design Windows apps"
(learn.microsoft.com/windows/apps/design/). Print one with
`scripts\design-doc.ps1 <topic>`:

| Tag | Topic for design-doc.ps1 |
| --- | --- |
| navigation | `navigation` |
| layout | `layout` |
| titlebar | `titlebar` |
| materials | `materials` |
| commanding | `commanding` |
| typography | `typography` |
| color | `color` |
| iconography | `icons` |
| motion | `motion` |
| usability | `usability` |
| accessibility | `accessibility` |
| keyboard | `input/keyboard-interactions` |
| focus | `input/guidelines-for-visualfeedback` |
| buttons | `controls/buttons` |
| dialogs | `controls/dialogs-and-flyouts/dialogs` |
| geometry | `signature-experiences/geometry` |
