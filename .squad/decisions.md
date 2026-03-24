# Squad Decisions

## Active Decisions

### 2026-03-24: Use a focused MonoGame porting squad
**By:** Christian Helle (confirmed by Squad)
**What:** The project starts with a focused squad: Keaton (Lead), McManus (Gameplay Dev), Fenster (Platform Dev), Hockney (UI Dev), and Redfoot (Tester), supported by Scribe and Ralph.
**Why:** The port needs clear ownership across gameplay parity, platform migration, UI adaptation, and regression coverage without overstaffing the early work.

### 2026-03-24: Preserve progress with small logical commits
**By:** Christian Helle (via Copilot)
**What:** During implementation, commit frequently in small, coherent groups so the repo keeps a detailed progress history.
**Why:** Incremental MonoGame porting will be easier to review, debug, and compare against the original XNA game when each change stays tightly scoped.

### 2026-03-24: Use the original XNA game as the behavior reference
**By:** Squad
**What:** The open-source XNA project at https://github.com/christianhelle/xnapuzzlegame is the reference point for gameplay behavior, flow, and feature parity decisions during the port.
**Why:** The goal is to preserve the original game's feel while moving it onto MonoGame for Windows.

## Governance

- All meaningful changes require team consensus.
- Document architectural decisions here.
- Keep history focused on work, decisions focused on direction.
