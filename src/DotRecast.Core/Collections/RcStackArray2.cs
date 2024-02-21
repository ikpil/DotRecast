using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray2<T>
    {
        public static RcStackArray2<T> Empty => new RcStackArray2<T>();

        private const int Size = 2;
        public int Length => Size;

        public T V0;
        public T V1;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

                return index switch
                {
                    0 => V0,
                    1 => V1,
                    _ => throw new IndexOutOfRangeException($"{index}")
                };
            }

            set
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

                switch (index)
                {
                    case 0: V0 = value; break;
                    case 1: V1 = value; break;
                }
            }
        }
    }
}