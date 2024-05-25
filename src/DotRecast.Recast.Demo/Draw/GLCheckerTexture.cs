/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
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

using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public class GLCheckerTexture
{
    private readonly GL _gl;
    private uint m_texId;

    public GLCheckerTexture(GL gl)
    {
        _gl = gl;
    }

    public unsafe void Release()
    {
        if (m_texId != 0)
        {
            fixed (uint* p = &m_texId)
            {
                _gl.DeleteTextures(1, p);
            }
        }
    }

    public unsafe void Bind()
    {
        if (m_texId == 0)
        {
            // Create checker pattern.
            int col0 = DebugDraw.DuRGBA(215, 215, 215, 255);
            int col1 = DebugDraw.DuRGBA(255, 255, 255, 255);
            uint TSIZE = 64;
            int[] data = new int[TSIZE * TSIZE];

            fixed (uint* p = &m_texId)
            {
                _gl.GenTextures(1, p);
            }

            _gl.BindTexture(GLEnum.Texture2D, m_texId);

            int level = 0;
            uint size = TSIZE;
            while (size > 0)
            {
                for (int y = 0; y < size; ++y)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        data[x + y * size] = (x == 0 || y == 0) ? col0 : col1;
                    }
                }

                _gl.TexImage2D<int>(GLEnum.Texture2D, level, InternalFormat.Rgba, size, size, 0, GLEnum.Rgba, GLEnum.UnsignedByte, data);
                size /= 2;
                level++;
            }

            uint linearMipmapNearest = (uint)GLEnum.LinearMipmapNearest;
            uint linear = (uint)GLEnum.Linear;
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, &linearMipmapNearest);
            _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, &linear);
        }
        else
        {
            _gl.BindTexture(GLEnum.Texture2D, m_texId);
        }
    }
}