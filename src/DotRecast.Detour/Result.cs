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
    public static class Results
    {
        public static Result<T> success<T>(T result)
        {
            return new Result<T>(result, Status.SUCCSESS, null);
        }

        public static Result<T> failure<T>()
        {
            return new Result<T>(default, Status.FAILURE, null);
        }

        public static Result<T> invalidParam<T>()
        {
            return new Result<T>(default, Status.FAILURE_INVALID_PARAM, null);
        }

        public static Result<T> failure<T>(string message)
        {
            return new Result<T>(default, Status.FAILURE, message);
        }

        public static Result<T> invalidParam<T>(string message)
        {
            return new Result<T>(default, Status.FAILURE_INVALID_PARAM, message);
        }

        public static Result<T> failure<T>(T result)
        {
            return new Result<T>(result, Status.FAILURE, null);
        }

        public static Result<T> partial<T>(T result)
        {
            return new Result<T>(default, Status.PARTIAL_RESULT, null);
        }

        public static Result<T> of<T>(Status status, string message)
        {
            return new Result<T>(default, status, message);
        }

        public static Result<T> of<T>(Status status, T result)
        {
            return new Result<T>(result, status, null);
        }
    }

    public readonly struct Result<T>
    {
        public readonly T result;
        public readonly Status status;
        public readonly string message;

        internal Result(T result, Status status, string message)
        {
            this.result = result;
            this.status = status;
            this.message = message;
        }


        public bool failed()
        {
            return status.isFailed();
        }

        public bool succeeded()
        {
            return status.isSuccess();
        }
    }
}