# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Porting strategy and cross-cutting decisions | Keaton | Port sequencing, architectural trade-offs, code reviews, reviewer gates |
| Gameplay systems and puzzle rules | McManus | Board logic, move resolution, scoring, win and loss conditions |
| MonoGame platform integration | Fenster | Game bootstrap, rendering plumbing, content pipeline, asset loading, desktop-specific behavior |
| UI screens and input feel | Hockney | Menus, HUD, screen transitions, scaling, keyboard and mouse polish |
| Testing and regression coverage | Redfoot | Parity checks, bug reproduction, test coverage, edge cases |
| Code review | Keaton | Review PRs, check quality, suggest improvements |
| Testing | Redfoot | Write tests, find edge cases, verify fixes |
| Scope and priorities | Keaton | What to build next, trade-offs, decisions |
| Session logging | Scribe | Automatic, never needs routing |
| Backlog monitoring | Ralph | Issue scans, PR follow-up, idle-watch, queue nudges |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Keaton |
| `squad:keaton` | Pick up issue and complete the work | Keaton |
| `squad:mcmanus` | Pick up issue and complete the work | McManus |
| `squad:fenster` | Pick up issue and complete the work | Fenster |
| `squad:hockney` | Pick up issue and complete the work | Hockney |
| `squad:redfoot` | Pick up issue and complete the work | Redfoot |

### How Issue Assignment Works

1. When a GitHub issue gets the `squad` label, the Lead triages it by analyzing the content, assigning the right `squad:{member}` label, and commenting with triage notes.
2. When a `squad:{member}` label is applied, that member picks up the issue in their next session.
3. Members can reassign by removing their label and adding another member's label.
4. The `squad` label is the inbox for untriaged issues waiting for Lead review.

## Rules

1. **Eager by default** - spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Scribe always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts -> coordinator answers directly.** Don't spawn an agent for simple status questions.
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." -> fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, Redfoot can start writing parity and regression checks from requirements immediately.
7. **Issue-labeled work** routes to the named member. Keaton handles all base `squad` triage.
