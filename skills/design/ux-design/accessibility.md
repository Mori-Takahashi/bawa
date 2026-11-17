# Accessibility audit

How to check step 7 of [SKILL.md](SKILL.md) on a running build, and the
WinUI properties that fix what it finds. Standard controls do most of this
on their own. Custom controls need the work.

## Contents

- Run the audit
- Keyboard
- Narrator
- Custom controls
- Color and contrast
- Motion and transparency
- Text size

## Run the audit

1. Build and open the app:
   `powershell -NoProfile -ExecutionPolicy Bypass -File build.ps1 -Open`.
2. Run Accessibility Insights for Windows
   (`winget install Microsoft.AccessibilityInsights`). Pick the app window
   and run FastPass on each new view.
3. Run the whole flow with the keyboard only: Tab, Shift+Tab, arrows,
   Enter, Space, Escape, and the shortcuts.
4. Turn on Narrator with Ctrl+Win+Enter. Move with Tab and the arrow keys,
   and with Caps Lock+arrows for Narrator's own navigation. Run the flow
   again, listening.
5. In Settings > Accessibility, turn on a contrast theme (Left Alt+Left
   Shift+Print Screen), set Text size to 150%, and turn off Animation
   effects and Transparency effects, one at a time. Look at each new view.

Done when FastPass shows no failures for the new views and both runs
finish the flow without the mouse.

## Keyboard

- Every step is reachable with the keyboard, in reading order. Set
  `TabIndex` only when the visual order and the tree order differ.
- The default button responds to Enter, Cancel to Escape (`ContentDialog`
  does both).
- A new item is selected and scrolled into view (`ListView.ScrollIntoView`).
- Lists move with the arrow keys; a whole list is one Tab stop.
- `AccessKey="N"` gives command bar buttons a key tip under Alt.

## Narrator

Narrator reads the UI Automation name, role, and state.

- Every icon-only control has a name:
  `AutomationProperties.Name="Delete"`, the same word as its tooltip's
  first words.
- A value shown only as color or an icon gets text Narrator can read:
  `AutomationProperties.Name="Starred"` on the star icon.
- Help beyond the name goes in `AutomationProperties.HelpText`.
- Headings: `AutomationProperties.HeadingLevel="Level1"` on the page title,
  so Narrator can jump by heading.
- A changing status (a scan count, a save result) is announced:
  `AutomationProperties.LiveSetting="Polite"` on the text that changes.
- Decorative images: `AutomationProperties.AccessibilityView="Raw"`.

## Custom controls

A control you draw yourself (a treemap, a chart, a custom slider) needs:

- **Name**: what it is, from `AutomationProperties.Name`.
- **Value**: its current value as text, like "Downloads, 12.4 GB, 31% of
  the drive".
- **Actions**: keyboard focus (`IsTabStop="True"`), Enter or Space to act,
  arrows to move between its parts.
- **Feedback**: a visible focus rectangle (`UseSystemFocusVisuals="True"`)
  and a change Narrator announces.

For a control with parts (a treemap of folders), give each part its own
focusable element with a name, or write an `AutomationPeer`
(`OnCreateAutomationPeer`) that exposes the parts as children. Then check
it with Accessibility Insights' Inspect mode.

## Color and contrast

- Text uses theme brushes, which meet 4.5:1 in light, dark, and contrast
  themes.
- Meaning never rests on color alone: an icon or a word goes with it.
- In a contrast theme, Windows replaces the theme brushes with the user's
  colors. Fixed colors stay and often become unreadable: never use them for
  text.
- Charts and treemaps use the accent plus neutral fills, with labels or a
  legend in text.

## Motion and transparency

- Built-in controls follow "Animation effects" and "Transparency effects"
  on their own.
- Custom motion checks `new Windows.UI.ViewManagement.UISettings().AnimationsEnabled`
  and shows the end state at once when it is off.
- Mica turns into a solid color when transparency is off. The app must look
  right in both.

## Text size

- Windows' Text size setting (Settings > Accessibility > Text size) scales
  text up to 225%. Controls grow with it.
- No fixed heights on anything that holds text. Let rows grow; wrap or
  truncate with a tooltip.
- Check the main pages at 150%: nothing clips or overlaps.
