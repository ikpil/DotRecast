/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2021 Piotr Piastucki piotr@jtilia.org

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

using DotRecast.Core;
using DotRecast.Recast.Demo.UI;
using ImGuiNET;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.Tools;

public class ToolsView : IRcView
{
    //private readonly NkColor white = NkColor.create();
    private int _currentToolIdx = 0;
    private Tool currentTool;
    private bool enabled;
    private readonly Tool[] tools;

    public ToolsView(params Tool[] tools)
    {
        this.tools = tools;
    }
    
    private bool _mouseInside;
    public bool IsMouseInside() => _mouseInside;

    public void Draw()
    {
        ImGui.Begin("Tools");
        _mouseInside = ImGui.IsWindowHovered();
        
        for (int i = 0; i < tools.Length; ++i)
        {
            var tool = tools[i];
            ImGui.RadioButton(tool.getName(), ref _currentToolIdx, i);
        }
        ImGui.NewLine();
        
        if (0 > _currentToolIdx || _currentToolIdx >= tools.Length)
        {
            ImGui.End();
            return;
        }

        currentTool = tools[_currentToolIdx];
        ImGui.Text(currentTool.getName());
        ImGui.Separator();
        currentTool.layout();

        ImGui.End();
    }

    public void setEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    public Tool getTool()
    {
        return currentTool;
    }

    public void setSample(Sample sample)
    {
        tools.forEach(t => t.setSample(sample));
    }

    public void handleUpdate(float dt)
    {
        tools.forEach(t => t.handleUpdate(dt));
    }
}