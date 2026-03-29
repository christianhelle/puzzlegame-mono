using System;
using Microsoft.Xna.Framework;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

public sealed class LoadingScreen : GameScreen
{
    private readonly bool loadingIsSlow;
    private readonly GameScreen[] screensToLoad;
    private bool otherScreensAreGone;

    private LoadingScreen(
        ScreenManager screenManager,
        bool loadingIsSlow,
        GameScreen[] screensToLoad)
    {
        this.loadingIsSlow = loadingIsSlow;
        this.screensToLoad = screensToLoad;
        IsSerializable = false;
        TransitionOnTime = TimeSpan.Zero;
    }

    public static void Load(
        ScreenManager screenManager,
        bool loadingIsSlow,
        PlayerIndex? controllingPlayer,
        params GameScreen[] screensToLoad)
    {
        for (var i = screenManager.ScreenCount - 1; i >= 0; i--)
        {
            screenManager.GetScreenAt(i).ExitScreen();
        }
        var loadingScreen = new LoadingScreen(screenManager, loadingIsSlow, screensToLoad);
        screenManager.AddScreen(loadingScreen, controllingPlayer);
    }

    public override void Update(
        GameTime gameTime,
        bool otherScreenHasFocus,
        bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        if (!otherScreensAreGone) return;
        ScreenManager.RemoveScreen(this);
        foreach (var screen in screensToLoad)
        {
            if (screen is not null) ScreenManager.AddScreen(screen, ControllingPlayer);
        }
        ScreenManager.Game.ResetElapsedTime();
    }

    public override void Draw(GameTime gameTime)
    {
        if (ScreenState == ScreenState.Active && ScreenManager.ScreenCount == 1)
            otherScreensAreGone = true;
        if (!loadingIsSlow) return;
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var layoutScale = DesktopUiChrome.GetViewportScale(viewport);
        var dotCount = (int)(gameTime.TotalGameTime.TotalSeconds * 4f) % 3 + 1;
        var message = $"Loading{new string(c: '.', dotCount)}";
        var subtitle = "Preparing the next screen";
        var titleScale = MathHelper.Clamp(1.02f * layoutScale, min: 0.95f, max: 1.18f);
        var subtitleScale = MathHelper.Clamp(0.58f * layoutScale, min: 0.52f, max: 0.72f);
        var panelBounds = new Rectangle(
            viewport.Width / 2 - (int)(190f * layoutScale),
            viewport.Height / 2 - (int)(78f * layoutScale),
            (int)(380f * layoutScale),
            (int)(156f * layoutScale));
        ScreenManager.FadeBackBufferToBlack(0.28f);
        spriteBatch.Begin();
        DesktopUiChrome.DrawPanel(
            ScreenManager,
            spriteBatch,
            panelBounds,
            TransitionAlpha,
            new(r: 15, g: 22, b: 34),
            new(r: 105, g: 129, b: 165),
            new(r: 69, g: 116, b: 184));

        var messageOrigin = font.MeasureString(message) / 2f;
        var messagePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + 50f * layoutScale);
        DesktopUiChrome.DrawShadowedString(
            spriteBatch,
            font,
            message,
            messagePosition,
            Color.White,
            messageOrigin,
            titleScale,
            TransitionAlpha);

        var subtitleOrigin = font.MeasureString(subtitle) / 2f;
        var subtitlePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + 84f * layoutScale);
        DesktopUiChrome.DrawShadowedString(
            spriteBatch,
            font,
            subtitle,
            subtitlePosition,
            new(r: 193, g: 205, b: 224),
            subtitleOrigin,
            subtitleScale,
            TransitionAlpha);

        var segmentWidth = Math.Max(val1: 22, (int)(28f * layoutScale));
        var segmentHeight = Math.Max(val1: 8, (int)(12f * layoutScale));
        var segmentGap = Math.Max(val1: 8, (int)(10f * layoutScale));
        var segmentStartX = panelBounds.Center.X - (segmentWidth * 3 + segmentGap * 2) / 2;
        var segmentY = panelBounds.Bottom - (int)(34f * layoutScale);
        var activeIndex = (int)(gameTime.TotalGameTime.TotalSeconds * 6f) % 3;

        for (var i = 0; i < 3; i++)
        {
            var bounds = new Rectangle(
                segmentStartX + i * (segmentWidth + segmentGap),
                segmentY,
                segmentWidth,
                segmentHeight);
            var isActive = i == activeIndex;
            var fill = isActive ? new(r: 255, g: 221, b: 96) : new Color(r: 81, g: 101, b: 132);
            var border = isActive ? new(r: 255, g: 241, b: 170) : new Color(r: 156, g: 174, b: 205);
            spriteBatch.Draw(
                ScreenManager.BlankTexture,
                bounds,
                fill * (TransitionAlpha * (isActive ? 1f : 0.45f)));
            DesktopUiChrome.DrawBorder(
                ScreenManager,
                spriteBatch,
                bounds,
                border * (TransitionAlpha * (isActive ? 0.7f : 0.18f)),
                thickness: 1);
        }

        spriteBatch.End();
    }
}
