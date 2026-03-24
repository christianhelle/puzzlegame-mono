using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChrisPuzzleGame.StateManagement;

public sealed class InputState
{
    public const int MaxInputs = 4;
    public InputState()
    {
        CurrentKeyboardStates = new KeyboardState[MaxInputs];
        CurrentGamePadStates = new GamePadState[MaxInputs];
        LastKeyboardStates = new KeyboardState[MaxInputs];
        LastGamePadStates = new GamePadState[MaxInputs];
        GamePadWasConnected = new bool[MaxInputs];
    }
    public KeyboardState[] CurrentKeyboardStates { get; }
    public GamePadState[] CurrentGamePadStates { get; }
    public KeyboardState[] LastKeyboardStates { get; }
    public GamePadState[] LastGamePadStates { get; }
    public bool[] GamePadWasConnected { get; }
    public MouseState CurrentMouseState { get; private set; }
    public MouseState LastMouseState { get; private set; }
    public Point MousePosition => CurrentMouseState.Position;
    public void Update()
    {
        for (var i = 0; i < MaxInputs; i++)
        {
            LastKeyboardStates[i] = CurrentKeyboardStates[i];
            LastGamePadStates[i] = CurrentGamePadStates[i];
            CurrentKeyboardStates[i] = Keyboard.GetState();
            CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            if (CurrentGamePadStates[i].IsConnected)
            {
                GamePadWasConnected[i] = true;
            }
        }
        LastMouseState = CurrentMouseState;
        CurrentMouseState = Mouse.GetState();
    }
    public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            playerIndex = controllingPlayer.Value;
            var index = (int)playerIndex;
            return CurrentKeyboardStates[index].IsKeyDown(key) && LastKeyboardStates[index].IsKeyUp(key);
        }
        return IsNewKeyPress(key, PlayerIndex.One, out playerIndex) || IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) || IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) || IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
    }
    public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            playerIndex = controllingPlayer.Value;
            var index = (int)playerIndex;
            return CurrentGamePadStates[index].IsButtonDown(button) && LastGamePadStates[index].IsButtonUp(button);
        }
        return IsNewButtonPress(button, PlayerIndex.One, out playerIndex) || IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) || IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) || IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
    }
    public bool IsNewLeftClick(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        playerIndex = controllingPlayer ?? PlayerIndex.One;
        return CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released;
    }
    public bool IsMenuSelect(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        return IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) || IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) || IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) || IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
    }
    public bool IsMenuCancel(PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) || IsNewKeyPress(Keys.Back, controllingPlayer, out playerIndex) || IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) || IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
    }
    public bool IsMenuUp(PlayerIndex? controllingPlayer)
    {
        return IsNewKeyPress(Keys.Up, controllingPlayer, out _) || IsNewKeyPress(Keys.W, controllingPlayer, out _) || IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out _) || IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out _);
    }
    public bool IsMenuDown(PlayerIndex? controllingPlayer)
    {
        return IsNewKeyPress(Keys.Down, controllingPlayer, out _) || IsNewKeyPress(Keys.S, controllingPlayer, out _) || IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out _) || IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out _);
    }
    public bool IsPauseGame(PlayerIndex? controllingPlayer)
    {
        return IsMenuCancel(controllingPlayer, out _) || IsNewButtonPress(Buttons.Start, controllingPlayer, out _);
    }
}
