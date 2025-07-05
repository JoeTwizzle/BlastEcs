using System.Diagnostics.CodeAnalysis;

namespace BlastEcs;

public struct IndexedTypeCollectionKey : IEquatable<IndexedTypeCollectionKey>, IEquatable<IndexedTypeCollectionKeyNoAlloc>
{
    public int Index;
    public TypeCollectionKey TypeCollectionKey;
    public IndexedTypeCollectionKey(IndexedTypeCollectionKeyNoAlloc key)
    {
        Index = key.Index;
        TypeCollectionKey = new TypeCollectionKey(key.TypeCollectionKeyNoAlloc);
    }

    public IndexedTypeCollectionKey(int index, TypeCollectionKey typeCollectionKey)
    {
        Index = index;
        TypeCollectionKey = typeCollectionKey;
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IndexedTypeCollectionKey key1 && Equals(key1);
    }

    public readonly bool Equals(IndexedTypeCollectionKeyNoAlloc other)
    {
        return other.Index == Index && other.TypeCollectionKeyNoAlloc.Equals(TypeCollectionKey);
    }

    public readonly bool Equals(IndexedTypeCollectionKey other)
    {
        return other.Index == Index && other.TypeCollectionKey.Equals(TypeCollectionKey);
    }

    public override readonly int GetHashCode()
    {
        return Index ^ TypeCollectionKey.GetHashCode();
    }
    public static bool operator ==(IndexedTypeCollectionKey left, IndexedTypeCollectionKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(IndexedTypeCollectionKey left, IndexedTypeCollectionKey right)
    {
        return !(left == right);
    }
}
