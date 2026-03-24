using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

public sealed class WinScreen : GameScreen
{
    private readonly int moveCount;
    private readonly TimeSpan playingTime;
    private SpriteFont? congratulationsFont;
    private SpriteFont? statsFont;

    public WinScreen(TimeSpan playingTime, int moveCount)
    {
        this.playingTime = playingTime;
        this.moveCount = moveCount;
        IsPopup = true;
        TransitionOnTime = TimeSpan.Zero;
        TransitionOffTime = TimeSpan.Zero;
    }

    public event EventHandler<PlayerIndexEventArgs>? Accepted;

    public event EventHandler<PlayerIndexEventArgs>? Cancelled;

    public override void LoadContent()
    {
        congratulationsFont = ScreenManager.Game.Content.Load<SpriteFont>("Congratulations");
        statsFont = ScreenManager.Game.Content.Load<SpriteFont>("GameTime");
    }

    public override void HandleInput(InputState input)
    {
        if (input.IsMenuSelect(ControllingPlayer, out var playerIndex))
        {
            OnAccept(playerIndex);
            return;
        }

        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            OnCancel(playerIndex);
            return;
        }

        if (!input.IsNewLeftClick(ControllingPlayer, out playerIndex))
        {
            return;
        }

        var (_, playAgainBounds, mainMenuBounds) = GetLayout();

        if (playAgainBounds.Contains(input.MousePosition))
        {
            OnAccept(playerIndex);
        }
        else if (mainMenuBounds.Contains(input.MousePosition))
        {
            OnCancel(playerIndex);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = statsFont ?? ScreenManager.Font;
        var titleFont = congratulationsFont ?? ScreenManager.Font;
        var input = ScreenManager.Input;
        var (dialogBounds, playAgainBounds, mainMenuBounds) = GetLayout();
        var title = "Congratulations!";
        var titleOrigin = titleFont.MeasureString(title) / 2f;
        var titlePosition = new Vector2(dialogBounds.Center.X, dialogBounds.Y + 72f);
        var summary = $"Time  {playingTime:mm\\:ss}\\nMoves {moveCount}";
        var summarySize = font.MeasureString(summary);
        var summaryPosition = new Vector2(
            dialogBounds.Center.X - summarySize.X / 2f,
            dialogBounds.Y + 178f);
        var prompt = "Play again or return to the main menu.";
        var promptSize = font.MeasureString(prompt);
        var promptPosition = new Vector2(
            dialogBounds.Center.X - promptSize.X / 2f,
            dialogBounds.Bottom - 122f);

        ScreenManager.FadeBackBufferToBlack(0.48f * TransitionAlpha);

        spriteBatch.Begin();
        spriteBatch.Draw(
            ScreenManager.GradientTexture,
            dialogBounds,
            Color.White * (0.92f * TransitionAlpha));
        spriteBatch.Draw(
            ScreenManager.BlankTexture,
            dialogBounds,
            new Color(r: 12, g: 17, b: 28) * (0.82f * TransitionAlpha));
        DrawBorder(spriteBatch, dialogBounds, new Color(r: 195, g: 206, b: 226) * TransitionAlpha);
        spriteBatch.DrawString(
            titleFont,
            title,
            titlePosition + new Vector2(x: 4f, y: 4f),
            Color.Black * (TransitionAlpha * 0.55f),
            rotation: 0f,
            titleOrigin,
            scale: 1f,
            SpriteEffects.None,
            layerDepth: 0f);
        spriteBatch.DrawString(
            titleFont,
            title,
            titlePosition,
            new Color(r: 255, g: 237, b: 178) * TransitionAlpha,
            rotation: 0f,
            titleOrigin,
            scale: 1f,
            SpriteEffects.None,
            layerDepth: 0f);
        spriteBatch.DrawString(
            font,
            summary,
            summaryPosition + new Vector2(x: 2f, y: 2f),
            Color.Black * (TransitionAlpha * 0.5f));
        spriteBatch.DrawString(font, summary, summaryPosition, Color.White * TransitionAlpha);
        spriteBatch.DrawString(
            font,
            prompt,
            promptPosition + new Vector2(x: 2f, y: 2f),
            Color.Black * (TransitionAlpha * 0.5f));
        spriteBatch.DrawString(
            font,
            prompt,
            promptPosition,
            new Color(r: 220, g: 228, b: 240) * TransitionAlpha);
        DrawButton(
            spriteBatch,
            ScreenManager.Font,
            "Play Again",
            playAgainBounds,
            playAgainBounds.Contains(input.MousePosition));
        DrawButton(
            spriteBatch,
            ScreenManager.Font,
            "Main Menu",
            mainMenuBounds,
            mainMenuBounds.Contains(input.MousePosition));
        spriteBatch.End();
    }

