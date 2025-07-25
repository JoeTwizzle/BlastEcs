using BlastEcs.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace BlastEcs.Collections;

public sealed class Edges<T> where T : class
{
    public struct Edge
    {
        public T? Add;
        public T? Remove;
    }

    private readonly TypeCollectionMap<Edge> _edgeMap;
    public Edges()
    {
        _edgeMap = new TypeCollectionMap<Edge>();
    }

    internal void AddEdgeAdd(TypeCollectionKey key, T item)
    {
        ref var edge = ref _edgeMap.GetValueRefOrAddDefault(key, out var exists);
        if (exists)
        {
            if (edge.Add != null)
            {
                ThrowHelper.ThrowArgumentException("Argument is already present");
            }
        }
        edge.Add = item;
    }

    internal void AddEdgeRemove(TypeCollectionKey key, T item)
    {
        ref var edge = ref _edgeMap.GetValueRefOrAddDefault(key, out var exists);
        if (exists)
        {
            if (edge.Remove != null)
            {
                ThrowHelper.ThrowArgumentException("Argument is already present");
            }
        }
        edge.Remove = item;
    }

    internal void RemoveEdgeAdd(TypeCollectionKeyNoAlloc key)
    {
        ref var edge = ref _edgeMap.GetValueRefOrNullRef(key, out var exists);
        if (exists)
        {
            edge.Add = default;
        }
    }

    internal void RemoveEdgeRemove(TypeCollectionKeyNoAlloc key)
    {
        ref var edge = ref _edgeMap.GetValueRefOrNullRef(key, out var exists);
        if (exists)
        {
            edge.Remove = default;
        }
    }

    public bool TryGetEdgeAdd(TypeCollectionKeyNoAlloc key, [NotNullWhen(true)] out T? item)
    {
        if (_edgeMap.TryGetValue(key, out var edge))
        {
            item = edge.Add;
            return item != null;
        }
        item = null;
        return false;
    }

    public bool TryGetEdgeRemove(TypeCollectionKeyNoAlloc key, [NotNullWhen(true)] out T? item)
    {
        if (_edgeMap.TryGetValue(key, out var edge))
        {
            item = edge.Remove;
            return item != null;
        }
        item = null;
        return false;
    }

    public T GetEdgeAdd(TypeCollectionKeyNoAlloc key)
    {
        return _edgeMap[key].Add!;
    }

    public T GetEdgeRemove(TypeCollectionKeyNoAlloc key)
    {
        return _edgeMap[key].Remove!;
    }

    public Edge this[TypeCollectionKeyNoAlloc key]
    {
        get
        {
            return _edgeMap[key];
        }
    }

    public TypeCollectionMap<Edge>.TypeCollectionMapEnumerator GetEnumerator()
    {
        return _edgeMap.GetEnumerator();
    }

    internal void Clear()
    {
        _edgeMap.Clear();
    }
}