using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PuzzleGame.Screens;

public class MenuEntry
{
    private float selectionFade;

    public MenuEntry(string text)
        => this.Text = text;

    public string Text { get; set; }

    public Vector2 Position { get; set; }

    public event EventHandler<PlayerIndexEventArgs>? Selected;

    protected internal virtual void OnSelectEntry(PlayerIndex playerIndex)
        => Selected?.Invoke(this, new(playerIndex));

    public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        var fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 5f;
        selectionFade = MathHelper.Clamp(
            selectionFade + (isSelected ? fadeSpeed : -fadeSpeed),
            min: 0f,
            max: 1f);
    }

    public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        var screenManager = screen.ScreenManager;
        var spriteBatch = screenManager.SpriteBatch;
        var font = screenManager.Font;
        var layoutScale = screen.LayoutScale;
        var shadowOffset = new Vector2(x: 2f, y: 2f) * layoutScale;
        var color = isSelected ? new(r: 255, g: 221, b: 96) : new Color(r: 235, g: 240, b: 248);
        var shadowColor = Color.Black * (screen.TransitionAlpha * (0.45f + selectionFade * 0.15f));
        var time = gameTime.TotalGameTime.TotalSeconds;
        var pulsate = (float)Math.Sin(time * 6f) + 1f;
        var scale = layoutScale * (1f + pulsate * 0.04f * selectionFade);
        color *= screen.TransitionAlpha;
        var textSize = font.MeasureString(Text);
        var origin = new Vector2(x: 0f, textSize.Y / 2f);
        var entryWidth = textSize.X * scale;
        var entryHeight = textSize.Y * scale;
        var highlightPaddingX = 26f * layoutScale;
        var highlightPaddingY = 16f * layoutScale;
        var highlightBounds = new Rectangle(
            (int)(Position.X - highlightPaddingX),
            (int)(Position.Y - entryHeight / 2f - highlightPaddingY / 2f),
            (int)(entryWidth + highlightPaddingX * 2f),
            (int)(entryHeight + highlightPaddingY));

        if (selectionFade > 0f)
        {
            var highlightAlpha = screen.TransitionAlpha * (0.16f + selectionFade * 0.32f);
            DesktopUiChrome.DrawPanel(
                screenManager,
                spriteBatch,
                highlightBounds,
                highlightAlpha,
                new(r: 34, g: 49, b: 72),
                new(r: 176, g: 194, b: 224),
                new(r: 255, g: 221, b: 96));

            var accentHeight = Math.Max(val1: 1, highlightBounds.Height - (int)(18f * layoutScale));
            var accentBounds = new Rectangle(
                highlightBounds.X + (int)(12f * layoutScale),
                highlightBounds.Y + (int)(9f * layoutScale),
                Math.Max(val1: 3, (int)(4f * layoutScale)),
                accentHeight);
            spriteBatch.Draw(
                screenManager.BlankTexture,
                accentBounds,
                new Color(r: 255, g: 221, b: 96) * (screen.TransitionAlpha * selectionFade));

            var indicatorPosition = new Vector2(highlightBounds.X + 16f * layoutScale, Position.Y);
            var indicatorOrigin = new Vector2(x: 0f, font.LineSpacing / 2f);
            spriteBatch.DrawString(
                font,
                ">",
                indicatorPosition + shadowOffset,
                shadowColor,
                rotation: 0f,
                indicatorOrigin,
                layoutScale,
                SpriteEffects.None,
                layerDepth: 0f);
            spriteBatch.DrawString(
                font,
                ">",
                indicatorPosition,
                new Color(r: 255, g: 221, b: 96) * (screen.TransitionAlpha * selectionFade),
                rotation: 0f,
                indicatorOrigin,
                layoutScale,
                SpriteEffects.None,
                layerDepth: 0f);
        }

        var textPosition = Position + new Vector2(selectionFade * (12f * layoutScale), y: 0f);
        spriteBatch.DrawString(
            font,
            Text,
            textPosition + shadowOffset,
            shadowColor,
            rotation: 0f,
            origin,
            scale,
            SpriteEffects.None,
            layerDepth: 0f);
        spriteBatch.DrawString(
            font,
            Text,
            textPosition,
            color,
            rotation: 0f,
            origin,
            scale,
            SpriteEffects.None,
            layerDepth: 0f);
    }

    public virtual int GetHeight(MenuScreen screen)
        => (int)(screen.ScreenManager.Font.LineSpacing * screen.LayoutScale);

    public virtual int GetWidth(MenuScreen screen)
        => (int)(screen.ScreenManager.Font.MeasureString(Text).X * screen.LayoutScale);
}
