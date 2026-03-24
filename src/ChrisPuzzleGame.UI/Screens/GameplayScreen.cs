using System;
using ChrisPuzzleGame.Gameplay;
using ChrisPuzzleGame.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChrisPuzzleGame.Screens;

public sealed class GameplayScreen : GameScreen
{
    private const int ShuffleMoves = 240;

    private readonly Random random = new();
    private readonly PuzzleBoard board = new();

    private bool hasStarted;
    private bool solvedPopupShown;
    private int moveCount;
    private TimeSpan elapsedTime;
    private TimeSpan startedAt;

    public GameplayScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }

    public override void LoadContent()
    {
        ResetBoard();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (!IsActive || solvedPopupShown)
        {
            return;
        }

        if (!hasStarted)
        {
            startedAt = gameTime.TotalGameTime;
            hasStarted = true;
        }

        elapsedTime = gameTime.TotalGameTime - startedAt;
    }

    public override void HandleInput(InputState input)
    {
        if (input.IsPauseGame(ControllingPlayer))
        {
            ReturnToMainMenu(ControllingPlayer ?? PlayerIndex.One);
            return;
        }

        if (input.IsNewKeyPress(Keys.R, ControllingPlayer, out _))
        {
            ResetBoard();
            return;
        }

        var moved = false;

        if (input.IsNewKeyPress(Keys.Up, ControllingPlayer, out _ ) || input.IsNewKeyPress(Keys.W, ControllingPlayer, out _))
        {
            moved = board.TryMoveBlankByOffset(0, 1);
        }
        else if (input.IsNewKeyPress(Keys.Down, ControllingPlayer, out _ ) || input.IsNewKeyPress(Keys.S, ControllingPlayer, out _))
        {
            moved = board.TryMoveBlankByOffset(0, -1);
        }
        else if (input.IsNewKeyPress(Keys.Left, ControllingPlayer, out _ ) || input.IsNewKeyPress(Keys.A, ControllingPlayer, out _))
        {
            moved = board.TryMoveBlankByOffset(1, 0);
        }
        else if (input.IsNewKeyPress(Keys.Right, ControllingPlayer, out _ ) || input.IsNewKeyPress(Keys.D, ControllingPlayer, out _))
        {
            moved = board.TryMoveBlankByOffset(-1, 0);
        }
        else if (input.IsNewLeftClick(ControllingPlayer, out _))
        {
            moved = TryMoveFromMouse(input.MousePosition);
        }

        if (!moved)
        {
            return;
        }

        moveCount++;

        if (board.IsSolved)
        {
            ShowSolvedDialog(ControllingPlayer ?? PlayerIndex.One);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        var spriteBatch = ScreenManager.SpriteBatch;
        var font = ScreenManager.Font;
        var viewport = ScreenManager.GraphicsDevice.Viewport;
        var boardBounds = GetBoardBounds(viewport);
        var tileSize = boardBounds.Width / board.Size;
        var accent = new Color(245, 214, 92);
        var body = new Color(235, 240, 255);

        spriteBatch.Begin();

        DrawPanel(spriteBatch, new Rectangle(40, 24, viewport.Width - 80, 104), new Color(9, 13, 22, 170), new Color(255, 255, 255, 40));
        DrawShadowedString(spriteBatch, font, "Chris' Puzzle Game", new Vector2(64f, 38f), Color.White, 0.95f);
        DrawShadowedString(spriteBatch, font, $"Moves {moveCount}", new Vector2(68f, 82f), body, 0.7f);
        DrawShadowedString(spriteBatch, font, $"Time {elapsedTime:mm\\:ss}", new Vector2(250f, 82f), body, 0.7f);
        DrawShadowedString(spriteBatch, font, "Arrow keys / WASD / click adjacent tile", new Vector2(430f, 82f), accent, 0.62f);

        DrawPanel(spriteBatch, boardBounds, new Color(7, 11, 18, 175), new Color(255, 255, 255, 36));

        for (var index = 0; index < board.Size * board.Size; index++)
        {
            var row = index / board.Size;
            var column = index % board.Size;
            var bounds = new Rectangle(
                boardBounds.X + (column * tileSize) + 4,
                boardBounds.Y + (row * tileSize) + 4,
                tileSize - 8,
                tileSize - 8);

            var tile = board.GetTileAt(index);

            if (tile == 0)
            {
                spriteBatch.Draw(ScreenManager.BlankTexture, bounds, new Color(2, 4, 8, 200));
                DrawFrame(spriteBatch, bounds, new Color(255, 255, 255, 26));
                continue;
            }

            var fill = GetTileColor(tile);
            spriteBatch.Draw(ScreenManager.GradientTexture, bounds, fill);
            spriteBatch.Draw(ScreenManager.BlankTexture, bounds, Color.Lerp(fill, Color.White, 0.12f));
            DrawFrame(spriteBatch, bounds, new Color(255, 255, 255, 56));

            var label = tile.ToString();
            var labelSize = font.MeasureString(label);
            var labelPosition = new Vector2(bounds.Center.X, bounds.Center.Y);
            spriteBatch.DrawString(font, label, labelPosition + new Vector2(2f, 2f), Color.Black * 0.45f, 0f, labelSize / 2f, 0.95f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, label, labelPosition, Color.White, 0f, labelSize / 2f, 0.95f, SpriteEffects.None, 0f);
        }

        DrawShadowedString(spriteBatch, font, "R reshuffles    Esc returns to menu", new Vector2(60f, viewport.Height - 46f), body, 0.6f);

        spriteBatch.End();

        if (TransitionPosition > 0f)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionPosition * 0.35f);
        }
    }

    private void ResetBoard()
    {
        board.Shuffle(ShuffleMoves, random);
        moveCount = 0;
        elapsedTime = TimeSpan.Zero;
        hasStarted = false;
        solvedPopupShown = false;
    }

    private bool TryMoveFromMouse(Point mousePosition)
    {
        var bounds = GetBoardBounds(ScreenManager.GraphicsDevice.Viewport);
        if (!bounds.Contains(mousePosition))
        {
            return false;
        }

        var tileSize = bounds.Width / board.Size;
        var column = Math.Clamp((mousePosition.X - bounds.X) / tileSize, 0, board.Size - 1);
        var row = Math.Clamp((mousePosition.Y - bounds.Y) / tileSize, 0, board.Size - 1);
        return board.TryMovePosition((row * board.Size) + column);
    }

    private void ShowSolvedDialog(PlayerIndex playerIndex)
    {
        if (solvedPopupShown)
        {
            return;
        }

        solvedPopupShown = true;

        var dialog = new MessageBoxScreen(
            $"Congratulations!\\n\\nSolved in {elapsedTime:mm\\:ss} with {moveCount} moves.\\n\\nPlay again?",
            includeCancel: true,
            acceptText: "Play Again",
            cancelText: "Main Menu");

        dialog.Accepted += (_, e) =>
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new BackgroundScreen(), new GameplayScreen());
        };

        dialog.Cancelled += (_, e) =>
        {
            ReturnToMainMenu(e.PlayerIndex);
        };

        ScreenManager.AddScreen(dialog, playerIndex);
    }

    private void ReturnToMainMenu(PlayerIndex playerIndex)
    {
        LoadingScreen.Load(ScreenManager, true, playerIndex, new BackgroundScreen(), new MainMenuScreen(() => new GameplayScreen()));
    }

    private Rectangle GetBoardBounds(Viewport viewport)
    {
        var size = Math.Min(viewport.Width - 180, viewport.Height - 220);
        size = Math.Max(size, 320);
        var x = (viewport.Width - size) / 2;
        var y = 138;
        return new Rectangle(x, y, size, size);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds, Color fill, Color border)
    {
        spriteBatch.Draw(ScreenManager.BlankTexture, bounds, fill);
        DrawFrame(spriteBatch, bounds, border);
    }

    private void DrawFrame(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        var texture = ScreenManager.BlankTexture;
        const int thickness = 2;
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, bounds.Width, thickness), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Bottom - thickness, bounds.Width, thickness), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height), color);
        spriteBatch.Draw(texture, new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height), color);
    }

    private static Color GetTileColor(int tile)
    {
        var blend = (tile - 1f) / 14f;
        return Color.Lerp(new Color(69, 116, 184), new Color(219, 126, 70), blend);
    }

    private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        spriteBatch.DrawString(font, text, position + new Vector2(2f, 2f), Color.Black * 0.45f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
