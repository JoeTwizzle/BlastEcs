using BlastEcs.Utils;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs;

public readonly struct TypeCollectionKey
{
    private readonly ulong[] _types;
    public readonly ReadOnlySpan<ulong> Types => _types;
    public readonly ReadOnlySpan<EcsHandle> Handles => MemoryMarshal.Cast<ulong, EcsHandle>(_types);

    public TypeCollectionKey(ulong[] types)
    {
        Array.Sort(types);
        _types = types;
    }

    public TypeCollectionKey(TypeCollectionKeyNoAlloc key)
    {
        _types = key.Types.ToArray();
    }

    public override bool Equals(object? obj)
    {
        return obj is TypeCollectionKey key && Equals(key);
    }

    public bool Equals(TypeCollectionKey key)
    {
        return Types.SequenceEqual(key.Types);
    }

    public override int GetHashCode()
    {
        return (int)GetHash();
    }

    public ulong GetHash()
    {
        return Hasher.Hash(_types);
    }

    public static bool operator ==(TypeCollectionKey left, TypeCollectionKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeCollectionKey left, TypeCollectionKey right)
    {
        return !(left == right);
    }

    public static implicit operator TypeCollectionKeyNoAlloc(TypeCollectionKey key) => new TypeCollectionKeyNoAlloc(key._types);

    [SkipLocalsInit]
    public bool Contains(TypeCollectionKeyNoAlloc other)
    {
        if (_types.Length > other.Length)
            return false;

        var otherTypes = other.Types;
        Span<ulong> maskedItems = [.. _types];
        maskedItems.MaskBits(0xFF000000_00000000);
        for (int i = 0; i < otherTypes.Length; i++)
        {
            var handle = new EcsHandle(otherTypes[i]);
            if (!Types.Contains(otherTypes[i]))
            {
                if (!handle.IsPair)
                {
                    return false;
                }
                else if (handle.Entity == EcsWorld.AnyId &&
                    handle.Target == EcsWorld.AnyId &&
                    !maskedItems.Contains(((ulong)EntityFlags.IsPair) << 56))
                {
                    return false;
                }
                else
                {
                    Types.CopyTo(maskedItems);
                    maskedItems.MaskBits(0x00FFFFFF00000000);
                    if (handle.Target == EcsWorld.AnyId &&
                    !maskedItems.Contains(((ulong)handle.Entity) << 32))
                    {
                        return false;
                    }
                    else
                    {
                        Types.CopyTo(maskedItems);
                        maskedItems.MaskBits(0x0000000000FFFFFF);
                        if (handle.Entity == EcsWorld.AnyId &&
                         !maskedItems.Contains(handle.Target))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    public bool HasType(EcsHandle handle)
    {
        if (handle.IsPair)
        {
            //Check if handle contains an "Any" query

            //Does the key contain any pair at all?
            if (handle.Entity == EcsWorld.AnyId && handle.Target == EcsWorld.AnyId)
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0xFF000000_00000000);
                return maskedItems.Contains(((ulong)EntityFlags.IsPair) << 56);
            }

            //Does the key contain any relationships with given kind
            else if (handle.Target == EcsWorld.AnyId)
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0x00FFFFFF00000000);
                return maskedItems.Contains(((ulong)handle.Entity) << 32);
            }

            //Does the key contain any relationships with the given target
            else if (handle.Entity == EcsWorld.AnyId)
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0x0000000000FFFFFF);
                return maskedItems.Contains(handle.Target);
            }

            return Types.Contains(handle.Id);
        }
        else
        {
            //Does the key contain any items at all?
            if (handle.Entity == EcsWorld.AnyId)
            {
                return _types.Length > 0;
            }

            //Check if an exact match is present
            return Types.Contains(handle.Id);
        }
    }

    public void ForeachPair(EcsHandle match, Action<EcsHandle> eachPair)
    {
        if (!match.IsPair || match.Entity != EcsWorld.AnyId || match.Target != EcsWorld.AnyId)
        {
            return;
        }
        Span<ulong> maskedItems = stackalloc ulong[_types.Length];
        Types.CopyTo(maskedItems);
        maskedItems.MaskBits(0xFF000000_00000000);
        for (int i = 0; i < maskedItems.Length; i++)
        {
            if ((maskedItems[i] & (((ulong)EntityFlags.IsPair) << 56)) != 0)
            {
                eachPair(new EcsHandle(Types[i]));
            }
        }
    }

    public void ForeachSource(EcsHandle match, Action<uint> eachSource)
    {
        if (!match.IsPair || match.Entity != EcsWorld.AnyId)
        {
            return;
        }
        if (match.Target != EcsWorld.AnyId)
        {
            //Does the key contain any relationships with given kind
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0x00FFFFFF00000000);
                for (int i = 0; i < maskedItems.Length; i++)
                {
                    if (maskedItems[i] == (((ulong)match.Target) << 32))
                    {
                        eachSource(new EcsHandle(Types[i]).Target);
                    }
                }
            }
        }
        else
        {
            Span<ulong> maskedItems = stackalloc ulong[_types.Length];
            Types.CopyTo(maskedItems);
            maskedItems.MaskBits(0xFF000000_00000000);
            for (int i = 0; i < maskedItems.Length; i++)
            {
                if ((maskedItems[i] & (((ulong)EntityFlags.IsPair) << 56)) != 0)
                {
                    eachSource(new EcsHandle(Types[i]).Target);
                }
            }
        }
    }

    [SkipLocalsInit]
    public void ForeachTarget(EcsHandle match, Action<uint> eachTarget)
    {
        if (!match.IsPair || match.Target != EcsWorld.AnyId)
        {
            return;
        }
        if (match.Entity != EcsWorld.AnyId)
        {
            //Does the key contain any relationships with given kind
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0x00FFFFFF00000000);
                for (int i = 0; i < maskedItems.Length; i++)
                {
                    if (maskedItems[i] == ((ulong)match.Entity << 32))
                    {
                        eachTarget(new EcsHandle(Types[i]).Target);
                    }
                }
            }
        }
        else //Does the key contain any relationships
        {
            Span<ulong> maskedItems = stackalloc ulong[_types.Length];
            Types.CopyTo(maskedItems);
            maskedItems.MaskBits(0xFF000000_00000000);
            for (int i = 0; i < maskedItems.Length; i++)
            {
                if ((maskedItems[i] & (((ulong)EntityFlags.IsPair) << 56)) != 0)
                {
                    eachTarget(new EcsHandle(Types[i]).Target);
                }
            }
        }
    }


    public bool TryGetTargets(EcsHandle match, Span<uint> idsBuffer, out int count)
    {
        count = 0;
        if (!match.IsPair)
        {
            return false;
        }
        //Check if handle contains an "Any" query
        //Does the key contain any pair at all?
        if (match.Entity == EcsWorld.AnyId && match.Target == EcsWorld.AnyId)
        {
            Span<ulong> maskedItems = stackalloc ulong[_types.Length];
            Types.CopyTo(maskedItems);
            maskedItems.MaskBits(0xFF000000_00000000);

            for (int i = 0; i < maskedItems.Length; i++)
            {
                if ((maskedItems[i] & (((ulong)EntityFlags.IsPair) << 56)) != 0)
                {
                    idsBuffer[count] = new EcsHandle(Types[i]).Entity;
                    count++;
                    idsBuffer[count] = new EcsHandle(Types[i]).Target;
                    count++;
                }
            }
            return count > 0;
        }

        //Does the key contain any relationships with given kind
        if (match.Target == EcsWorld.AnyId)
        {
            Span<ulong> maskedItems = stackalloc ulong[_types.Length];
            Types.CopyTo(maskedItems);
            maskedItems.MaskBits(0x0000000000FFFFFF);
            for (int i = 0; i < maskedItems.Length; i++)
            {
                if (maskedItems[i] == (((ulong)match.Entity) << 32) && idsBuffer.Length <= count)
                {
                    idsBuffer[count] = new EcsHandle(Types[i]).Target;
                    count++;
                }
            }
            return count > 0 && idsBuffer.Length <= count;
        }

        //Does the key contain any relationships with the given target
        if (match.Entity == EcsWorld.AnyId)
        {
            Span<ulong> maskedItems = stackalloc ulong[_types.Length];
            Types.CopyTo(maskedItems);
            maskedItems.MaskBits(0x00FFFFFF00000000);
            for (int i = 0; i < maskedItems.Length; i++)
            {
                if (maskedItems[i] == (ulong)match.Target && idsBuffer.Length <= count)
                {
                    idsBuffer[count] = new EcsHandle(Types[i]).Target;
                    count++;
                }
            }
            return count > 0 && idsBuffer.Length <= count;
        }
        return false;
    }
}
