/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

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

using System.Numerics;
using DotRecast.Core.Collections;
using DotRecast.Recast.Demo.Tools;
using ImGuiNET;

namespace DotRecast.Recast.Demo.UI;

public class RcToolsetView : IRcView
{
    //private readonly NkColor white = NkColor.Create();
    private int _currentToolIdx = 0;
    private ISampleTool _currentSampleTool;
    private bool enabled;
    private readonly ISampleTool[] tools;
    private bool _isHovered;
    public bool IsHovered() => _isHovered;

    private RcCanvas _canvas;

    public RcToolsetView(params ISampleTool[] tools)
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
        ImGui.Begin("Tools");

        // size reset
        var size = ImGui.GetItemRectSize();
        if (32 >= size.X && 32 >= size.Y)
        {
            int width = 310;
            //ImGui.SetWindowPos(new Vector2(0, 0));
            ImGui.SetWindowSize(new Vector2(width, _canvas.Size.Y));
        }

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

        _currentSampleTool = tools[_currentToolIdx];
        ImGui.Text(_currentSampleTool.GetTool().GetName());
        ImGui.Separator();
        _currentSampleTool.Layout();

        ImGui.End();
    }

    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    public ISampleTool GetTool()
    {
        return _currentSampleTool;
    }

    public void SetSample(DemoSample sample)
    {
        tools.ForEach(t => t.SetSample(sample));
        tools.ForEach(t => t.OnSampleChanged());
    }
}