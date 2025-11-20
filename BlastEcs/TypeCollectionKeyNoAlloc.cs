using BlastEcs.Utils;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs;

public readonly ref struct TypeCollectionKeyNoAlloc : IEquatable<TypeCollectionKeyNoAlloc>
{
    private readonly ref readonly ulong first;
    public readonly int Length;

    public readonly ReadOnlySpan<ulong> Types => MemoryMarshal.CreateReadOnlySpan(in first, Length);

    public TypeCollectionKeyNoAlloc(Span<ulong> types)
    {
        types.Sort();
        first = ref (types.Length > 0) ? ref types[0] : ref Unsafe.NullRef<ulong>();
        Length = types.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is TypeCollectionKey key && Equals(key);
    }

    public bool Equals(TypeCollectionKeyNoAlloc key)
    {
        return Types.SequenceEqual(key.Types);
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
        return Hasher.Hash(Types);
    }

    public static bool operator ==(TypeCollectionKeyNoAlloc left, TypeCollectionKeyNoAlloc right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeCollectionKeyNoAlloc left, TypeCollectionKeyNoAlloc right)
    {
        return !(left == right);
    }

    public static bool operator ==(TypeCollectionKeyNoAlloc left, TypeCollectionKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeCollectionKeyNoAlloc left, TypeCollectionKey right)
    {
        return !(left == right);
    }

    public readonly ulong this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)Length);
            return Unsafe.Add(ref Unsafe.AsRef(in first), index);
        }
    }
}

