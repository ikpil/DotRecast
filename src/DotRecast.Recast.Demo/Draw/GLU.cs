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

using System;
using DotRecast.Core;
using Silk.NET.OpenGL;

namespace DotRecast.Recast.Demo.Draw;

public class GLU
{
    public static float[] gluPerspective(float fovy, float aspect, float near, float far)
    {
        float[] projectionMatrix = new float[16];
        glhPerspectivef2(projectionMatrix, fovy, aspect, near, far);
        //glLoadMatrixf(projectionMatrix);
        return projectionMatrix;
    }

    public static void glhPerspectivef2(float[] matrix, float fovyInDegrees, float aspectRatio, float znear,
        float zfar)
    {
        float ymax, xmax;
        ymax = (float)(znear * Math.Tan(fovyInDegrees * Math.PI / 360.0));
        xmax = ymax * aspectRatio;
        glhFrustumf2(matrix, -xmax, xmax, -ymax, ymax, znear, zfar);
    }

    private static void glhFrustumf2(float[] matrix, float left, float right, float bottom, float top, float znear,
        float zfar)
    {
        float temp, temp2, temp3, temp4;
        temp = 2.0f * znear;
        temp2 = right - left;
        temp3 = top - bottom;
        temp4 = zfar - znear;
        matrix[0] = temp / temp2;
        matrix[1] = 0.0f;
        matrix[2] = 0.0f;
        matrix[3] = 0.0f;
        matrix[4] = 0.0f;
        matrix[5] = temp / temp3;
        matrix[6] = 0.0f;
        matrix[7] = 0.0f;
        matrix[8] = (right + left) / temp2;
        matrix[9] = (top + bottom) / temp3;
        matrix[10] = (-zfar - znear) / temp4;
        matrix[11] = -1.0f;
        matrix[12] = 0.0f;
        matrix[13] = 0.0f;
        matrix[14] = (-temp * zfar) / temp4;
        matrix[15] = 0.0f;
    }

    public static int glhUnProjectf(float winx, float winy, float winz, float[] modelview, float[] projection, int[] viewport, ref Vector3f objectCoordinate)
    {
        // Transformation matrices
        float[] m = new float[16], A = new float[16];
        float[] @in = new float[4], @out = new float[4];
        // Calculation for inverting a matrix, compute projection x modelview
        // and store in A[16]
        MultiplyMatrices4by4OpenGL_FLOAT(A, projection, modelview);
        // Now compute the inverse of matrix A
        if (glhInvertMatrixf2(A, m) == 0)
            return 0;
        // Transformation of normalized coordinates between -1 and 1
        @in[0] = (winx - viewport[0]) / viewport[2] * 2.0f - 1.0f;
        @in[1] = (winy - viewport[1]) / viewport[3] * 2.0f - 1.0f;
        @in[2] = 2.0f * winz - 1.0f;
        @in[3] = 1.0f;
        // Objects coordinates
        MultiplyMatrixByVector4by4OpenGL_FLOAT(@out, m, @in);
        if (@out[3] == 0.0)
            return 0;
        @out[3] = 1.0f / @out[3];
        objectCoordinate[0] = @out[0] * @out[3];
        objectCoordinate[1] = @out[1] * @out[3];
        objectCoordinate[2] = @out[2] * @out[3];
        return 1;
    }

