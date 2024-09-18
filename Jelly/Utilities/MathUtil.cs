using System;
using Microsoft.Xna.Framework;

namespace Jelly.Utilities;

public static class MathUtil
{
    public static float Approach(float value, float target, float rate)
    {
        if(value < target)
            return MathHelper.Min(value + rate, target);
        else
            return MathHelper.Max(value - rate, target);
    }

    public static float Approach(ref float value, float target, float rate)
    {
        if(value < target)
            value = MathHelper.Min(value + rate, target);
        else
            value = MathHelper.Max(value - rate, target);
        return value;
    }

    public static int Approach(int value, int target, int rate)
    {
        if(value < target)
            return MathHelper.Min(value + rate, target);
        else
            return MathHelper.Max(value - rate, target);
    }

    public static int Approach(ref int value, int target, int rate)
    {
        if(value < target)
            value = MathHelper.Min(value + rate, target);
        else
            value = MathHelper.Max(value - rate, target);
        return value;
    }

    public static float Sqr(float value)
    {
        return value*value;
    }

    public static int Sqr(int value)
    {
        return value*value;
    }

    public static float Snap(float value, float interval)
    {
        return MathF.Floor(value / interval) * interval;
    }

    public static float Snap(ref float value, float interval)
    {
        value = MathF.Floor(value / interval) * interval;
        return value;
    }

    public static Vector2 Snap(Vector2 value, Vector2 interval)
    {
        value.X = Snap(value.X, interval.X);
        value.Y = Snap(value.Y, interval.Y);
        return value;
    }

    public static Vector2 Snap(ref Vector2 value, Vector2 interval)
    {
        value.X = Snap(value.X, interval.X);
        value.Y = Snap(value.Y, interval.Y);
        return value;
    }

    public static Vector2 Snap(Vector2 value, float interval)
    {
        value.X = Snap(value.X, interval);
        value.Y = Snap(value.Y, interval);
        return value;
    }

    public static Vector2 Snap(ref Vector2 value, float interval)
    {
        value.X = Snap(value.X, interval);
        value.Y = Snap(value.Y, interval);
        return value;
    }

    public static float SnapCeiling(float value, float interval)
    {
        return MathF.Ceiling(value / interval) * interval;
    }

    public static float SnapCeiling(ref float value, float interval)
    {
        value = MathF.Ceiling(value / interval) * interval;
        return value;
    }

    public static Vector2 SnapCeiling(Vector2 value, Vector2 interval)
    {
        value.X = SnapCeiling(value.X, interval.X);
        value.Y = SnapCeiling(value.Y, interval.Y);
        return value;
    }

    public static Vector2 SnapCeiling(ref Vector2 value, Vector2 interval)
    {
        value.X = SnapCeiling(value.X, interval.X);
        value.Y = SnapCeiling(value.Y, interval.Y);
        return value;
    }

    public static Vector2 SnapCeiling(Vector2 value, float interval)
    {
        value.X = SnapCeiling(value.X, interval);
        value.Y = SnapCeiling(value.Y, interval);
        return value;
    }

    public static Vector2 SnapCeiling(ref Vector2 value, float interval)
    {
        value.X = SnapCeiling(value.X, interval);
        value.Y = SnapCeiling(value.Y, interval);
        return value;
    }

    public static int AbsMax(int value, int max)
    {
        return MathHelper.Max(Math.Abs(value), Math.Abs(max)) * Math.Sign(value);
    }

    public static int AbsMin(int value, int min)
    {
        return MathHelper.Min(Math.Abs(value), Math.Abs(min)) * Math.Sign(value);
    }

    public static int RoundToInt(float value)
    {
        return (int)Math.Round(value);
    }

    public static int CeilToInt(float value)
    {
        return (int)Math.Ceiling(value);
    }

    public static int FloorToInt(float value)
    {
        return (int)Math.Floor(value);
    }
}
