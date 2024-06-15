using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs.Utils;

internal static class Helpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TTo> Cast<TFrom, TTo>(this Span<TFrom> span)
    where TFrom : unmanaged
    where TTo : unmanaged
    {
        return MemoryMarshal.Cast<TFrom, TTo>(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(this ReadOnlySpan<TFrom> span)
    where TFrom : unmanaged
    where TTo : unmanaged
    {
        return MemoryMarshal.Cast<TFrom, TTo>(span);
    }

}
