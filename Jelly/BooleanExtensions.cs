using System.IO;

namespace Jelly;

public static class BooleanExtensions
{
    public static Bit ToBit(this bool value)
    {
        return value;
    }

    public static Bit ToBit(this int value)
    {
        return value;
    }

    public static byte ToByte(this bool value)
    {
        return (byte)(value ? 1 : 0);
    }

    public static int ToInt32(this bool value)
    {
        return value ? 1 : 0;
    }

    public static uint ToUInt32(this bool value)
    {
        return (uint)(value ? 1 : 0);
    }

    public static long ToInt64(this bool value)
    {
        return value ? 1 : 0;
    }

    public static ulong ToUInt64(this bool value)
    {
        return (ulong)(value ? 1 : 0);
    }

    public static float ToSingle(this bool value)
    {
        return value ? 1.0f : 0.0f;
    }

    public static double ToDouble(this bool value)
    {
        return value ? 1.0 : 0.0;
    }

    /// <summary>
    /// Writes a one-byte Bit value to the current stream.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value">The Bit value to write (0 or 1).</param>
    public static void Write(this BinaryWriter writer, Bit value)
    {
        writer.Write((byte)value);
    }

    public static Bit ReadBit(this BinaryReader reader)
    {
        return reader.ReadBoolean();
    }
}
