using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray8<T>
    {
        public static readonly RcStackArray8<T> Empty = new RcStackArray8<T>();

        private const int Size = 8;
        public int Length => Size;
        
        public T V0;
        public T V1;
        public T V2;
        public T V3;
        public T V4;
        public T V5;
        public T V6;
        public T V7;
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowExceptionIfIndexOutOfRange(int index)
        {
            if (0 > index || index >= Size)
            {
                throw new IndexOutOfRangeException($"{index}");
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowExceptionIfIndexOutOfRange(index);

                return index switch
                {
                    0 => V0,
                    1 => V1,
                    2 => V2,
                    3 => V3,
                    4 => V4,
                    5 => V5,
                    6 => V6,
                    7 => V7,
                    _ => throw new IndexOutOfRangeException($"{index}")
                };
            }

            set
            {
                ThrowExceptionIfIndexOutOfRange(index);

                switch (index)
                {
                    case 0: V0 = value; break;
                    case 1: V1 = value; break;
                    case 2: V2 = value; break;
                    case 3: V3 = value; break;
                    case 4: V4 = value; break;
                    case 5: V5 = value; break;
                    case 6: V6 = value; break;
                    case 7: V7 = value; break;
                }
            }
        }
    }
}