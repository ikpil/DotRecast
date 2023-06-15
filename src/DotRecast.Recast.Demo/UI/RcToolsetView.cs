/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.Numerics;
using DotRecast.Core;
using DotRecast.Recast.Demo.Tools;
using DotRecast.Recast.DemoTool;
using ImGuiNET;

namespace DotRecast.Recast.Demo.UI;

public class RcToolsetView : IRcView
{
    //private readonly NkColor white = NkColor.Create();
    private int _currentToolIdx = 0;
    private IRcTool currentTool;
    private bool enabled;
    private readonly IRcTool[] tools;
    private bool _isHovered;
    public bool IsHovered() => _isHovered;

    private RcCanvas _canvas;

    public RcToolsetView(params IRcTool[] tools)
    {
        this.tools = tools;
    }

    public void Bind(RcCanvas canvas)
    {
        _canvas = canvas;
    }

    public void Update(double dt)
    {
    }

    public void Draw(double dt)
    {
        int width = 310;
        ImGui.SetNextWindowPos(new Vector2(0, 0));
        ImGui.SetNextWindowSize(new Vector2(width, _canvas.Size.Y));
        ImGui.Begin("Tools", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
        _isHovered = ImGui.IsWindowHovered(ImGuiHoveredFlags.RectOnly | ImGuiHoveredFlags.RootAndChildWindows);

        for (int i = 0; i < tools.Length; ++i)
        {
            var tool = tools[i];
            ImGui.RadioButton(tool.GetTool().GetName(), ref _currentToolIdx, i);
        }

        ImGui.NewLine();

        if (0 > _currentToolIdx || _currentToolIdx >= tools.Length)
        {
            ImGui.End();
            return;
        }

        currentTool = tools[_currentToolIdx];
        ImGui.Text(currentTool.GetTool().GetName());
        ImGui.Separator();
        currentTool.Layout();

        ImGui.End();
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    public IRcTool GetTool()
    {
        return currentTool;
    }

    public void SetSample(Sample sample)
    {
        tools.ForEach(t => t.GetTool().SetSample(sample));
        tools.ForEach(t => t.OnSampleChanged());
    }

    public void HandleUpdate(float dt)
    {
        tools.ForEach(t => t.HandleUpdate(dt));
    }
}