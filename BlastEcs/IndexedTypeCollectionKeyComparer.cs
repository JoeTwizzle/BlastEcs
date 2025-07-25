using System.Diagnostics.CodeAnalysis;

namespace BlastEcs;

sealed class IndexedTypeCollectionKeyComparer : IEqualityComparer<IndexedTypeCollectionKey>, IAlternateEqualityComparer<IndexedTypeCollectionKeyNoAlloc, IndexedTypeCollectionKey>
{
    public bool Equals(IndexedTypeCollectionKey x, IndexedTypeCollectionKey y)
    {
        return x.Equals(y);
    }

    public int GetHashCode([DisallowNull] IndexedTypeCollectionKey obj)
    {
        return obj.GetHashCode();
    }

    public IndexedTypeCollectionKey Create(IndexedTypeCollectionKeyNoAlloc alternate)
    {
        return new IndexedTypeCollectionKey(alternate);
    }

    public bool Equals(IndexedTypeCollectionKeyNoAlloc alternate, IndexedTypeCollectionKey other)
    {
        return alternate.Equals(other);
    }

    public int GetHashCode(IndexedTypeCollectionKeyNoAlloc alternate)
    {
        return alternate.GetHashCode();
    }
}
