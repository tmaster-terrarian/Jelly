using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Graphics;

public static class SpriteBatchExtensions
{
    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        var split = text.Split(' ');
        float x = 0;
        foreach(var word in split)
        {
            spriteBatch.DrawString(font, word, position + (Vector2.UnitX * x * scale.X), color, rotation, origin, scale, effects, layerDepth, rtl);
            x += font.MeasureString(word).X + spaceSize;
        }
    }

    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize, float rotation, Vector2 origin, float scale = 1, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0, bool rtl = false)
    {
        spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, rotation, origin, new Vector2(scale), effects, layerDepth, rtl);
    }

    public static void DrawStringSpacesFix(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, int spaceSize)
    {
        spriteBatch.DrawStringSpacesFix(font, text, position, color, spaceSize, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
    }

    public static void Draw(this SpriteBatch spriteBatch, TextComponent textComponent, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(textComponent);
        textComponent.Draw(spriteBatch, position, color, rotation, origin, scale, effects, layerDepth);
    }

    public static void Draw(this SpriteBatch spriteBatch, TextComponent textComponent, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(textComponent);
        textComponent.Draw(spriteBatch, position, color, rotation, origin, scale, effects, layerDepth);
    }

    public static void Draw(this SpriteBatch spriteBatch, TextComponent textComponent, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(textComponent);
        textComponent.Draw(spriteBatch, position, color);
    }
}