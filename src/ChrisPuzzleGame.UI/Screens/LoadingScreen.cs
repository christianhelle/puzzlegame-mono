using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public sealed class LoadingScreen : GameScreen
{
    private readonly bool loadingIsSlow;
    private readonly GameScreen[] screensToLoad;
    private bool otherScreensAreGone;
    private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow, GameScreen[] screensToLoad)
    {
        this.loadingIsSlow = loadingIsSlow;
        this.screensToLoad = screensToLoad;
        IsSerializable = false;
        TransitionOnTime = TimeSpan.Zero;
    }
    public static void Load(ScreenManager screenManager, bool loadingIsSlow, PlayerIndex? controllingPlayer, params GameScreen[] screensToLoad)
    {
        foreach (var screen in screenManager.GetScreens()) screen.ExitScreen();
        var loadingScreen = new LoadingScreen(screenManager, loadingIsSlow, screensToLoad);
        screenManager.AddScreen(loadingScreen, controllingPlayer);
    }
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        if (!otherScreensAreGone) return;
        ScreenManager.RemoveScreen(this);
        foreach (var screen in screensToLoad) if (screen is not null) ScreenManager.AddScreen(screen, ControllingPlayer);
        ScreenManager.Game.ResetElapsedTime();
    }
    public override void Draw(GameTime gameTime)
    {
        if (ScreenState == ScreenState.Active && ScreenManager.GetScreens().Length == 1) otherScreensAreGone = true;
        if (!loadingIsSlow) return;
        const string message = "Loading...";
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var textSize = font.MeasureString(message);
        var textPosition = new Vector2((viewport.Width - textSize.X) / 2f, (viewport.Height - textSize.Y) / 2f);
        var color = Color.White * TransitionAlpha;
        ScreenManager.FadeBackBufferToBlack(0.2f);
        spriteBatch.Begin();
        spriteBatch.DrawString(font, message, textPosition + new Vector2(3f, 3f), Color.Black * (TransitionAlpha * 0.65f));
        spriteBatch.DrawString(font, message, textPosition, color);
        spriteBatch.End();
    }
}
