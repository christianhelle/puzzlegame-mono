using System;

namespace PuzzleGame.Gameplay;

internal sealed class PuzzleBoard
{
    private readonly int[] tiles;

    public PuzzleBoard(int size = 4)
    {
        if (size < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Puzzle size must be at least 2.");
        }

        Size = size;
        tiles = new int[size * size];
        Reset();
    }

    public int Size { get; }

    public int BlankIndex { get; private set; }

    public bool IsSolved
    {
        get
        {
            for (var index = 0; index < tiles.Length - 1; index++)
            {
                if (tiles[index] != index + 1)
                {
                    return false;
                }
            }

            return tiles[^1] == 0;
        }
    }

    public void Reset()
    {
        for (var index = 0; index < tiles.Length - 1; index++)
        {
            tiles[index] = index + 1;
        }

        tiles[^1] = 0;
        BlankIndex = tiles.Length - 1;
    }

    public void Shuffle(int moveCount, Random random)
    {
        Reset();

        Span<int> neighbors = stackalloc int[4];
        var previousBlankIndex = -1;

        for (var move = 0; move < moveCount; move++)
        {
            var neighborCount = WriteNeighborIndexes(BlankIndex, neighbors);
            var validNeighbors = neighbors[..neighborCount];

            if (previousBlankIndex >= 0 && neighborCount > 1)
            {
                neighborCount = RemoveValue(validNeighbors, previousBlankIndex);
                validNeighbors = neighbors[..neighborCount];
            }

            var nextBlankIndex = validNeighbors[random.Next(neighborCount)];
            previousBlankIndex = BlankIndex;
            Swap(BlankIndex, nextBlankIndex);
            BlankIndex = nextBlankIndex;
        }

        if (IsSolved)
        {
            var neighborCount = WriteNeighborIndexes(BlankIndex, neighbors);
            var nextBlankIndex = neighbors[0];
            Swap(BlankIndex, nextBlankIndex);
            BlankIndex = nextBlankIndex;
        }
    }

    public int GetTileAt(int positionIndex) => tiles[positionIndex];

    public bool TryMoveBlankByOffset(int xOffset, int yOffset)
    {
        var blankRow = BlankIndex / Size;
        var blankColumn = BlankIndex % Size;
        var targetRow = blankRow + yOffset;
        var targetColumn = blankColumn + xOffset;

        if (targetRow < 0 || targetRow >= Size || targetColumn < 0 || targetColumn >= Size)
        {
            return false;
        }

        var targetIndex = targetRow * Size + targetColumn;
        Swap(BlankIndex, targetIndex);
        BlankIndex = targetIndex;
        return true;
    }

    public bool TryMovePosition(int positionIndex)
    {
        if (positionIndex < 0 || positionIndex >= tiles.Length || positionIndex == BlankIndex)
        {
            return false;
        }

        var blankRow = BlankIndex / Size;
        var blankColumn = BlankIndex % Size;
        var tileRow = positionIndex / Size;
        var tileColumn = positionIndex % Size;

        if (Math.Abs(blankRow - tileRow) + Math.Abs(blankColumn - tileColumn) != 1)
        {
            return false;
        }

        Swap(BlankIndex, positionIndex);
        BlankIndex = positionIndex;
        return true;
    }

    public int[] GetTiles() => (int[])tiles.Clone();

    public void CopyTilesTo(Span<int> destination) => tiles.AsSpan().CopyTo(destination);

    public void Restore(int[] tileState)
    {
        ArgumentNullException.ThrowIfNull(tileState);

        if (tileState.Length != tiles.Length)
        {
            throw new ArgumentException(
                "Tile state length does not match the puzzle board.",
                nameof(tileState));
        }

        var restoredTiles = new int[tileState.Length];
        var seenTiles = new bool[tileState.Length];
        var restoredBlankIndex = -1;

        for (var index = 0; index < tileState.Length; index++)
        {
            var tile = tileState[index];
            if (tile < 0 || tile >= tileState.Length)
            {
                throw new ArgumentException(
                    "Tile state contains an out-of-range tile value.",
                    nameof(tileState));
            }

            if (seenTiles[tile])
            {
                throw new ArgumentException(
                    "Tile state contains duplicate tile values.",
                    nameof(tileState));
            }

            seenTiles[tile] = true;
            restoredTiles[index] = tile;

            if (tile == 0)
            {
                restoredBlankIndex = index;
            }
        }

        if (restoredBlankIndex < 0)
        {
            throw new ArgumentException("Tile state is missing the blank tile.", nameof(tileState));
        }

        if (!IsSolvable(restoredTiles, restoredBlankIndex))
        {
            throw new ArgumentException("Tile state is not solvable.", nameof(tileState));
        }

        Array.Copy(restoredTiles, tiles, tiles.Length);
        BlankIndex = restoredBlankIndex;
    }

    private int WriteNeighborIndexes(int index, Span<int> buffer)
    {
        var row = index / Size;
        var column = index % Size;
        var count = 0;

        if (row > 0)
        {
            buffer[count++] = index - Size;
        }

        if (row < Size - 1)
        {
            buffer[count++] = index + Size;
        }

        if (column > 0)
        {
            buffer[count++] = index - 1;
        }

        if (column < Size - 1)
        {
            buffer[count++] = index + 1;
        }

        return count;
    }

    private static int RemoveValue(Span<int> span, int value)
    {
        var count = span.Length;
        for (var i = 0; i < count; i++)
        {
            if (span[i] == value)
            {
                span[i] = span[count - 1];
                return count - 1;
            }
        }

        return count;
    }

    private void Swap(int firstIndex, int secondIndex)
    {
        (tiles[firstIndex], tiles[secondIndex]) = (tiles[secondIndex], tiles[firstIndex]);
    }

    private bool IsSolvable(int[] tileState, int blankTileIndex)
    {
        var inversionCount = 0;
        for (var firstIndex = 0; firstIndex < tileState.Length; firstIndex++)
        {
            var firstTile = tileState[firstIndex];
            if (firstTile == 0)
            {
                continue;
            }

            for (var secondIndex = firstIndex + 1; secondIndex < tileState.Length; secondIndex++)
            {
                var secondTile = tileState[secondIndex];
                if (secondTile == 0)
                {
                    continue;
                }

                if (firstTile > secondTile)
                {
                    inversionCount++;
                }
            }
        }

        if (Size % 2 != 0)
        {
            return inversionCount % 2 == 0;
        }

        var blankRowFromBottom = Size - blankTileIndex / Size;
        if (blankRowFromBottom % 2 == 0)
        {
            return inversionCount % 2 != 0;
        }

        return inversionCount % 2 == 0;
    }
}