    static void MultiplyMatrices4by4OpenGL_FLOAT(float[] result, float[] matrix1, float[] matrix2)
    {
        result[0] = matrix1[0] * matrix2[0] + matrix1[4] * matrix2[1] + matrix1[8] * matrix2[2] + matrix1[12] * matrix2[3];
        result[4] = matrix1[0] * matrix2[4] + matrix1[4] * matrix2[5] + matrix1[8] * matrix2[6] + matrix1[12] * matrix2[7];
        result[8] = matrix1[0] * matrix2[8] + matrix1[4] * matrix2[9] + matrix1[8] * matrix2[10] + matrix1[12] * matrix2[11];
        result[12] = matrix1[0] * matrix2[12] + matrix1[4] * matrix2[13] + matrix1[8] * matrix2[14] + matrix1[12] * matrix2[15];
        
        result[1] = matrix1[1] * matrix2[0] + matrix1[5] * matrix2[1] + matrix1[9] * matrix2[2] + matrix1[13] * matrix2[3];
        result[5] = matrix1[1] * matrix2[4] + matrix1[5] * matrix2[5] + matrix1[9] * matrix2[6] + matrix1[13] * matrix2[7];
        result[9] = matrix1[1] * matrix2[8] + matrix1[5] * matrix2[9] + matrix1[9] * matrix2[10] + matrix1[13] * matrix2[11];
        result[13] = matrix1[1] * matrix2[12] + matrix1[5] * matrix2[13] + matrix1[9] * matrix2[14] + matrix1[13] * matrix2[15];
        
        result[2] = matrix1[2] * matrix2[0] + matrix1[6] * matrix2[1] + matrix1[10] * matrix2[2] + matrix1[14] * matrix2[3];
        result[6] = matrix1[2] * matrix2[4] + matrix1[6] * matrix2[5] + matrix1[10] * matrix2[6] + matrix1[14] * matrix2[7];
        result[10] = matrix1[2] * matrix2[8] + matrix1[6] * matrix2[9] + matrix1[10] * matrix2[10] + matrix1[14] * matrix2[11];
        result[14] = matrix1[2] * matrix2[12] + matrix1[6] * matrix2[13] + matrix1[10] * matrix2[14] + matrix1[14] * matrix2[15];
        
        result[3] = matrix1[3] * matrix2[0] + matrix1[7] * matrix2[1] + matrix1[11] * matrix2[2] + matrix1[15] * matrix2[3];
        result[7] = matrix1[3] * matrix2[4] + matrix1[7] * matrix2[5] + matrix1[11] * matrix2[6] + matrix1[15] * matrix2[7];
        result[11] = matrix1[3] * matrix2[8] + matrix1[7] * matrix2[9] + matrix1[11] * matrix2[10] + matrix1[15] * matrix2[11];
        result[15] = matrix1[3] * matrix2[12] + matrix1[7] * matrix2[13] + matrix1[11] * matrix2[14] + matrix1[15] * matrix2[15];
    }

    static void MultiplyMatrixByVector4by4OpenGL_FLOAT(float[] resultvector, float[] matrix, float[] pvector)
    {
        resultvector[0] = matrix[0] * pvector[0] + matrix[4] * pvector[1] + matrix[8] * pvector[2] + matrix[12] * pvector[3];
        resultvector[1] = matrix[1] * pvector[0] + matrix[5] * pvector[1] + matrix[9] * pvector[2] + matrix[13] * pvector[3];
        resultvector[2] = matrix[2] * pvector[0] + matrix[6] * pvector[1] + matrix[10] * pvector[2] + matrix[14] * pvector[3];
        resultvector[3] = matrix[3] * pvector[0] + matrix[7] * pvector[1] + matrix[11] * pvector[2] + matrix[15] * pvector[3];
    }

