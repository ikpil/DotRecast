/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

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

namespace DotRecast.Detour
{
    public class Status
    {
        public static Status FAILURE = new Status(0);
        public static Status SUCCSESS = new Status(1);
        public static Status IN_PROGRESS = new Status(2);
        public static Status PARTIAL_RESULT = new Status(3);
        public static Status FAILURE_INVALID_PARAM = new Status(4);

        public int Value { get; }

        private Status(int vlaue)
        {
            Value = vlaue;
        }
    }

    public static class StatusEx
    {
        public static bool isFailed(this Status @this)
        {
            return @this == Status.FAILURE || @this == Status.FAILURE_INVALID_PARAM;
        }

        public static bool isInProgress(this Status @this)
        {
            return @this == Status.IN_PROGRESS;
        }

        public static bool isSuccess(this Status @this)
        {
            return @this == Status.SUCCSESS || @this == Status.PARTIAL_RESULT;
        }

        public static bool isPartial(this Status @this)
        {
            return @this == Status.PARTIAL_RESULT;
        }
    }
}