# Project Context

- **Owner:** Christian Helle
- **Project:** MonoGame port of Chris' Puzzle Game, originally built with XNA for Windows Phone 7.
- **Stack:** C#, MonoGame, .NET, original XNA codebase for reference
- **Reference:** https://github.com/christianhelle/xnapuzzlegame
- **Created:** 2026-03-24T15:50:35.669Z

## Learnings

- Day 1: This repo is a MonoGame port of Chris' Puzzle Game, with the original XNA Windows Phone 7 codebase used as the behavior reference.
- Day 1: Christian prefers frequent small logical commits during implementation so progress is easy to review and trace.
- Day 1: Fenster owns MonoGame integration, platform seams, and content-loading concerns for Windows.
- Day 1: The original content set includes both Content\Penguins.jpg and Content\Puzzles\Penguins.jpg; keep the puzzle-sized image on the original `Penguins` asset name and compile the root image under `Originals/Penguins` to avoid collisions.
- Day 1: The original Menu and Congratulations spritefonts target Segoe UI Mono and Kootenay, but the Windows build machine does not have those fonts installed, so the port uses Consolas and Georgia fallbacks to keep `dotnet build` dependable.
- Day 1: UI persistence now saves resumable gameplay to `%LocalAppData%\ChrisPuzzleGame\gameplay-state.bin`, stages writes through a sibling `.tmp` file, and falls back to the normal main-menu startup if the save is missing or corrupt.
- Day 1: GitHub CI should stay explicit and Windows-only for now: restore the trusted UI project and build `src\ChrisPuzzleGame.UI` so hosted validation exercises the same MonoGame content pipeline path as local development.
- Day 1: GitHub Pages deployment should stay in its own workflow, publish the checked-in `site/` directory with the official Pages actions, and leave the Windows CI build unchanged.
