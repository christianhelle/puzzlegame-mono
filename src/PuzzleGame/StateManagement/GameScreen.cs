using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace PuzzleGame.StateManagement;

public abstract class GameScreen
{
    private bool otherScreenHasFocus;
    private ScreenState screenState = ScreenState.TransitionOn;
    public bool IsPopup { get; protected set; }
    public TimeSpan TransitionOnTime { get; protected set; } = TimeSpan.Zero;
    public TimeSpan TransitionOffTime { get; protected set; } = TimeSpan.Zero;
    public float TransitionPosition { get; protected set; } = 1f;
    public float TransitionAlpha => 1f - TransitionPosition;
    public ScreenState ScreenState { get => screenState; protected set => screenState = value; }
    public bool IsExiting { get; protected internal set; }
    public bool IsActive => !otherScreenHasFocus && (screenState == ScreenState.TransitionOn || screenState == ScreenState.Active);
    public ScreenManager ScreenManager { get; internal set; } = default!;
    public PlayerIndex? ControllingPlayer { get; internal set; }
    public bool IsSerializable { get; protected set; } = true;
    public virtual void LoadContent() { }
    public virtual void UnloadContent() { }
    public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        this.otherScreenHasFocus = otherScreenHasFocus;
        if (IsExiting)
        {
            screenState = ScreenState.TransitionOff;
            if (!UpdateTransition(gameTime, TransitionOffTime, 1))
            {
                ScreenManager.RemoveScreen(this);
            }
            return;
        }
        if (coveredByOtherScreen)
        {
            if (UpdateTransition(gameTime, TransitionOffTime, 1))
            {
                screenState = ScreenState.TransitionOff;
            }
            else
            {
                screenState = ScreenState.Hidden;
            }
            return;
        }
        if (UpdateTransition(gameTime, TransitionOnTime, -1))
        {
            screenState = ScreenState.TransitionOn;
        }
        else
        {
            screenState = ScreenState.Active;
        }
    }
    public virtual void HandleInput(InputState input) { }
    public virtual void Draw(GameTime gameTime) { }
    public virtual void Serialize(Stream stream) { }
    public virtual void Deserialize(Stream stream) { }
    public void ExitScreen()
    {
        if (TransitionOffTime == TimeSpan.Zero)
        {
            ScreenManager.RemoveScreen(this);
            return;
        }
        IsExiting = true;
    }
    private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
    {
        var transitionDelta = time == TimeSpan.Zero ? 1f : (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);
        TransitionPosition += transitionDelta * direction;
        if ((direction < 0 && TransitionPosition <= 0f) || (direction > 0 && TransitionPosition >= 1f))
        {
            TransitionPosition = MathHelper.Clamp(TransitionPosition, 0f, 1f);
            return false;
        }
        return true;
    }
}
