using BlastEcs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
