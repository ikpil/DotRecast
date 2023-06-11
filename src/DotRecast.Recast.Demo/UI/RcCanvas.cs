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

using System.Numerics;
using ImGuiNET;
using Serilog;
using Serilog.Core;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.UI;

public class RcCanvas
{
    private static readonly ILogger Logger = Log.ForContext<RecastDemo>();

    private readonly IWindow _window;
    private readonly IRcView[] _views;
    private bool _mouseOverUI;
    public bool IsMouseOverUI() => _mouseOverUI;

    public Vector2D<int> Size => _window.Size;

    public RcCanvas(IWindow window, params IRcView[] views)
    {
        _window = window;
        _views = views;
        foreach (var view in _views)
        {
            view.Bind(this);
        }

        // SetupClipboard(window);
        // GlfwSetCharCallback(window, (w, codepoint) => Nk_input_unicode(ctx, codepoint));
        // glContext = new NuklearGL(this);
    }


    private void SetupClipboard(long window)
    {
        // ctx.Clip().copy((handle, text, len) => {
        //     if (len == 0) {
        //         return;
        //     }
        //
        //     try (MemoryStack stack = StackPush()) {
        //         ByteBuffer str = stack.Malloc(len + 1);
        //         MemCopy(text, MemAddress(str), len);
        //         str.Put(len, (byte) 0);
        //         GlfwSetClipboardString(window, str);
        //     }
        // });
        // ctx.Clip().paste((handle, edit) => {
        //     long text = NglfwGetClipboardString(window);
        //     if (text != NULL) {
        //         Nnk_textedit_paste(edit, text, Nnk_strlen(text));
        //     }
        // });
    }

    public void InputBegin()
    {
        //Nk_input_begin(ctx);
    }

    public void InputEnd(IWindow win)
    {
        // NkMouse mouse = ctx.Input().Mouse();
        // if (mouse.Grab()) {
        //     GlfwSetInputMode(win, GLFW_CURSOR, GLFW_CURSOR_HIDDEN);
        // } else if (mouse.Grabbed()) {
        //     float prevX = mouse.Prev().x();
        //     float prevY = mouse.Prev().y();
        //     GlfwSetCursorPos(win, prevX, prevY);
        //     mouse.Pos().x(prevX);
        //     mouse.Pos().y(prevY);
        // } else if (mouse.Ungrab()) {
        //     GlfwSetInputMode(win, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
        // }
        // Nk_input_end(ctx);
    }

    public void Update(double dt)
    {
        foreach (var view in _views)
        {
            view.Update(dt);
        }
    }

    public void Draw(double dt)
    {
        _mouseOverUI = false;
        foreach (var view in _views)
        {
            view.Draw(dt);
            _mouseOverUI |= view.IsMouseInside();
        }
    }
}