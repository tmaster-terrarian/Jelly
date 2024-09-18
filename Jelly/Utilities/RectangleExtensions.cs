using Microsoft.Xna.Framework;

namespace Jelly.Utilities;

public static class RectangleExtensions
{
    public static Rectangle Shift(this Rectangle rectangle, Point offset)
    {
        return new(rectangle.Location + offset, rectangle.Size);
    }

    public static Rectangle Shift(this Rectangle rectangle, int offsetX, int offsetY)
    {
        return new(rectangle.X + offsetX, rectangle.Y + offsetY, rectangle.Width, rectangle.Height);
    }

    public static Rectangle Scale(this Rectangle rectangle, Point scale)
    {
        return new(rectangle.X, rectangle.Y, rectangle.Width * scale.X, rectangle.Height * scale.Y);
    }

    public static Rectangle Scale(this Rectangle rectangle, Vector2 scale)
    {
        return new(rectangle.X, rectangle.Y, (int)(rectangle.Width * scale.X), (int)(rectangle.Height * scale.Y));
    }

    public static Rectangle ScalePosition(this Rectangle rectangle, Point scale)
    {
        rectangle.X *= scale.X;
        rectangle.Y *= scale.Y;
        rectangle.Width *= scale.X;
        rectangle.Height *= scale.Y;
        return rectangle;
    }

    public static Rectangle ScalePosition(this Rectangle rectangle, Vector2 scale)
    {
        rectangle.X = (int)(rectangle.X * scale.X);
        rectangle.Y = (int)(rectangle.Y * scale.Y);
        rectangle.Width = (int)(rectangle.Width * scale.X);
        rectangle.Height = (int)(rectangle.Height * scale.Y);
        return rectangle;
    }

    public static Rectangle ScalePosition(this Rectangle rectangle, int scale) => ScalePosition(rectangle, new Point(scale, scale));
    public static Rectangle ScalePosition(this Rectangle rectangle, float scale) => ScalePosition(rectangle, Vector2.One * scale);
}
