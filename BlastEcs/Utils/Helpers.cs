using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs.Utils;

internal static class Helpers
{
    public static ref TValue GetRefOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out bool exists) where TKey : notnull
    {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out exists)!;
    }

    public static ref TValue GetRefOrNullRef<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(dict, key);
    }

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
