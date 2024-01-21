using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray4<T>
    {
        public static RcStackArray4<T> Empty => new RcStackArray4<T>();

        private const int Size = 4;
        public int Length => Size;

        public T V0;
        public T V1;
        public T V2;
        public T V3;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

                return index switch
                {
                    0 => V0,
                    1 => V1,
                    2 => V2,
                    3 => V3,
                    _ => throw new IndexOutOfRangeException($"{index}")
                };
            }

            set
            {
                ThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

                switch (index)
                {
                    case 0: V0 = value; break;
                    case 1: V1 = value; break;
                    case 2: V2 = value; break;
                    case 3: V3 = value; break;
                }
            }
        }
    }
}