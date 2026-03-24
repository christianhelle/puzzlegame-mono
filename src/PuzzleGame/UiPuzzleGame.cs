using System;
using Microsoft.Xna.Framework;
using PuzzleGame.Persistence;
using PuzzleGame.Screens;
using PuzzleGame.StateManagement;

namespace PuzzleGame;

public sealed class PuzzleGame : Game
{
    private const int DefaultBackBufferWidth = 1200;
    private const int DefaultBackBufferHeight = 720;

    private readonly GraphicsDeviceManager graphics;
    private readonly GameplayPersistence gameplayPersistence;
    private readonly ScreenManager screenManager;

    public PuzzleGame()
    {
        Content.RootDirectory = "Content";
        gameplayPersistence = new GameplayPersistence();

        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = DefaultBackBufferWidth,
            PreferredBackBufferHeight = DefaultBackBufferHeight,
            SynchronizeWithVerticalRetrace = true,
        };

        TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "Chris' Puzzle Game";

        screenManager = new ScreenManager(this);
        Components.Add(screenManager);
        Exiting += HandleGameExiting;

        ConfigureStartupScreens();
    }

    private void ConfigureStartupScreens()
    {
        screenManager.AddScreen(new BackgroundScreen(), null);

        var savedGameplay = gameplayPersistence.TryLoad();
        if (savedGameplay is not null)
        {
            screenManager.AddScreen(savedGameplay, null);
            return;
        }

        screenManager.AddScreen(new MainMenuScreen(() => new GameplayScreen()), null);
    }

    private void HandleGameExiting(object? sender, EventArgs e)
    {
        gameplayPersistence.SaveOrDelete(screenManager);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
    }
}
