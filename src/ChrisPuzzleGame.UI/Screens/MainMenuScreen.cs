using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;

namespace ChrisPuzzleGame.Screens;

public sealed class MainMenuScreen : MenuScreen
{
    private readonly Func<GameScreen>? createGameplayScreen;
    protected override string FooterText => "Arrow keys, mouse wheel, or Tab to move | Enter or click to choose | Esc or right-click to exit";
    public MainMenuScreen(Func<GameScreen>? createGameplayScreen = null) : base("Chris' Puzzle Game")
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
    private void PlayGameMenuEntrySelected(object? sender, PlayerIndexEventArgs e)
    {
        if (createGameplayScreen is not null)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new BackgroundScreen(), createGameplayScreen());
            return;
        }
        ScreenManager.AddScreen(new MessageBoxScreen("The desktop shell is in place and launches cleanly. Gameplay wiring can be plugged into this menu next.", includeCancel: false), e.PlayerIndex);
    }
    private void CreditsMenuEntrySelected(object? sender, PlayerIndexEventArgs e) => ScreenManager.AddScreen(new CreditsScreen(), e.PlayerIndex);
    protected override void OnCancel(PlayerIndex playerIndex) => ScreenManager.Game.Exit();
}
