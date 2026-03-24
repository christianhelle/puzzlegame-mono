using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public sealed class PreviewScreen : GameScreen
{
    private readonly string puzzleImageAssetName;
    private readonly Color tint;
    private Texture2D? previewTexture;

    public PreviewScreen(string puzzleImageAssetName, Color tint)
    {
        this.puzzleImageAssetName = puzzleImageAssetName;
        this.tint = tint;
        TransitionOnTime = TimeSpan.Zero;
        TransitionOffTime = TimeSpan.Zero;
    }

    public override void LoadContent() => previewTexture ??= ScreenManager.Game.Content.Load<Texture2D>(puzzleImageAssetName);

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var viewportBounds = ScreenManager.GraphicsDevice.Viewport.Bounds;

        ScreenManager.GraphicsDevice.Clear(Color.Black);

        spriteBatch.Begin();
        if (previewTexture is not null)
        {
            spriteBatch.Draw(previewTexture, viewportBounds, tint * TransitionAlpha);
        }
        spriteBatch.End();
    }
}
