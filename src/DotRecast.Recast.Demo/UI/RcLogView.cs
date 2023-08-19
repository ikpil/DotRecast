using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using DotRecast.Recast.Demo.Logging.Sinks;
using DotRecast.Recast.Demo.Tools;
using DotRecast.Recast.Demo.UI.ViewModels;
using ImGuiNET;

namespace DotRecast.Recast.Demo.UI;

public class RcLogView : IRcView
{
    private RcCanvas _canvas;
    private bool _isHovered;
    public bool IsHovered() => _isHovered;

    private readonly List<LogMessageItem> _lines;
    private readonly ConcurrentQueue<LogMessageItem> _queues;


    public RcLogView()
    {
        _lines = new();
        _queues = new();

        LogMessageBrokerSink.OnEmitted += OnOut;
    }

    private void OnOut(int level, string message)
    {
        var lines = message
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new LogMessageItem { Level = level, Message = x });

        _lines.AddRange(lines);
    }

    public void Clear()
    {
        _lines.Clear();
    }

    public void Bind(RcCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Update(double dt)
    {
        while (_queues.TryDequeue(out var item))
            _lines.Add(item);

        // buffer
        if (10240 < _lines.Count)
        {
            _lines.RemoveRange(0, _lines.Count - 8196);
        }
    }


    public void Draw(double dt)
    {
        if (!ImGui.Begin("Log"))
        {
            ImGui.End();
            return;
        }

        // size reset
        var size = ImGui.GetItemRectSize();
        if (32 >= size.X && 32 >= size.Y)
        {
            int otherWidth = 310;
            int height = 234;
            var width = _canvas.Size.X - (otherWidth * 2);
            //var posX = _canvas.Size.X - width;
            // ImGui.SetNextWindowPos(new Vector2(otherWidth1, _canvas.Size.Y - height));
            ImGui.SetWindowSize(new Vector2(width, height));
        }


        if (ImGui.BeginChild("scrolling", Vector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar))
        {
            _isHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly | ImGuiHoveredFlags.RootAndChildWindows);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

            unsafe
            {
                var clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                clipper.Begin(_lines.Count);
                while (clipper.Step())
                {
                    for (int lineNo = clipper.DisplayStart; lineNo < clipper.DisplayEnd; lineNo++)
                    {
                        ImGui.TextUnformatted(_lines[lineNo].Message);
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