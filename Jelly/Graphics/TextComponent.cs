using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Graphics;

public class TextComponent
{
    public string Text { get; set; } = "";

    public SpriteFont Font { get; set; }

    public Color Color { get; set; } = Color.White;

    public TextAlignment TextAlignment { get; set; } = TextAlignmentPresets.TopLeft;

    public float SpaceWidth { get; set; } = 4;

    public float LineSpacing { get; set; } = 12;

    public bool RenderNewlines { get; set; } = true;

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if(Font is null) return;
        if(Text is null) return;
        if(Text.Length == 0) return;

        string[] lines = RenderNewlines ? Text.Split("\n") : [Text];

        bool rtl = true;

        if((TextAlignment & TextAlignmentPresets.BitsHorizontal) == TextAlignment.Right)
        {
            rtl = false;
        }

        float y = 0;

        if((TextAlignment & TextAlignmentPresets.BitsVertical) == TextAlignment.Bottom)
        {
            y -= LineSpacing * lines.Length;
        }

        if((TextAlignment & TextAlignmentPresets.BitsVertical) == TextAlignment.CenterY)
        {
            y -= LineSpacing / 2 * lines.Length;
        }

        foreach(string line in lines)
        {
            string[] split = line.Split(' ');
            float x = 0;

            if(rtl)
            {
                for(int i = 0; i < split.Length; i++)
                {
                    string word = split[i];
                    Vector2 offset = new Vector2(x, y);

                    spriteBatch.DrawString(Font, word, position, color, rotation, origin - offset, scale, effects, layerDepth);
                    x += Font.MeasureString(word).X + SpaceWidth;
                }
            }
            else
            {
                for(int i = split.Length - 1; i >= 0; i--)
                {
                    string word = split[i];
                    Vector2 offset = new Vector2(x, y);

                    spriteBatch.DrawString(Font, word, position, color, rotation, origin - offset, scale, effects, layerDepth, false);
                    x -= Font.MeasureString(word).X + SpaceWidth;
                }
            }
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        Draw(spriteBatch, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        Draw(spriteBatch, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
    }
}
