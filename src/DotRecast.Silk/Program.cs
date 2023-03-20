using System;
using DotRecast.Silk;
using Serilog;
using Serilog.Enrichers;

public class Program
{
    // Only Graphics Test
    public static void Main(string[] args)
    {
        var format = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} [{MemberName}()] [{ThreadName}:{ThreadId}] at {FilePath}:{LineNumber} {NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithProperty(ThreadNameEnricher.ThreadNamePropertyName, "main")
            .WriteTo.Console(outputTemplate: format)
            .WriteTo.File(
                "logs/log.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                outputTemplate: format)
            .CreateLogger();

        var demo = new SilkDemo();
        demo.Run();
    }
}