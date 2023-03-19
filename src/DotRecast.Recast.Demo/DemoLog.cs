using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Serilog;
using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo;

public static class DemoLog
{
    private static readonly ILogger Logger = Log.ForContext(typeof(DemoLog));
    private static HashSet<string> messages = new();
    
    public static void LogIfGlError(GL gl, [CallerMemberName] string method = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        var err = gl.GetError();
        if (err == GLEnum.NoError)
            return;

        var s = $"{method}() err({err}) in {file}:{line}";
        if (messages.Contains(s))
            return;
        
        messages.Add(s);
        Logger.Error(s);
    }

}