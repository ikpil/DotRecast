using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray64<T>
    {
        public static RcStackArray64<T> Empty => new RcStackArray64<T>();

        private const int Size = 64;
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
        public T V32;
        public T V33;
        public T V34;
        public T V35;
        public T V36;
        public T V37;
        public T V38;
        public T V39;
        public T V40;
        public T V41;
        public T V42;
        public T V43;
        public T V44;
        public T V45;
        public T V46;
        public T V47;
        public T V48;
        public T V49;
        public T V50;
        public T V51;
        public T V52;
        public T V53;
        public T V54;
        public T V55;
        public T V56;
        public T V57;
        public T V58;
        public T V59;
        public T V60;
        public T V61;
        public T V62;
        public T V63;

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
                    32 => V32,
                    33 => V33,
                    34 => V34,
                    35 => V35,
                    36 => V36,
                    37 => V37,
                    38 => V38,
                    39 => V39,
                    40 => V40,
                    41 => V41,
                    42 => V42,
                    43 => V43,
                    44 => V44,
                    45 => V45,
                    46 => V46,
                    47 => V47,
                    48 => V48,
                    49 => V49,
                    50 => V50,
                    51 => V51,
                    52 => V52,
                    53 => V53,
                    54 => V54,
                    55 => V55,
                    56 => V56,
                    57 => V57,
                    58 => V58,
                    59 => V59,
                    60 => V60,
                    61 => V61,
                    62 => V62,
                    63 => V63,
                    _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
                };
            }

            set
            {
                RcThrowHelper.ThrowExceptionIfIndexOutOfRange(index, Length);

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
                    case 32 : V32 = value; break;
                    case 33 : V33 = value; break;
                    case 34 : V34 = value; break;
                    case 35 : V35 = value; break;
                    case 36 : V36 = value; break;
                    case 37 : V37 = value; break;
                    case 38 : V38 = value; break;
                    case 39 : V39 = value; break;
                    case 40 : V40 = value; break;
                    case 41 : V41 = value; break;
                    case 42 : V42 = value; break;
                    case 43 : V43 = value; break;
                    case 44 : V44 = value; break;
                    case 45 : V45 = value; break;
                    case 46 : V46 = value; break;
                    case 47 : V47 = value; break;
                    case 48 : V48 = value; break;
                    case 49 : V49 = value; break;
                    case 50 : V50 = value; break;
                    case 51 : V51 = value; break;
                    case 52 : V52 = value; break;
                    case 53 : V53 = value; break;
                    case 54 : V54 = value; break;
                    case 55 : V55 = value; break;
                    case 56 : V56 = value; break;
                    case 57 : V57 = value; break;
                    case 58 : V58 = value; break;
                    case 59 : V59 = value; break;
                    case 60 : V60 = value; break;
                    case 61 : V61 = value; break;
                    case 62 : V62 = value; break;
                    case 63 : V63 = value; break;
                }
            }
        }
    }
}