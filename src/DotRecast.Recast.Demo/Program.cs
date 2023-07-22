using System.IO;
using DotRecast.Core;
using DotRecast.Recast.Demo.Logging.Sinks;
using Serilog;
using Serilog.Enrichers;

namespace DotRecast.Recast.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        InitializeWorkingDirectory();
        InitializeLogger();
        StartDemo();
    }

    private static void InitializeLogger()
    {
        var format = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} [{ThreadName}:{ThreadId}]{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithProperty(ThreadNameEnricher.ThreadNamePropertyName, "main")
            .WriteTo.Async(c => c.LogMessageBroker(outputTemplate: format))
            .WriteTo.Async(c => c.Console(outputTemplate: format))
            .WriteTo.Async(c => c.File(
                "logs/log.log",
                rollingInterval: RollingInterval.Hour,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: null,
                outputTemplate: format)
            )
            .CreateLogger();
    }

    private static void InitializeWorkingDirectory()
    {
        var path = Loader.FindParentPath("dungeon.obj");
        path = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(path))
        {
            var workingDirectory = Path.Combine(path, "..");
            workingDirectory = Path.GetFullPath(workingDirectory);
            Directory.SetCurrentDirectory(workingDirectory);
        }
    }

    private static void StartDemo()
    {
        var demo = new RecastDemo();
        demo.Run();
    }
}