using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PuzzleGame.Gameplay;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Screens;

public sealed class GameplayScreen : GameScreen
{
    private const int ShuffleMoves = 240;
    private const double CommandIntervalMilliseconds = 10d;
    private const int SaveDataVersion = 2;

    private static readonly Random PuzzleRandom = new();
    private static readonly string[] PuzzleImages =
    [
        "Chrysanthemum",
        "Desert",
        "Hydrangeas",
        "Jellyfish",
        "Koala",
        "Lighthouse",
        "Penguins",
        "Tulips",
    ];

    private static int lastUsedPuzzleIndex = -1;

    private readonly Random random = new();
    private readonly Queue<Keys> pendingCommands = [];
    private readonly PuzzleBoard board = new();

    private Texture2D? puzzleTexture;
    private SpriteFont? gameTimerFont;
    private TimeSpan playingTime;
    private double commandTimer;
    private int moveCount;
    private bool boardInitialized;
    private bool restoreFlowPending;
    private bool winFlowShown;
    private SessionFlow sessionFlow = SessionFlow.Active;

    private GameplayScreen(bool selectPuzzleImage)
    {
        CurrentPuzzleImage = selectPuzzleImage ? SelectPuzzleImage() : string.Empty;
        TransitionOnTime = TimeSpan.Zero;
        TransitionOffTime = TimeSpan.Zero;
    }

    public GameplayScreen() : this(selectPuzzleImage: true) { }

    public string CurrentPuzzleImage { get; private set; }

    internal bool CanResume => boardInitialized;

    internal static GameplayScreen CreateForLoad() => new(selectPuzzleImage: false);

    public override void LoadContent()
    {
        if (string.IsNullOrWhiteSpace(CurrentPuzzleImage))
        {
            throw new InvalidOperationException("Gameplay screen cannot load without a puzzle image.");
        }

        puzzleTexture = ScreenManager.Game.Content.Load<Texture2D>(CurrentPuzzleImage);
        gameTimerFont = ScreenManager.Game.Content.Load<SpriteFont>("GameTime");

        if (!boardInitialized)
        {
            ScrambleBoard();
            boardInitialized = true;
        }
    }

    public override void UnloadContent()
    {
        puzzleTexture = null;
        gameTimerFont = null;
    }

