using System;
using System.Diagnostics;

namespace Jelly;

public class Logger(string name = "Main")
{
    private static readonly DateTime _startDate = DateTime.Now;

    public string Name { get; } = name;

    public static void Log(string name, MessageType type, object message)
    {
        var date = DateTime.Now - _startDate;
        Trace.WriteLine($"[{date.Hours:D2}:{date.Minutes:D2}:{date.Seconds:D2}] [{name}{(type != MessageType.NONE ? $"/{type}" : "")}] {message ?? "null"}");
    }

    public enum MessageType
    {
        INFO,
        ERROR,
        WARN,
        NONE
    }

    public void Log(MessageType type, object message) => Log(Name, type, message);

    public void Info(object message) => Log(Name, MessageType.INFO, message);

    public void Error(object message) => Log(Name, MessageType.ERROR, message);

    public void Warn(object message) => Log(Name, MessageType.WARN, message);

    public static void Info(string name, object message) => Log(name, MessageType.INFO, message);

    public static void Error(string name, object message) => Log(name, MessageType.ERROR, message);

    public static void Warn(string name, object message) => Log(name, MessageType.WARN, message);
}
