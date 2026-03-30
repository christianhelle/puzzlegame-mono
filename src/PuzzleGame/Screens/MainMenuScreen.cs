using System;
using Microsoft.Xna.Framework;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

public sealed class MainMenuScreen : MenuScreen
{
    private readonly Func<GameScreen>? createGameplayScreen;

    public MainMenuScreen(Func<GameScreen>? createGameplayScreen = null) : base(
        "Chris' Puzzle Game")
    {
        this.createGameplayScreen = createGameplayScreen;
        var playGameMenuEntry = new MenuEntry("New Game");
        var creditsMenuEntry = new MenuEntry("Credits");
        var exitMenuEntry = new MenuEntry("Exit");
        playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
        creditsMenuEntry.Selected += CreditsMenuEntrySelected;
        exitMenuEntry.Selected += OnCancel;
        MenuEntries.Add(playGameMenuEntry);
        MenuEntries.Add(creditsMenuEntry);
        MenuEntries.Add(exitMenuEntry);
    }

    protected override string FooterText
        => "Arrow keys, mouse wheel, or Tab to move | Enter or click to choose | Esc or right-click to exit";

    private void PlayGameMenuEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        if (createGameplayScreen is not null)
        {
            LoadingScreen.Load(
                ScreenManager,
                loadingIsSlow: true,
                e.PlayerIndex,
                new BackgroundScreen(),
                createGameplayScreen());
            return;
        }

        ScreenManager.AddScreen(new ImageSelectionScreen(), e.PlayerIndex);
    }

    private void CreditsMenuEntrySelected(object? sender, PlayerIndexEventArgs e)
        => ScreenManager.AddScreen(new CreditsScreen(), e.PlayerIndex);

    protected override void OnCancel(PlayerIndex playerIndex) => ScreenManager.Game.Exit();
}
