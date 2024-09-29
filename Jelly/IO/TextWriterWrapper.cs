using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jelly.IO;

public class TextWriterWrapper(TextWriter baseWriter) : TextWriter
{
    public TextWriter BaseWriter { get; set; } = baseWriter;

    public override Encoding Encoding => BaseWriter.Encoding;

    public event EventHandler<TextWriterEventArgs> OnWrite;
    public event EventHandler<TextWriterFormattedEventArgs> OnWriteFormatted;
    public event EventHandler<TextWriterEventArgs> OnWriteLine;
    public event EventHandler<TextWriterFormattedEventArgs> OnWriteLineFormatted;

    public override void Write(ulong value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(bool value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(char value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(char[]? buffer)
    {
        BaseWriter.Write(buffer);
        OnWrite?.Invoke(this, new(buffer));
    }

    public override void Write(int value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(decimal value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(long value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(object value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(float value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(string? value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0)
    {
        BaseWriter.Write(format, arg0);
        OnWriteFormatted?.Invoke(this, new(format, [arg0]));
    }

    public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
    {
        BaseWriter.Write(format, arg0, arg1);
        OnWriteFormatted?.Invoke(this, new(format, [arg0, arg1]));
    }

    public override void Write([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
    {
        BaseWriter.Write(format, arg0, arg1, arg2);
        OnWriteFormatted?.Invoke(this, new(format, [arg0, arg1, arg2]));
    }

    public override void Write([StringSyntax("CompositeFormat")] string format, params object?[]? arg)
    {
        BaseWriter.Write(format, arg);
        OnWriteFormatted?.Invoke(this, new(format, arg));
    }

    public override void Write(uint value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override void Write(char[] buffer, int index, int count)
    {
        BaseWriter.Write(buffer, index, count);
        OnWrite?.Invoke(this, new(buffer, index, count)); // OnWrite?.Invoke(this, new(buffer[index..(index + count - 1)]));
    }

    public override void Write(double value)
    {
        BaseWriter.Write(value);
        OnWrite?.Invoke(this, new(value));
    }

    public override Task WriteAsync(char value)
    {
        var r = BaseWriter.WriteAsync(value);
        OnWrite?.Invoke(this, new(value));
        return r;
    }

    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        var r = BaseWriter.WriteAsync(buffer, index, count);
        OnWrite?.Invoke(this, new(buffer, index, count));
        return r;
    }

    public override Task WriteAsync(string value)
    {
        var r = BaseWriter.WriteAsync(value);
        OnWrite?.Invoke(this, new(value));
        return r;
    }

    public override void WriteLine()
    {
        BaseWriter.WriteLine();
        OnWriteLine?.Invoke(this, new());
    }

    public override void WriteLine(uint value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0)
    {
        BaseWriter.WriteLine(format, arg0);
        OnWriteLineFormatted?.Invoke(this, new(format, [arg0]));
    }

    public override void WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1)
    {
        BaseWriter.WriteLine(format, arg0, arg1);
        OnWriteLineFormatted?.Invoke(this, new(format, [arg0, arg1]));
    }

    public override void WriteLine([StringSyntax("CompositeFormat")] string format, object? arg0, object? arg1, object? arg2)
    {
        BaseWriter.WriteLine(format, arg0, arg1, arg2);
        OnWriteLineFormatted?.Invoke(this, new(format, [arg0, arg1, arg2]));
    }

    public override void WriteLine([StringSyntax("CompositeFormat")] string format, params object?[]? arg)
    {
        BaseWriter.WriteLine(format, arg);
        OnWriteLineFormatted?.Invoke(this, new(format, arg));
    }

    public override void WriteLine(bool value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(char[]? buffer)
    {
        BaseWriter.WriteLine(buffer);
        OnWriteLine?.Invoke(this, new(buffer));
    }

    public override void WriteLine(char[] buffer, int index, int count)
    {
        BaseWriter.WriteLine(buffer, index, count);
        OnWrite?.Invoke(this, new(buffer, index, count));
    }

    public override void WriteLine(decimal value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(double value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(ulong value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(int value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(object? value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(float value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(string? value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(long value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override void WriteLine(char value)
    {
        BaseWriter.WriteLine(value);
        OnWriteLine?.Invoke(this, new(value));
    }

    public override Task WriteLineAsync()
    {
        var r = BaseWriter.WriteLineAsync();
        OnWriteLine?.Invoke(this, new());
        return r;
    }

    public override Task WriteLineAsync(char value)
    {
        var r = BaseWriter.WriteLineAsync(value);
        OnWriteLine?.Invoke(this, new(value));
        return r;
    }

    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
        var r = BaseWriter.WriteLineAsync(buffer, index, count);
        OnWriteLine?.Invoke(this, new(buffer, index, count));
        return r;
    }

    public override Task WriteLineAsync(string value)
    {
        var r = BaseWriter.WriteLineAsync(value);
        OnWriteLine?.Invoke(this, new(value));
        return r;
    }
}

public class TextWriterEventArgs(object? value = null, int? index = null, int? count = null) : EventArgs
{
    public object? Value { get; } = value;

    public int? Index { get; } = index;

    public int? Count { get; } = count;
}

public class TextWriterFormattedEventArgs([StringSyntax("CompositeFormat")] string format, object?[]? arg) : EventArgs
{
    public string Format { get; } = format;

    public object?[]? Arg { get; } = arg;
}
