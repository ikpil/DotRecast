using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    public struct RcStackArray512<T>
    {
        public static RcStackArray512<T> Empty => new RcStackArray512<T>();

        private const int Size = 512;
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
        public T V256;
        public T V257;
        public T V258;
        public T V259;
        public T V260;
        public T V261;
        public T V262;
        public T V263;
        public T V264;
        public T V265;
        public T V266;
        public T V267;
        public T V268;
        public T V269;
        public T V270;
        public T V271;
        public T V272;
        public T V273;
        public T V274;
        public T V275;
        public T V276;
        public T V277;
        public T V278;
        public T V279;
        public T V280;
        public T V281;
        public T V282;
        public T V283;
        public T V284;
        public T V285;
        public T V286;
        public T V287;
        public T V288;
        public T V289;
        public T V290;
        public T V291;
        public T V292;
        public T V293;
        public T V294;
        public T V295;
        public T V296;
        public T V297;
        public T V298;
        public T V299;
        public T V300;
        public T V301;
        public T V302;
        public T V303;
        public T V304;
        public T V305;
        public T V306;
        public T V307;
        public T V308;
        public T V309;
        public T V310;
        public T V311;
        public T V312;
        public T V313;
        public T V314;
        public T V315;
        public T V316;
        public T V317;
        public T V318;
        public T V319;
        public T V320;
        public T V321;
        public T V322;
        public T V323;
        public T V324;
        public T V325;
        public T V326;
        public T V327;
        public T V328;
        public T V329;
        public T V330;
        public T V331;
        public T V332;
        public T V333;
        public T V334;
        public T V335;
        public T V336;
        public T V337;
        public T V338;
        public T V339;
        public T V340;
        public T V341;
        public T V342;
        public T V343;
        public T V344;
        public T V345;
        public T V346;
        public T V347;
        public T V348;
        public T V349;
        public T V350;
        public T V351;
        public T V352;
        public T V353;
        public T V354;
        public T V355;
        public T V356;
        public T V357;
        public T V358;
        public T V359;
        public T V360;
        public T V361;
        public T V362;
        public T V363;
        public T V364;
        public T V365;
        public T V366;
        public T V367;
        public T V368;
        public T V369;
        public T V370;
        public T V371;
        public T V372;
        public T V373;
        public T V374;
        public T V375;
        public T V376;
        public T V377;
        public T V378;
        public T V379;
        public T V380;
        public T V381;
        public T V382;
        public T V383;
        public T V384;
        public T V385;
        public T V386;
        public T V387;
        public T V388;
        public T V389;
        public T V390;
        public T V391;
        public T V392;
        public T V393;
        public T V394;
        public T V395;
        public T V396;
        public T V397;
        public T V398;
        public T V399;
        public T V400;
        public T V401;
        public T V402;
        public T V403;
        public T V404;
        public T V405;
        public T V406;
        public T V407;
        public T V408;
        public T V409;
        public T V410;
        public T V411;
        public T V412;
        public T V413;
        public T V414;
        public T V415;
        public T V416;
        public T V417;
        public T V418;
        public T V419;
        public T V420;
        public T V421;
        public T V422;
        public T V423;
        public T V424;
        public T V425;
        public T V426;
        public T V427;
        public T V428;
        public T V429;
        public T V430;
        public T V431;
        public T V432;
        public T V433;
        public T V434;
        public T V435;
        public T V436;
        public T V437;
        public T V438;
        public T V439;
        public T V440;
        public T V441;
        public T V442;
        public T V443;
        public T V444;
        public T V445;
        public T V446;
        public T V447;
        public T V448;
        public T V449;
        public T V450;
        public T V451;
        public T V452;
        public T V453;
        public T V454;
        public T V455;
        public T V456;
        public T V457;
        public T V458;
        public T V459;
        public T V460;
        public T V461;
        public T V462;
        public T V463;
        public T V464;
        public T V465;
        public T V466;
        public T V467;
        public T V468;
        public T V469;
        public T V470;
        public T V471;
        public T V472;
        public T V473;
        public T V474;
        public T V475;
        public T V476;
        public T V477;
        public T V478;
        public T V479;
        public T V480;
        public T V481;
        public T V482;
        public T V483;
        public T V484;
        public T V485;
        public T V486;
        public T V487;
        public T V488;
        public T V489;
        public T V490;
        public T V491;
        public T V492;
        public T V493;
        public T V494;
        public T V495;
        public T V496;
        public T V497;
        public T V498;
        public T V499;
        public T V500;
        public T V501;
        public T V502;
        public T V503;
        public T V504;
        public T V505;
        public T V506;
        public T V507;
        public T V508;
        public T V509;
        public T V510;
        public T V511;

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
                    256 => V256,
                    257 => V257,
                    258 => V258,
                    259 => V259,
                    260 => V260,
                    261 => V261,
                    262 => V262,
                    263 => V263,
                    264 => V264,
                    265 => V265,
                    266 => V266,
                    267 => V267,
                    268 => V268,
                    269 => V269,
                    270 => V270,
                    271 => V271,
                    272 => V272,
                    273 => V273,
                    274 => V274,
                    275 => V275,
                    276 => V276,
                    277 => V277,
                    278 => V278,
                    279 => V279,
                    280 => V280,
                    281 => V281,
                    282 => V282,
                    283 => V283,
                    284 => V284,
                    285 => V285,
                    286 => V286,
                    287 => V287,
                    288 => V288,
                    289 => V289,
                    290 => V290,
                    291 => V291,
                    292 => V292,
                    293 => V293,
                    294 => V294,
                    295 => V295,
                    296 => V296,
                    297 => V297,
                    298 => V298,
                    299 => V299,
                    300 => V300,
                    301 => V301,
                    302 => V302,
                    303 => V303,
                    304 => V304,
                    305 => V305,
                    306 => V306,
                    307 => V307,
                    308 => V308,
                    309 => V309,
                    310 => V310,
                    311 => V311,
                    312 => V312,
                    313 => V313,
                    314 => V314,
                    315 => V315,
                    316 => V316,
                    317 => V317,
                    318 => V318,
                    319 => V319,
                    320 => V320,
                    321 => V321,
                    322 => V322,
                    323 => V323,
                    324 => V324,
                    325 => V325,
                    326 => V326,
                    327 => V327,
                    328 => V328,
                    329 => V329,
                    330 => V330,
                    331 => V331,
                    332 => V332,
                    333 => V333,
                    334 => V334,
                    335 => V335,
                    336 => V336,
                    337 => V337,
                    338 => V338,
                    339 => V339,
                    340 => V340,
                    341 => V341,
                    342 => V342,
                    343 => V343,
                    344 => V344,
                    345 => V345,
                    346 => V346,
                    347 => V347,
                    348 => V348,
                    349 => V349,
                    350 => V350,
                    351 => V351,
                    352 => V352,
                    353 => V353,
                    354 => V354,
                    355 => V355,
                    356 => V356,
                    357 => V357,
                    358 => V358,
                    359 => V359,
                    360 => V360,
                    361 => V361,
                    362 => V362,
                    363 => V363,
                    364 => V364,
                    365 => V365,
                    366 => V366,
                    367 => V367,
                    368 => V368,
                    369 => V369,
                    370 => V370,
                    371 => V371,
                    372 => V372,
                    373 => V373,
                    374 => V374,
                    375 => V375,
                    376 => V376,
                    377 => V377,
                    378 => V378,
                    379 => V379,
                    380 => V380,
                    381 => V381,
                    382 => V382,
                    383 => V383,
                    384 => V384,
                    385 => V385,
                    386 => V386,
                    387 => V387,
                    388 => V388,
                    389 => V389,
                    390 => V390,
                    391 => V391,
                    392 => V392,
                    393 => V393,
                    394 => V394,
                    395 => V395,
                    396 => V396,
                    397 => V397,
                    398 => V398,
                    399 => V399,
                    400 => V400,
                    401 => V401,
                    402 => V402,
                    403 => V403,
                    404 => V404,
                    405 => V405,
                    406 => V406,
                    407 => V407,
                    408 => V408,
                    409 => V409,
                    410 => V410,
                    411 => V411,
                    412 => V412,
                    413 => V413,
                    414 => V414,
                    415 => V415,
                    416 => V416,
                    417 => V417,
                    418 => V418,
                    419 => V419,
                    420 => V420,
                    421 => V421,
                    422 => V422,
                    423 => V423,
                    424 => V424,
                    425 => V425,
                    426 => V426,
                    427 => V427,
                    428 => V428,
                    429 => V429,
                    430 => V430,
                    431 => V431,
                    432 => V432,
                    433 => V433,
                    434 => V434,
                    435 => V435,
                    436 => V436,
                    437 => V437,
                    438 => V438,
                    439 => V439,
                    440 => V440,
                    441 => V441,
                    442 => V442,
                    443 => V443,
                    444 => V444,
                    445 => V445,
                    446 => V446,
                    447 => V447,
                    448 => V448,
                    449 => V449,
                    450 => V450,
                    451 => V451,
                    452 => V452,
                    453 => V453,
                    454 => V454,
                    455 => V455,
                    456 => V456,
                    457 => V457,
                    458 => V458,
                    459 => V459,
                    460 => V460,
                    461 => V461,
                    462 => V462,
                    463 => V463,
                    464 => V464,
                    465 => V465,
                    466 => V466,
                    467 => V467,
                    468 => V468,
                    469 => V469,
                    470 => V470,
                    471 => V471,
                    472 => V472,
                    473 => V473,
                    474 => V474,
                    475 => V475,
                    476 => V476,
                    477 => V477,
                    478 => V478,
                    479 => V479,
                    480 => V480,
                    481 => V481,
                    482 => V482,
                    483 => V483,
                    484 => V484,
                    485 => V485,
                    486 => V486,
                    487 => V487,
                    488 => V488,
                    489 => V489,
                    490 => V490,
                    491 => V491,
                    492 => V492,
                    493 => V493,
                    494 => V494,
                    495 => V495,
                    496 => V496,
                    497 => V497,
                    498 => V498,
                    499 => V499,
                    500 => V500,
                    501 => V501,
                    502 => V502,
                    503 => V503,
                    504 => V504,
                    505 => V505,
                    506 => V506,
                    507 => V507,
                    508 => V508,
                    509 => V509,
                    510 => V510,
                    511 => V511,

                    _ => throw new ArgumentOutOfRangeException(nameof(index), index, null)
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
                    case 256: V256 = value; break;
                    case 257: V257 = value; break;
                    case 258: V258 = value; break;
                    case 259: V259 = value; break;
                    case 260: V260 = value; break;
                    case 261: V261 = value; break;
                    case 262: V262 = value; break;
                    case 263: V263 = value; break;
                    case 264: V264 = value; break;
                    case 265: V265 = value; break;
                    case 266: V266 = value; break;
                    case 267: V267 = value; break;
                    case 268: V268 = value; break;
                    case 269: V269 = value; break;
                    case 270: V270 = value; break;
                    case 271: V271 = value; break;
                    case 272: V272 = value; break;
                    case 273: V273 = value; break;
                    case 274: V274 = value; break;
                    case 275: V275 = value; break;
                    case 276: V276 = value; break;
                    case 277: V277 = value; break;
                    case 278: V278 = value; break;
                    case 279: V279 = value; break;
                    case 280: V280 = value; break;
                    case 281: V281 = value; break;
                    case 282: V282 = value; break;
                    case 283: V283 = value; break;
                    case 284: V284 = value; break;
                    case 285: V285 = value; break;
                    case 286: V286 = value; break;
                    case 287: V287 = value; break;
                    case 288: V288 = value; break;
                    case 289: V289 = value; break;
                    case 290: V290 = value; break;
                    case 291: V291 = value; break;
                    case 292: V292 = value; break;
                    case 293: V293 = value; break;
                    case 294: V294 = value; break;
                    case 295: V295 = value; break;
                    case 296: V296 = value; break;
                    case 297: V297 = value; break;
                    case 298: V298 = value; break;
                    case 299: V299 = value; break;
                    case 300: V300 = value; break;
                    case 301: V301 = value; break;
                    case 302: V302 = value; break;
                    case 303: V303 = value; break;
                    case 304: V304 = value; break;
                    case 305: V305 = value; break;
                    case 306: V306 = value; break;
                    case 307: V307 = value; break;
                    case 308: V308 = value; break;
                    case 309: V309 = value; break;
                    case 310: V310 = value; break;
                    case 311: V311 = value; break;
                    case 312: V312 = value; break;
                    case 313: V313 = value; break;
                    case 314: V314 = value; break;
                    case 315: V315 = value; break;
                    case 316: V316 = value; break;
                    case 317: V317 = value; break;
                    case 318: V318 = value; break;
                    case 319: V319 = value; break;
                    case 320: V320 = value; break;
                    case 321: V321 = value; break;
                    case 322: V322 = value; break;
                    case 323: V323 = value; break;
                    case 324: V324 = value; break;
                    case 325: V325 = value; break;
                    case 326: V326 = value; break;
                    case 327: V327 = value; break;
                    case 328: V328 = value; break;
                    case 329: V329 = value; break;
                    case 330: V330 = value; break;
                    case 331: V331 = value; break;
                    case 332: V332 = value; break;
                    case 333: V333 = value; break;
                    case 334: V334 = value; break;
                    case 335: V335 = value; break;
                    case 336: V336 = value; break;
                    case 337: V337 = value; break;
                    case 338: V338 = value; break;
                    case 339: V339 = value; break;
                    case 340: V340 = value; break;
                    case 341: V341 = value; break;
                    case 342: V342 = value; break;
                    case 343: V343 = value; break;
                    case 344: V344 = value; break;
                    case 345: V345 = value; break;
                    case 346: V346 = value; break;
                    case 347: V347 = value; break;
                    case 348: V348 = value; break;
                    case 349: V349 = value; break;
                    case 350: V350 = value; break;
                    case 351: V351 = value; break;
                    case 352: V352 = value; break;
                    case 353: V353 = value; break;
                    case 354: V354 = value; break;
                    case 355: V355 = value; break;
                    case 356: V356 = value; break;
                    case 357: V357 = value; break;
                    case 358: V358 = value; break;
                    case 359: V359 = value; break;
                    case 360: V360 = value; break;
                    case 361: V361 = value; break;
                    case 362: V362 = value; break;
                    case 363: V363 = value; break;
                    case 364: V364 = value; break;
                    case 365: V365 = value; break;
                    case 366: V366 = value; break;
                    case 367: V367 = value; break;
                    case 368: V368 = value; break;
                    case 369: V369 = value; break;
                    case 370: V370 = value; break;
                    case 371: V371 = value; break;
                    case 372: V372 = value; break;
                    case 373: V373 = value; break;
                    case 374: V374 = value; break;
                    case 375: V375 = value; break;
                    case 376: V376 = value; break;
                    case 377: V377 = value; break;
                    case 378: V378 = value; break;
                    case 379: V379 = value; break;
                    case 380: V380 = value; break;
                    case 381: V381 = value; break;
                    case 382: V382 = value; break;
                    case 383: V383 = value; break;
                    case 384: V384 = value; break;
                    case 385: V385 = value; break;
                    case 386: V386 = value; break;
                    case 387: V387 = value; break;
                    case 388: V388 = value; break;
                    case 389: V389 = value; break;
                    case 390: V390 = value; break;
                    case 391: V391 = value; break;
                    case 392: V392 = value; break;
                    case 393: V393 = value; break;
                    case 394: V394 = value; break;
                    case 395: V395 = value; break;
                    case 396: V396 = value; break;
                    case 397: V397 = value; break;
                    case 398: V398 = value; break;
                    case 399: V399 = value; break;
                    case 400: V400 = value; break;
                    case 401: V401 = value; break;
                    case 402: V402 = value; break;
                    case 403: V403 = value; break;
                    case 404: V404 = value; break;
                    case 405: V405 = value; break;
                    case 406: V406 = value; break;
                    case 407: V407 = value; break;
                    case 408: V408 = value; break;
                    case 409: V409 = value; break;
                    case 410: V410 = value; break;
                    case 411: V411 = value; break;
                    case 412: V412 = value; break;
                    case 413: V413 = value; break;
                    case 414: V414 = value; break;
                    case 415: V415 = value; break;
                    case 416: V416 = value; break;
                    case 417: V417 = value; break;
                    case 418: V418 = value; break;
                    case 419: V419 = value; break;
                    case 420: V420 = value; break;
                    case 421: V421 = value; break;
                    case 422: V422 = value; break;
                    case 423: V423 = value; break;
                    case 424: V424 = value; break;
                    case 425: V425 = value; break;
                    case 426: V426 = value; break;
                    case 427: V427 = value; break;
                    case 428: V428 = value; break;
                    case 429: V429 = value; break;
                    case 430: V430 = value; break;
                    case 431: V431 = value; break;
                    case 432: V432 = value; break;
                    case 433: V433 = value; break;
                    case 434: V434 = value; break;
                    case 435: V435 = value; break;
                    case 436: V436 = value; break;
                    case 437: V437 = value; break;
                    case 438: V438 = value; break;
                    case 439: V439 = value; break;
                    case 440: V440 = value; break;
                    case 441: V441 = value; break;
                    case 442: V442 = value; break;
                    case 443: V443 = value; break;
                    case 444: V444 = value; break;
                    case 445: V445 = value; break;
                    case 446: V446 = value; break;
                    case 447: V447 = value; break;
                    case 448: V448 = value; break;
                    case 449: V449 = value; break;
                    case 450: V450 = value; break;
                    case 451: V451 = value; break;
                    case 452: V452 = value; break;
                    case 453: V453 = value; break;
                    case 454: V454 = value; break;
                    case 455: V455 = value; break;
                    case 456: V456 = value; break;
                    case 457: V457 = value; break;
                    case 458: V458 = value; break;
                    case 459: V459 = value; break;
                    case 460: V460 = value; break;
                    case 461: V461 = value; break;
                    case 462: V462 = value; break;
                    case 463: V463 = value; break;
                    case 464: V464 = value; break;
                    case 465: V465 = value; break;
                    case 466: V466 = value; break;
                    case 467: V467 = value; break;
                    case 468: V468 = value; break;
                    case 469: V469 = value; break;
                    case 470: V470 = value; break;
                    case 471: V471 = value; break;
                    case 472: V472 = value; break;
                    case 473: V473 = value; break;
                    case 474: V474 = value; break;
                    case 475: V475 = value; break;
                    case 476: V476 = value; break;
                    case 477: V477 = value; break;
                    case 478: V478 = value; break;
                    case 479: V479 = value; break;
                    case 480: V480 = value; break;
                    case 481: V481 = value; break;
                    case 482: V482 = value; break;
                    case 483: V483 = value; break;
                    case 484: V484 = value; break;
                    case 485: V485 = value; break;
                    case 486: V486 = value; break;
                    case 487: V487 = value; break;
                    case 488: V488 = value; break;
                    case 489: V489 = value; break;
                    case 490: V490 = value; break;
                    case 491: V491 = value; break;
                    case 492: V492 = value; break;
                    case 493: V493 = value; break;
                    case 494: V494 = value; break;
                    case 495: V495 = value; break;
                    case 496: V496 = value; break;
                    case 497: V497 = value; break;
                    case 498: V498 = value; break;
                    case 499: V499 = value; break;
                    case 500: V500 = value; break;
                    case 501: V501 = value; break;
                    case 502: V502 = value; break;
                    case 503: V503 = value; break;
                    case 504: V504 = value; break;
                    case 505: V505 = value; break;
                    case 506: V506 = value; break;
                    case 507: V507 = value; break;
                    case 508: V508 = value; break;
                    case 509: V509 = value; break;
                    case 510: V510 = value; break;
                    case 511: V511 = value; break;
                    
                }
            }
        }
    }
}