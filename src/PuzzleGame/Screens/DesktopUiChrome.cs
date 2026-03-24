using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

internal static class DesktopUiChrome
{
    public static float GetViewportScale(Viewport viewport)
    {
        var widthScale = viewport.Width / 1280f;
        var heightScale = viewport.Height / 720f;
        return MathHelper.Clamp(MathF.Min(widthScale, heightScale), 0.62f, 1.3f);
    }

    public static void DrawPanel(ScreenManager screenManager, SpriteBatch spriteBatch, Rectangle bounds, float alpha, Color fill, Color border, Color accent)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var clampedAlpha = MathHelper.Clamp(alpha, 0f, 1f);
        spriteBatch.Draw(screenManager.GradientTexture, bounds, Color.White * (0.9f * clampedAlpha));
        spriteBatch.Draw(screenManager.BlankTexture, bounds, fill * clampedAlpha);

        var accentHeight = Math.Clamp(bounds.Height / 26, 3, 10);
        var accentBounds = new Rectangle(bounds.X + 2, bounds.Y + 2, Math.Max(0, bounds.Width - 4), accentHeight);
        spriteBatch.Draw(screenManager.BlankTexture, accentBounds, accent * clampedAlpha);

        var sheenBounds = new Rectangle(bounds.X + 2, bounds.Bottom - 3, Math.Max(0, bounds.Width - 4), 1);
        spriteBatch.Draw(screenManager.BlankTexture, sheenBounds, Color.White * (0.08f * clampedAlpha));

        DrawBorder(screenManager, spriteBatch, bounds, border * clampedAlpha);
    }

    public static void DrawBorder(ScreenManager screenManager, SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness = 2)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var texture = screenManager.BlankTexture;
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, bounds.Width, thickness), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Bottom - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height), color);
    }

    public static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, Vector2 origin, float scale, float alpha)
    {
        var clampedAlpha = MathHelper.Clamp(alpha, 0f, 1f);
        spriteBatch.DrawString(font, text, position + new Vector2(2f, 2f), Color.Black * (0.5f * clampedAlpha), 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, text, position, color * clampedAlpha, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale, float alpha)
    {
        DrawShadowedString(spriteBatch, font, text, position, color, Vector2.Zero, scale, alpha);
    }
}