using System.Diagnostics.CodeAnalysis;

namespace BlastEcs;

public ref struct IndexedTypeCollectionKeyNoAlloc : IEquatable<IndexedTypeCollectionKey>, IEquatable<IndexedTypeCollectionKeyNoAlloc>
{
    public int Index;
    public TypeCollectionKeyNoAlloc TypeCollectionKeyNoAlloc;

    public IndexedTypeCollectionKeyNoAlloc(int index, TypeCollectionKeyNoAlloc typeCollectionKeyNoAlloc)
    {
        Index = index;
        TypeCollectionKeyNoAlloc = typeCollectionKeyNoAlloc;
    }

    public readonly bool Equals(IndexedTypeCollectionKey other)
    {
        return Index == other.Index && TypeCollectionKeyNoAlloc.Equals(other.TypeCollectionKey);
    }

    public readonly bool Equals(IndexedTypeCollectionKeyNoAlloc other)
    {
        return Index == other.Index && TypeCollectionKeyNoAlloc.Equals(other.TypeCollectionKeyNoAlloc);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IndexedTypeCollectionKey key1 && Equals(key1);
    }

    public override readonly int GetHashCode()
    {
        return Index ^ TypeCollectionKeyNoAlloc.GetHashCode();
    }

    public static bool operator ==(IndexedTypeCollectionKeyNoAlloc left, IndexedTypeCollectionKeyNoAlloc right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IndexedTypeCollectionKeyNoAlloc left, IndexedTypeCollectionKeyNoAlloc right)
    {
        return !(left == right);
    }
}
