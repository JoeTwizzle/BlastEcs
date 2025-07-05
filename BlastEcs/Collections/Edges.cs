using BlastEcs.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace BlastEcs.Collections;

//sealed class TypeCollectionKeyComparer : IEqualityComparer<TypeCollectionKey>, IAlternateEqualityComparer<TypeCollectionKeyNoAlloc, TypeCollectionKey>
//{
//    public bool Equals(TypeCollectionKey x, TypeCollectionKey y)
//    {
//        return x.Equals(y);
//    }

//    public int GetHashCode([DisallowNull] TypeCollectionKey obj)
//    {
//        return obj.GetHashCode();
//    }

//    public TypeCollectionKey Create(TypeCollectionKeyNoAlloc alternate)
//    {
//        return new TypeCollectionKey(alternate);
//    }

//    public bool Equals(TypeCollectionKeyNoAlloc alternate, TypeCollectionKey other)
//    {
//        return alternate.Equals(other);
//    }

//    public int GetHashCode(TypeCollectionKeyNoAlloc alternate)
//    {
//        return alternate.GetHashCode();
//    }
//}
public sealed class Edges<T> where T : class
{
    public struct Edge
    {
        public T? Add;
        public T? Remove;
    }

    private readonly Dictionary<IndexedTypeCollectionKey, Edge> _edgeMap;
    public Edges()
    {
        _edgeMap = new(new IndexedTypeCollectionKeyComparer());
    }

    internal void AddEdgeAdd(int id, TypeCollectionKey key, T item)
    {
        ref var edge = ref CollectionsMarshal.GetValueRefOrAddDefault(_edgeMap, new IndexedTypeCollectionKey(id, key), out var exists);
        if (exists)
        {
            if (edge.Add != null)
            {
                ThrowHelper.ThrowArgumentException("Argument is already present");
            }
        }
        edge.Add = item;
    }

    internal void AddEdgeRemove(int id, TypeCollectionKey key, T item)
    {
        ref var edge = ref CollectionsMarshal.GetValueRefOrAddDefault(_edgeMap, new IndexedTypeCollectionKey(id, key), out var exists);
        if (exists)
        {
            if (edge.Remove != null)
            {
                ThrowHelper.ThrowArgumentException("Argument is already present");
            }
        }
        edge.Remove = item;
    }

    internal void RemoveEdgeAdd(int id, TypeCollectionKeyNoAlloc key)
    {
        var altKey = new IndexedTypeCollectionKeyNoAlloc(id, key);
        var alternate = _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>();
        if (alternate.ContainsKey(altKey))
        {
            var val = alternate[altKey];
            val.Add = null!;
            alternate[altKey] = val;
        }
    }

    internal void RemoveEdgeRemove(int id, TypeCollectionKeyNoAlloc key)
    {
        var altKey = new IndexedTypeCollectionKeyNoAlloc(id, key);
        var alternate = _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>();
        if (alternate.ContainsKey(altKey))
        {
            var val = alternate[altKey];
            val.Remove = null!;
            alternate[altKey] = val;
        }
    }

    public bool TryGetEdgeAdd(int id, TypeCollectionKeyNoAlloc key, [NotNullWhen(true)] out T? item)
    {
        var alternate = _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>();
        if (alternate.TryGetValue(new IndexedTypeCollectionKeyNoAlloc(id, key), out var edge))
        {
            item = edge.Add;
            return item != null;
        }
        item = null;
        return false;
    }

    public bool TryGetEdgeRemove(int id, TypeCollectionKeyNoAlloc key, [NotNullWhen(true)] out T? item)
    {
        var alternate = _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>();
        if (alternate.TryGetValue(new IndexedTypeCollectionKeyNoAlloc(id, key), out var edge))
        {
            item = edge.Remove;
            return item != null;
        }
        item = null;
        return false;
    }

    public T GetEdgeAdd(int id, TypeCollectionKeyNoAlloc key)
    {
        return _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>()[new IndexedTypeCollectionKeyNoAlloc(id, key)].Add!;
    }

    public T GetEdgeRemove(int id, TypeCollectionKeyNoAlloc key)
    {
        return _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>()[new IndexedTypeCollectionKeyNoAlloc(id, key)].Remove!;
    }

    public Edge this[IndexedTypeCollectionKeyNoAlloc key]
    {
        get
        {
            return _edgeMap.GetAlternateLookup<IndexedTypeCollectionKeyNoAlloc>()[key];
        }
    }

    public Dictionary<IndexedTypeCollectionKey, Edge>.Enumerator GetEnumerator()
    {
        return _edgeMap.GetEnumerator();
    }
}
