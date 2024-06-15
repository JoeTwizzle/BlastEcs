using BlastEcs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlastEcs;

public readonly struct TypeCollectionKey
{
    private readonly ulong[] _types;
    public readonly ReadOnlySpan<ulong> Types => _types;

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
            if (handle.Target == EcsWorld.AnyId)
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0x0000000000FFFFFF);
                return maskedItems.Contains(handle.Entity);
            }

            //Does the key contain any relationships with the given target
            if (handle.Entity == EcsWorld.AnyId)
            {
                Span<ulong> maskedItems = stackalloc ulong[_types.Length];
                Types.CopyTo(maskedItems);
                maskedItems.MaskBits(0x00FFFFFF00000000);
                return maskedItems.Contains(((ulong)handle.Target) << 32);
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
}
