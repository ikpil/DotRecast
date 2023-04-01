using System;

namespace DotRecast.Core
{
    public struct Vector2f
    {
        public float x;
        public float y;

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

                default: throw new IndexOutOfRangeException($"{index}-{value}");
            }
        }
    }
}