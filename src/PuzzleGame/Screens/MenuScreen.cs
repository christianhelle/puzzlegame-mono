using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

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
    internal float LayoutScale => DesktopUiChrome.GetViewportScale(ScreenManager.GraphicsDevice.Viewport);
    protected virtual float TitleScale => 1.25f;
    protected virtual string FooterText => "Arrow keys, mouse wheel, or Tab to move | Enter or click to choose | Esc or right-click to go back";
    protected virtual Rectangle GetMenuEntryHitBounds(MenuEntry entry)
    {
        var padding = (int)(MenuEntryPadding * LayoutScale);
        var horizontalPadding = (int)(28f * LayoutScale);
        var width = entry.GetWidth(this) + (horizontalPadding * 2);
        var height = entry.GetHeight(this) + (padding * 2);
        return new Rectangle((int)entry.Position.X - horizontalPadding, (int)(entry.Position.Y - (entry.GetHeight(this) / 2f) - padding), width, height);
    }
    public override void HandleInput(InputState input)
    {
        if (menuEntries.Count == 0) return;
        UpdateMenuEntryLocations();
        var hoveredEntryIndex = GetHoveredEntryIndex(input);
        if (hoveredEntryIndex.HasValue) selectedEntry = hoveredEntryIndex.Value;
        if (input.IsMenuUp(ControllingPlayer) || input.MouseWheelDelta >= 120) selectedEntry = (selectedEntry - 1 + menuEntries.Count) % menuEntries.Count;
        else if (input.IsMenuDown(ControllingPlayer) || input.MouseWheelDelta <= -120) selectedEntry = (selectedEntry + 1) % menuEntries.Count;
        if (input.IsMenuSelect(ControllingPlayer, out var playerIndex)) OnSelectEntry(selectedEntry, playerIndex);
        else if (input.IsNewLeftClick(ControllingPlayer, out playerIndex) && hoveredEntryIndex.HasValue) OnSelectEntry(hoveredEntryIndex.Value, playerIndex);
        else if (input.IsNewRightClick(ControllingPlayer, out playerIndex)) OnCancel(playerIndex);
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
        var panelBounds = GetMenuPanelBounds(safeArea);
        var titleScale = MathHelper.Clamp(TitleScale * LayoutScale, 1.15f, 1.55f);
        var transitionOffset = (float)Math.Pow(TransitionPosition, 2);
        var titlePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + (54f * LayoutScale));
        var titleOrigin = font.MeasureString(menuTitle) / 2f;
        var titleColor = new Color(232, 236, 242);
        var footerScale = MathHelper.Clamp(0.58f * LayoutScale, 0.5f, 0.72f);
        var wrappedFooter = string.IsNullOrWhiteSpace(FooterText)
            ? string.Empty
            : TextLayout.WrapText(font, FooterText, (safeArea.Width - (48f * LayoutScale)) / footerScale);
        spriteBatch.Begin();
        DesktopUiChrome.DrawPanel(ScreenManager, spriteBatch, panelBounds, TransitionAlpha, new Color(14, 22, 34), new Color(108, 128, 160), new Color(69, 116, 184));

        var titleRuleBounds = new Rectangle(panelBounds.X + (int)(30f * LayoutScale), panelBounds.Y + (int)(92f * LayoutScale), panelBounds.Width - (int)(60f * LayoutScale), 1);
        spriteBatch.Draw(ScreenManager.BlankTexture, titleRuleBounds, new Color(174, 189, 214) * (TransitionAlpha * 0.38f));

        for (var i = 0; i < menuEntries.Count; i++)
        {
            var isSelected = IsActive && i == selectedEntry;
            menuEntries[i].Draw(this, isSelected, gameTime);
        }
        titlePosition.Y -= transitionOffset * 72f;
        DesktopUiChrome.DrawShadowedString(spriteBatch, font, menuTitle, titlePosition, titleColor, titleOrigin, titleScale, TransitionAlpha);

        if (!string.IsNullOrWhiteSpace(wrappedFooter))
        {
            var footerPosition = new Vector2(safeArea.Center.X, safeArea.Bottom - (12f * LayoutScale));
            var footerOrigin = new Vector2(font.MeasureString(wrappedFooter).X / 2f, font.MeasureString(wrappedFooter).Y);
            DesktopUiChrome.DrawShadowedString(spriteBatch, font, wrappedFooter, footerPosition, new Color(190, 196, 205), footerOrigin, footerScale, TransitionAlpha);
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
        var panelBounds = GetMenuPanelBounds(safeArea);
        var padding = MenuEntryPadding * LayoutScale;
        var transitionOffset = (float)Math.Pow(TransitionPosition, 2);
        var totalMenuHeight = 0f;
        foreach (var menuEntry in menuEntries) totalMenuHeight += menuEntry.GetHeight(this) + (padding * 2f);

        var contentTop = panelBounds.Y + (104f * LayoutScale);
        var contentBottom = panelBounds.Bottom - (28f * LayoutScale);
        var availableHeight = Math.Max(0f, contentBottom - contentTop);
        var startY = contentTop + MathF.Max(0f, (availableHeight - totalMenuHeight) / 2f);
        var position = new Vector2(0f, startY + (menuEntries[0].GetHeight(this) / 2f));
        foreach (var menuEntry in menuEntries)
        {
            position.X = panelBounds.Center.X - (menuEntry.GetWidth(this) / 2f);
            if (ScreenState == ScreenState.TransitionOn) position.X -= transitionOffset * (210f * LayoutScale);
            else position.X += transitionOffset * (360f * LayoutScale);
            menuEntry.Position = position;
            position.Y += menuEntry.GetHeight(this) + (padding * 2f);
        }
    }
    private Rectangle GetMenuPanelBounds(Rectangle safeArea)
    {
        var maxEntryWidth = 0f;
        var totalMenuHeight = 0f;
        var padding = MenuEntryPadding * LayoutScale;

        foreach (var menuEntry in menuEntries)
        {
            maxEntryWidth = MathF.Max(maxEntryWidth, menuEntry.GetWidth(this));
            totalMenuHeight += menuEntry.GetHeight(this) + (padding * 2f);
        }

        var desiredWidth = MathF.Max(420f * LayoutScale, maxEntryWidth + (170f * LayoutScale));
        var minWidth = MathF.Min(safeArea.Width, 360f * LayoutScale);
        var maxWidth = MathF.Min(safeArea.Width, 760f * LayoutScale);
        var panelWidth = Math.Clamp(desiredWidth, minWidth, maxWidth);

        var desiredHeight = MathF.Max(290f * LayoutScale, totalMenuHeight + (150f * LayoutScale));
        var minHeight = MathF.Min(safeArea.Height, 260f * LayoutScale);
        var maxHeight = MathF.Min(safeArea.Height, 520f * LayoutScale);
        var panelHeight = Math.Clamp(desiredHeight, minHeight, maxHeight);

        var x = safeArea.Center.X - (int)(panelWidth / 2f);
        var y = Math.Clamp(safeArea.Center.Y - (int)(panelHeight / 2f), safeArea.Top, safeArea.Bottom - (int)panelHeight);
        return new Rectangle(x, y, (int)panelWidth, (int)panelHeight);
    }
    private int? GetHoveredEntryIndex(InputState input)
    {
        for (var i = 0; i < menuEntries.Count; i++) if (GetMenuEntryHitBounds(menuEntries[i]).Contains(input.MousePosition)) return i;
        return null;
    }
}
