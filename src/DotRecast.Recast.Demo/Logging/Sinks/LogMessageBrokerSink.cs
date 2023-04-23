using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace DotRecast.Recast.Demo.Logging.Sinks;

public class LogMessageBrokerSink : ILogEventSink
{
    public static event Action<int, string> OnEmitted;

    private readonly ITextFormatter _formatter;

    public LogMessageBrokerSink(ITextFormatter formatter)
    {
        _formatter = formatter;
    }

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        _formatter.Format(logEvent, writer);
        OnEmitted?.Invoke((int)logEvent.Level, writer.ToString());
    }
}