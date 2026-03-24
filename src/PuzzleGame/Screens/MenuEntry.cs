using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public class MenuEntry
{
    private float selectionFade;
    private string text;
    public MenuEntry(string text) { this.text = text; }
    public string Text { get => text; set => text = value; }
    public Vector2 Position { get; set; }
    public event EventHandler<PlayerIndexEventArgs>? Selected;
    protected internal virtual void OnSelectEntry(PlayerIndex playerIndex) => Selected?.Invoke(this, new PlayerIndexEventArgs(playerIndex));
    public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        var fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 5f;
        selectionFade = MathHelper.Clamp(selectionFade + (isSelected ? fadeSpeed : -fadeSpeed), 0f, 1f);
    }
    public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
    {
        var screenManager = screen.ScreenManager;
        var spriteBatch = screenManager.SpriteBatch;
        var font = screenManager.Font;
        var layoutScale = screen.LayoutScale;
        var shadowOffset = new Vector2(2f, 2f) * layoutScale;
        var color = isSelected ? new Color(255, 221, 96) : new Color(235, 240, 248);
        var shadowColor = Color.Black * (screen.TransitionAlpha * (0.45f + (selectionFade * 0.15f)));
        var time = gameTime.TotalGameTime.TotalSeconds;
        var pulsate = (float)Math.Sin(time * 6f) + 1f;
        var scale = layoutScale * (1f + (pulsate * 0.04f * selectionFade));
        color *= screen.TransitionAlpha;
        var textSize = font.MeasureString(text);
        var origin = new Vector2(0f, textSize.Y / 2f);
        var entryWidth = textSize.X * scale;
        var entryHeight = textSize.Y * scale;
        var highlightPaddingX = 26f * layoutScale;
        var highlightPaddingY = 16f * layoutScale;
        var highlightBounds = new Rectangle(
            (int)(Position.X - highlightPaddingX),
            (int)(Position.Y - (entryHeight / 2f) - (highlightPaddingY / 2f)),
            (int)(entryWidth + (highlightPaddingX * 2f)),
            (int)(entryHeight + highlightPaddingY));

        if (selectionFade > 0f)
        {
            var highlightAlpha = screen.TransitionAlpha * (0.16f + (selectionFade * 0.32f));
            DesktopUiChrome.DrawPanel(screenManager, spriteBatch, highlightBounds, highlightAlpha, new Color(34, 49, 72), new Color(176, 194, 224), new Color(255, 221, 96));

            var accentHeight = Math.Max(1, highlightBounds.Height - (int)(18f * layoutScale));
            var accentBounds = new Rectangle(highlightBounds.X + (int)(12f * layoutScale), highlightBounds.Y + (int)(9f * layoutScale), Math.Max(3, (int)(4f * layoutScale)), accentHeight);
            spriteBatch.Draw(screenManager.BlankTexture, accentBounds, new Color(255, 221, 96) * (screen.TransitionAlpha * selectionFade));

            var indicatorPosition = new Vector2(highlightBounds.X + (16f * layoutScale), Position.Y);
            var indicatorOrigin = new Vector2(0f, font.LineSpacing / 2f);
            spriteBatch.DrawString(font, ">", indicatorPosition + shadowOffset, shadowColor, 0f, indicatorOrigin, layoutScale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, ">", indicatorPosition, new Color(255, 221, 96) * (screen.TransitionAlpha * selectionFade), 0f, indicatorOrigin, layoutScale, SpriteEffects.None, 0f);
        }

        var textPosition = Position + new Vector2(selectionFade * (12f * layoutScale), 0f);
        spriteBatch.DrawString(font, text, textPosition + shadowOffset, shadowColor, 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, text, textPosition, color, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    public virtual int GetHeight(MenuScreen screen) => (int)(screen.ScreenManager.Font.LineSpacing * screen.LayoutScale);
    public virtual int GetWidth(MenuScreen screen) => (int)(screen.ScreenManager.Font.MeasureString(Text).X * screen.LayoutScale);
}
