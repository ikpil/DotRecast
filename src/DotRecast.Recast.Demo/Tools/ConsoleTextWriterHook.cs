using System;
using System.IO;
using System.Text;

namespace DotRecast.Recast.Demo.Tools;

public class ConsoleTextWriterHook : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;
    private readonly Action<string> _event;

    public ConsoleTextWriterHook(Action<string> relay)
    {
        _event = relay;
    }

    public override void Write(char[] buffer, int index, int count)
    {
        var s = new string(new Span<char>(buffer, index, count));
        _event?.Invoke(s);
    }
}