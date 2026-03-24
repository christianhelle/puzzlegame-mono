# Project Context

- **Owner:** Christian Helle
- **Project:** MonoGame port of Chris' Puzzle Game, originally built with XNA for Windows Phone 7.
- **Stack:** C#, MonoGame, .NET, original XNA codebase for reference
- **Reference:** https://github.com/christianhelle/xnapuzzlegame
- **Created:** 2026-03-24T15:50:35.669Z

## Learnings

- Day 1: This repo is a MonoGame port of Chris' Puzzle Game, with the original XNA Windows Phone 7 codebase used as the behavior reference.
- Day 1: Christian prefers frequent small logical commits during implementation so progress is easy to review and trace.
- Day 1: McManus owns puzzle logic, gameplay flow, and preserving the original game's feel.
- Day 1: Desktop gameplay feel depends on repeated held-direction movement and the momentary Enter/F1 full-image preview, not just single-step input handling.
- Day 1: The preview/options pause flow can unload and later re-add the same `GameplayScreen` instance as long as `LoadContent()` only restores assets and does not reshuffle gameplay state.
- Day 1: The UI project's content build needed an explicit clean rebuild and output-folder reset so stale XNB aliases could not mask the original asset names.
- Day 1: Gameplay persistence has to capture the current puzzle asset by name, not just by list position, so saved sessions survive harmlessly if puzzle ordering changes later.
- Day 1: The paused options flow owns a live `GameplayScreen` reference even when gameplay is off the screen stack, so persistence has to pull state through `InGameOptionsScreen` to save an honest session.
