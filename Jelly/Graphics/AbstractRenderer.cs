using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Graphics;

public abstract class AbstractRenderer : IDisposable
{
    public virtual void PreDraw(GameTime gameTime) {}

    public virtual void Draw(GameTime gameTime) {}

    public virtual void DrawUI(GameTime gameTime) {}

    ~AbstractRenderer()
    {
        Dispose(true);
    }

    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool byGC)
    {
        Disposed?.Invoke(this, new(byGC));
    }

    public event EventHandler<ObjectDisposedEventArgs> Disposed;
}
