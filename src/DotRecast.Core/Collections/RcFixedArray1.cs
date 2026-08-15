using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0169

namespace DotRecast.Core.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RcFixedArray1<T> where T : unmanaged
    {
        public const int Size = 1;

        private T _v0000;

        public readonly int Length => Size;

        public readonly ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref AsSpanUnsafe()[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly Span<T> AsSpanUnsafe()
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in _v0000), Size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<T> AsReadOnlySpan()
        {
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in _v0000), Size);
        }
    }
}