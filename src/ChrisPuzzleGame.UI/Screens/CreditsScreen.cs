using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public sealed class CreditsScreen : GameScreen
{
    private const string BodyText = "Created by Christian Helle.\n\nOriginally released as an XNA puzzle game for Windows Phone 7.\n\nThis MonoGame desktop shell keeps the menu flow, transitions, and input affordances ready for the gameplay layer.";
    public CreditsScreen()
    {
        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.25);
        TransitionOffTime = TimeSpan.FromSeconds(0.25);
    }
    public override void HandleInput(InputState input)
    {
        if (input.IsMenuSelect(ControllingPlayer, out _) || input.IsMenuCancel(ControllingPlayer, out _) || input.IsNewLeftClick(ControllingPlayer, out _)) ExitScreen();
    }
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var safeArea = ScreenManager.GetSafeArea(0.18f, 0.16f);
        var panelBounds = new Rectangle(safeArea.X, safeArea.Y, safeArea.Width, safeArea.Height);
        var wrappedBody = TextLayout.WrapText(font, BodyText, panelBounds.Width - 96f);
        var title = "Credits";
        var prompt = "Press Enter, Esc, or click to return.";
        ScreenManager.FadeBackBufferToBlack(0.54f * TransitionAlpha);
        spriteBatch.Begin();
        spriteBatch.Draw(ScreenManager.GradientTexture, panelBounds, Color.White * (0.92f * TransitionAlpha));
        spriteBatch.Draw(ScreenManager.BlankTexture, panelBounds, new Color(20, 28, 40) * (0.78f * TransitionAlpha));
        var titleOrigin = font.MeasureString(title) / 2f;
        var titlePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + 52f);
        spriteBatch.DrawString(font, title, titlePosition + new Vector2(3f, 3f), Color.Black * (TransitionAlpha * 0.55f), 0f, titleOrigin, 1.3f, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, title, titlePosition, Color.White * TransitionAlpha, 0f, titleOrigin, 1.3f, SpriteEffects.None, 0f);
        var bodyPosition = new Vector2(panelBounds.X + 48f, panelBounds.Y + 136f);
        spriteBatch.DrawString(font, wrappedBody, bodyPosition + new Vector2(2f, 2f), Color.Black * (TransitionAlpha * 0.5f));
        spriteBatch.DrawString(font, wrappedBody, bodyPosition, new Color(226, 232, 240) * TransitionAlpha);
        var url = "christianhelle.com";
        var urlPosition = new Vector2(panelBounds.X + 48f, panelBounds.Bottom - 132f);
        spriteBatch.DrawString(font, url, urlPosition, new Color(140, 190, 255) * TransitionAlpha);
        var promptOrigin = font.MeasureString(prompt) / 2f;
        var promptPosition = new Vector2(panelBounds.Center.X, panelBounds.Bottom - 40f);
        spriteBatch.DrawString(font, prompt, promptPosition, new Color(190, 196, 205) * TransitionAlpha, 0f, promptOrigin, 0.6f, SpriteEffects.None, 0f);
        spriteBatch.End();
    }
}
