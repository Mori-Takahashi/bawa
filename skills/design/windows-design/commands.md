# Commands, menus, and shortcuts

Windows apps rarely have a menu bar. Their commands live in command bars, in
context menus, and on the keyboard. One list, `AppCommands` in
`Navigation.cs`, feeds all three.

## Contents

- One list of commands
- The page command bar
- Context menus
- Shortcuts
- Access keys
- Check

## One list of commands

```csharp
public static readonly AppCommand NewItem = new("new", "New item", "", VirtualKey.N, VirtualKeyModifiers.Control);
```

Each command has an id, a title in sentence case, a Segoe Fluent Icons
glyph, and a shortcut. `CommandUi.Bind(button, command)` gives an
`AppBarButton` its label, icon, accelerator, and tooltip ("New item
(Ctrl+N)"). `CommandUi.MenuItem(command, action)` builds the context menu
item with the shortcut shown as text. Add every new action to
`AppCommands.All`; the self-test checks that each has a shortcut and that
none is used twice.

Find glyph codes in the Segoe Fluent Icons list
(`design-doc.ps1 style/segoe-fluent-icons-font`). Common ones: Add `E710`,
Delete `E74D`, Edit `E70F`, Save `E74E`, Search `E721`, Settings `E713`,
Refresh `E72C`, Favorite star `E734` (filled `E735`), Folder `E8B7`, Share `E72D`, More `E712`.

## The page command bar

- One `CommandBar` per page, in the header on the right,
  `DefaultLabelPosition="Right"`, transparent background.
- Most important first. About four labeled buttons stay visible on narrow
  windows. The rest go to `SecondaryCommands` (the `...` overflow menu).
- The command bar acts on the page or its selection. Buttons that need a
  selection are disabled without one (`CanExecute`).
- Window-wide commands (Settings) are not in a page's command bar.

## Context menus

- Every row a user can act on has one. It opens with right-click,
  Shift+F10, and the Menu key; the template handles all three in
  `ContextRequested`.
- Right-click selects the row first, so the menu acts on what the user
  sees selected.
- Leave out items that do not apply (unlike the command bar, which
  disables them). Group with separators; the destructive item comes last.
- Show the shortcut as text, not as a second accelerator.
- Text boxes keep the system menu (cut, copy, paste). Do not replace it.

## Shortcuts

Standard Windows shortcuts keep their meaning:

| Keys | Action |
| --- | --- |
| Ctrl+N | New |
| Ctrl+O | Open |
| Ctrl+S | Save |
| Ctrl+Z, Ctrl+Y | Undo, Redo |
| Ctrl+C, Ctrl+X, Ctrl+V | Copy, Cut, Paste |
| Ctrl+A | Select all |
| Ctrl+F | Find (focus the search box) |
| Ctrl+W | Close the tab or document |
| F2 | Rename |
| F5 | Refresh |
| Delete | Delete the selection |
| Escape | Cancel, close a dialog or flyout, clear a search |
| Alt+F4 | Close the window (the system handles it) |

- Never use a single letter, Delete, Enter, or arrows as a window-wide
  shortcut while a text box can have focus: typing would trigger it. The
  template's Delete accelerator belongs to the list page's command bar,
  which has no text box.
- Ctrl+, opens Settings in the template. It is common in modern apps, not
  a Windows standard; show it in the tooltip.
- An app-specific shortcut uses Ctrl or Ctrl+Shift with a letter that
  matches the command's name.

## Access keys

Pressing Alt shows key tips on elements that have `AccessKey`. Give the
page's main command bar buttons one letter each:

```xml
<AppBarButton x:Name="NewButton" AccessKey="N" />
```

## Check

- The self-test passes "every command has a shortcut" and "no shortcut is
  used twice".
- `ui-check.cs` (`test-windows-app`) presses the shortcuts of the commands
  a story adds.
- A snapshot with a row selected shows the command bar buttons enabled.