    private void OnAccept(PlayerIndex playerIndex)
    {
        Accepted?.Invoke(this, new(playerIndex));
        ExitScreen();
    }

    private void OnCancel(PlayerIndex playerIndex)
    {
        Cancelled?.Invoke(this, new(playerIndex));
        ExitScreen();
    }

    private (Rectangle DialogBounds, Rectangle PlayAgainBounds, Rectangle MainMenuBounds)
        GetLayout()
    {
        var safeArea = ScreenManager.GetSafeArea(
            horizontalMarginRatio: 0.18f,
            verticalMarginRatio: 0.18f);
        var dialogWidth = Math.Min(safeArea.Width, val2: 620);
        var dialogHeight = Math.Min(safeArea.Height, val2: 400);
        var dialogBounds = new Rectangle(
            safeArea.Center.X - dialogWidth / 2,
            safeArea.Center.Y - dialogHeight / 2,
            dialogWidth,
            dialogHeight);

        const int buttonWidth = 188;
        const int buttonHeight = 54;
        var buttonsY = dialogBounds.Bottom - buttonHeight - 30;
        var playAgainBounds = new Rectangle(
            dialogBounds.Center.X - buttonWidth - 12,
            buttonsY,
            buttonWidth,
            buttonHeight);
        var mainMenuBounds = new Rectangle(
            dialogBounds.Center.X + 12,
            buttonsY,
            buttonWidth,
            buttonHeight);

        return (dialogBounds, playAgainBounds, mainMenuBounds);
    }

    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        var texture = ScreenManager.BlankTexture;
        const int thickness = 2;
        spriteBatch.Draw(
            texture,
            new Rectangle(bounds.Left, bounds.Top, bounds.Width, thickness),
            color);
        spriteBatch.Draw(
            texture,
            new Rectangle(bounds.Left, bounds.Bottom - thickness, bounds.Width, thickness),
            color);
        spriteBatch.Draw(
            texture,
            new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height),
            color);
        spriteBatch.Draw(
            texture,
            new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height),
            color);
    }

    private void DrawButton(
        SpriteBatch spriteBatch,
        SpriteFont font,
        string label,
        Rectangle bounds,
        bool hovered)
    {
        var backgroundColor = hovered ? new(r: 93, g: 133, b: 196) : new Color(r: 48, g: 65, b: 96);
        var labelSize = font.MeasureString(label);
        var labelPosition = new Vector2(
            bounds.Center.X - labelSize.X / 2f,
            bounds.Center.Y - labelSize.Y / 2f);

        spriteBatch.Draw(ScreenManager.BlankTexture, bounds, backgroundColor * TransitionAlpha);
        DrawBorder(spriteBatch, bounds, new Color(r: 191, g: 206, b: 230) * TransitionAlpha);
        spriteBatch.DrawString(
            font,
            label,
            labelPosition + new Vector2(x: 2f, y: 2f),
            Color.Black * (TransitionAlpha * 0.5f));
        spriteBatch.DrawString(font, label, labelPosition, Color.White * TransitionAlpha);
    }
}
