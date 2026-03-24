using System;
using System.Collections.Generic;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.Screens;

public abstract class MenuScreen : GameScreen
{
    private const int MenuEntryPadding = 12;
    private readonly List<MenuEntry> menuEntries = [];
    private readonly string menuTitle;
    private int selectedEntry;
    protected MenuScreen(string menuTitle)
    {
        this.menuTitle = menuTitle;
        TransitionOnTime = TimeSpan.FromSeconds(0.35);
        TransitionOffTime = TimeSpan.FromSeconds(0.25);
    }
    protected IList<MenuEntry> MenuEntries => menuEntries;
    protected virtual float TitleScale => 1.45f;
    protected virtual string FooterText => "Arrow keys or mouse to move - Enter or click to choose - Esc to go back";
    protected virtual Rectangle GetMenuEntryHitBounds(MenuEntry entry)
    {
        var width = entry.GetWidth(this) + 48;
        var height = entry.GetHeight(this) + (MenuEntryPadding * 2);
        return new Rectangle((int)entry.Position.X - 24, (int)(entry.Position.Y - (entry.GetHeight(this) / 2f) - MenuEntryPadding), width, height);
    }
    public override void HandleInput(InputState input)
    {
        if (menuEntries.Count == 0) return;
        UpdateMenuEntryLocations();
        var hoveredEntryIndex = GetHoveredEntryIndex(input);
        if (hoveredEntryIndex.HasValue) selectedEntry = hoveredEntryIndex.Value;
        if (input.IsMenuUp(ControllingPlayer)) selectedEntry = (selectedEntry - 1 + menuEntries.Count) % menuEntries.Count;
        else if (input.IsMenuDown(ControllingPlayer)) selectedEntry = (selectedEntry + 1) % menuEntries.Count;
        if (input.IsMenuSelect(ControllingPlayer, out var playerIndex)) OnSelectEntry(selectedEntry, playerIndex);
        else if (input.IsNewLeftClick(ControllingPlayer, out playerIndex) && hoveredEntryIndex.HasValue) OnSelectEntry(hoveredEntryIndex.Value, playerIndex);
        else if (input.IsMenuCancel(ControllingPlayer, out playerIndex)) OnCancel(playerIndex);
    }
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        if (menuEntries.Count == 0) return;
        selectedEntry = Math.Clamp(selectedEntry, 0, menuEntries.Count - 1);
        for (var i = 0; i < menuEntries.Count; i++)
        {
            var isSelected = IsActive && i == selectedEntry;
            menuEntries[i].Update(this, isSelected, gameTime);
        }
    }
    public override void Draw(GameTime gameTime)
    {
        UpdateMenuEntryLocations();
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var safeArea = ScreenManager.GetSafeArea(0.12f, 0.12f);
        spriteBatch.Begin();
        for (var i = 0; i < menuEntries.Count; i++)
        {
            var isSelected = IsActive && i == selectedEntry;
            menuEntries[i].Draw(this, isSelected, gameTime);
        }
        var transitionOffset = (float)Math.Pow(TransitionPosition, 2);
        var titlePosition = new Vector2(safeArea.Center.X, safeArea.Top + (safeArea.Height * 0.14f));
        var titleOrigin = font.MeasureString(menuTitle) / 2f;
        var titleColor = new Color(232, 236, 242) * TransitionAlpha;
        titlePosition.Y -= transitionOffset * 72f;
        spriteBatch.DrawString(font, menuTitle, titlePosition + new Vector2(4f, 4f), Color.Black * (TransitionAlpha * 0.65f), 0f, titleOrigin, TitleScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0f, titleOrigin, TitleScale, SpriteEffects.None, 0f);
        if (!string.IsNullOrWhiteSpace(FooterText))
        {
            const float footerScale = 0.6f;
            var footerPosition = new Vector2(safeArea.Center.X, safeArea.Bottom - 18f);
            var footerOrigin = font.MeasureString(FooterText) * 0.5f;
            var footerColor = new Color(190, 196, 205) * TransitionAlpha;
            spriteBatch.DrawString(font, FooterText, footerPosition, footerColor, 0f, footerOrigin, footerScale, SpriteEffects.None, 0f);
        }
        spriteBatch.End();
    }
    protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex) => menuEntries[entryIndex].OnSelectEntry(playerIndex);
    protected virtual void OnCancel(PlayerIndex playerIndex) => ExitScreen();
    protected void OnCancel(object? sender, PlayerIndexEventArgs e) => OnCancel(e.PlayerIndex);
    protected virtual void UpdateMenuEntryLocations()
    {
        if (menuEntries.Count == 0) return;
        var safeArea = ScreenManager.GetSafeArea(0.12f, 0.12f);
        var transitionOffset = (float)Math.Pow(TransitionPosition, 2);
        var totalMenuHeight = 0f;
        foreach (var menuEntry in menuEntries) totalMenuHeight += menuEntry.GetHeight(this) + (MenuEntryPadding * 2);
        var startY = MathF.Max(safeArea.Top + (safeArea.Height * 0.30f), safeArea.Center.Y - (totalMenuHeight / 2f));
        var position = new Vector2(0f, startY + (ScreenManager.Font.LineSpacing / 2f));
        foreach (var menuEntry in menuEntries)
        {
            position.X = safeArea.Center.X - (menuEntry.GetWidth(this) / 2f);
            if (ScreenState == ScreenState.TransitionOn) position.X -= transitionOffset * 192f;
            else position.X += transitionOffset * 320f;
            menuEntry.Position = position;
            position.Y += menuEntry.GetHeight(this) + (MenuEntryPadding * 2);
        }
    }
    private int? GetHoveredEntryIndex(InputState input)
    {
        for (var i = 0; i < menuEntries.Count; i++) if (GetMenuEntryHitBounds(menuEntries[i]).Contains(input.MousePosition)) return i;
        return null;
    }
}
