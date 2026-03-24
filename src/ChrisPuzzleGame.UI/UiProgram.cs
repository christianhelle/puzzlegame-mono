using System;

namespace ChrisPuzzleGame;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new PuzzleGame();
        game.Run();
    }
}
