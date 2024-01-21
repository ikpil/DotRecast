using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray256<T>
    {
        public static RcStackArray256<T> Empty => new RcStackArray256<T>();

        private const int Size = 256;
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
        public T V128;
        public T V129;
        public T V130;
        public T V131;
        public T V132;
        public T V133;
        public T V134;
        public T V135;
        public T V136;
        public T V137;
        public T V138;
        public T V139;
        public T V140;
        public T V141;
        public T V142;
        public T V143;
        public T V144;
        public T V145;
        public T V146;
        public T V147;
        public T V148;
        public T V149;
        public T V150;
        public T V151;
        public T V152;
        public T V153;
        public T V154;
        public T V155;
        public T V156;
        public T V157;
        public T V158;
        public T V159;
        public T V160;
        public T V161;
        public T V162;
        public T V163;
        public T V164;
        public T V165;
        public T V166;
        public T V167;
        public T V168;
        public T V169;
        public T V170;
        public T V171;
        public T V172;
        public T V173;
        public T V174;
        public T V175;
        public T V176;
        public T V177;
        public T V178;
        public T V179;
        public T V180;
        public T V181;
        public T V182;
        public T V183;
        public T V184;
        public T V185;
        public T V186;
        public T V187;
        public T V188;
        public T V189;
        public T V190;
        public T V191;
        public T V192;
        public T V193;
        public T V194;
        public T V195;
        public T V196;
        public T V197;
        public T V198;
        public T V199;
        public T V200;
        public T V201;
        public T V202;
        public T V203;
        public T V204;
        public T V205;
        public T V206;
        public T V207;
        public T V208;
        public T V209;
        public T V210;
        public T V211;
        public T V212;
        public T V213;
        public T V214;
        public T V215;
        public T V216;
        public T V217;
        public T V218;
        public T V219;
        public T V220;
        public T V221;
        public T V222;
        public T V223;
        public T V224;
        public T V225;
        public T V226;
        public T V227;
        public T V228;
        public T V229;
        public T V230;
        public T V231;
        public T V232;
        public T V233;
        public T V234;
        public T V235;
        public T V236;
        public T V237;
        public T V238;
        public T V239;
        public T V240;
        public T V241;
        public T V242;
        public T V243;
        public T V244;
        public T V245;
        public T V246;
        public T V247;
        public T V248;
        public T V249;
        public T V250;
        public T V251;
        public T V252;
        public T V253;
        public T V254;
        public T V255;

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
                    128 => V128,
                    129 => V129,
                    130 => V130,
                    131 => V131,
                    132 => V132,
                    133 => V133,
                    134 => V134,
                    135 => V135,
                    136 => V136,
                    137 => V137,
                    138 => V138,
                    139 => V139,
                    140 => V140,
                    141 => V141,
                    142 => V142,
                    143 => V143,
                    144 => V144,
                    145 => V145,
                    146 => V146,
                    147 => V147,
                    148 => V148,
                    149 => V149,
                    150 => V150,
                    151 => V151,
                    152 => V152,
                    153 => V153,
                    154 => V154,
                    155 => V155,
                    156 => V156,
                    157 => V157,
                    158 => V158,
                    159 => V159,
                    160 => V160,
                    161 => V161,
                    162 => V162,
                    163 => V163,
                    164 => V164,
                    165 => V165,
                    166 => V166,
                    167 => V167,
                    168 => V168,
                    169 => V169,
                    170 => V170,
                    171 => V171,
                    172 => V172,
                    173 => V173,
                    174 => V174,
                    175 => V175,
                    176 => V176,
                    177 => V177,
                    178 => V178,
                    179 => V179,
                    180 => V180,
                    181 => V181,
                    182 => V182,
                    183 => V183,
                    184 => V184,
                    185 => V185,
                    186 => V186,
                    187 => V187,
                    188 => V188,
                    189 => V189,
                    190 => V190,
                    191 => V191,
                    192 => V192,
                    193 => V193,
                    194 => V194,
                    195 => V195,
                    196 => V196,
                    197 => V197,
                    198 => V198,
                    199 => V199,
                    200 => V200,
                    201 => V201,
                    202 => V202,
                    203 => V203,
                    204 => V204,
                    205 => V205,
                    206 => V206,
                    207 => V207,
                    208 => V208,
                    209 => V209,
                    210 => V210,
                    211 => V211,
                    212 => V212,
                    213 => V213,
                    214 => V214,
                    215 => V215,
                    216 => V216,
                    217 => V217,
                    218 => V218,
                    219 => V219,
                    220 => V220,
                    221 => V221,
                    222 => V222,
                    223 => V223,
                    224 => V224,
                    225 => V225,
                    226 => V226,
                    227 => V227,
                    228 => V228,
                    229 => V229,
                    230 => V230,
                    231 => V231,
                    232 => V232,
                    233 => V233,
                    234 => V234,
                    235 => V235,
                    236 => V236,
                    237 => V237,
                    238 => V238,
                    239 => V239,
                    240 => V240,
                    241 => V241,
                    242 => V242,
                    243 => V243,
                    244 => V244,
                    245 => V245,
                    246 => V246,
                    247 => V247,
                    248 => V248,
                    249 => V249,
                    250 => V250,
                    251 => V251,
                    252 => V252,
                    253 => V253,
                    254 => V254,
                    255 => V255,
                    _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
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
                    case 32: V32 = value; break;
                    case 33: V33 = value; break;
                    case 34: V34 = value; break;
                    case 35: V35 = value; break;
                    case 36: V36 = value; break;
                    case 37: V37 = value; break;
                    case 38: V38 = value; break;
                    case 39: V39 = value; break;
                    case 40: V40 = value; break;
                    case 41: V41 = value; break;
                    case 42: V42 = value; break;
                    case 43: V43 = value; break;
                    case 44: V44 = value; break;
                    case 45: V45 = value; break;
                    case 46: V46 = value; break;
                    case 47: V47 = value; break;
                    case 48: V48 = value; break;
                    case 49: V49 = value; break;
                    case 50: V50 = value; break;
                    case 51: V51 = value; break;
                    case 52: V52 = value; break;
                    case 53: V53 = value; break;
                    case 54: V54 = value; break;
                    case 55: V55 = value; break;
                    case 56: V56 = value; break;
                    case 57: V57 = value; break;
                    case 58: V58 = value; break;
                    case 59: V59 = value; break;
                    case 60: V60 = value; break;
                    case 61: V61 = value; break;
                    case 62: V62 = value; break;
                    case 63: V63 = value; break;
                    case 64: V64 = value; break;
                    case 65: V65 = value; break;
                    case 66: V66 = value; break;
                    case 67: V67 = value; break;
                    case 68: V68 = value; break;
                    case 69: V69 = value; break;
                    case 70: V70 = value; break;
                    case 71: V71 = value; break;
                    case 72: V72 = value; break;
                    case 73: V73 = value; break;
                    case 74: V74 = value; break;
                    case 75: V75 = value; break;
                    case 76: V76 = value; break;
                    case 77: V77 = value; break;
                    case 78: V78 = value; break;
                    case 79: V79 = value; break;
                    case 80: V80 = value; break;
                    case 81: V81 = value; break;
                    case 82: V82 = value; break;
                    case 83: V83 = value; break;
                    case 84: V84 = value; break;
                    case 85: V85 = value; break;
                    case 86: V86 = value; break;
                    case 87: V87 = value; break;
                    case 88: V88 = value; break;
                    case 89: V89 = value; break;
                    case 90: V90 = value; break;
                    case 91: V91 = value; break;
                    case 92: V92 = value; break;
                    case 93: V93 = value; break;
                    case 94: V94 = value; break;
                    case 95: V95 = value; break;
                    case 96: V96 = value; break;
                    case 97: V97 = value; break;
                    case 98: V98 = value; break;
                    case 99: V99 = value; break;
                    case 100: V100 = value; break;
                    case 101: V101 = value; break;
                    case 102: V102 = value; break;
                    case 103: V103 = value; break;
                    case 104: V104 = value; break;
                    case 105: V105 = value; break;
                    case 106: V106 = value; break;
                    case 107: V107 = value; break;
                    case 108: V108 = value; break;
                    case 109: V109 = value; break;
                    case 110: V110 = value; break;
                    case 111: V111 = value; break;
                    case 112: V112 = value; break;
                    case 113: V113 = value; break;
                    case 114: V114 = value; break;
                    case 115: V115 = value; break;
                    case 116: V116 = value; break;
                    case 117: V117 = value; break;
                    case 118: V118 = value; break;
                    case 119: V119 = value; break;
                    case 120: V120 = value; break;
                    case 121: V121 = value; break;
                    case 122: V122 = value; break;
                    case 123: V123 = value; break;
                    case 124: V124 = value; break;
                    case 125: V125 = value; break;
                    case 126: V126 = value; break;
                    case 127: V127 = value; break;
                    case 128: V128 = value; break;
                    case 129: V129 = value; break;
                    case 130: V130 = value; break;
                    case 131: V131 = value; break;
                    case 132: V132 = value; break;
                    case 133: V133 = value; break;
                    case 134: V134 = value; break;
                    case 135: V135 = value; break;
                    case 136: V136 = value; break;
                    case 137: V137 = value; break;
                    case 138: V138 = value; break;
                    case 139: V139 = value; break;
                    case 140: V140 = value; break;
                    case 141: V141 = value; break;
                    case 142: V142 = value; break;
                    case 143: V143 = value; break;
                    case 144: V144 = value; break;
                    case 145: V145 = value; break;
                    case 146: V146 = value; break;
                    case 147: V147 = value; break;
                    case 148: V148 = value; break;
                    case 149: V149 = value; break;
                    case 150: V150 = value; break;
                    case 151: V151 = value; break;
                    case 152: V152 = value; break;
                    case 153: V153 = value; break;
                    case 154: V154 = value; break;
                    case 155: V155 = value; break;
                    case 156: V156 = value; break;
                    case 157: V157 = value; break;
                    case 158: V158 = value; break;
                    case 159: V159 = value; break;
                    case 160: V160 = value; break;
                    case 161: V161 = value; break;
                    case 162: V162 = value; break;
                    case 163: V163 = value; break;
                    case 164: V164 = value; break;
                    case 165: V165 = value; break;
                    case 166: V166 = value; break;
                    case 167: V167 = value; break;
                    case 168: V168 = value; break;
                    case 169: V169 = value; break;
                    case 170: V170 = value; break;
                    case 171: V171 = value; break;
                    case 172: V172 = value; break;
                    case 173: V173 = value; break;
                    case 174: V174 = value; break;
                    case 175: V175 = value; break;
                    case 176: V176 = value; break;
                    case 177: V177 = value; break;
                    case 178: V178 = value; break;
                    case 179: V179 = value; break;
                    case 180: V180 = value; break;
                    case 181: V181 = value; break;
                    case 182: V182 = value; break;
                    case 183: V183 = value; break;
                    case 184: V184 = value; break;
                    case 185: V185 = value; break;
                    case 186: V186 = value; break;
                    case 187: V187 = value; break;
                    case 188: V188 = value; break;
                    case 189: V189 = value; break;
                    case 190: V190 = value; break;
                    case 191: V191 = value; break;
                    case 192: V192 = value; break;
                    case 193: V193 = value; break;
                    case 194: V194 = value; break;
                    case 195: V195 = value; break;
                    case 196: V196 = value; break;
                    case 197: V197 = value; break;
                    case 198: V198 = value; break;
                    case 199: V199 = value; break;
                    case 200: V200 = value; break;
                    case 201: V201 = value; break;
                    case 202: V202 = value; break;
                    case 203: V203 = value; break;
                    case 204: V204 = value; break;
                    case 205: V205 = value; break;
                    case 206: V206 = value; break;
                    case 207: V207 = value; break;
                    case 208: V208 = value; break;
                    case 209: V209 = value; break;
                    case 210: V210 = value; break;
                    case 211: V211 = value; break;
                    case 212: V212 = value; break;
                    case 213: V213 = value; break;
                    case 214: V214 = value; break;
                    case 215: V215 = value; break;
                    case 216: V216 = value; break;
                    case 217: V217 = value; break;
                    case 218: V218 = value; break;
                    case 219: V219 = value; break;
                    case 220: V220 = value; break;
                    case 221: V221 = value; break;
                    case 222: V222 = value; break;
                    case 223: V223 = value; break;
                    case 224: V224 = value; break;
                    case 225: V225 = value; break;
                    case 226: V226 = value; break;
                    case 227: V227 = value; break;
                    case 228: V228 = value; break;
                    case 229: V229 = value; break;
                    case 230: V230 = value; break;
                    case 231: V231 = value; break;
                    case 232: V232 = value; break;
                    case 233: V233 = value; break;
                    case 234: V234 = value; break;
                    case 235: V235 = value; break;
                    case 236: V236 = value; break;
                    case 237: V237 = value; break;
                    case 238: V238 = value; break;
                    case 239: V239 = value; break;
                    case 240: V240 = value; break;
                    case 241: V241 = value; break;
                    case 242: V242 = value; break;
                    case 243: V243 = value; break;
                    case 244: V244 = value; break;
                    case 245: V245 = value; break;
                    case 246: V246 = value; break;
                    case 247: V247 = value; break;
                    case 248: V248 = value; break;
                    case 249: V249 = value; break;
                    case 250: V250 = value; break;
                    case 251: V251 = value; break;
                    case 252: V252 = value; break;
                    case 253: V253 = value; break;
                    case 254: V254 = value; break;
                    case 255: V255 = value; break;
                }
            }
        }
    }
}