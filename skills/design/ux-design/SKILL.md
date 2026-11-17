---
name: ux-design
description: UX pass for one feature of a Windows app, covering names, flow, where it opens (inline, flyout, dialog, pane, window, page), states, words, tooltips, accessibility, and AI features. Use before coding a story that adds a screen, an action, or text, when user-stories or improve-loop call it, or when the user asks for a UX review, clearer wording, better names, or whether something should be a dialog.
argument-hint: "[story ID or screen]"
---

# UX design

A feature that works can still confuse people. This pass decides what
things are called, how a person moves through the feature, where it opens,
and what it says, before any code exists.

Run it once per story that adds a screen, an action, or words. The output
is a `UX:` block in the story in `docs/STORIES.md`, plus new terms in
`docs/GLOSSARY.md`. To review a built screen, run the same steps against
its snapshot and list what fails, most serious first.

## 0. Read

Read `docs/BRIEF.md`, `docs/GLOSSARY.md` (create it if missing), the story,
and its reference screenshots.

## 1. Name it

Every thing in the app has exactly one name, and every name lives in the
glossary.

```markdown
| Term | Means | Not |
| --- | --- | --- |
| Task | One thing to do, with an optional due date | to-do, item, entry |
| Complete | Mark a task done; it leaves the list | check, finish, close |
```

- Name the things first (the nouns in the brief), then the actions on them.
- Use the user's words, not the code's: "Task", not "TaskEntity" or "record".
- Use Windows' words for system actions: Settings, Close, Undo, Move to the
  Recycle Bin (when it can come back), Delete (when it cannot), Open in
  File Explorer, Uninstall.
- One word per idea across buttons, menus, titles, tooltips, and errors.
- Name a feature for what it does for the person, not the technology. A
  toggle gets a verb phrase that says what and for whom ("Show hidden
  files", not "Enumerate system attributes"). Say the name in a sentence
  out loud. Method: [writing.md](writing.md#names).

Check: every noun and verb the story puts on screen is in the glossary,
and no glossary term has a second name anywhere.

## 2. Map the flow

Write the flow as one line, from every way in to a clear end.

```text
Ctrl+N, command bar "New task", context menu -> title box has focus -> Enter saves, row appears -> focus stays for the next task
```

- List every entry point: shortcut, command bar, context menu, drag and
  drop, the tray icon or a global hotkey when the app has one.
- Cut steps: fill in good defaults, ask only for what you cannot guess.
- Every step has a way back: Escape cancels, Ctrl+Z undoes.
- The end is visible: the new row, a checkmark, an InfoBar, the dialog
  closing.

Check: the flow fits on one line, and each step names its way back.

## 3. Pick the surface

Go down this list and use the first one that fits. Lighter is better.

| The feature | Use |
| --- | --- |
| Changes one value on something already on screen | Inline (box, toggle, combo box in the row) |
| Shows details of the selection while people keep working | Details pane on the right |
| A few related choices tied to one control | Flyout |
| A one-time hint about a control | TeachingTip |
| A tool people keep open beside the work | A pane in the page, or a second window |
| One short task that must finish or cancel before going on | ContentDialog |
| A place people come back to, like a list or a collection | Navigation page |
| A rare action that destroys data and cannot be undone | Confirmation ContentDialog |
| Status, a result, or a problem people can act on later | InfoBar |

- Show one dialog at a time (WinUI allows only one `ContentDialog` open).
  Never open a flyout from a dialog's button that opens another dialog.
- A dialog's title names its task: "Rename project", not "Edit".
- Enter confirms the default button, Escape cancels, and the confirm button
  names the action. For destructive actions the safe button is the default.
- Ask before closing only when people would lose what they typed.
- Information that needs no action goes inline or in an InfoBar, never in a
  dialog.

Details and WinUI code for each surface: [presentation.md](presentation.md).

Check: the `UX:` block names the surface and the table row that picked it.

## 4. Cover the states

Every view the story touches has words and a look for each state:

- **Empty**: what it is for, plus the first action as a button.
- **Loading**: only when it can take longer than half a second. Show
  cached content first. A `ProgressBar` when the length is known, with
  words for what is happening ("Scanning 12,408 folders"), and Cancel when
  it is safe. A `ProgressRing` only for short waits of unknown length.
- **Error**: inline, next to the problem, with the fix. An InfoBar with
  `Severity="Error"` for errors that are not tied to one control.
- **Too much**: long names truncate with the full text in a tooltip; a
  hundred thousand rows still scroll smoothly (virtualized lists).
- **Done**: visible feedback at once, and Undo where it applies.

Check: each state has a snapshot page or seed data that shows it.

## 5. Write the words

- Sentence case everywhere: buttons, menu items, titles, labels, and
  messages. "Add task", not "Add Task". This is the Windows rule.
- Buttons and menu items start with a verb: "Add task", "Export...".
- Add an ellipsis when the command asks for more input before it acts.
- Errors say what happened and how to fix it, without blame. Write
  "Can't load tasks", never "We're having trouble".
- Confirmations repeat the action: "Delete 3 tasks?" with buttons "Delete"
  and "Cancel", never "Yes" and "No".

Patterns and before and after examples: [writing.md](writing.md).

A feature that uses AI (a summary, generated text, a model's suggestion)
also follows [ai-features.md](ai-features.md).

Check: read every string aloud. Each one is short, uses glossary terms
only, and a new user could say what each button does.

## 6. Add tooltips

- Every icon-only button gets a tooltip and an `AutomationProperties.Name`:
  what it does, starting with a verb, then its shortcut, "Add a task to
  this list (Ctrl+N)". Not its name again. Rules:
  [writing.md](writing.md#tooltips).
- Hover can reveal secondary actions on a row. The same actions are also in
  the context menu and on the keyboard.
- Hover feedback shows at once and never shifts the layout.
- Information people need is always visible. Hover only adds detail.

Check: hover every icon-only control on the built app. Each one shows a
tooltip.

## 7. Check accessibility

- The whole flow works with the keyboard alone, in a logical tab order.
- Narrator reads every control with the same words as the screen.
- Meaning never rests on color alone: add an icon or a word.
- Text is 12 px or larger, scales with Windows' text size setting, and
  reads well in a high contrast theme.
- With "Animation effects" off, slides and bounces become fades or nothing.
- Every custom control has a name, a value, actions, and feedback for UI
  Automation.

The audit steps and WinUI properties: [accessibility.md](accessibility.md).

Check: you ran the flow keyboard-only and with Narrator, and the
Accessibility Insights FastPass shows no issues for the new views.

## 8. Write the UX block

Add the result under the story's acceptance lines:

```markdown
UX:
- Flow: Ctrl+N, command bar "New task", context menu -> title box -> Enter saves -> focus stays
- Surface: inline row at the top of the list (changes one value on screen)
- States: empty "No tasks yet" with "New task"; error "Give the task a title." under the box
- Words: "New task", tooltip "Add a task to this list (Ctrl+N)", InfoBar "Task deleted [Undo]"
- Access: Enter saves, Escape cancels; Narrator "New task title"
```

Done when the story has a `UX:` block, every term it uses is in
`docs/GLOSSARY.md`, and steps 1 to 7 pass on its snapshots.
