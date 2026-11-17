# App shell

The frame around the pages: the title bar, the navigation pane, the page
header, and an optional details pane on the right. Get these to fit
together once; every page then sits inside them.

## Contents

- Title bar
- Navigation pane
- Page header
- Details pane on the right
- Narrow windows
- Traps
- Review

## Title bar

- The template extends the content into the title bar
  (`ExtendsContentIntoTitleBar = true`, `SetTitleBar(TitleBarArea)`): a 48 px
  row with the 16 px app icon and the app name in `CaptionTextBlockStyle`,
  16 px from the left edge, over Mica.
- The caption buttons (minimize, maximize, close) are drawn by Windows. The
  template sets their colors from the app theme (`MainWindow.SetThemeOverride`),
  because they otherwise follow the Windows theme and vanish when the app
  is light and Windows is dark.
- Keep the title bar free of buttons. The whole row is the drag area. Search
  and actions go in the page header.
- Read the current guidance: `design-doc.ps1 titlebar`.

## Navigation pane

- `NavigationView` on the left: one item per page, a Segoe Fluent Icons
  glyph and a one-word or two-word label. Settings is the built-in item at
  the bottom.
- The pane background is Mica, unchanged. The page area is the raised
  layer that `NavigationView` draws with its own rounded top-left corner.
- Selection changes the page at once. Never ask before leaving a page.
- Hierarchy: at most one level of child items (`MenuItems` inside an item).
  More levels belong inside the page.
- Counts ("Inbox 12") go in an `InfoBadge` on the item, not in the label.

## Page header

- Page title in `PageTitleTextStyle` at 24 px from the left and top of the
  page area (`PageHeaderMargin`), the page's `CommandBar` on the right of
  the same row.
- A search box, when the page has one, sits in the header between title and
  command bar, or above the list on narrow windows.
- The content starts 12 px under the header row. Lists line their text up
  with the title (`layout-grid`, rule 5).

## Details pane on the right

For the selected item's details on wide windows:

- A second column in the page `Grid`, 320 to 360 px wide, with a 1 px
  left border in `CardStrokeColorDefaultBrush` and the page background.
  No extra material.
- The pane shows the selection's details and actions. With no selection it
  shows a short empty state, not a blank column.
- Under about 1000 px of window width, hide the pane and open details on a
  page of their own (a `Frame` navigation with a back button).
- The pane's content uses the same 24 px margins and the same type ramp as
  the page.

## Narrow windows

Windows snaps apps to halves and thirds, so a window can be 500 px wide.

- `PaneDisplayMode="Auto"` lets the navigation pane shrink to icons below
  1008 px and to a menu button below 641 px. The template uses `Left`; switch
  when the app is used snapped.
- Command bars move overflowing buttons into the `...` menu by themselves.
- Lists keep one column. Cards stack.
- Snapshot a narrow size before release: resize in a snapshot pass.
  `AppWindow.Resize` takes physical pixels, so multiply by the display
  scaling like the template's `MainWindow` does:
  `(int)(640 * GetDpiForWindow(Hwnd) / 96.0)`.

## Traps

- A custom background on the navigation pane or title bar hides Mica and
  breaks the active and inactive window look.
- A control in the title bar row without a passthrough region cannot be
  clicked: the row is the drag area.
- A page with its own scroll viewer inside a `ScrollViewer` scrolls twice.
  Let the list scroll; keep the header fixed.
- Fixed widths on the page content clip on narrow windows. Use `MaxWidth`.

## Review

Snapshots of every page in light and dark show: the icon and app name in
the title bar, visible caption buttons, the selected page highlighted in
the pane, the header title on the same left edge as the content, and no
custom background behind the pane.
