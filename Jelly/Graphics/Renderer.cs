using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Graphics;

public static class Renderer
{
    static GraphicsDeviceManager _graphics;

    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static RenderTarget2D RenderTarget { get; private set; }
    public static RenderTarget2D UIRenderTarget { get; private set; }

    public static GameWindow Window { get; private set; }

    /// <summary>
    /// How large a native pixel is on the client's screen.
    /// </summary>
    public static int PixelScale { get; set; } = 3;

    /// <summary>
    /// The native render resolution.
    /// </summary>
    public static Point ScreenSize { get; set; } = new Point(640, 360);

    public static SpriteFont RegularFont { get; private set; }
    public static SpriteFont RegularFontBold { get; private set; }

    /// <summary>
    /// Represents a missing (empty) <see cref="Texture2D"/>.
    /// </summary>
    public static Texture2D EmptyTexture { get; private set; }

    public static void Initialize(GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, GameWindow window)
    {
        _graphics = graphics;
        GraphicsDevice = graphicsDevice;
        Window = window;

        RenderTarget = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);
        UIRenderTarget = new RenderTarget2D(GraphicsDevice, ScreenSize.X, ScreenSize.Y);

        EmptyTexture = new Texture2D(GraphicsDevice, 1, 1);
        EmptyTexture.SetData<byte>([0, 0, 0, 0]);

        Window.Position = new((GraphicsDevice.DisplayMode.Width - _graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - _graphics.PreferredBackBufferHeight) / 2);

        if(GraphicsDevice.DisplayMode.Height == _graphics.PreferredBackBufferHeight)
        {
            Window.Position = Point.Zero;
            Window.IsBorderless = true;
        }

        _graphics.ApplyChanges();
    }

    public static void LoadContent(ContentManager content)
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        RegularFont = content.Load<SpriteFont>("Fonts/default");
        RegularFontBold = content.Load<SpriteFont>("Fonts/defaultBold");
    }

    public static void BeginDraw(SamplerState samplerState = null, Matrix? transformMatrix = null, SpriteSortMode sortMode = SpriteSortMode.Deferred)
    {
        GraphicsDevice.SetRenderTarget(RenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(sortMode: sortMode, samplerState: samplerState, transformMatrix: transformMatrix);
    }

    public static void EndDraw()
    {
        SpriteBatch.End();
    }

    public static void BeginDrawUI(Point? canvasSize = null)
    {
        Point size = canvasSize ?? Point.Zero;
        if(canvasSize is not null && size != Point.Zero && UIRenderTarget.Bounds.Size != new Point(Math.Abs(size.X), Math.Abs(size.Y)))
        {
            UIRenderTarget = new RenderTarget2D(GraphicsDevice, Math.Abs(size.X), Math.Abs(size.Y));
        }

        GraphicsDevice.SetRenderTarget(UIRenderTarget);
        GraphicsDevice.Clear(Color.Transparent);

        SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
    }

    public static void EndDrawUI()
    {
        SpriteBatch.End();
    }

    public static void FinalizeDraw()
    {
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        SpriteBatch.Begin(samplerState: SamplerState.PointWrap, blendState: BlendState.Opaque);
        SpriteBatch.Draw(RenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, PixelScale, SpriteEffects.None, 0);
        SpriteBatch.End();

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        SpriteBatch.Draw(UIRenderTarget, Vector2.Zero, null, Color.White, 0, Vector2.Zero, PixelScale, SpriteEffects.None, 0);
        SpriteBatch.End();
    }
}
