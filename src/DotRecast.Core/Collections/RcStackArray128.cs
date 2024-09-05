using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray128<T>
    {
        public static RcStackArray128<T> Empty => new RcStackArray128<T>();

        private const int Size = 128;
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
        public T V64;
        public T V65;
        public T V66;
        public T V67;
        public T V68;
        public T V69;
        public T V70;
        public T V71;
        public T V72;
        public T V73;
        public T V74;
        public T V75;
        public T V76;
        public T V77;
        public T V78;
        public T V79;
        public T V80;
        public T V81;
        public T V82;
        public T V83;
        public T V84;
        public T V85;
        public T V86;
        public T V87;
        public T V88;
        public T V89;
        public T V90;
        public T V91;
        public T V92;
        public T V93;
        public T V94;
        public T V95;
        public T V96;
        public T V97;
        public T V98;
        public T V99;
        public T V100;
        public T V101;
        public T V102;
        public T V103;
        public T V104;
        public T V105;
        public T V106;
        public T V107;
        public T V108;
        public T V109;
        public T V110;
        public T V111;
        public T V112;
        public T V113;
        public T V114;
        public T V115;
        public T V116;
        public T V117;
        public T V118;
        public T V119;
        public T V120;
        public T V121;
        public T V122;
        public T V123;
        public T V124;
        public T V125;
        public T V126;
        public T V127;

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
                    64 => V64,
                    65 => V65,
                    66 => V66,
                    67 => V67,
                    68 => V68,
                    69 => V69,
                    70 => V70,
                    71 => V71,
                    72 => V72,
                    73 => V73,
                    74 => V74,
                    75 => V75,
                    76 => V76,
                    77 => V77,
                    78 => V78,
                    79 => V79,
                    80 => V80,
                    81 => V81,
                    82 => V82,
                    83 => V83,
                    84 => V84,
                    85 => V85,
                    86 => V86,
                    87 => V87,
                    88 => V88,
                    89 => V89,
                    90 => V90,
                    91 => V91,
                    92 => V92,
                    93 => V93,
                    94 => V94,
                    95 => V95,
                    96 => V96,
                    97 => V97,
                    98 => V98,
                    99 => V99,
                    100 => V100,
                    101 => V101,
                    102 => V102,
                    103 => V103,
                    104 => V104,
                    105 => V105,
                    106 => V106,
                    107 => V107,
                    108 => V108,
                    109 => V109,
                    110 => V110,
                    111 => V111,
                    112 => V112,
                    113 => V113,
                    114 => V114,
                    115 => V115,
                    116 => V116,
                    117 => V117,
                    118 => V118,
                    119 => V119,
                    120 => V120,
                    121 => V121,
                    122 => V122,
                    123 => V123,
                    124 => V124,
                    125 => V125,
                    126 => V126,
                    127 => V127,
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
                    case 64 : V64 = value; break;
                    case 65 : V65 = value; break;
                    case 66 : V66 = value; break;
                    case 67 : V67 = value; break;
                    case 68 : V68 = value; break;
                    case 69 : V69 = value; break;
                    case 70 : V70 = value; break;
                    case 71 : V71 = value; break;
                    case 72 : V72 = value; break;
                    case 73 : V73 = value; break;
                    case 74 : V74 = value; break;
                    case 75 : V75 = value; break;
                    case 76 : V76 = value; break;
                    case 77 : V77 = value; break;
                    case 78 : V78 = value; break;
                    case 79 : V79 = value; break;
                    case 80 : V80 = value; break;
                    case 81 : V81 = value; break;
                    case 82 : V82 = value; break;
                    case 83 : V83 = value; break;
                    case 84 : V84 = value; break;
                    case 85 : V85 = value; break;
                    case 86 : V86 = value; break;
                    case 87 : V87 = value; break;
                    case 88 : V88 = value; break;
                    case 89 : V89 = value; break;
                    case 90 : V90 = value; break;
                    case 91 : V91 = value; break;
                    case 92 : V92 = value; break;
                    case 93 : V93 = value; break;
                    case 94 : V94 = value; break;
                    case 95 : V95 = value; break;
                    case 96 : V96 = value; break;
                    case 97 : V97 = value; break;
                    case 98 : V98 = value; break;
                    case 99 : V99 = value; break;
                    case 100 : V100 = value; break;
                    case 101 : V101 = value; break;
                    case 102 : V102 = value; break;
                    case 103 : V103 = value; break;
                    case 104 : V104 = value; break;
                    case 105 : V105 = value; break;
                    case 106 : V106 = value; break;
                    case 107 : V107 = value; break;
                    case 108 : V108 = value; break;
                    case 109 : V109 = value; break;
                    case 110 : V110 = value; break;
                    case 111 : V111 = value; break;
                    case 112 : V112 = value; break;
                    case 113 : V113 = value; break;
                    case 114 : V114 = value; break;
                    case 115 : V115 = value; break;
                    case 116 : V116 = value; break;
                    case 117 : V117 = value; break;
                    case 118 : V118 = value; break;
                    case 119 : V119 = value; break;
                    case 120 : V120 = value; break;
                    case 121 : V121 = value; break;
                    case 122 : V122 = value; break;
                    case 123 : V123 = value; break;
                    case 124 : V124 = value; break;
                    case 125 : V125 = value; break;
                    case 126 : V126 = value; break;
                    case 127 : V127 = value; break;
                }
            }
        }
    }
}