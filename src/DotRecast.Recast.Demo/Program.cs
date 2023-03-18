using System.IO;
using DotRecast.Core;
using Serilog;

namespace DotRecast.Recast.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();
        
        var path = Loader.ToRPath("dungeon.obj");
        path = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(path))
        {
            var workingDirectory = Path.Combine(path, "..");
            workingDirectory = Path.GetFullPath(workingDirectory);
            Directory.SetCurrentDirectory(workingDirectory);
        }

        var demo = new RecastDemo();
        demo.start();
    }
}