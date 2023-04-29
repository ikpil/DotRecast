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

namespace DotRecast.Core
{
    public struct Vector3f
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public static Vector3f Zero { get; } = new Vector3f(0, 0, 0);
        public static Vector3f Up { get; } = new Vector3f(0, 1, 0);

        public static Vector3f Of(float[] f)
        {
            return new Vector3f(f);
        }

        public static Vector3f Of(float x, float y, float z)
        {
            return new Vector3f(x, y, z);
        }

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3f(float[] f)
        {
            x = f[0];
            y = f[1];
            z = f[2];
        }

        public float this[int index]
        {
            get => GetElement(index);
            set => SetElement(index, value);
        }

        public float GetElement(int index)
        {
            switch (index)
            {
                case 0: return x;
                case 1: return y;
                case 2: return z;
                default: throw new IndexOutOfRangeException($"{index}");
            }
        }

        public void SetElement(int index, float value)
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;

                default: throw new IndexOutOfRangeException($"{index}-{value}");
            }
        }

        public static bool operator ==(Vector3f left, Vector3f right)
        {
            return left.x.Equals(right.x)
                   && left.y.Equals(right.y)
                   && left.z.Equals(right.z);
        }

        public static bool operator !=(Vector3f left, Vector3f right)
        {
            return !left.Equals(right);
        }
    }
}
