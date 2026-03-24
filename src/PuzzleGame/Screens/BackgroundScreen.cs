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
        TransitionOnTime = TimeSpan.FromSeconds(0.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }
    public override void LoadContent() => backgroundTexture ??= ScreenManager.LoadOptionalTexture("background") ?? ScreenManager.LoadOptionalTexture("Background");
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) => base.Update(gameTime, otherScreenHasFocus, false);
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var fullScreen = viewport.Bounds;
        var layoutScale = DesktopUiChrome.GetViewportScale(viewport);
        spriteBatch.Begin();
        if (backgroundTexture is not null)
        {
            spriteBatch.Draw(backgroundTexture, fullScreen, Color.White * TransitionAlpha);
            spriteBatch.Draw(ScreenManager.BlankTexture, fullScreen, new Color(6, 10, 18) * (0.38f * TransitionAlpha));
        }
        else
        {
            DrawFallbackBackdrop(spriteBatch, fullScreen);
        }

        DrawPuzzleBackdrop(spriteBatch, fullScreen, layoutScale);
        spriteBatch.End();
    }

    private void DrawFallbackBackdrop(SpriteBatch spriteBatch, Rectangle fullScreen)
    {
        spriteBatch.Draw(ScreenManager.BlankTexture, fullScreen, new Color(11, 17, 28) * TransitionAlpha);

        const int bandCount = 9;
        var bandHeight = Math.Max(1, fullScreen.Height / bandCount);
        for (var i = 0; i < bandCount; i++)
        {
            var bandBounds = new Rectangle(fullScreen.X, fullScreen.Y + (i * bandHeight), fullScreen.Width, bandHeight + 1);
            var bandColor = Color.Lerp(new Color(11, 17, 28), new Color(23, 39, 60), i / (bandCount - 1f));
            spriteBatch.Draw(ScreenManager.BlankTexture, bandBounds, bandColor * (0.32f * TransitionAlpha));
        }

        var topShade = new Rectangle(fullScreen.X, fullScreen.Y, fullScreen.Width, (int)(fullScreen.Height * 0.2f));
        var bottomShade = new Rectangle(fullScreen.X, fullScreen.Bottom - (int)(fullScreen.Height * 0.24f), fullScreen.Width, (int)(fullScreen.Height * 0.24f));
        spriteBatch.Draw(ScreenManager.BlankTexture, topShade, Color.Black * (0.18f * TransitionAlpha));
        spriteBatch.Draw(ScreenManager.BlankTexture, bottomShade, Color.Black * (0.28f * TransitionAlpha));
    }

    private void DrawPuzzleBackdrop(SpriteBatch spriteBatch, Rectangle fullScreen, float layoutScale)
    {
        var motifBounds = new Rectangle(
            fullScreen.Center.X - (int)(280f * layoutScale),
            fullScreen.Center.Y - (int)(170f * layoutScale),
            (int)(560f * layoutScale),
            (int)(340f * layoutScale));

        spriteBatch.Draw(ScreenManager.BlankTexture, motifBounds, new Color(7, 11, 18) * (0.18f * TransitionAlpha));
        DesktopUiChrome.DrawBorder(ScreenManager, spriteBatch, motifBounds, new Color(86, 108, 144) * (0.18f * TransitionAlpha), 1);

        var cellGap = Math.Max(6, (int)(10f * layoutScale));
        var cellSize = Math.Min((motifBounds.Width - (cellGap * 5)) / 4, (motifBounds.Height - (cellGap * 5)) / 4);
        var gridWidth = (cellSize * 4) + (cellGap * 3);
        var gridHeight = (cellSize * 4) + (cellGap * 3);
        var startX = motifBounds.Center.X - (gridWidth / 2);
        var startY = motifBounds.Center.Y - (gridHeight / 2);

        for (var row = 0; row < 4; row++)
        {
            for (var column = 0; column < 4; column++)
            {
                var bounds = new Rectangle(
                    startX + (column * (cellSize + cellGap)),
                    startY + (row * (cellSize + cellGap)),
                    cellSize,
                    cellSize);

                var isGap = row == 3 && column == 3;
                var fill = isGap
                    ? new Color(7, 11, 18)
                    : Color.Lerp(new Color(69, 116, 184), new Color(219, 126, 70), ((row * 4f) + column) / 14f);

                var alpha = isGap ? 0.16f : 0.11f;
                spriteBatch.Draw(ScreenManager.BlankTexture, bounds, fill * (alpha * TransitionAlpha));
                DesktopUiChrome.DrawBorder(ScreenManager, spriteBatch, bounds, new Color(255, 255, 255) * (0.12f * TransitionAlpha), 1);
            }
        }
    }
}
