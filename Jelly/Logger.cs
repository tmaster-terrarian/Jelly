using System;

namespace Jelly;

public class Logger
{
    private static readonly DateTime _startDate = DateTime.Now;
    private readonly string _name;

    public string Name => _name;

    public Logger(string name = "main")
    {
        this._name = name;
    }

    public void Info(object message) => _Log("INFO", message ?? "null");

    public void Error(object message) => _Log("ERROR", message ?? "null");

    public void Warn(object message) => _Log("WARN", message ?? "null");

    private void _Log(string type, object message)
    {
        var date = DateTime.Now - _startDate;
        Console.Out.WriteLine($"[{date.Hours.ToString("D2")}:{date.Minutes.ToString("D2")}:{date.Seconds.ToString("D2")}] [{Name}/{type}] {message}");
    }
}
