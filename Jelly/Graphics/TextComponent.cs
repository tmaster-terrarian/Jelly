using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jelly.Graphics;

public class TextComponent
{
    private static readonly char[] splitChars = [' ', '-', '\t'];

    public string Text { get; set; } = "";

    public string WrappedText => WordWrap(Text, (int)((MaxWidth > 0 ? MaxWidth : 10000000000) / Font.MeasureString("0").X));

    public SpriteFont Font { get; set; }

    public Color Color { get; set; } = Color.White;

    public TextAlignment TextAlignment { get; set; } = TextAlignmentPresets.TopLeft;

    public float SpaceWidth { get; set; } = 4;

    public float MaxWidth { get; set; } = -1;

    public float LineSpacing { get; set; } = 12;

    public bool RenderNewlines { get; set; } = true;

    public virtual float CalculateHeight()
    {
        var textWrapped = WrappedText;

        string[] lines = RenderNewlines
            ? textWrapped.Split("\n")
            : [textWrapped.Replace("\n", " ")];

        return LineSpacing * lines.Length;
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color? color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if(Font is null) return;
        if(Text is null) return;
        if(Text.Length == 0) return;

        bool rtl = true;

        if((TextAlignment & TextAlignmentPresets.BitsHorizontal) == TextAlignment.Right)
        {
            rtl = false;
        }

        var textWrapped = WrappedText;

        string[] lines = RenderNewlines
            ? textWrapped.Split("\n")
            : [textWrapped.Replace("\n", " ")];

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

                    spriteBatch.DrawString(Font, word, position, color ?? Color, rotation, origin - offset, scale, effects, layerDepth);
                    x += Font.MeasureString(word).X + SpaceWidth;
                }
            }
            else
            {
                for(int i = split.Length - 1; i >= 0; i--)
                {
                    string word = split[i];
                    Vector2 offset = new Vector2(x, y);

                    spriteBatch.DrawString(Font, word, position, color ?? Color, rotation, origin - offset, scale, effects, layerDepth, false);
                    x -= Font.MeasureString(word).X + SpaceWidth;
                }
            }

            y += LineSpacing;
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color? color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        Draw(spriteBatch, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Vector2 position, Color? color)
    {
        Draw(spriteBatch, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
    }

    // taken from https://stackoverflow.com/a/17635, modified a bit
    public static string WordWrap(string str, int width)
    {
        string[] words = Explode(str, splitChars);

        int curLineLength = 0;
        StringBuilder strBuilder = new();
        for(int i = 0; i < words.Length; i += 1)
        {
            string word = words[i];

            // If adding the new word to the current line would be too long,
            // then put it on a new line (and split it up if it's too long).
            if (curLineLength + word.Length > width)
            {
                // Only move down to a new line if we have text on the current line.
                // Avoids situation where
                // wrapped whitespace causes emptylines in text.
                if (curLineLength > 0)
                {
                    strBuilder.Append(Environment.NewLine);
                    curLineLength = 0;
                }

                // If the current word is too long
                // to fit on a line (even on its own),
                // then split the word up.
                while (word.Length > width)
                {
                    strBuilder.Append(word[..(width - 1)] + "-");
                    word = word[(width - 1)..];

                    strBuilder.Append(Environment.NewLine);
                }

                // Remove leading whitespace from the word,
                // so the new line starts flush to the left.
                word = word.TrimStart();
            }
            strBuilder.Append(word);
            curLineLength += word.Length;
        }

        return strBuilder.ToString();
    }

    private static string[] Explode(string str, char[] splitChars)
    {
        List<string> parts = [];
        int startIndex = 0;
        while (true)
        {
            int index = str.IndexOfAny(splitChars, startIndex);
            
            if (index == -1)
            {
                parts.Add(str[startIndex..]);
                return [.. parts];
            }

            string word = str[startIndex..index];
            char nextChar = str.Substring(index, 1)[0];

            // Dashes and the like should stick to the word occuring before it.
            // Whitespace doesn't have to.
            if (char.IsWhiteSpace(nextChar))
            {
                parts.Add(word);
                parts.Add(nextChar.ToString());
            }
            else
            {
                parts.Add(word + nextChar);
            }

            startIndex = index + 1;
        }
    }
}
