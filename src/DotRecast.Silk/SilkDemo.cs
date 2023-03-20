using Serilog;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace DotRecast.Silk;

public class SilkDemo
{
    private static readonly ILogger Logger = Log.ForContext<SilkDemo>();

    private IWindow _win;

    public void Run()
    {
        Log.Logger.Information("running");
        
        var options = WindowOptions.Default;
        options.Title = "silk demo";
        options.Size = new Vector2D<int>(1024, 768);
        options.VSync = false;
        //options.ShouldSwapAutomatically = false;
        _win = Window.Create(options);

        _win.Closing += OnWindowClosing;
        _win.Load += OnWindowLoad;
        _win.Resize += OnWindowResize;
        _win.FramebufferResize += OnWindowFramebufferResize;
        _win.Update += OnWindowUpdate;
        _win.Render += OnWindowRender;
        
        _win.Run();
    }

    private void OnWindowClosing()
    {
        
    }

    private void OnWindowLoad()
    {
        
    }

    private void OnWindowResize(Vector2D<int> size)
    {
        
    }

    private void OnWindowFramebufferResize(Vector2D<int> size)
    {
        
    }

    private void OnWindowUpdate(double dt)
    {
        
    }

    private void OnWindowRender(double dt)
    {
        
    }
}