using System;
using System.Runtime.CompilerServices;

namespace DotRecast.Core.Collections
{
    // Mutable access to RcFixedArrayN goes through these `this ref` extensions:
    // calling them on an `in` parameter or readonly field is a compile error,
    // which prevents the silent defensive-copy trap. Read-only access uses the
    // readonly members (indexer, AsReadOnlySpan) on the struct itself.
    public static class RcFixedArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray1<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray2<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray4<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray8<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray16<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray32<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray64<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray128<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray256<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray512<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this ref RcFixedArray1024<T> array) where T : unmanaged
        {
            return array.AsSpanUnsafe();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray1<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray2<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray4<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray8<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray16<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray32<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray64<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray128<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray256<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray512<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom<T>(this ref RcFixedArray1024<T> array, ReadOnlySpan<T> source, int length) where T : unmanaged
        {
            source.Slice(0, length).CopyTo(array.AsSpanUnsafe());
        }
    }
}
