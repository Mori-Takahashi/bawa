# BAWA

Agent skills for building native Windows apps with C# and WinUI 3. A port
of BAMA (Build Any Mac App) by Dave Ebbelaar. Installed as a Claude Code
and Codex plugin from `.claude-plugin/`, or through `npx skills add`.
`AGENTS.md` at the root is a copy of this file (`.claude/CLAUDE.md`);
keep them identical.

## Layout

- `skills/<stage>/<name>/SKILL.md`, stages in pipeline order: `start`, `plan`,
  `code`, `design`, `ship`.
- References sit next to `SKILL.md` (`recipes.md`, `patterns.md`) and are
  linked relatively. Scripts go in `scripts/`, templates in `template/`.
- Scripts are PowerShell that runs on Windows PowerShell 5.1 (`.ps1`, no
  `pwsh`-only syntax). Skills call them with
  `powershell -NoProfile -ExecutionPolicy Bypass -File <script>`. C# tools
  are file programs run with `dotnet run <file>.cs`.
- Only skills listed in `plugin.json` are ported. Other folders under
  `skills/` are BAMA's macOS skills waiting to be rebuilt for Windows.
- `agents/openai.yaml` in every skill: Codex display name and short
  description. A user-invoked skill also sets
  `policy.allow_implicit_invocation: false`.
- `.claude-plugin/plugin.json` lists every shipped skill in `skills`.
- `specs/<app>.md`: build plans for small apps. Each has
  Brief, How it works, Stories, Gotchas, and a "Done when" line, in that
  order. `specs/README.md` lists them.

## Writing skills

Follow Matt Pocock's `writing-for-agents` rules:

- The description is a **context pointer**: one short sentence on what it
  is, then "Use when" and one trigger per branch. Front-load the leading
  word.
- Inline what every branch needs. Push what only some branches need into a
  sibling file behind a link.
- Every step ends on a checkable condition. Every skill ends with a
  "Done when" line.
- Prompt the positive. Use a prohibition only as a hard guardrail, next to
  the positive target.
- Cache only what the environment cannot tell the agent: conventions,
  reasons, gotchas from real apps.
- One skill calls another with "Call the Skill tool with `<name>`".
- No em dashes. Plain words, short sentences, active voice.

## Adding a skill

1. Create `skills/<stage>/<name>/SKILL.md` and `agents/openai.yaml`.
2. Add it to `plugin.json` `skills`, the README table, and the router
   (`skills/start/bawa/SKILL.md`).
3. Run every command the skill names once on Windows. For template changes,
   run `skills/start/new-windows-app/scripts/check-template.ps1`: it must
   print `TEMPLATE PASS`.
4. Run `claude plugin validate . --strict`.

## Releasing

1. Bump `version` in `plugin.json`. Installed copies update only when this
   string changes. Keep `version` out of `marketplace.json`.
2. Run `claude plugin validate . --strict`.
3. Merge to `main`. Use a merge commit for a branch with contributor commits,
   so their pull requests show as merged.
4. On `main`, run `claude plugin tag . --push`. It creates `bawa--v<version>`.
5. Run `gh release create bawa--v<version> --generate-notes`.

Done when `claude plugin update bawa@bawa` installs the new version.
