using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using DotRecast.Recast.Demo.Tools;
using ImGuiNET;

namespace DotRecast.Recast.Demo.UI;

public class RcLogView : IRcView
{
    private RecastDemoCanvas _canvas;
    private bool _mouseInside;

    private List<string> _lines = new();
    private readonly ConcurrentQueue<string> _output = new();
    private readonly StringBuilder _outputStringBuilder = new();

    private readonly ConcurrentQueue<string> _error = new();
    private readonly StringBuilder _errorStringBuilder = new();



    public RcLogView()
    {
        Console.SetOut(new ConsoleTextWriterHook(OnOut));
        Console.SetError(new ConsoleTextWriterHook(OnError));
    }

    private void OnOut(string log)
    {
        _output.Enqueue(log);
    }

    private void OnError(string log)
    {
        _error.Enqueue(log);
    }

    public void Clear()
    {
        _lines.Clear();
    }

    private void MergeLines(ConcurrentQueue<string> queue, StringBuilder builder)
    {
        while (queue.TryDequeue(out var s))
        {
            if (s != "\r\n")
            {
                builder.Append(s);
            }
            else
            {
                _lines.Add(builder.ToString());
                builder.Clear();
            }
        }
    }


    public void Bind(RecastDemoCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Update(double dt)
    {
        MergeLines(_output, _outputStringBuilder);
        MergeLines(_error, _errorStringBuilder);
        
        // buffer
        if (10240 < _lines.Count)
        {
            _lines.RemoveRange(0, _lines.Count - 8196);
        }
    }
    
    public bool IsMouseInside() => _mouseInside;

    public void Draw(double dt)
    {
        int otherWidth = 310;
        int height = 234;
        var width = _canvas.Size.X - (otherWidth * 2);
        //var posX = _canvas.Size.X - width;
        ImGui.SetNextWindowPos(new Vector2(otherWidth, _canvas.Size.Y - height));
        ImGui.SetNextWindowSize(new Vector2(width, height));
        if (!ImGui.Begin("Log", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
        {
            ImGui.End();
            return;
        }


        if (ImGui.BeginChild("scrolling", Vector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            _mouseInside = ImGui.IsWindowHovered();
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

            unsafe
            {
                var clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                clipper.Begin(_lines.Count);
                while (clipper.Step())
                {
                    for (int lineNo = clipper.DisplayStart; lineNo < clipper.DisplayEnd; lineNo++)
                    {
                        ImGui.TextUnformatted(_lines[lineNo]);
                    }
                }

                clipper.End();
                clipper.Destroy();
            }

            ImGui.PopStyleVar();

            if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            {
                ImGui.SetScrollHereY(1.0f);
            }
        }

        ImGui.EndChild();
        ImGui.End();
    }
}