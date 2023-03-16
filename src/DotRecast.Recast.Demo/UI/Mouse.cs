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

using System.Collections.Generic;
using Silk.NET.Input;
using Silk.NET.Windowing;

namespace DotRecast.Recast.Demo.UI;

public class Mouse
{
    private double x;
    private double y;
    private double scrollX;
    private double scrollY;

    private double px;
    private double py;
    private double pScrollX;
    private double pScrollY;
    private readonly HashSet<int> pressed = new();
    private readonly List<MouseListener> listeners = new();

    public Mouse(IInputContext input)
    {
        foreach (IMouse mouse in input.Mice)
        {
            mouse.MouseDown += (mouse, button) => buttonPress((int)button, 0);
            mouse.MouseUp += (mouse, button) => buttonRelease((int)button, 0);
            // if (action == GLFW_PRESS) {
            //     buttonPress(button, mods);
            // } else if (action == GLFW_RELEASE) {
            //     buttonRelease(button, mods);
            // }
        }
        // glfwSetCursorPosCallback(window, (win, x, y) => cursorPos(x, y));
        // glfwSetScrollCallback(window, (win, x, y) => scroll(x, y));
    }

    public void cursorPos(double x, double y)
    {
        foreach (MouseListener l in listeners)
        {
            l.position(x, y);
        }

        this.x = x;
        this.y = y;
    }

    public void scroll(double xoffset, double yoffset)
    {
        foreach (MouseListener l in listeners)
        {
            l.scroll(xoffset, yoffset);
        }

        scrollX += xoffset;
        scrollY += yoffset;
    }

    public double getDX()
    {
        return x - px;
    }

    public double getDY()
    {
        return y - py;
    }

    public double getDScrollX()
    {
        return scrollX - pScrollX;
    }

    public double getDScrollY()
    {
        return scrollY - pScrollY;
    }

    public double getX()
    {
        return x;
    }

    public void setX(double x)
    {
        this.x = x;
    }

    public double getY()
    {
        return y;
    }

    public void setY(double y)
    {
        this.y = y;
    }

    public void setDelta()
    {
        px = x;
        py = y;
        pScrollX = scrollX;
        pScrollY = scrollY;
    }

    public void buttonPress(int button, int mods)
    {
        foreach (MouseListener l in listeners)
        {
            l.button(button, mods, true);
        }

        pressed.Add(button);
    }

    public void buttonRelease(int button, int mods)
    {
        foreach (MouseListener l in listeners)
        {
            l.button(button, mods, false);
        }

        pressed.Remove(button);
    }

    public bool isPressed(int button)
    {
        return pressed.Contains(button);
    }

    public void addListener(MouseListener listener)
    {
        listeners.Add(listener);
    }
}