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
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.Tools;

public class ToolsUI : IRcView {

    //private readonly NkColor white = NkColor.create();
    private Tool currentTool;
    private bool enabled;
    private readonly Tool[] tools;

    public ToolsUI(params Tool[] tools) {
        this.tools = tools;
    }

    public bool render(IWindow ctx, int x, int y, int width, int height, int mouseX, int mouseY) {
        bool mouseInside = false;
        // nk_rgb(255, 255, 255, white);
        // try (MemoryStack stack = stackPush()) {
        //     NkRect rect = NkRect.mallocStack(stack);
        //     if (nk_begin(ctx, "Tools", nk_rect(5, 5, 250, height - 10, rect), NK_WINDOW_BORDER | NK_WINDOW_MOVABLE | NK_WINDOW_TITLE)) {
        //         if (enabled) {
        //             foreach (Tool tool in tools) {
        //                 nk_layout_row_dynamic(ctx, 20, 1);
        //                 if (nk_option_label(ctx, tool.getName(), tool == currentTool)) {
        //                     currentTool = tool;
        //                 }
        //             }
        //             nk_layout_row_dynamic(ctx, 3, 1);
        //             nk_spacing(ctx, 1);
        //             if (currentTool != null) {
        //                 currentTool.layout(ctx);
        //             }
        //         }
        //         nk_window_get_bounds(ctx, rect);
        //         if (mouseX >= rect.x() && mouseX <= rect.x() + rect.w() && mouseY >= rect.y() && mouseY <= rect.y() + rect.h()) {
        //             mouseInside = true;
        //         }
        //     }
        //     nk_end(ctx);
        // }
        return mouseInside;
    }

    public void setEnabled(bool enabled) {
        this.enabled = enabled;
    }

    public Tool getTool() {
        return currentTool;
    }

    public void setSample(Sample sample) {
        tools.forEach(t => t.setSample(sample));
    }

    public void handleUpdate(float dt) {
        tools.forEach(t => t.handleUpdate(dt));
    }

}
