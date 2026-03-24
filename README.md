# Chris' Puzzle Game

A classic 4×4 sliding tile puzzle game, built with **MonoGame** for Windows.

> **This project is a MonoGame port of Chris' Puzzle Game**, originally released for Windows Phone 7 using XNA.  
> The original XNA project is available at: [christianhelle/xnapuzzlegame](https://github.com/christianhelle/xnapuzzlegame)

---

## Project Status

✅ **Fully Playable** — The desktop port is complete and runs on Windows.

| Feature | Status |
|---------|--------|
| Main menu with keyboard, mouse & Tab navigation | ✅ Working |
| New game / random puzzle selection | ✅ Working |
| 4×4 sliding puzzle gameplay | ✅ Working |
| Arrow keys, WASD, and click-to-move controls | ✅ Working |
| Live preview (hold Enter or F1) | ✅ Working |
| Win screen with time and move count | ✅ Working |
| Play Again / Main Menu flow | ✅ Working |
| In-game options (Esc to pause) | ✅ Working |
| Session persistence (save/restore on exit) | ✅ Working |
| Credits screen | ✅ Working |
| Resizable window with layout scaling | ✅ Working |

---

## Build & Run

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (or later)
- Windows (the project targets `net9.0-windows` with DirectX)

### Build

```powershell
dotnet build ChrisPuzzleGame.slnx
```

### Run

```powershell
dotnet run --project src\ChrisPuzzleGame.UI\ChrisPuzzleGame.UI.csproj
```

The game window opens at **1200×720** by default and is resizable.

---

## Website

The repository's static site lives under `site/` and is deployed to **GitHub Pages** with `.github\workflows\pages.yml`.

- Pushes to `main` that change `site/` (or the Pages workflow itself) trigger a deployment.
- You can also run the Pages workflow manually from the **Actions** tab.
- The committed `site\.nojekyll` file keeps Pages publishing the static files as-is.

---

## Controls

### Menus

| Input | Action |
|-------|--------|
| ↑ / ↓ / Tab / Mouse Wheel | Navigate menu entries |
| Enter / Left Click | Select |
| Esc / Right Click | Back / Exit |

### Gameplay

| Input | Action |
|-------|--------|
| Arrow Keys / WASD | Slide tiles |
| Left Click | Click an adjacent tile to move it |
| R / F5 | Reshuffle the puzzle |
| Enter / F1 (hold) | Preview the solved image |
| Esc | Open in-game options |

---

## Gameplay Overview

The goal is to rearrange the scrambled tiles to recreate the original image. One tile is missing—slide adjacent tiles into the empty space until every piece is in its correct position.

When you solve the puzzle, a **Congratulations** screen shows your completion time and total moves. From there you can play again with a new image or return to the main menu.

**Included puzzle images:** Chrysanthemum, Desert, Hydrangeas, Jellyfish, Koala, Lighthouse, Penguins, and Tulips.

---

## Repository Layout

```
puzzlegame-mono/
├── ChrisPuzzleGame.slnx           # Solution file (VS 2022+)
├── site/                          # Static website published with GitHub Pages
└── src/
    └── ChrisPuzzleGame.UI/        # Main game project
        ├── Content/               # MonoGame content (images, fonts, puzzles)
        │   ├── Puzzles/           # Puzzle image assets
        │   └── Content.mgcb       # MonoGame Content Builder manifest
        ├── Gameplay/              # Puzzle board logic
        ├── Persistence/           # Save/load session state
        ├── Screens/               # Menu, gameplay, win, credits screens
        ├── StateManagement/       # Screen manager and input handling
        ├── UiProgram.cs           # Entry point
        └── UiPuzzleGame.cs        # Game class
```

---

## About the Port

This MonoGame version preserves the spirit of the original Windows Phone 7 game while adapting the UI and input handling for desktop play:

- **Mouse and keyboard first** — menus respond to clicks, wheel scrolling, and Tab navigation.
- **Resizable window** — layouts scale smoothly across different display sizes.
- **Session persistence** — the game saves your progress on exit and restores it on next launch.

The original XNA codebase serves as the behavior reference for gameplay parity decisions.

---

## Author

**Christian Resma Helle**  
[christianhelle.com](https://christianhelle.com)

---

## License

See the original [xnapuzzlegame](https://github.com/christianhelle/xnapuzzlegame) repository for licensing details.
