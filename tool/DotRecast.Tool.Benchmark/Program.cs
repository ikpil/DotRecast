using System.Reflection;
using BenchmarkDotNet.Running;

namespace DotRecast.Tool.Benchmark;

public static class Program
{
    public static int Main(string[] args)
    {
        var switcher = BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly());

        if (args == null || args.Length == 0)
        {
            switcher.RunAll();
        }
        else
        {
            switcher.Run(args);
        }

        return 0;
    }
}