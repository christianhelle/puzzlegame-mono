using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

public sealed class ImageSelectionScreen : GameScreen
{
    private const int ThumbnailWidth = 200;
    private const int ThumbnailHeight = 150;
    private const int GridPadding = 20;
    private const int ItemSpacing = 24;

    private readonly List<SelectionItem> items = [];
    private readonly Dictionary<string, Texture2D> thumbnailCache = [];
    private int selectedIndex;
    private int scrollOffset;
    private Rectangle lastViewportBounds;
    private bool layoutDirty = true;

    public ImageSelectionScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.35);
        TransitionOffTime = TimeSpan.FromSeconds(0.25);
        BuildItemsList();
    }

    private record SelectionItem(string Name, string AssetPath, Rectangle Bounds, bool IsRandom);

    private float LayoutScale
        => DesktopUiChrome.GetViewportScale(ScreenManager.GraphicsDevice.Viewport);

    public override void LoadContent()
    {
        foreach (var item in items)
        {
            if (item.IsRandom) continue;

            try
            {
                var texture = ScreenManager.Game.Content.Load<Texture2D>(item.AssetPath);
                thumbnailCache[item.Name] = texture;
            }
            catch
            {
            }
        }
    }

    public override void UnloadContent()
    {
        thumbnailCache.Clear();
    }

    public override void HandleInput(InputState input)
    {
        if (items.Count == 0) return;

        UpdateLayout();
        var hoveredIndex = GetHoveredItemIndex(input);
        if (hoveredIndex.HasValue)
        {
            selectedIndex = hoveredIndex.Value;
        }

        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var safeArea = ScreenManager.GetSafeArea(
            horizontalMarginRatio: 0.08f,
            verticalMarginRatio: 0.08f);

        if (input.IsMenuUp(ControllingPlayer))
        {
            selectedIndex = Math.Max(0, selectedIndex - GetColumnsPerRow(safeArea.Width));
        }
        else if (input.IsMenuDown(ControllingPlayer))
        {
            selectedIndex = Math.Min(items.Count - 1, selectedIndex + GetColumnsPerRow(safeArea.Width));
        }
        else if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _)
              || input.IsNewKeyPress(Keys.A, ControllingPlayer, out _)
              || input.IsNewButtonPress(Buttons.DPadLeft, ControllingPlayer, out _)
              || input.IsNewButtonPress(Buttons.LeftThumbstickLeft, ControllingPlayer, out _))
        {
            selectedIndex = Math.Max(0, selectedIndex - 1);
        }
        else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _)
              || input.IsNewKeyPress(Keys.D, ControllingPlayer, out _)
              || input.IsNewButtonPress(Buttons.DPadRight, ControllingPlayer, out _)
              || input.IsNewButtonPress(Buttons.LeftThumbstickRight, ControllingPlayer, out _))
        {
            selectedIndex = Math.Min(items.Count - 1, selectedIndex + 1);
        }

        if (input.MouseWheelDelta != 0)
        {
            scrollOffset -= (int)(input.MouseWheelDelta * 0.5f);
            scrollOffset = Math.Max(0, scrollOffset);
            layoutDirty = true;
        }

        if (input.IsMenuSelect(ControllingPlayer, out var playerIndex))
        {
            OnSelectItem(selectedIndex, playerIndex);
        }
        else if (input.IsNewLeftClick(ControllingPlayer, out playerIndex) && hoveredIndex.HasValue)
        {
            OnSelectItem(hoveredIndex.Value, playerIndex);
        }
        else if (input.IsMenuCancel(ControllingPlayer, out playerIndex)
              || input.IsNewRightClick(ControllingPlayer, out playerIndex))
        {
            OnCancel(playerIndex);
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        selectedIndex = Math.Clamp(selectedIndex, min: 0, items.Count - 1);

        var viewport = ScreenManager.GraphicsDevice.Viewport;
        if (viewport.Bounds != lastViewportBounds)
        {
            lastViewportBounds = viewport.Bounds;
            layoutDirty = true;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        UpdateLayout();

        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var safeArea = ScreenManager.GetSafeArea(
            horizontalMarginRatio: 0.08f,
            verticalMarginRatio: 0.08f);

        var panelBounds = new Rectangle(
            safeArea.X,
            safeArea.Y,
            safeArea.Width,
            safeArea.Height);

        spriteBatch.Begin();

        DesktopUiChrome.DrawPanel(
            ScreenManager,
            spriteBatch,
            panelBounds,
            TransitionAlpha,
            new(r: 14, g: 22, b: 34),
            new(r: 108, g: 128, b: 160),
            new(r: 69, g: 116, b: 184));

        var titleScale = MathHelper.Clamp(1.25f * LayoutScale, min: 1.15f, max: 1.55f);
        var titleText = "Choose a Puzzle";
        var titlePosition = new Vector2(panelBounds.Center.X, panelBounds.Y + 54f * LayoutScale);
        var titleOrigin = font.MeasureString(titleText) / 2f;
        var titleColor = new Color(r: 232, g: 236, b: 242);

        DesktopUiChrome.DrawShadowedString(
            spriteBatch,
            font,
            titleText,
            titlePosition,
            titleColor,
            titleOrigin,
            titleScale,
            TransitionAlpha);

        var titleRuleBounds = new Rectangle(
            panelBounds.X + (int)(30f * LayoutScale),
            panelBounds.Y + (int)(92f * LayoutScale),
            panelBounds.Width - (int)(60f * LayoutScale),
            height: 1);
        spriteBatch.Draw(
            ScreenManager.BlankTexture,
            titleRuleBounds,
            new Color(r: 174, g: 189, b: 214) * (TransitionAlpha * 0.38f));

        var contentTop = panelBounds.Y + (int)(110f * LayoutScale);
        var contentHeight = panelBounds.Height - (int)(140f * LayoutScale);
        var scissorRect = new Rectangle(
            panelBounds.X,
            contentTop,
            panelBounds.Width,
            contentHeight);

        spriteBatch.End();
        spriteBatch.Begin(rasterizerState: new RasterizerState { ScissorTestEnable = true });
        ScreenManager.GraphicsDevice.ScissorRectangle = scissorRect;

        DrawItems(spriteBatch, font, gameTime);

        spriteBatch.End();
        spriteBatch.Begin();

        var footerText = "Arrow keys or mouse to select | Enter or click to choose | Esc or right-click to go back";
        var footerScale = MathHelper.Clamp(0.58f * LayoutScale, min: 0.5f, max: 0.72f);
        var wrappedFooter = TextLayout.WrapText(
            font,
            footerText,
            (safeArea.Width - 48f * LayoutScale) / footerScale);
        var footerPosition = new Vector2(
            safeArea.Center.X,
            safeArea.Bottom - 12f * LayoutScale);
        var footerOrigin = new Vector2(
            font.MeasureString(wrappedFooter).X / 2f,
            font.MeasureString(wrappedFooter).Y);

        DesktopUiChrome.DrawShadowedString(
            spriteBatch,
            font,
            wrappedFooter,
            footerPosition,
            new(r: 190, g: 196, b: 205),
            footerOrigin,
            footerScale,
            TransitionAlpha);

        spriteBatch.End();
    }

    private void BuildItemsList()
    {
        items.Clear();
        items.Add(new SelectionItem("Random", string.Empty, Rectangle.Empty, IsRandom: true));

        foreach (var imageName in GameplayScreen.PuzzleImages)
        {
            var assetPath = $"Puzzles/{imageName}";
            items.Add(new SelectionItem(imageName, assetPath, Rectangle.Empty, IsRandom: false));
        }
    }

    private void UpdateLayout()
    {
        if (!layoutDirty) return;

        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var safeArea = ScreenManager.GetSafeArea(
            horizontalMarginRatio: 0.08f,
            verticalMarginRatio: 0.08f);

        var scaledThumbWidth = (int)(ThumbnailWidth * LayoutScale);
        var scaledThumbHeight = (int)(ThumbnailHeight * LayoutScale);
        var scaledSpacing = (int)(ItemSpacing * LayoutScale);
        var scaledPadding = (int)(GridPadding * LayoutScale);

        var contentTop = safeArea.Y + (int)(110f * LayoutScale);
        var contentWidth = safeArea.Width - scaledPadding * 2;
        var columns = GetColumnsPerRow(contentWidth);

        var x = safeArea.X + scaledPadding;
        var y = contentTop + scaledPadding - scrollOffset;
        var column = 0;

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var bounds = new Rectangle(x, y, scaledThumbWidth, scaledThumbHeight);
            items[i] = item with { Bounds = bounds };

            column++;
            if (column >= columns)
            {
                column = 0;
                x = safeArea.X + scaledPadding;
                y += scaledThumbHeight + scaledSpacing;
            }
            else
            {
                x += scaledThumbWidth + scaledSpacing;
            }
        }

        layoutDirty = false;
    }

    private void DrawItems(SpriteBatch spriteBatch, SpriteFont font, GameTime gameTime)
    {
        var highlightColor = new Color(r: 255, g: 221, b: 96);
        var normalBorderColor = new Color(r: 108, g: 128, b: 160);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var isSelected = IsActive && i == selectedIndex;
            var alpha = TransitionAlpha;

            var borderColor = isSelected ? highlightColor : normalBorderColor;
            DesktopUiChrome.DrawBorder(
                ScreenManager,
                spriteBatch,
                item.Bounds,
                borderColor * alpha,
                thickness: isSelected ? 3 : 2);

            if (item.IsRandom)
            {
                var bgColor = new Color(r: 30, g: 40, b: 60);
                spriteBatch.Draw(
                    ScreenManager.BlankTexture,
                    item.Bounds,
                    bgColor * alpha);

                var questionMark = "?";
                var qmScale = 3.0f * LayoutScale;
                var qmSize = font.MeasureString(questionMark) * qmScale;
                var qmPosition = new Vector2(
                    item.Bounds.Center.X,
                    item.Bounds.Center.Y);
                var qmOrigin = font.MeasureString(questionMark) / 2f;

                DesktopUiChrome.DrawShadowedString(
                    spriteBatch,
                    font,
                    questionMark,
                    qmPosition,
                    Color.White,
                    qmOrigin,
                    qmScale,
                    alpha);
            }
            else if (thumbnailCache.TryGetValue(item.Name, out var texture))
            {
                var sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
                var destRect = item.Bounds;

                var sourceAspect = (float)texture.Width / texture.Height;
                var destAspect = (float)destRect.Width / destRect.Height;

                if (sourceAspect > destAspect)
                {
                    var newHeight = (int)(destRect.Width / sourceAspect);
                    var yOffset = (destRect.Height - newHeight) / 2;
                    destRect = new Rectangle(destRect.X, destRect.Y + yOffset, destRect.Width, newHeight);
                }
                else
                {
                    var newWidth = (int)(destRect.Height * sourceAspect);
                    var xOffset = (destRect.Width - newWidth) / 2;
                    destRect = new Rectangle(destRect.X + xOffset, destRect.Y, newWidth, destRect.Height);
                }

                spriteBatch.Draw(texture, destRect, sourceRect, Color.White * alpha);
            }

            var labelScale = 0.7f * LayoutScale;
            var labelPosition = new Vector2(
                item.Bounds.Center.X,
                item.Bounds.Bottom + 8f * LayoutScale);
            var labelOrigin = font.MeasureString(item.Name) / 2f;
            var labelColor = isSelected ? highlightColor : new Color(r: 200, g: 210, b: 220);

            DesktopUiChrome.DrawShadowedString(
                spriteBatch,
                font,
                item.Name,
                labelPosition,
                labelColor,
                labelOrigin,
                labelScale,
                alpha);
        }
    }

    private int GetColumnsPerRow(int availableWidth)
    {
        var scaledThumbWidth = (int)(ThumbnailWidth * LayoutScale);
        var scaledSpacing = (int)(ItemSpacing * LayoutScale);
        var scaledPadding = (int)(GridPadding * LayoutScale);

        var effectiveWidth = availableWidth - scaledPadding * 2;
        var columns = Math.Max(1, effectiveWidth / (scaledThumbWidth + scaledSpacing));
        return Math.Min(4, columns);
    }

    private int? GetHoveredItemIndex(InputState input)
    {
        var mousePos = input.MousePosition;
        if (mousePos == input.LastMouseState.Position)
        {
            return null;
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].Bounds.Contains(mousePos))
            {
                return i;
            }
        }

        return null;
    }

    private void OnSelectItem(int index, PlayerIndex playerIndex)
    {
        if (index < 0 || index >= items.Count) return;

        var item = items[index];
        GameScreen nextScreen;

        if (item.IsRandom)
        {
            nextScreen = new GameplayScreen();
        }
        else
        {
            nextScreen = new GameplayScreen(item.AssetPath);
        }

        LoadingScreen.Load(
            ScreenManager,
            loadingIsSlow: true,
            playerIndex,
            new BackgroundScreen(),
            nextScreen);
    }

    private void OnCancel(PlayerIndex playerIndex)
    {
        LoadingScreen.Load(
            ScreenManager,
            loadingIsSlow: true,
            playerIndex,
            new BackgroundScreen(),
            new MainMenuScreen());
    }
}
