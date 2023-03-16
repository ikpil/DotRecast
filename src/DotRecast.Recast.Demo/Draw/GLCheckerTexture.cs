/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
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

using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public class GLCheckerTexture
{
    int m_texId;

    public void release()
    {
        // if (m_texId != 0) {
        //     glDeleteTextures(m_texId);
        // }
    }

    public void bind()
    {
        // if (m_texId == 0) {
        //     // Create checker pattern.
        //     int col0 = DebugDraw.duRGBA(215, 215, 215, 255);
        //     int col1 = DebugDraw.duRGBA(255, 255, 255, 255);
        //     int TSIZE = 64;
        //     int[] data = new int[TSIZE * TSIZE];
        //
        //     m_texId = glGenTextures();
        //     glBindTexture(GL_TEXTURE_2D, m_texId);
        //
        //     int level = 0;
        //     int size = TSIZE;
        //     while (size > 0) {
        //         for (int y = 0; y < size; ++y)
        //             for (int x = 0; x < size; ++x)
        //                 data[x + y * size] = (x == 0 || y == 0) ? col0 : col1;
        //         glTexImage2D(GL_TEXTURE_2D, level, GL_RGBA, size, size, 0, GL_RGBA, GL_UNSIGNED_BYTE, data);
        //         size /= 2;
        //         level++;
        //     }
        //
        //     glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_NEAREST);
        //     glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        // } else {
        //     glBindTexture(GL_TEXTURE_2D, m_texId);
        // }
    }
}