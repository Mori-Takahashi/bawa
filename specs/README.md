# Specs

Build plans for small Windows apps you can build today with BAWA. Each spec
is one app. Give it to your agent and it builds the app for you.

## Build one

Install BAWA, copy a spec into an empty folder, open your agent there, and
tell it:

```text
Build the app in disk-space.md with BAWA.
```

A link works too:
`https://github.com/Mori-Takahashi/bawa/blob/main/specs/disk-space.md`.

The agent copies the spec's **Brief** into `docs/BRIEF.md` and its
**Stories** into `docs/STORIES.md`, then runs `new-windows-app` and
`improve-loop`. Change the name, the shortcuts, or the scope first if you
want. A spec is a starting point, not a contract.

## The specs

| Spec | What it does | Replaces |
| --- | --- | --- |
| [disk-space.md](disk-space.md) | Scan a drive, see the biggest folders, files, and programs, and move what you pick to the Recycle Bin | WinDirStat, TreeSize Free, WizTree |

The other files in this folder are BAMA's macOS specs, taken from Onyx by
Dave Ebbelaar. They are being rethought for Windows: some become tray apps,
and some (like the window snapper) are already built into Windows. Until a
spec is in the table above, it describes macOS APIs and will not build on
Windows.

## What a spec holds

Every spec has the same parts, in this order:

1. **Brief**: the `app-brief` format, filled in. The agent skips the
   interview.
2. **How it works**: the Windows APIs, what each one needs, and the main
   types.
3. **Stories**: the `user-stories` format, in milestones. Every acceptance
   line says how to check it.
4. **Gotchas**: what goes wrong on Windows, so your agent gets it right the
   first time.
5. **Done when**: the check that the app is finished.

## Write your own

Copy a spec that's close to your idea and change it. Keep the five parts.
Keep version one small: one shape (window, tray icon, or quick panel), two
to five jobs, and 5 to 10 stories in the first milestone.
