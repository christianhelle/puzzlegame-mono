using System;
using Microsoft.Xna.Framework;

namespace ChrisPuzzleGame.Screens;

public sealed class PlayerIndexEventArgs(PlayerIndex playerIndex) : EventArgs
{
    public PlayerIndex PlayerIndex { get; } = playerIndex;
}
