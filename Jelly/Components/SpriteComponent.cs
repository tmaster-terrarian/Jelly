using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Jelly.Graphics;

namespace Jelly.Components;

public class SpriteComponent : Component
{
    public string TexturePath { get; set; } = null;

    public Rectangle? SourceRectangle { get; set; } = null;

    public Vector2 Scale { get; set; } = Vector2.One;

    public Color Color { get; set; } = Color.White;

    public float Alpha { get; set; } = 1;

    public float Rotation { get; set; } = 0;

    public Vector2 Pivot { get; set; } = Vector2.Zero;

    public SpriteEffects SpriteEffects { get; set; } = SpriteEffects.None;

    public override void Draw()
    {
        base.Draw();

        if(TexturePath is null) return;

        if(JellyBackend.ContentProvider.TryGetTexture(TexturePath, out Texture2D tex))
        {
            Renderer.SpriteBatch.Draw(tex, Entity.Position.ToVector2(), SourceRectangle, Color * Alpha, Rotation, Pivot, Scale, SpriteEffects, 0);
        }
    }
}
