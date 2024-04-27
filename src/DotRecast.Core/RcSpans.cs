using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core
{
    public static class RcSpans
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(Span<T> source, Span<T> destination)
        {
            Copy(source, 0, destination, 0, source.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(Span<T> source, int sourceIdx, Span<T> destination, int destinationIdx, int length)
        {
            var src = source.Slice(sourceIdx, length);
            var dst = destination.Slice(destinationIdx);
            src.CopyTo(dst);
        }
    }
}