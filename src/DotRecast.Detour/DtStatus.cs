/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023-2024 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System.Runtime.CompilerServices;

namespace DotRecast.Detour
{
    public enum DtStatus : long
    {
        // High level status.
        DT_FAILURE = (1u << 31), // Operation failed. 
        DT_SUCCESS = (1u << 30), // Operation succeed. 
        DT_IN_PROGRESS = (1u << 29), // Operation still in progress. 

        // Detail information for status.
        DT_STATUS_DETAIL_MASK = (0x0ffffff),
        DT_STATUS_NOTHING = (0), // nothing
        DT_WRONG_MAGIC = (1 << 0), // Input data is not recognized.
        DT_WRONG_VERSION = (1 << 1), // Input data is in wrong version.
        DT_OUT_OF_MEMORY = (1 << 2), // Operation ran out of memory.
        DT_INVALID_PARAM = (1 << 3), // An input parameter was invalid.
        DT_BUFFER_TOO_SMALL = (1 << 4), // Result buffer for the query was too small to store all results.
        DT_OUT_OF_NODES = (1 << 5), // Query ran out of nodes during search.
        DT_PARTIAL_RESULT = (1 << 6), // Query did not reach the end location, returning best guess. 
        DT_ALREADY_OCCUPIED = (1 << 7), // A tile has already been assigned to the given x,y coordinate
    }

    public static class DtStatusEx
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this DtStatus Value)
        {
            return 0 == Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Succeeded(this DtStatus Value)
        {
            return 0 != (Value & (DtStatus.DT_SUCCESS | DtStatus.DT_PARTIAL_RESULT));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Failed(this DtStatus Value)
        {
            return 0 != (Value & (DtStatus.DT_FAILURE | DtStatus.DT_INVALID_PARAM));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InProgress(this DtStatus Value)
        {
            return 0 != (Value & DtStatus.DT_IN_PROGRESS);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPartial(this DtStatus Value)
        {
            return 0 != (Value & DtStatus.DT_PARTIAL_RESULT);
        }
    }

    //public readonly struct DtStatus
    //{
    //    // High level status.
    //    public static readonly DtStatus DT_FAILURE = new DtStatus(1u << 31); // Operation failed. 
    //    public static readonly DtStatus DT_SUCCESS = new DtStatus(1u << 30); // Operation succeed. 
    //    public static readonly DtStatus DT_IN_PROGRESS = new DtStatus(1u << 29); // Operation still in progress. 

    //    // Detail information for status.
    //    public static readonly DtStatus DT_STATUS_DETAIL_MASK = new DtStatus(0x0ffffff);
    //    public static readonly DtStatus DT_STATUS_NOTHING = new DtStatus(0); // nothing
    //    public static readonly DtStatus DT_WRONG_MAGIC = new DtStatus(1 << 0); // Input data is not recognized.
    //    public static readonly DtStatus DT_WRONG_VERSION = new DtStatus(1 << 1); // Input data is in wrong version.
    //    public static readonly DtStatus DT_OUT_OF_MEMORY = new DtStatus(1 << 2); // Operation ran out of memory.
    //    public static readonly DtStatus DT_INVALID_PARAM = new DtStatus(1 << 3); // An input parameter was invalid.
    //    public static readonly DtStatus DT_BUFFER_TOO_SMALL = new DtStatus(1 << 4); // Result buffer for the query was too small to store all results.
    //    public static readonly DtStatus DT_OUT_OF_NODES = new DtStatus(1 << 5); // Query ran out of nodes during search.
    //    public static readonly DtStatus DT_PARTIAL_RESULT = new DtStatus(1 << 6); // Query did not reach the end location, returning best guess. 
    //    public static readonly DtStatus DT_ALREADY_OCCUPIED = new DtStatus(1 << 7); // A tile has already been assigned to the given x,y coordinate

    //    public readonly uint Value;

    //    private DtStatus(uint value)
    //    {
    //        Value = value;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool IsEmpty()
    //    {
    //        return 0 == Value;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool Succeeded()
    //    {
    //        return 0 != (Value & (DT_SUCCESS.Value | DT_PARTIAL_RESULT.Value));
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool Failed()
    //    {
    //        return 0 != (Value & (DT_FAILURE.Value | DT_INVALID_PARAM.Value));
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool InProgress()
    //    {
    //        return 0 != (Value & DT_IN_PROGRESS.Value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public bool IsPartial()
    //    {
    //        return 0 != (Value & DT_PARTIAL_RESULT.Value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public static DtStatus operator |(DtStatus left, DtStatus right)
    //    {
    //        return new DtStatus(left.Value | right.Value);
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public static DtStatus operator &(DtStatus left, DtStatus right)
    //    {
    //        return new DtStatus(left.Value & right.Value);
    //    }

    //    public override string ToString()
    //    {
    //        return $"status {Value}";
    //    }
    //}
}