using System;

namespace DotRecast.Core
{
    public struct Vector2f
    {
        public float x;
        public float y;

        public float this[int index]
        {
            set => SetElement(index, value);
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

                default: throw new IndexOutOfRangeException($"{index}-{value}");
            }
        }
    }
}