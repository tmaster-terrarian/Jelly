using System;

using Microsoft.Xna.Framework;

namespace Jelly;

public static class PointExtensions
{
    public static Point Multiply(this Point point, int value)
    {
        point.X *= value;
        point.Y *= value;
        return point;
    }

    public static Point Divide(this Point point, int value)
    {
        point.X /= value;
        point.Y /= value;
        return point;
    }

    public static int DistanceSquared(this Point point, Point other)
    {
        var p = point - other;
        return p.X * p.X + p.Y * p.Y;
    }

    /// <summary>
    /// Slower than <see cref="DistanceSquared"/>
    /// </summary>
    public static float Distance(this Point point, Point other)
    {
        var p = point - other;
        return MathF.Sqrt(p.X * p.X + p.Y * p.Y);
    }

    public static float ToRotation(this Point value)
    {
        return MathF.Atan2(value.Y, value.X);
    }

    public static float ToRotation(this Vector2 value)
    {
        return MathF.Atan2(value.Y, value.X);
    }

    public static Point Clamp(this Point value, Point min, Point max)
    {
        return new(MathHelper.Clamp(value.X, min.X, max.X), MathHelper.Clamp(value.Y, min.Y, max.Y));
    }
}
