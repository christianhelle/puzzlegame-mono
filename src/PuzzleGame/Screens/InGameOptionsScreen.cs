using System;
using Microsoft.Xna.Framework;

namespace PuzzleGame.Screens;

public sealed class InGameOptionsScreen : MenuScreen
{
    private readonly GameplayScreen gameplayScreen;

    public InGameOptionsScreen(GameplayScreen gameplayScreen) : base("Options")
    {
        this.gameplayScreen = gameplayScreen;
        IsPopup = true;
        TransitionOnTime = TimeSpan.Zero;
        TransitionOffTime = TimeSpan.Zero;

        var resumeGame = new MenuEntry("Resume Game");
        var quitGame = new MenuEntry("Return to Main Menu");

        resumeGame.Selected += OnResumeGameSelected;
        quitGame.Selected += OnQuitGameSelected;

        MenuEntries.Add(resumeGame);
        MenuEntries.Add(quitGame);
    }

    protected override string FooterText => "Esc resumes the puzzle";

    protected override float TitleScale => 1.2f;

    internal bool TryGetGameplayScreenForPersistence(out GameplayScreen screen)
    {
        if (gameplayScreen.CanResume)
        {
            screen = gameplayScreen;
            return true;
        }

        screen = null!;
        return false;
    }

    public override void Draw(GameTime gameTime)
    {
        ScreenManager.FadeBackBufferToBlack(0.32f * TransitionAlpha);
        base.Draw(gameTime);
    }

    protected override void OnCancel(PlayerIndex playerIndex) => ResumeGame(playerIndex);

    private void OnResumeGameSelected(object? sender, PlayerIndexEventArgs e)
        => ResumeGame(e.PlayerIndex);

    private void OnQuitGameSelected(object? sender, PlayerIndexEventArgs e)
    {
        LoadingScreen.Load(
            ScreenManager,
            loadingIsSlow: true,
            e.PlayerIndex,
            new BackgroundScreen(),
            new MainMenuScreen(() => new GameplayScreen()));
    }

    private void ResumeGame(PlayerIndex playerIndex)
    {
        gameplayScreen.PrepareToResumeFromPause();
        LoadingScreen.Load(ScreenManager, loadingIsSlow: false, playerIndex, gameplayScreen);
    }
}
