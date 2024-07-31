using System;
using System.Diagnostics;

namespace Jelly;

public class Logger(string name = "main")
{
    private static readonly DateTime _startDate = DateTime.Now;

    public string Name { get; } = name;

    public void Info(object message) => Log(Name, "INFO", message);

    public void Error(object message) => Log(Name, "ERROR", message);

    public void Warn(object message) => Log(Name, "WARN", message);

    public void Log(object message) => Log(Name, null, message);

    private static void Log(string name, string? type, object message)
    {
        var date = DateTime.Now - _startDate;
        Trace.WriteLine($"[{date.Hours:D2}:{date.Minutes:D2}:{date.Seconds:D2}] [{name}{(type is not null ? $"/{type}" : "")}] {message ?? "null"}");
    }

    public static void InfoGeneric(string name, object message) => Log(name, "INFO", message);

    public static void ErrorGeneric(string name, object message) => Log(name, "ERROR", message);

    public static void WarnGeneric(string name, object message) => Log(name, "WARN", message);

    public static void LogGeneric(string name, object message) => Log(name, null, message);

    static Logger()
    {
        TextWriterTraceListener tr2 = new TextWriterTraceListener(System.IO.File.CreateText("latest.log"));
        Trace.Listeners.Add(tr2);
    }
}
