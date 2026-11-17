# Writing patterns

Patterns for the words in a Windows app. Each one has a rule and a before
and after. Most rules follow the Microsoft Writing Style Guide and the
Windows design guidance on typography and dialogs.

## Contents

- Voice
- Names
- Buttons and menu items
- Titles
- Empty states
- Errors
- Dialogs
- Confirmations
- Undo
- Settings
- Fields and placeholders
- Permissions and administrator rights
- Progress
- Tooltips
- Counts and plurals
- Words to cut

## Voice

Pick three words for the app's voice and keep them in `docs/GLOSSARY.md`
under `## Voice`, for example "calm, direct, brief". Tone changes with the
moment: plain and serious for errors, a little warmer for a finished goal.

- Talk to people as "you". Never "the user".
- Say "select" for buttons and items. It fits mouse, touch, and keyboard.
- Use contractions: "can't", "don't", "it's". They read as friendly and
  plain.
- No idioms or jokes that do not translate. Explain a technical term the
  first time, or drop it.

## Names

A good name fits the app's other words, sets the right expectation, and
works in every language.

- Name a feature by what it does for the person, not by the technology:
  "Find large files", not "Size index".
- A toggle gets a phrase that answers "what" and "for whom": "Show hidden
  files".
- Avoid clever names that raise questions. A label must not sound like a
  judgment when its value is low or zero ("Wasted space: 0 bytes").
- Where trust matters (money, health, privacy, deleting files), use the
  plain word ("Recycle Bin", not "Trash can").
- Reuse words already in the app and in Windows. A new name for an old
  idea is a bug.
- Test each name in sentences: "Turn on Show hidden files", "Search for it
  in Recent". If the sentence sounds odd, the name is wrong.

To find a name: write who it is for, then three lists of words, what the
feature makes people think, feel, and do. Group them into themes, cut what
fails the rules above, and try the rest in sentences.

## Buttons and menu items

Start with a verb. Name the object when the verb alone is unclear.
Sentence case.

| Before | After |
| --- | --- |
| OK | Save |
| Let's go! | Create project |
| New | New task |
| Export | Export... (it opens a save dialog first) |
| Click here to learn more | Learn about smart lists |

- An ellipsis means "asks for more before it acts". Leave it off commands
  that act at once.
- Multi-step flows start with "Get started", move with "Next", and end
  with "Done". Keep the same words in every flow.

## Titles

The window title is the app's name. Page titles name the page. Dialog
titles name the task.

| Before | After |
| --- | --- |
| Edit | Rename project |
| Settings Page | Settings |
| Large Files Overview | Large files |

## Empty states

Say what the page is for and give the first action as a button.

| Before | After |
| --- | --- |
| Nothing here. | No tasks yet. Tasks you add show up here. [New task] |
| Your inbox is empty! 🎉 | All done. New tasks land here first. |

Keep critical information out of empty states. They disappear.

## Errors

Say what happened and how to fix it, next to the problem, without blame.

| Before | After |
| --- | --- |
| Oops! Something went wrong. | Can't save the task. The disk is full. |
| Invalid name. | Use letters and numbers only. |
| Password too short. | Choose a password with at least 8 characters. |
| Error 0x80070005 | Can't open this folder. You don't have permission. |
| We're having trouble loading this. | Can't load tasks. |

Say what to do, not what is forbidden: "Use only letters for your name",
not "Numbers are not allowed".

When the same error keeps happening to many people, change the design, not
the message.

## Dialogs

- Sentence case for the title and the buttons. A question ends with a
  question mark: "Delete 3 tasks?"
- At most two lines of content. Never "Error", "Warning", or an error code
  as the title.
- The content never explains the buttons. The buttons name the action.
- The close button says "Cancel" when the dialog asks for a decision, or
  "Close" when it only informs (and then it should be an InfoBar).

## Confirmations

Ask only for rare actions that cannot be undone, or that remove a lot. The
title is a question with the verb. The button repeats the verb.

| Before | After |
| --- | --- |
| Are you sure? [Yes] [No] | Delete 3 tasks? [Delete] [Cancel] |
| Warning! This cannot be undone. [OK] | Empty the Recycle Bin? You can't undo this. [Empty Recycle Bin] [Cancel] |
| Delete files? [OK] [Cancel] | Move 14 items (3.2 GB) to the Recycle Bin? [Move to Recycle Bin] [Cancel] |

## Undo

Name every undoable action, so the InfoBar and the Undo tooltip say what
comes back: "Task deleted [Undo]", tooltip "Undo delete task (Ctrl+Z)".
WinUI has no undo manager of its own: keep a small stack of
`(string Name, Action Undo)` in the view model.

## Settings

Label the setting by what it does. Describe the on state only; people
infer the off state. Point to a Windows setting by its path, or open it
(`ms-settings:` links) instead of describing where it is.

| Before | After |
| --- | --- |
| Enable notifications for tasks that are due if the app is not open | Notify me when a task is due |
| Sync | Keep tasks on all my PCs |

## Fields and placeholders

Every box has a visible label (`Header`). The placeholder shows an example
or the format, never the only label.

| Before | After |
| --- | --- |
| (placeholder only) Email | Header "Email", placeholder "name@example.com" |
| Enter date | Header "Due date", placeholder "Tomorrow, Friday, Oct 30" |

## Permissions and administrator rights

A desktop app asks for little: camera, microphone, and location go through
Windows' privacy settings, and files the user can open need no prompt.

- Ask at the moment of first use, never at launch. One short sentence says
  how and why: "Recordings use the microphone to capture your voice."
- When Windows blocks access (privacy setting off), say so and offer one
  button that opens the right page: "Open privacy settings"
  (`ms-settings:privacy-microphone`).
- Never require "Run as administrator" for the whole app. When one action
  needs it (a folder only administrators can read, a program's own
  uninstaller), say so at that action: "Can't read 12 folders. They need
  administrator rights." Leave them out, or hand off to the tool that has
  the right (the uninstaller asks for it itself).

## Progress

Say what is happening, with a count when there is one.

| Before | After |
| --- | --- |
| Loading... | Scanning 12,408 folders... |
| Processing | Summarizing your notes... |
| Please wait | Exporting "Q3 report"... |

## Tooltips

These rules cover tooltips on controls. An information tooltip (the full
text of a truncated value, a chart value, why a control is disabled) just
shows that information.

Say what the control does, starting with a verb, then add its shortcut in
parentheses: "(Ctrl+N)", "(Ctrl+Shift+S)". Do not repeat the control's
name: the tooltip adds what the name does not say. Sentence case, no
period, at most 60 to 75 characters. Skip tooltips on buttons whose text
label already says it all.

| Before | After |
| --- | --- |
| (none) | Add a task to this list (Ctrl+N) |
| New task (Ctrl+N) | Add a task to this list (Ctrl+N) |
| Click this button to create a new task in the current list | Add a task to this list (Ctrl+N) |

## Counts and plurals

Write both forms, and format numbers for the user's language.

```csharp
string Count(int n) => n == 1 ? "1 task" : $"{n:N0} tasks";
```

Sizes use the units people see in File Explorer: "3.2 GB", "820 MB",
"12 KB", with one decimal above 1 GB.

## Words to cut

- "we", "our": unclear who that is. "Can't sync", not "We couldn't sync".
- "Oops", "Uh-oh", "Whoops": sound insincere in an error.
- "Simply", "just", "easily": they blame people who find it hard.
- "Please": once in a while is fine, every message is noise.
- "Your", "my": "Favorites" already means yours.
- Code words: entity, record, payload, null, HRESULT, 0x80070005.
