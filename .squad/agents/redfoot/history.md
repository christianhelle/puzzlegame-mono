# Project Context

- **Owner:** Christian Helle
- **Project:** MonoGame port of Chris' Puzzle Game, originally built with XNA for Windows Phone 7.
- **Stack:** C#, MonoGame, .NET, original XNA codebase for reference
- **Reference:** https://github.com/christianhelle/xnapuzzlegame
- **Created:** 2026-03-24T15:50:35.669Z

## Learnings

- Day 1: This repo is a MonoGame port of Chris' Puzzle Game, with the original XNA Windows Phone 7 codebase used as the behavior reference.
- Day 1: Christian prefers frequent small logical commits during implementation so progress is easy to review and trace.
- Day 1: Redfoot owns regression strategy, parity checks, and reviewer scrutiny for risky changes.
- Day 1: QA Checklist drafted. Current implementation has solid foundation (screens, input, gameplay logic) but is intentionally incomplete—missing InGameOptionsScreen, state serialization implementation, and puzzle content assets. Core acceptance criteria are clear: build, launch, input, screen flow, gameplay loop, state persistence, and 8 puzzle images. Five verification traps identified (input routing, content pipeline, state persistence, screen transitions, pause state) that will require careful testing.
- Day 1: Final verification pass on `src\ChrisPuzzleGame.UI` was a no-go. The MonoGame port now builds, opens a real 1200×720 window, renders the menu, enters live gameplay from `New Game`, accepts keyboard movement, shows preview/pause flows, and closes cleanly. Release is still blocked because no save/load path exists in the UI port (`GameScreen.Serialize/Deserialize` are empty stubs and no `%AppData%\ChrisPuzzleGame` save location was created), which fails a critical acceptance gate.
- Day 1: Re-ran final acceptance after the persistence work landed. Verified build, live window launch, menu/gameplay/options navigation, save-on-exit to `%LocalAppData%\ChrisPuzzleGame\gameplay-state.bin`, load-on-startup restoring the exact in-progress board, corrupt-save fallback to a clean menu, and a real runtime `WinScreen` using a controlled near-win save. Current tester verdict for `src\ChrisPuzzleGame.UI`: GO.
- Day 1: Documentation/workflow review is shippable. README matches the current MonoGame port, explicitly points back to the original XNA repo, the documented `dotnet build` and `dotnet run` paths work, and the Windows CI workflow successfully restores MonoGame tooling and builds on a Windows runner. README still has no screenshots, but I did not block on that because this review environment had no clean screenshot capture path for the live WinExe window.
- Day 1: GitHub Pages package is reviewer-ready. `site\index.html` clearly identifies the project as a MonoGame Windows port, links both the port repo and the original XNA repo, honestly labels the gallery as bundled puzzle artwork rather than screenshots, and all referenced local assets resolve. `pages.yml` uploads the checked-in `site\` folder directly, `.nojekyll` is present, and README's new website section stays consistent with the deployed package.
