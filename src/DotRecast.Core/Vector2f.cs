using System;

namespace DotRecast.Core
{
    public struct Vector2f
    {
        public float x;
        public float y;

        public static Vector2f Zero { get; } = new Vector2f { x = 0, y = 0 };

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