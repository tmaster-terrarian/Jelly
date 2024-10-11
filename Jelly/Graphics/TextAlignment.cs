using System;

namespace Jelly.Graphics;

[Flags]
public enum TextAlignment
{
    Left =      0b0001,
    Right =     0b0010,
    CenterX =   0b0011,
    Top =       0b0100,
    Bottom =    0b1000,
    CenterY =   0b1100,
}

// combo presets
public static class TextAlignmentPresets
{
    public const TextAlignment TopLeft =     TextAlignment.Top | TextAlignment.Left;
    public const TextAlignment TopRight =    TextAlignment.Top | TextAlignment.Right;
    public const TextAlignment BottomLeft =  TextAlignment.Bottom | TextAlignment.Left;
    public const TextAlignment BottomRight = TextAlignment.Bottom | TextAlignment.Right;
    public const TextAlignment Center =      TextAlignment.CenterX | TextAlignment.CenterY;

    public const TextAlignment BitsHorizontal = (TextAlignment)0b0011;
    public const TextAlignment BitsVertical = (TextAlignment)0b1100;

    public static TextAlignment Default { get; set; } = TopLeft;
}
