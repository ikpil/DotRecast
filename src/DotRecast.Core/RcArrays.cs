using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core
{
    public static class RcArrays
    {
        // Type Safe Copy
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(T[] sourceArray, long sourceIndex, T[] destinationArray, long destinationIndex, long length)
        {
            Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(Span<T> sourceArray, int sourceIndex, Span<T> destinationArray, int destinationIndex, int length)
        {
            sourceArray.Slice(sourceIndex, length).CopyTo(destinationArray.Slice(destinationIndex));
        }


        // Type Safe Copy
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(T[] sourceArray, T[] destinationArray, long length)
        {
            Array.Copy(sourceArray, destinationArray, length);
        }

        public static T[] CopyOf<T>(T[] source, int startIdx, int length)
        {
            var deatArr = new T[length];
            for (int i = 0; i < length; ++i)
            {
                deatArr[i] = source[startIdx + i];
            }

            return deatArr;
        }

        public static T[] CopyOf<T>(T[] source, long length)
        {
            var deatArr = new T[length];
            var count = Math.Max(0, Math.Min(source.Length, length));
            for (int i = 0; i < count; ++i)
            {
                deatArr[i] = source[i];
            }

            return deatArr;
        }

        public static T[][] Of<T>(int len1, int len2)
        {
            var temp = new T[len1][];

            for (int i = 0; i < len1; ++i)
            {
                temp[i] = new T[len2];
            }

            return temp;
        }
    }
}