using System;

namespace DotRecast.Core
{
    public struct Vector2f
    {
        public float x;
        public float y;

        public float Get(int idx)
        {
            if (0 == idx)
                return x;

            if (1 == idx)
                return y;

            throw new IndexOutOfRangeException("vector2f index out of range");
        }
    }
}