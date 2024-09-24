using System;
using System.Diagnostics;

namespace Jelly;

public class Logger(string name)
{
    private static readonly DateTime _startDate = DateTime.Now;

    public string Name { get; } = name;

    public enum MessageType
    {
        INFO,
        ERROR,
        WARN,
        NONE
    }

    internal static void Log(string name, MessageType type, object? message)
    {
        var date = DateTime.Now - _startDate;

        string outputMessage = $"[{date.Hours:D2}:{date.Minutes:D2}:{date.Seconds:D2}] [{name}{(type != MessageType.NONE ? $"/{type}" : "")}] {message ?? "null"}";

        switch(type)
        {
            case MessageType.ERROR:
                Console.Error.WriteLine(outputMessage);
                break;
            default:
                Console.Out.WriteLine(outputMessage);
                break;
        }
    }

    public void Log(MessageType type, object? message) => Log(Name, type, message);

    public void LogInfo(object? message) => Log(Name, MessageType.INFO, message);

    public void LogError(object? message) => Log(Name, MessageType.ERROR, message);

    public void LogWarning(object? message) => Log(Name, MessageType.WARN, message);
}
