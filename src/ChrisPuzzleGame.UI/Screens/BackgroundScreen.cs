using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public sealed class BackgroundScreen : GameScreen
{
    private Texture2D? backgroundTexture;
    public BackgroundScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.35);
        TransitionOffTime = TimeSpan.FromSeconds(0.35);
    }
    public override void LoadContent() => backgroundTexture ??= ScreenManager.LoadOptionalTexture("background") ?? ScreenManager.LoadOptionalTexture("Background");
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) => base.Update(gameTime, otherScreenHasFocus, false);
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var fullScreen = ScreenManager.GraphicsDevice.Viewport.Bounds;
        spriteBatch.Begin();
        if (backgroundTexture is not null)
        {
            spriteBatch.Draw(backgroundTexture, fullScreen, Color.White * TransitionAlpha);
            spriteBatch.Draw(ScreenManager.BlankTexture, fullScreen, new Color(6, 10, 18) * 0.38f);
        }
        else
        {
            spriteBatch.Draw(ScreenManager.BlankTexture, fullScreen, new Color(14, 21, 32) * TransitionAlpha);
        }
        spriteBatch.End();
    }
}
