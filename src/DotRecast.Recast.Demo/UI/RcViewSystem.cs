/*
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org

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

using ImGuiNET;
using Serilog;
using Serilog.Core;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.UI;

public class RcViewSystem
{
    private static readonly ILogger Logger = Log.ForContext<RecastDemo>();
    
    private readonly IRcView[] _views;
    private bool _mouseOverUI;
    public bool IsMouseOverUI() => _mouseOverUI;

    public RcViewSystem(IWindow window, IInputContext input, params IRcView[] views)
    {
        // setupClipboard(window);
        // glfwSetCharCallback(window, (w, codepoint) => nk_input_unicode(ctx, codepoint));
        // glContext = new NuklearGL(this);
        _views = views;
    }


    private void setupClipboard(long window)
    {
        // ctx.clip().copy((handle, text, len) => {
        //     if (len == 0) {
        //         return;
        //     }
        //
        //     try (MemoryStack stack = stackPush()) {
        //         ByteBuffer str = stack.malloc(len + 1);
        //         memCopy(text, memAddress(str), len);
        //         str.put(len, (byte) 0);
        //         glfwSetClipboardString(window, str);
        //     }
        // });
        // ctx.clip().paste((handle, edit) => {
        //     long text = nglfwGetClipboardString(window);
        //     if (text != NULL) {
        //         nnk_textedit_paste(edit, text, nnk_strlen(text));
        //     }
        // });
    }

    public void inputBegin()
    {
        //nk_input_begin(ctx);
    }

    public void inputEnd(IWindow win)
    {
        // NkMouse mouse = ctx.input().mouse();
        // if (mouse.grab()) {
        //     glfwSetInputMode(win, GLFW_CURSOR, GLFW_CURSOR_HIDDEN);
        // } else if (mouse.grabbed()) {
        //     float prevX = mouse.prev().x();
        //     float prevY = mouse.prev().y();
        //     glfwSetCursorPos(win, prevX, prevY);
        //     mouse.pos().x(prevX);
        //     mouse.pos().y(prevY);
        // } else if (mouse.ungrab()) {
        //     glfwSetInputMode(win, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
        // }
        // nk_input_end(ctx);
    }

    public void Draw()
    {
        _mouseOverUI = false;
        foreach (IRcView m in _views)
        {
            m.Draw();
            _mouseOverUI |= m.IsMouseInside();
            // if (_mouseOverUI)
            // {
            //     Logger.Information("mouse hover!");
            // }
        }
    }
}