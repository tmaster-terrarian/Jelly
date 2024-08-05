using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Jelly.IO;

public class BinaryStream : Stream
{
    private long _position = 0;

    private readonly List<byte> bytes;

    public ReadOnlyCollection<byte> Buffer => bytes.AsReadOnly();

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => true;

    public override long Length => bytes.Count;

    public override long Position
    {
        get => _position;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, Length, nameof(value));

            _position = value;
        }
    }

    public bool EndOfStream => Position >= Length;

    public BinaryStream() => bytes = [];

    public BinaryStream(List<byte> data) => bytes = data ?? [];

    public BinaryStream(byte[] data) => bytes = [.. data ?? []];

    public override int ReadByte()
    {
        return bytes[(int)Position++];
    }

    public override void WriteByte(byte value)
    {
        if(EndOfStream)
            bytes.Add(value);
        else
            bytes.Insert((int)Position, value);

        Position++;
    }

    public override void Flush()
    {
        bytes.Clear();
        Position = 0;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if(count == 0 || EndOfStream)
            return 0;

        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if(offset + count >= buffer.Length)
            throw new ArgumentException("Provided offset and count is out of the bounds of the buffer.");

        ArgumentNullException.ThrowIfNull(buffer);

        int num = 0;
        for(int i = 0; i < count; i++)
        {
            if(Position + i >= Length)
                break;

            buffer[i + offset] = (byte)ReadByte();
            num++;
        }

        return num;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch(origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length - 1 - offset;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin));
        }

        return Position;
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if(count == 0)
            return;

        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if(offset + count >= buffer.Length)
            throw new ArgumentException("Provided offset and count is out of the bounds of the buffer.");

        ArgumentNullException.ThrowIfNull(buffer);

        for(int i = 0; i < count; i++)
        {
            if(EndOfStream)
                bytes.Add(buffer[i + offset]);
            else
                bytes.Insert((int)Position, buffer[i + offset]);

            Position++;
        }
    }
}
