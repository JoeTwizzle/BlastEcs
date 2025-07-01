using System.Runtime.CompilerServices;

namespace BlastEcs.Collections;

public sealed class FastMap<T>
{
    public readonly ulong FastRange;
    private readonly T[] FastValues;
    private readonly LongKeyMap<T> SlowRange;

    public FastMap(ulong fastRange = 2048)
    {
        FastRange = fastRange;
        FastValues = new T[fastRange];
        SlowRange = new LongKeyMap<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetOrCreateRefAt(ulong index)
    {
        if (index < FastRange)
        {
            return ref FastValues[index];
        }
        else
        {
            return ref SlowRange.GetValueRefOrAddDefault(index, out _);
        }
    }

    public ref T GetRefAt(ulong index)
    {
        if (index < FastRange)
        {
            return ref FastValues[index];
        }
        else
        {
            return ref SlowRange[index];
        }
    }

    public ref T TryGetRefAt(ulong index)
    {
        if (index < FastRange)
        {
            return ref FastValues[index];
        }
        else
        {
            return ref TryGetRefAt(index);
        }
    }
}
