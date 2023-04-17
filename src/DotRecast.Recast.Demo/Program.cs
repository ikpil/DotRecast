using System.IO;
using DotRecast.Core;
using Serilog;
using Serilog.Enrichers;

namespace DotRecast.Recast.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        
        var path = Loader.ToRPath("dungeon.obj");
        path = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(path))
        {
            var workingDirectory = Path.Combine(path, "..");
            workingDirectory = Path.GetFullPath(workingDirectory);
            Directory.SetCurrentDirectory(workingDirectory);
        }

        var format = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj} [{ThreadName}:{ThreadId}]{NewLine}{Exception}";
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

        Run();
    }

    public static void Run()
    {
        var demo = new RecastDemo();
        demo.Run();
    }
}