    // This code comes directly from GLU except that it is for float
    static int glhInvertMatrixf2(float[] m, float[] @out)
    {
        float[][] wtmp = ArrayUtils.Of<float>(4, 8);
        float m0, m1, m2, m3, s;
        float[] r0, r1, r2, r3;
        r0 = wtmp[0];
        r1 = wtmp[1];
        r2 = wtmp[2];
        r3 = wtmp[3];
        r0[0] = MAT(m, 0, 0);
        r0[1] = MAT(m, 0, 1);
        r0[2] = MAT(m, 0, 2);
        r0[3] = MAT(m, 0, 3);
        r0[4] = 1.0f;
        r0[5] = r0[6] = r0[7] = 0.0f;
        r1[0] = MAT(m, 1, 0);
        r1[1] = MAT(m, 1, 1);
        r1[2] = MAT(m, 1, 2);
        r1[3] = MAT(m, 1, 3);
        r1[5] = 1.0f;
        r1[4] = r1[6] = r1[7] = 0.0f;
        r2[0] = MAT(m, 2, 0);
        r2[1] = MAT(m, 2, 1);
        r2[2] = MAT(m, 2, 2);
        r2[3] = MAT(m, 2, 3);
        r2[6] = 1.0f;
        r2[4] = r2[5] = r2[7] = 0.0f;
        r3[0] = MAT(m, 3, 0);
        r3[1] = MAT(m, 3, 1);
        r3[2] = MAT(m, 3, 2);
        r3[3] = MAT(m, 3, 3);
        r3[7] = 1.0f;
        r3[4] = r3[5] = r3[6] = 0.0f;
        /* choose pivot - or die */
        if (Math.Abs(r3[0]) > Math.Abs(r2[0]))
        {
            float[] r = r2;
            r2 = r3;
            r3 = r;
        }

        if (Math.Abs(r2[0]) > Math.Abs(r1[0]))
        {
            float[] r = r2;
            r2 = r1;
            r1 = r;
        }

        if (Math.Abs(r1[0]) > Math.Abs(r0[0]))
        {
            float[] r = r1;
            r1 = r0;
            r0 = r;
        }

        if (0.0 == r0[0])
            return 0;
        /* eliminate first variable */
        m1 = r1[0] / r0[0];
        m2 = r2[0] / r0[0];
        m3 = r3[0] / r0[0];
        s = r0[1];
        r1[1] -= m1 * s;
        r2[1] -= m2 * s;
        r3[1] -= m3 * s;
        s = r0[2];
        r1[2] -= m1 * s;
        r2[2] -= m2 * s;
        r3[2] -= m3 * s;
        s = r0[3];
        r1[3] -= m1 * s;
        r2[3] -= m2 * s;
        r3[3] -= m3 * s;
        s = r0[4];
        if (s != 0.0)
        {
            r1[4] -= m1 * s;
            r2[4] -= m2 * s;
            r3[4] -= m3 * s;
        }

        s = r0[5];
        if (s != 0.0)
        {
            r1[5] -= m1 * s;
            r2[5] -= m2 * s;
            r3[5] -= m3 * s;
        }

        s = r0[6];
        if (s != 0.0)
        {
            r1[6] -= m1 * s;
            r2[6] -= m2 * s;
            r3[6] -= m3 * s;
        }

        s = r0[7];
        if (s != 0.0)
        {
            r1[7] -= m1 * s;
            r2[7] -= m2 * s;
            r3[7] -= m3 * s;
        }

        /* choose pivot - or die */
        if (Math.Abs(r3[1]) > Math.Abs(r2[1]))
        {
            float[] r = r2;
            r2 = r3;
            r3 = r;
        }

        if (Math.Abs(r2[1]) > Math.Abs(r1[1]))
        {
            float[] r = r2;
            r2 = r1;
            r1 = r;
        }

        if (0.0 == r1[1])
            return 0;
        /* eliminate second variable */
        m2 = r2[1] / r1[1];
        m3 = r3[1] / r1[1];
        r2[2] -= m2 * r1[2];
        r3[2] -= m3 * r1[2];
        r2[3] -= m2 * r1[3];
        r3[3] -= m3 * r1[3];
        s = r1[4];
        if (0.0 != s)
        {
            r2[4] -= m2 * s;
            r3[4] -= m3 * s;
        }

        s = r1[5];
        if (0.0 != s)
        {
            r2[5] -= m2 * s;
            r3[5] -= m3 * s;
        }

        s = r1[6];
        if (0.0 != s)
        {
            r2[6] -= m2 * s;
            r3[6] -= m3 * s;
        }

        s = r1[7];
        if (0.0 != s)
        {
            r2[7] -= m2 * s;
            r3[7] -= m3 * s;
        }

        /* choose pivot - or die */
        if (Math.Abs(r3[2]) > Math.Abs(r2[2]))
        {
            float[] r = r2;
            r2 = r3;
            r3 = r;
        }

        if (0.0 == r2[2])
            return 0;
        /* eliminate third variable */
        m3 = r3[2] / r2[2];
        r3[3] -= m3 * r2[3];
        r3[4] -= m3 * r2[4];
        r3[5] -= m3 * r2[5];
        r3[6] -= m3 * r2[6];
        r3[7] -= m3 * r2[7];
        /* last check */
        if (0.0 == r3[3])
            return 0;
        s = 1.0f / r3[3]; /* now back substitute row 3 */
        r3[4] *= s;
        r3[5] *= s;
        r3[6] *= s;
        r3[7] *= s;
        m2 = r2[3]; /* now back substitute row 2 */
        s = 1.0f / r2[2];
        r2[4] = s * (r2[4] - r3[4] * m2);
        r2[5] = s * (r2[5] - r3[5] * m2);
        r2[6] = s * (r2[6] - r3[6] * m2);
        r2[7] = s * (r2[7] - r3[7] * m2);
        m1 = r1[3];
        r1[4] -= r3[4] * m1;
        r1[5] -= r3[5] * m1;
        r1[6] -= r3[6] * m1;
        r1[7] -= r3[7] * m1;
        m0 = r0[3];
        r0[4] -= r3[4] * m0;
        r0[5] -= r3[5] * m0;
        r0[6] -= r3[6] * m0;
        r0[7] -= r3[7] * m0;
        m1 = r1[2]; /* now back substitute row 1 */
        s = 1.0f / r1[1];
        r1[4] = s * (r1[4] - r2[4] * m1);
        r1[5] = s * (r1[5] - r2[5] * m1);
        r1[6] = s * (r1[6] - r2[6] * m1);
        r1[7] = s * (r1[7] - r2[7] * m1);
        m0 = r0[2];
        r0[4] -= r2[4] * m0;
        r0[5] -= r2[5] * m0;
        r0[6] -= r2[6] * m0;
        r0[7] -= r2[7] * m0;
        m0 = r0[1]; /* now back substitute row 0 */
        s = 1.0f / r0[0];
        r0[4] = s * (r0[4] - r1[4] * m0);
        r0[5] = s * (r0[5] - r1[5] * m0);
        r0[6] = s * (r0[6] - r1[6] * m0);
        r0[7] = s * (r0[7] - r1[7] * m0);
        MAT(@out, 0, 0, r0[4]);
        MAT(@out, 0, 1, r0[5]);
        MAT(@out, 0, 2, r0[6]);
        MAT(@out, 0, 3, r0[7]);
        MAT(@out, 1, 0, r1[4]);
        MAT(@out, 1, 1, r1[5]);
        MAT(@out, 1, 2, r1[6]);
        MAT(@out, 1, 3, r1[7]);
        MAT(@out, 2, 0, r2[4]);
        MAT(@out, 2, 1, r2[5]);
        MAT(@out, 2, 2, r2[6]);
        MAT(@out, 2, 3, r2[7]);
        MAT(@out, 3, 0, r3[4]);
        MAT(@out, 3, 1, r3[5]);
        MAT(@out, 3, 2, r3[6]);
        MAT(@out, 3, 3, r3[7]);
        return 1;
    }

