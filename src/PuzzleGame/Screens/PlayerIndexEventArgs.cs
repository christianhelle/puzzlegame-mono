using System;
using Microsoft.Xna.Framework;

namespace PuzzleGame.Screens;

public sealed class PlayerIndexEventArgs(
    PlayerIndex playerIndex) : EventArgs
{
    public PlayerIndex PlayerIndex { get; } = playerIndex;
}