    public override void Serialize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!boardInitialized || string.IsNullOrWhiteSpace(CurrentPuzzleImage))
        {
            throw new InvalidOperationException("Gameplay state is not ready to be persisted.");
        }

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(SaveDataVersion);
        writer.Write(CurrentPuzzleImage);
        writer.Write(playingTime.Ticks);
        writer.Write(moveCount);
        writer.Write((int)sessionFlow);
        writer.Write(board.Size);

        foreach (var tile in board.GetTiles())
        {
            writer.Write(tile);
        }
    }

    public override void Deserialize(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var saveDataVersion = reader.ReadInt32();

        string restoredPuzzleImage;
        TimeSpan restoredPlayingTime;
        int restoredMoveCount;
        SessionFlow restoredFlow;

        switch (saveDataVersion)
        {
            case 1:
                restoredPuzzleImage = GetPuzzleImageAssetName(reader.ReadInt32());
                restoredPlayingTime = ReadNonNegativeTimeSpan(reader);
                restoredMoveCount = ReadNonNegativeInteger(reader, "Move count");
                restoredFlow = SessionFlow.Active;
                break;
            case SaveDataVersion:
                restoredPuzzleImage = reader.ReadString();
                EnsureSupportedPuzzleImage(restoredPuzzleImage);
                restoredPlayingTime = ReadNonNegativeTimeSpan(reader);
                restoredMoveCount = ReadNonNegativeInteger(reader, "Move count");
                restoredFlow = ReadSessionFlow(reader.ReadInt32());
                break;
            default:
                throw new InvalidDataException($"Unsupported gameplay save data version '{saveDataVersion}'.");
        }

        var boardSize = reader.ReadInt32();
        if (boardSize != board.Size)
        {
            throw new InvalidDataException($"Unsupported puzzle board size '{boardSize}'.");
        }

        var restoredTiles = new int[boardSize * boardSize];
        for (var index = 0; index < restoredTiles.Length; index++)
        {
            restoredTiles[index] = reader.ReadInt32();
        }

        try
        {
            board.Restore(restoredTiles);
        }
        catch (ArgumentException ex)
        {
            throw new InvalidDataException("Gameplay board state is invalid.", ex);
        }

        ValidateRestoredFlow(restoredFlow);

        CurrentPuzzleImage = restoredPuzzleImage;
        RememberPuzzleImage(CurrentPuzzleImage);
        playingTime = restoredPlayingTime;
        moveCount = restoredMoveCount;
        pendingCommands.Clear();
        commandTimer = 0d;
        winFlowShown = false;
        sessionFlow = restoredFlow;
        restoreFlowPending = restoredFlow is SessionFlow.Options or SessionFlow.Win;
        boardInitialized = true;
        puzzleTexture = null;
        gameTimerFont = null;

        if (ScreenManager is not null)
        {
            LoadContent();
        }
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (!IsActive || winFlowShown)
        {
            return;
        }

        if (restoreFlowPending)
        {
            restoreFlowPending = false;

            switch (sessionFlow)
            {
                case SessionFlow.Options:
                    ShowOptions(ControllingPlayer ?? PlayerIndex.One);
                    return;
                case SessionFlow.Win:
                    ShowWinScreen(ControllingPlayer ?? PlayerIndex.One);
                    return;
            }
        }

        playingTime += gameTime.ElapsedGameTime;

        if (IsShowingLivePreview())
        {
            return;
        }

        QueueHeldCommand(GetHeldMovementCommand());
        commandTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

        while (commandTimer >= CommandIntervalMilliseconds)
        {
            commandTimer -= CommandIntervalMilliseconds;

            if (pendingCommands.Count == 0)
            {
                break;
            }

            if (!TryMove(pendingCommands.Dequeue()))
            {
                continue;
            }

            moveCount++;

            if (board.IsSolved)
            {
                ShowWinScreen(ControllingPlayer ?? PlayerIndex.One);
                break;
            }
        }
    }

    public override void HandleInput(InputState input)
    {
        if (winFlowShown)
        {
            return;
        }

        var playerIndex = ControllingPlayer ?? PlayerIndex.One;

        if (input.IsPauseGame(ControllingPlayer))
        {
            ShowOptions(playerIndex);
            return;
        }

        if (IsShowingLivePreview())
        {
            return;
        }

        if (input.IsNewKeyPress(Keys.R, ControllingPlayer, out _) || input.IsNewKeyPress(Keys.F5, ControllingPlayer, out _))
        {
            ScrambleBoard();
            return;
        }

        if (!input.IsNewLeftClick(ControllingPlayer, out _))
        {
            return;
        }

        if (!TryMoveFromMouse(input.MousePosition))
        {
            return;
        }

        moveCount++;

        if (board.IsSolved)
        {
            ShowWinScreen(playerIndex);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        ScreenManager.GraphicsDevice.Clear(Color.Black);

        var spriteBatch = ScreenManager.SpriteBatch;
        var viewportBounds = ScreenManager.GraphicsDevice.Viewport.Bounds;
        var isShowingPreview = IsShowingLivePreview();

        spriteBatch.Begin();

        if (puzzleTexture is not null)
        {
            if (isShowingPreview)
            {
                spriteBatch.Draw(puzzleTexture, viewportBounds, Color.White);
            }
            else
            {
                DrawPuzzle(spriteBatch, viewportBounds);
            }
        }

        DrawHud(spriteBatch, viewportBounds, isShowingPreview);
        spriteBatch.End();
    }

    internal void PrepareToResumeFromPause()
    {
        pendingCommands.Clear();
        commandTimer = 0d;
        restoreFlowPending = false;
        sessionFlow = SessionFlow.Active;
        winFlowShown = false;
    }

    private void ScrambleBoard()
    {
        board.Shuffle(ShuffleMoves, random);
        pendingCommands.Clear();
        moveCount = 0;
        playingTime = TimeSpan.Zero;
        commandTimer = 0d;
        restoreFlowPending = false;
        sessionFlow = SessionFlow.Active;
        winFlowShown = false;
    }

    private void DrawPuzzle(SpriteBatch spriteBatch, Rectangle bounds)
    {
        if (puzzleTexture is null)
        {
            return;
        }

        for (var index = 0; index < board.Size * board.Size; index++)
        {
            var tile = board.GetTileAt(index);
            if (tile == 0)
            {
                continue;
            }

            var destinationRow = index / board.Size;
            var destinationColumn = index % board.Size;
            var sourceIndex = tile - 1;
            var sourceRow = sourceIndex / board.Size;
            var sourceColumn = sourceIndex % board.Size;

            var destinationBounds = GetCellBounds(bounds, destinationRow, destinationColumn, board.Size);
            var sourceBounds = GetCellBounds(new Rectangle(0, 0, puzzleTexture.Width, puzzleTexture.Height), sourceRow, sourceColumn, board.Size);

            spriteBatch.Draw(puzzleTexture, destinationBounds, sourceBounds, Color.White);
        }
    }

    private void DrawHud(SpriteBatch spriteBatch, Rectangle viewportBounds, bool isShowingPreview)
    {
        var font = gameTimerFont ?? ScreenManager.Font;
        var accent = new Color(244, 215, 111);
        var body = new Color(236, 242, 252);

        if (isShowingPreview)
        {
            const string prompt = "Release Enter or F1 to return to the puzzle";
            var promptSize = ScreenManager.Font.MeasureString(prompt);
            DrawShadowedString(
                spriteBatch,
                ScreenManager.Font,
                prompt,
                new Vector2(viewportBounds.Right - promptSize.X - 26f, viewportBounds.Bottom - 42f),
                accent,
                0.65f);
            return;
        }

        DrawShadowedString(spriteBatch, font, $"Time {playingTime:mm\\:ss}", new Vector2(16f, 12f), body, 1f);
        DrawShadowedString(spriteBatch, font, $"Moves {moveCount}", new Vector2(16f, 34f), body, 1f);
        DrawShadowedString(spriteBatch, ScreenManager.Font, "Arrow keys / WASD move", new Vector2(16f, viewportBounds.Bottom - 70f), accent, 0.6f);
        DrawShadowedString(spriteBatch, ScreenManager.Font, "Click an adjacent tile   R or F5 reshuffles   Esc opens options   Enter or F1 previews", new Vector2(16f, viewportBounds.Bottom - 44f), body, 0.55f);
    }

    private bool TryMoveFromMouse(Point mousePosition)
    {
        var bounds = ScreenManager.GraphicsDevice.Viewport.Bounds;
        if (!bounds.Contains(mousePosition))
        {
            return false;
        }

        var column = Math.Clamp(((mousePosition.X - bounds.X) * board.Size) / Math.Max(1, bounds.Width), 0, board.Size - 1);
        var row = Math.Clamp(((mousePosition.Y - bounds.Y) * board.Size) / Math.Max(1, bounds.Height), 0, board.Size - 1);
        return board.TryMovePosition((row * board.Size) + column);
    }

    private void ShowOptions(PlayerIndex playerIndex)
    {
        pendingCommands.Clear();
        commandTimer = 0d;
        restoreFlowPending = false;
        sessionFlow = SessionFlow.Options;
        LoadingScreen.Load(ScreenManager, false, playerIndex, new PreviewScreen(CurrentPuzzleImage, Color.Gray), new InGameOptionsScreen(this));
    }

    private void ShowWinScreen(PlayerIndex playerIndex)
    {
        if (winFlowShown)
        {
            return;
        }

        winFlowShown = true;
        pendingCommands.Clear();
        commandTimer = 0d;
        restoreFlowPending = false;
        sessionFlow = SessionFlow.Win;

        var winScreen = new WinScreen(playingTime, moveCount);
        winScreen.Accepted += (_, e) => LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new GameplayScreen());
        winScreen.Cancelled += (_, e) => LoadingScreen.Load(ScreenManager, true, e.PlayerIndex, new BackgroundScreen(), new MainMenuScreen(() => new GameplayScreen()));

        ScreenManager.AddScreen(winScreen, playerIndex);
    }

    private bool TryMove(Keys command)
    {
        return command switch
        {
            Keys.Up => board.TryMoveBlankByOffset(0, 1),
            Keys.Down => board.TryMoveBlankByOffset(0, -1),
            Keys.Left => board.TryMoveBlankByOffset(1, 0),
            Keys.Right => board.TryMoveBlankByOffset(-1, 0),
            _ => false,
        };
    }

    private void QueueHeldCommand(Keys command)
    {
        if (command == Keys.None || pendingCommands.Contains(command))
        {
            return;
        }

        pendingCommands.Enqueue(command);
    }

    private Keys GetHeldMovementCommand()
    {
        var keyboardState = GetKeyboardState();

        if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
        {
            return Keys.Up;
        }

        if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
        {
            return Keys.Down;
        }

        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            return Keys.Left;
        }

        if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            return Keys.Right;
        }

        return Keys.None;
    }

    private bool IsShowingLivePreview()
    {
        var keyboardState = GetKeyboardState();
        return keyboardState.IsKeyDown(Keys.Enter) || keyboardState.IsKeyDown(Keys.F1);
    }

    private KeyboardState GetKeyboardState()
    {
        var playerIndex = (int)(ControllingPlayer ?? PlayerIndex.One);
        return ScreenManager.Input.CurrentKeyboardStates[playerIndex];
    }

    private static Rectangle GetCellBounds(Rectangle bounds, int row, int column, int gridSize)
    {
        var left = bounds.X + ((column * bounds.Width) / gridSize);
        var top = bounds.Y + ((row * bounds.Height) / gridSize);
        var right = bounds.X + (((column + 1) * bounds.Width) / gridSize);
        var bottom = bounds.Y + (((row + 1) * bounds.Height) / gridSize);
        return new Rectangle(left, top, right - left, bottom - top);
    }

    private static string SelectPuzzleImage()
    {
        var index = lastUsedPuzzleIndex;
        while (PuzzleImages.Length > 1 && index == lastUsedPuzzleIndex)
        {
            index = PuzzleRandom.Next(0, PuzzleImages.Length);
        }

        lastUsedPuzzleIndex = index;
        return GetPuzzleImageAssetName(index);
    }

    private static string GetPuzzleImageAssetName(int puzzleImageIndex)
    {
        if (puzzleImageIndex < 0 || puzzleImageIndex >= PuzzleImages.Length)
        {
            throw new InvalidDataException($"Unsupported puzzle image index '{puzzleImageIndex}'.");
        }

        return $"Puzzles/{PuzzleImages[puzzleImageIndex]}";
    }

    private static int GetPuzzleImageIndex(string puzzleImageAssetName)
    {
        for (var index = 0; index < PuzzleImages.Length; index++)
        {
            if (string.Equals(puzzleImageAssetName, GetPuzzleImageAssetName(index), StringComparison.Ordinal))
            {
                return index;
            }
        }

        throw new InvalidOperationException($"Unsupported puzzle image '{puzzleImageAssetName}'.");
    }

    private static void EnsureSupportedPuzzleImage(string puzzleImageAssetName) => _ = GetPuzzleImageIndex(puzzleImageAssetName);

    private static void RememberPuzzleImage(string puzzleImageAssetName)
    {
        lastUsedPuzzleIndex = GetPuzzleImageIndex(puzzleImageAssetName);
    }

    private static TimeSpan ReadNonNegativeTimeSpan(BinaryReader reader)
    {
        var ticks = reader.ReadInt64();
        if (ticks < 0)
        {
            throw new InvalidDataException("Gameplay time cannot be negative.");
        }

        try
        {
            return TimeSpan.FromTicks(ticks);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new InvalidDataException("Gameplay time is out of range.", ex);
        }
    }

    private static int ReadNonNegativeInteger(BinaryReader reader, string valueName)
    {
        var value = reader.ReadInt32();
        if (value < 0)
        {
            throw new InvalidDataException($"{valueName} cannot be negative.");
        }

        return value;
    }

    private static SessionFlow ReadSessionFlow(int rawValue)
    {
        return rawValue switch
        {
            (int)SessionFlow.Active => SessionFlow.Active,
            (int)SessionFlow.Options => SessionFlow.Options,
            (int)SessionFlow.Win => SessionFlow.Win,
            _ => throw new InvalidDataException($"Unsupported gameplay flow '{rawValue}'."),
        };
    }

    private void ValidateRestoredFlow(SessionFlow restoredFlow)
    {
        if (restoredFlow == SessionFlow.Win && !board.IsSolved)
        {
            throw new InvalidDataException("Win flow requires a solved puzzle board.");
        }

        if (restoredFlow != SessionFlow.Win && board.IsSolved)
        {
            throw new InvalidDataException("Solved puzzle boards must restore through the win flow.");
        }
    }

    private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, float scale)
    {
        spriteBatch.DrawString(font, text, position + new Vector2(2f, 2f), Color.Black * 0.5f, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private enum SessionFlow
    {
        Active = 0,
        Options = 1,
        Win = 2,
    }
}
