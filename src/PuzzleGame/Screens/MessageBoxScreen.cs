using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

public sealed class MessageBoxScreen : GameScreen
{
    private readonly string acceptText;
    private readonly string cancelText;
    private readonly bool includeCancel;
    private readonly string message;
    public MessageBoxScreen(string message, bool includeCancel = true, string acceptText = "OK", string cancelText = "Cancel")
    {
        this.message = message;
        this.includeCancel = includeCancel;
        this.acceptText = acceptText;
        this.cancelText = cancelText;
        IsPopup = true;
        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }
    public event EventHandler<PlayerIndexEventArgs>? Accepted;
    public event EventHandler<PlayerIndexEventArgs>? Cancelled;
    public override void HandleInput(InputState input)
    {
        if (input.IsMenuSelect(ControllingPlayer, out var playerIndex)) { OnAccept(playerIndex); return; }
        if (input.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            if (includeCancel) OnCancel(playerIndex); else ExitScreen();
            return;
        }
        if (!input.IsNewLeftClick(ControllingPlayer, out playerIndex)) return;
        var (_, acceptBounds, cancelBounds, _) = GetLayout();
        if (acceptBounds.Contains(input.MousePosition)) OnAccept(playerIndex);
        else if (includeCancel && cancelBounds.Contains(input.MousePosition)) OnCancel(playerIndex);
    }
    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var input = ScreenManager.Input;
        ScreenManager.FadeBackBufferToBlack(0.62f * TransitionAlpha);
        var (dialogBounds, acceptBounds, cancelBounds, wrappedMessage) = GetLayout();
        var mousePosition = input.MousePosition;
        spriteBatch.Begin();
        spriteBatch.Draw(ScreenManager.GradientTexture, dialogBounds, Color.White * (0.92f * TransitionAlpha));
        spriteBatch.Draw(ScreenManager.BlankTexture, dialogBounds, new Color(15, 21, 32) * (0.7f * TransitionAlpha));
        DrawBorder(spriteBatch, dialogBounds, new Color(110, 131, 168) * TransitionAlpha);
        var messagePosition = new Vector2(dialogBounds.X + 28f, dialogBounds.Y + 24f);
        spriteBatch.DrawString(font, wrappedMessage, messagePosition + new Vector2(2f, 2f), Color.Black * (TransitionAlpha * 0.5f));
        spriteBatch.DrawString(font, wrappedMessage, messagePosition, Color.White * TransitionAlpha);
        DrawButton(spriteBatch, font, acceptText, acceptBounds, acceptBounds.Contains(mousePosition));
        if (includeCancel) DrawButton(spriteBatch, font, cancelText, cancelBounds, cancelBounds.Contains(mousePosition));
        spriteBatch.End();
    }
    private void OnAccept(PlayerIndex playerIndex) { Accepted?.Invoke(this, new PlayerIndexEventArgs(playerIndex)); ExitScreen(); }
    private void OnCancel(PlayerIndex playerIndex) { Cancelled?.Invoke(this, new PlayerIndexEventArgs(playerIndex)); ExitScreen(); }
    private (Rectangle DialogBounds, Rectangle AcceptBounds, Rectangle CancelBounds, string WrappedMessage) GetLayout()
    {
        var safeArea = ScreenManager.GetSafeArea(0.18f, 0.18f);
        var font = ScreenManager.Font;
        var wrappedMessage = TextLayout.WrapText(font, message, safeArea.Width * 0.72f);
        var textSize = font.MeasureString(wrappedMessage);
        var buttonWidth = 180;
        var buttonHeight = 54;
        var dialogWidth = (int)MathF.Min(safeArea.Width, MathF.Max(460f, textSize.X + 56f));
        var dialogHeight = (int)MathF.Min(safeArea.Height, MathF.Max(220f, textSize.Y + 128f));
        var dialogBounds = new Rectangle(safeArea.Center.X - (dialogWidth / 2), safeArea.Center.Y - (dialogHeight / 2), dialogWidth, dialogHeight);
        var buttonsY = dialogBounds.Bottom - buttonHeight - 24;
        var acceptX = includeCancel ? dialogBounds.Center.X - buttonWidth - 10 : dialogBounds.Center.X - (buttonWidth / 2);
        var acceptBounds = new Rectangle(acceptX, buttonsY, buttonWidth, buttonHeight);
        var cancelBounds = includeCancel ? new Rectangle(dialogBounds.Center.X + 10, buttonsY, buttonWidth, buttonHeight) : Rectangle.Empty;
        return (dialogBounds, acceptBounds, cancelBounds, wrappedMessage);
    }
    private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        var texture = ScreenManager.BlankTexture;
        const int thickness = 2;
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, bounds.Width, thickness), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Bottom - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height), color);
    }
    private void DrawButton(SpriteBatch spriteBatch, SpriteFont font, string label, Rectangle bounds, bool hovered)
    {
        var backgroundColor = hovered ? new Color(93, 133, 196) : new Color(48, 65, 96);
        var labelSize = font.MeasureString(label);
        var labelPosition = new Vector2(bounds.Center.X - (labelSize.X / 2f), bounds.Center.Y - (labelSize.Y / 2f));
        spriteBatch.Draw(ScreenManager.BlankTexture, bounds, backgroundColor * TransitionAlpha);
        DrawBorder(spriteBatch, bounds, new Color(191, 206, 230) * TransitionAlpha);
        spriteBatch.DrawString(font, label, labelPosition + new Vector2(2f, 2f), Color.Black * (TransitionAlpha * 0.5f));
        spriteBatch.DrawString(font, label, labelPosition, Color.White * TransitionAlpha);
    }
}
