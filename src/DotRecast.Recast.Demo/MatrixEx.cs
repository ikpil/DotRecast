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

using System.Numerics;

namespace DotRecast.Recast.Demo;

static class MatrixEx
{
    public static void CopyTo(this Matrix4x4 s, float[] m)
    {
        m[0] = s.M11;
        m[1] = s.M12;
        m[2] = s.M13;
        m[3] = s.M14;
        m[4] = s.M21;
        m[5] = s.M22;
        m[6] = s.M23;
        m[7] = s.M24;
        m[8] = s.M31;
        m[9] = s.M32;
        m[10] = s.M33;
        m[11] = s.M34;
        m[12] = s.M41;
        m[13] = s.M42;
        m[14] = s.M43;
        m[15] = s.M44;
    }
}