    static float MAT(float[] m, int r, int c)
    {
        return m[(c) * 4 + (r)];
    }

    static void MAT(float[] m, int r, int c, float v)
    {
        m[(c) * 4 + (r)] = v;
    }

    public static float[] build_4x4_rotation_matrix(float a, float x, float y, float z)
    {
        float[] matrix = new float[16];
        a = (float)(a * Math.PI / 180.0); // convert to radians
        float s = (float)Math.Sin(a);
        float c = (float)Math.Cos(a);
        float t = 1.0f - c;

        float tx = t * x;
        float ty = t * y;
        float tz = t * z;

        float sz = s * z;
        float sy = s * y;
        float sx = s * x;

        matrix[0] = tx * x + c;
        matrix[1] = tx * y + sz;
        matrix[2] = tx * z - sy;
        matrix[3] = 0;

        matrix[4] = tx * y - sz;
        matrix[5] = ty * y + c;
        matrix[6] = ty * z + sx;
        matrix[7] = 0;

        matrix[8] = tx * z + sy;
        matrix[9] = ty * z - sx;
        matrix[10] = tz * z + c;
        matrix[11] = 0;

        matrix[12] = 0;
        matrix[13] = 0;
        matrix[14] = 0;
        matrix[15] = 1;
        return matrix;
    }

    public static float[] mul(float[] left, float[] right)
    {
        float m00 = left[0] * right[0] + left[4] * right[1] + left[8] * right[2] + left[12] * right[3];
        float m01 = left[1] * right[0] + left[5] * right[1] + left[9] * right[2] + left[13] * right[3];
        float m02 = left[2] * right[0] + left[6] * right[1] + left[10] * right[2] + left[14] * right[3];
        float m03 = left[3] * right[0] + left[7] * right[1] + left[11] * right[2] + left[15] * right[3];
        float m10 = left[0] * right[4] + left[4] * right[5] + left[8] * right[6] + left[12] * right[7];
        float m11 = left[1] * right[4] + left[5] * right[5] + left[9] * right[6] + left[13] * right[7];
        float m12 = left[2] * right[4] + left[6] * right[5] + left[10] * right[6] + left[14] * right[7];
        float m13 = left[3] * right[4] + left[7] * right[5] + left[11] * right[6] + left[15] * right[7];
        float m20 = left[0] * right[8] + left[4] * right[9] + left[8] * right[10] + left[12] * right[11];
        float m21 = left[1] * right[8] + left[5] * right[9] + left[9] * right[10] + left[13] * right[11];
        float m22 = left[2] * right[8] + left[6] * right[9] + left[10] * right[10] + left[14] * right[11];
        float m23 = left[3] * right[8] + left[7] * right[9] + left[11] * right[10] + left[15] * right[11];
        float m30 = left[0] * right[12] + left[4] * right[13] + left[8] * right[14] + left[12] * right[15];
        float m31 = left[1] * right[12] + left[5] * right[13] + left[9] * right[14] + left[13] * right[15];
        float m32 = left[2] * right[12] + left[6] * right[13] + left[10] * right[14] + left[14] * right[15];
        float m33 = left[3] * right[12] + left[7] * right[13] + left[11] * right[14] + left[15] * right[15];

        float[] dest = new float[16];
        dest[0] = m00;
        dest[1] = m01;
        dest[2] = m02;
        dest[3] = m03;
        dest[4] = m10;
        dest[5] = m11;
        dest[6] = m12;
        dest[7] = m13;
        dest[8] = m20;
        dest[9] = m21;
        dest[10] = m22;
        dest[11] = m23;
        dest[12] = m30;
        dest[13] = m31;
        dest[14] = m32;
        dest[15] = m33;

        return dest;
    }
}