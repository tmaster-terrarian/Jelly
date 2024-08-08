using System;
using System.Numerics;

namespace Jelly;

public readonly struct Bit(bool value) : IComparable<Bit>, IEquatable<Bit>, IEquatable<bool>, IBitwiseOperators<Bit, Bit, Bit>, IEqualityOperators<Bit, Bit, bool>, IComparisonOperators<Bit, Bit, bool>, IMinMaxValue<Bit>, IModulusOperators<Bit, Bit, Bit>
{
    public readonly bool Value => value;

    private readonly byte ByteValue => IfElse(this.Value, (byte)1, (byte)0);

    public static Bit MaxValue => true;
    public static Bit MinValue => false;

    private static T IfElse<T>(bool v, T a, T b) => v ? a : b;

    public override string ToString()
    {
        return this.Value ? "1" : "0";
    }

    public override int GetHashCode()
    {
        return (int)this;
    }

    public int CompareTo(Bit other)
    {
        return Math.Sign(this.ByteValue - other.ByteValue);
    }

    public bool Equals(Bit other)
    {
        return this.Value == other.Value;
    }

    public bool Equals(bool other)
    {
        return this.Value == other;
    }

    public static bool operator ==(Bit left, Bit right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Bit left, Bit right)
    {
        return !(left == right);
    }

    public static Bit operator ^(Bit left, Bit right)
    {
        return new(left.Value ^ right.Value);
    }

    public static Bit operator &(Bit left, Bit right)
    {
        return new(left.Value & right.Value);
    }

    public static Bit operator |(Bit left, Bit right)
    {
        return new(left.Value | right.Value);
    }

    public static Bit operator ~(Bit value)
    {
        return !value;
    }

    public static Bit operator %(Bit left, Bit right)
    {
        return left.ByteValue % right.ByteValue;
    }

    public override bool Equals(object obj)
    {
        return obj is Bit bit && Equals(bit);
    }

    public static implicit operator Bit(bool value)
    {
        return new(value);
    }

    /// <summary>
    /// Uses the first bit of <c><paramref name="value"/></c>
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Bit(int value)
    {
        return new((value & 1) == 1);
    }

    public static explicit operator bool(Bit value)
    {
        return value.Value;
    }

    public static explicit operator byte(Bit value)
    {
        return IfElse<byte>(value.Value, 1, 0);
    }

    public static explicit operator sbyte(Bit value)
    {
        return IfElse<sbyte>(value.Value, 1, 0);
    }

    public static explicit operator ushort(Bit value)
    {
        return IfElse<ushort>(value.Value, 1, 0);
    }

    public static explicit operator short(Bit value)
    {
        return IfElse<short>(value.Value, 1, 0);
    }

    public static explicit operator uint(Bit value)
    {
        return IfElse<uint>(value.Value, 1, 0);
    }

    public static explicit operator int(Bit value)
    {
        return IfElse<int>(value.Value, 1, 0);
    }

    public static explicit operator ulong(Bit value)
    {
        return IfElse<ulong>(value.Value, 1, 0);
    }

    public static explicit operator long(Bit value)
    {
        return IfElse<long>(value.Value, 1, 0);
    }

    public static bool operator <(Bit left, Bit right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Bit left, Bit right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Bit left, Bit right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Bit left, Bit right)
    {
        return left.CompareTo(right) >= 0;
    }

    public static Bit operator !(Bit value)
    {
        return new(!value.Value);
    }
}
