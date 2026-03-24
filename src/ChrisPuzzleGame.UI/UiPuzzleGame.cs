using System;
using ChrisPuzzleGame.Screens;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame;

public sealed class PuzzleGame : Game
{
    private const int DefaultBackBufferWidth = 1200;
    private const int DefaultBackBufferHeight = 720;

    private readonly GraphicsDeviceManager graphics;
    private readonly ScreenManager screenManager;

    public PuzzleGame()
    {
        Content.RootDirectory = "Content";

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

        screenManager.AddScreen(new BackgroundScreen(), null);
        screenManager.AddScreen(new MainMenuScreen(() => new GameplayScreen()), null);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
    }
}
