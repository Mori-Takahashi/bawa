---
name: bawa
description: Build any Windows app. Start here to pick the next skill for a native Windows app.
disable-model-invocation: true
---

# Build any Windows app

Find where the user is on the path below, then call the Skill tool with the
skill for that step. One skill per call.

```text
idea -> app-brief -> user-stories -> new-windows-app
     -> windows-design -> improve-loop (ux, code, design, verify, commit) -> repeat
```

Pick the app's name first (app-brief, step 1) and create
`<parent>\<AppName>\docs\` before anything else, so every skill writes into
the same folder. Design comes right after the scaffold: the design plan and
the accent before the first feature.

## Plan

| The user has | Skill |
| --- | --- |
| An idea, or asks "what should I build" | `app-brief` |
| A brief, no backlog | `user-stories` |
| A spec from BAWA's `specs/` folder | `app-brief` (it copies the spec) |

## Build

| The user wants to | Skill |
| --- | --- |
| Start a new project | `new-windows-app` |
| Write or fix C# and XAML, or a build error | `csharp-winui` |

## Design

| The user wants to | Skill |
| --- | --- |
| Decide how a feature works and what it says: names, flow, dialogs, accessibility | `ux-design` |
| Make it look and feel like a Windows 11 app, or set the accent color | `windows-design` |
| Fix spacing and alignment, or check the layout on the grid | `layout-grid` |

## Ship

| The user wants to | Skill |
| --- | --- |
| Build it and look at every page | `build-and-verify` |
| Tests, a self-test, or checks that drive the running app | `test-windows-app` |
| Work through the backlog on its own | `improve-loop` |

## Not ported yet

These BAMA skills are being rebuilt for Windows: `research`, `clone`,
`performance`, `os-integrations` (hotkeys, tray icon, Explorer),
`improve-architecture`, `agent-cli`, `design-system`, `design-kit`,
`design-from-app`, `app-icon`. Until then:

- Research or cloning: interview the user in `app-brief` and note the
  reference app.
- Performance: follow `csharp-winui`, "The UI thread".
- An icon: edit `Icon\AppIcon.svg` in the app; `build.ps1` renders it.

Done when you named the step, called its skill, and told the user which
skill comes after it.
