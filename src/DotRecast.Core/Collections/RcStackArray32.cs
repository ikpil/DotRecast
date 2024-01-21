using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray32<T>
    {
        public static RcStackArray32<T> Empty => new RcStackArray32<T>();

        private const int Size = 32;
        public int Length => Size;
        
        public T V0;
        public T V1;
        public T V2;
        public T V3;
        public T V4;
        public T V5;
        public T V6;
        public T V7;
        public T V8;
        public T V9;
        public T V10;
        public T V11;
        public T V12;
        public T V13;
        public T V14;
        public T V15;
        public T V16;
        public T V17;
        public T V18;
        public T V19;
        public T V20;
        public T V21;
        public T V22;
        public T V23;
        public T V24;
        public T V25;
        public T V26;
        public T V27;
        public T V28;
        public T V29;
        public T V30;
        public T V31;

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
                    4 => V4,
                    5 => V5,
                    6 => V6,
                    7 => V7,
                    8 => V8,
                    9 => V9,
                    10 => V10,
                    11 => V11,
                    12 => V12,
                    13 => V13,
                    14 => V14,
                    15 => V15,
                    16 => V16,
                    17 => V17,
                    18 => V18,
                    19 => V19,
                    20 => V20,
                    21 => V21,
                    22 => V22,
                    23 => V23,
                    24 => V24,
                    25 => V25,
                    26 => V26,
                    27 => V27,
                    28 => V28,
                    29 => V29,
                    30 => V30,
                    31 => V31,
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
                    case 4: V4 = value; break;
                    case 5: V5 = value; break;
                    case 6: V6 = value; break;
                    case 7: V7 = value; break;
                    case 8: V8 = value; break;
                    case 9: V9 = value; break;
                    case 10: V10 = value; break;
                    case 11: V11 = value; break;
                    case 12: V12 = value; break;
                    case 13: V13 = value; break;
                    case 14: V14 = value; break;
                    case 15: V15 = value; break;
                    case 16: V16 = value; break;
                    case 17: V17 = value; break;
                    case 18: V18 = value; break;
                    case 19: V19 = value; break;
                    case 20: V20 = value; break;
                    case 21: V21 = value; break;
                    case 22: V22 = value; break;
                    case 23: V23 = value; break;
                    case 24: V24 = value; break;
                    case 25: V25 = value; break;
                    case 26: V26 = value; break;
                    case 27: V27 = value; break;
                    case 28: V28 = value; break;
                    case 29: V29 = value; break;
                    case 30: V30 = value; break;
                    case 31: V31 = value; break;
                }
            }
        }
    }
}