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

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2f))
                return false;

            return Equals((Vector2f)obj);
        }

        public bool Equals(Vector2f other)
        {
            return x.Equals(other.x) &&
                   y.Equals(other.y);
        }

        public override int GetHashCode()
        {
            int hash = x.GetHashCode();
            hash = RcHashCodes.CombineHashCodes(hash, y.GetHashCode());
            return hash;
        }

        public static bool operator ==(Vector2f left, Vector2f right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2f left, Vector2f right)
        {
            return !left.Equals(right);
        }
    }
}