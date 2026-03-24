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
        var shadowOffset = new Vector2(3f, 3f);
        var color = isSelected ? new Color(255, 221, 96) : Color.White;
        var shadowColor = Color.Black * (screen.TransitionAlpha * 0.7f);
        var time = gameTime.TotalGameTime.TotalSeconds;
        var pulsate = (float)Math.Sin(time * 6f) + 1f;
        var scale = 1f + (pulsate * 0.04f * selectionFade);
        color *= screen.TransitionAlpha;
        var origin = new Vector2(0f, font.LineSpacing / 2f);
        spriteBatch.DrawString(font, text, Position + shadowOffset, shadowColor, 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, text, Position, color, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    public virtual int GetHeight(MenuScreen screen) => screen.ScreenManager.Font.LineSpacing;
    public virtual int GetWidth(MenuScreen screen) => (int)screen.ScreenManager.Font.MeasureString(Text).X;
}
