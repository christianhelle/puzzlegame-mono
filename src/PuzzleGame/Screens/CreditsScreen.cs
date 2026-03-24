using System;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public sealed class CreditsScreen : GameScreen
{
    private static readonly (string Label, string Value)[] Details =
    [
        ("Title", "Chris' Puzzle Game"),
        ("Author", "Christian Resma Helle"),
        ("Publisher", "Commentor AppFabric"),
        ("Website", "christianhelle.com"),
    ];

    private const string BodyText = "Originally released as an XNA puzzle game for Windows Phone 7, this MonoGame desktop shell keeps the menu flow, transitions, and input feel ready for the final port work.";
    public CreditsScreen()
    {
        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.25);
        TransitionOffTime = TimeSpan.FromSeconds(0.25);
    }
    public override void HandleInput(InputState input)
    {
        if (input.IsMenuSelect(ControllingPlayer, out _) || input.IsMenuCancel(ControllingPlayer, out _) || input.IsNewLeftClick(ControllingPlayer, out _) || input.IsNewRightClick(ControllingPlayer, out _)) ExitScreen();
    }
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var layoutScale = DesktopUiChrome.GetViewportScale(viewport);
        var safeArea = ScreenManager.GetSafeArea(0.16f, 0.14f);
        var titleScale = MathHelper.Clamp(1.15f * layoutScale, 1f, 1.3f);
        var labelScale = MathHelper.Clamp(0.62f * layoutScale, 0.58f, 0.78f);
        var valueScale = MathHelper.Clamp(0.78f * layoutScale, 0.72f, 0.94f);
        var bodyScale = MathHelper.Clamp(0.64f * layoutScale, 0.6f, 0.8f);
        var promptScale = MathHelper.Clamp(0.56f * layoutScale, 0.5f, 0.72f);
        var panelWidth = Math.Clamp(560f * layoutScale, Math.Min(420f, safeArea.Width), safeArea.Width);
        var wrappedBody = TextLayout.WrapText(font, BodyText, (panelWidth - (72f * layoutScale)) / bodyScale);
        var bodySize = font.MeasureString(wrappedBody);
        var detailRowHeight = Math.Max(font.LineSpacing * valueScale, font.LineSpacing * labelScale) + (18f * layoutScale);
        var desiredHeight = (145f * layoutScale) + (Details.Length * detailRowHeight) + (bodySize.Y * bodyScale) + (90f * layoutScale);
        var panelHeight = Math.Clamp(desiredHeight, Math.Min(320f, safeArea.Height), safeArea.Height);
        var panelBounds = new Rectangle(
            safeArea.Center.X - (int)(panelWidth / 2f),
            safeArea.Center.Y - (int)(panelHeight / 2f),
            (int)panelWidth,
            (int)panelHeight);
        var title = "Credits";
        var prompt = "Enter, Esc, click, or right-click to return";
        ScreenManager.FadeBackBufferToBlack(0.54f * TransitionAlpha);
        spriteBatch.Begin();
        DesktopUiChrome.DrawPanel(ScreenManager, spriteBatch, panelBounds, TransitionAlpha, new Color(15, 22, 34), new Color(110, 131, 168), new Color(69, 116, 184));

        var titleOrigin = font.MeasureString(title) / 2f;
        var titlePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + (42f * layoutScale));
        DesktopUiChrome.DrawShadowedString(spriteBatch, font, title, titlePosition, Color.White, titleOrigin, titleScale, TransitionAlpha);

        var subtitle = "Original release details";
        var subtitleOrigin = font.MeasureString(subtitle) / 2f;
        var subtitlePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + (78f * layoutScale));
        DesktopUiChrome.DrawShadowedString(spriteBatch, font, subtitle, subtitlePosition, new Color(193, 205, 224), subtitleOrigin, labelScale, TransitionAlpha);

        var labelWidth = 0f;
        foreach (var detail in Details) labelWidth = MathF.Max(labelWidth, font.MeasureString(detail.Label.ToUpperInvariant()).X * labelScale);

        var labelX = panelBounds.X + (36f * layoutScale);
        var valueX = labelX + labelWidth + (18f * layoutScale);
        var rowY = panelBounds.Y + (114f * layoutScale);
        for (var i = 0; i < Details.Length; i++)
        {
            var (label, value) = Details[i];
            var labelText = label.ToUpperInvariant();
            var labelPosition = new Vector2(labelX, rowY);
            DesktopUiChrome.DrawShadowedString(spriteBatch, font, labelText, labelPosition, new Color(177, 189, 206), labelScale, TransitionAlpha);

            var valueColor = label == "Website" ? new Color(140, 190, 255) : new Color(226, 232, 240);
            var valuePosition = new Vector2(valueX, rowY - (2f * layoutScale));
            DesktopUiChrome.DrawShadowedString(spriteBatch, font, value, valuePosition, valueColor, valueScale, TransitionAlpha);

            if (i < Details.Length - 1)
            {
                var dividerY = (int)(rowY + (font.LineSpacing * valueScale) + (6f * layoutScale));
                var dividerBounds = new Rectangle(panelBounds.X + (int)(28f * layoutScale), dividerY, panelBounds.Width - (int)(56f * layoutScale), 1);
                spriteBatch.Draw(ScreenManager.BlankTexture, dividerBounds, new Color(183, 196, 219) * (TransitionAlpha * 0.18f));
            }

            rowY += detailRowHeight;
        }

        var bodyPosition = new Vector2(panelBounds.X + (36f * layoutScale), rowY + (8f * layoutScale));
        DesktopUiChrome.DrawShadowedString(spriteBatch, font, wrappedBody, bodyPosition, new Color(226, 232, 240), bodyScale, TransitionAlpha);

        var promptOrigin = font.MeasureString(prompt) / 2f;
        var promptPosition = new Vector2(panelBounds.Center.X, panelBounds.Bottom - (24f * layoutScale));
        DesktopUiChrome.DrawShadowedString(spriteBatch, font, prompt, promptPosition, new Color(190, 196, 205), promptOrigin, promptScale, TransitionAlpha);
        spriteBatch.End();
    }
}
