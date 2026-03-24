using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ChrisPuzzleGame.StateManagement;

public sealed class ScreenManager : DrawableGameComponent
{
    private readonly List<GameScreen> screens = [];
    private readonly List<GameScreen> screensToUpdate = [];
    private readonly InputState input = new();
    private SpriteBatch? spriteBatch;
    private SpriteFont? font;
    private Texture2D? blankTexture;
    private Texture2D? gradientTexture;
    private bool isInitialized;
    public ScreenManager(Game game) : base(game) { }
    public SpriteBatch SpriteBatch => spriteBatch ?? throw new InvalidOperationException("SpriteBatch is not loaded.");
    public SpriteFont Font => font ?? throw new InvalidOperationException("Menu font is not loaded.");
    public Texture2D BlankTexture => blankTexture ?? throw new InvalidOperationException("Blank texture is not loaded.");
    public Texture2D GradientTexture => gradientTexture ?? BlankTexture;
    public InputState Input => input;
    public bool TraceEnabled { get; set; }
    public override void Initialize() { base.Initialize(); isInitialized = true; }
    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        font = Game.Content.Load<SpriteFont>("Menu");
        gradientTexture = LoadOptionalTexture("gradient");
        blankTexture = new Texture2D(GraphicsDevice, 1, 1);
        blankTexture.SetData([Color.White]);
        foreach (var screen in screens)
        {
            screen.LoadContent();
        }
    }
    protected override void UnloadContent()
    {
        foreach (var screen in screens)
        {
            screen.UnloadContent();
        }
        blankTexture?.Dispose();
        blankTexture = null;
        spriteBatch?.Dispose();
        spriteBatch = null;
    }
    public override void Update(GameTime gameTime)
    {
        input.Update();
        screensToUpdate.Clear();
        screensToUpdate.AddRange(screens);
        var otherScreenHasFocus = !Game.IsActive;
        var coveredByOtherScreen = false;
        while (screensToUpdate.Count > 0)
        {
            var screen = screensToUpdate[^1];
            screensToUpdate.RemoveAt(screensToUpdate.Count - 1);
            screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (screen.ScreenState is ScreenState.TransitionOn or ScreenState.Active)
            {
                if (!otherScreenHasFocus)
                {
                    screen.HandleInput(input);
                    otherScreenHasFocus = true;
                }
                if (!screen.IsPopup)
                {
                    coveredByOtherScreen = true;
                }
            }
        }
        if (TraceEnabled)
        {
            TraceScreens();
        }
    }
    public override void Draw(GameTime gameTime)
    {
        foreach (var screen in screens)
        {
            if (screen.ScreenState == ScreenState.Hidden)
            {
                continue;
            }
            screen.Draw(gameTime);
        }
    }
    public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
    {
        screen.ControllingPlayer = controllingPlayer;
        screen.ScreenManager = this;
        screen.IsExiting = false;
        if (isInitialized)
        {
            screen.LoadContent();
        }
        screens.Add(screen);
    }
    public void RemoveScreen(GameScreen screen)
    {
        if (isInitialized)
        {
            screen.UnloadContent();
        }
        screens.Remove(screen);
        screensToUpdate.Remove(screen);
    }
    public GameScreen[] GetScreens() => [.. screens];
    public Rectangle GetSafeArea(float horizontalMarginRatio = 0.08f, float verticalMarginRatio = 0.08f)
    {
        var bounds = GraphicsDevice.Viewport.Bounds;
        var xMargin = Math.Clamp((int)(bounds.Width * horizontalMarginRatio), 0, bounds.Width / 4);
        var yMargin = Math.Clamp((int)(bounds.Height * verticalMarginRatio), 0, bounds.Height / 4);
        return new Rectangle(bounds.X + xMargin, bounds.Y + yMargin, bounds.Width - (xMargin * 2), bounds.Height - (yMargin * 2));
    }
    public Texture2D? LoadOptionalTexture(string assetName)
    {
        try { return Game.Content.Load<Texture2D>(assetName); }
        catch (ContentLoadException) { return null; }
    }
    public void FadeBackBufferToBlack(float alpha)
    {
        var viewport = GraphicsDevice.Viewport;
        SpriteBatch.Begin();
        SpriteBatch.Draw(BlankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.Black * MathHelper.Clamp(alpha, 0f, 1f));
        SpriteBatch.End();
    }
    private void TraceScreens()
    {
        var screenNames = new List<string>();
        foreach (var screen in screens)
        {
            screenNames.Add(screen.GetType().Name);
        }
        Debug.WriteLine(string.Join(", ", screenNames));
    }
}
