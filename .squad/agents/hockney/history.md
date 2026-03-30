# Project Context

- **Owner:** Christian Helle
- **Project:** MonoGame port of Chris' Puzzle Game, originally built with XNA for Windows Phone 7.
- **Stack:** C#, MonoGame, .NET, original XNA codebase for reference
- **Reference:** https://github.com/christianhelle/xnapuzzlegame
- **Created:** 2026-03-24T15:50:35.669Z

## Learnings

- Day 1: This repo is a MonoGame port of Chris' Puzzle Game, with the original XNA Windows Phone 7 codebase used as the behavior reference.
- Day 1: Christian prefers frequent small logical commits during implementation so progress is easy to review and trace.
- Day 1: Hockney owns player-facing screens, input affordances, and desktop-friendly UI polish.
- Day 1: The desktop shell now treats mouse wheel, Tab, and right-click as first-class menu affordances so keyboard and pointer navigation feel native on Windows.
- Day 1: When no background texture is available, the UI shell uses a procedural puzzle-board backdrop so menus still read as intentional rather than placeholder.
- Day 1: Created comprehensive root README with project summary, status table, build instructions, controls documentation, and repo layout. Skipped screenshots because the QA captures had too much surrounding desktop noise to be suitable for documentation.
- Day 1: Second attempt at README screenshots also failed. CopyFromScreen captures include visible desktop clutter (text from adjacent windows bleeding into the edges). PrintWindow API had compilation issues. The shared desktop environment makes artifact-free window captures impractical without a dedicated clean display or a headless rendering approach.
- Day 1: The GitHub Pages landing page should stay fully self-contained under `site/`, reuse shipped puzzle artwork and UI textures, and label those assets as artwork instead of pretending they are gameplay screenshots.
- Day 1 (Image Selection): Added 6 procedurally-generated puzzle images (Aurora, Mosaic, Sunset, Ocean, Mountain, Nebula) at 800x600 resolution using Python/Pillow, expanding the puzzle collection from 8 to 14 images.
- Day 1 (Image Selection): Created ImageSelectionScreen as a grid-based menu for choosing puzzle images, supporting keyboard (arrow keys), mouse (hover+click, scroll wheel), and gamepad navigation with the same dark blue/gold color scheme as other menus.
- Day 1 (Image Selection): ImageSelectionScreen includes a "Random" option (displayed as "?") alongside thumbnails of all available puzzles, with responsive grid layout that adapts columns based on viewport scale.
- Day 1 (Image Selection): Updated game flow so "New Game" now navigates to ImageSelectionScreen instead of directly creating a random GameplayScreen, and "Play Again" after winning also returns to ImageSelectionScreen.
- Day 1 (Image Selection): GameplayScreen.PuzzleImages array is now internal (not private) to allow ImageSelectionScreen access, and added a public constructor accepting specific puzzle image asset paths.
- Day 1 (Image Selection): All menu returns now navigate back to MainMenuScreen without the gameplay factory, simplifying the screen stack flow.
