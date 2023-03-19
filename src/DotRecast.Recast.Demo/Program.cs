using System.IO;
using DotRecast.Core;
using Serilog;

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

        Log.Logger = new LoggerConfiguration()
            .Enrich.WithThreadId()
            //.Enrich.WithExceptionDetails()
            .Enrich.FromLogContext()
            //.Enrich.WithMethodFullName()
            .MinimumLevel.Verbose()
            .WriteTo.File("logs/log.txt")
            .WriteTo.Console()
            .CreateLogger();
        
        var demo = new RecastDemo();
        demo.start();
    }
}