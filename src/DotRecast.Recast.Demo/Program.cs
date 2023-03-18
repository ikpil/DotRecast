using System.IO;
using DotRecast.Core;

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

        var demo = new RecastDemo();
        demo.start();
    }
}