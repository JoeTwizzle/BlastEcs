using BlastEcs.Collections;
using BlastEcs.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace BlastEcs;

public sealed class Edges<T> where T : class
{
    public struct Edge
    {
        public T? Add;
        public T? Remove;
    }

    private readonly TypeCollectionMap<Edge> map;
    public Edges()
    {
        map = new TypeCollectionMap<Edge>();
    }

    internal void AddEdgeAdd(TypeCollectionKey key, T item)
    {
        ref var edge = ref map.GetValueRefOrAddDefault(key, out var exists);
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
        ref var edge = ref map.GetValueRefOrAddDefault(key, out var exists);
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
        ref var edge = ref map.GetValueRefOrNullRef(key, out var exists);
        if (exists)
        {
            edge.Add = default;
        }
    }

    internal void RemoveEdgeRemove(TypeCollectionKeyNoAlloc key)
    {
        ref var edge = ref map.GetValueRefOrNullRef(key, out var exists);
        if (exists)
        {
            edge.Remove = default;
        }
    }

    public bool TryGetEdgeAdd(TypeCollectionKeyNoAlloc key, [NotNullWhen(true)] out T? item)
    {
        if (map.TryGetValue(key, out var edge))
        {
            item = edge.Add;
            return item != null;
        }
        item = null;
        return false;
    }

    public bool TryGetEdgeRemove(TypeCollectionKeyNoAlloc key, [NotNullWhen(true)] out T? item)
    {
        if (map.TryGetValue(key, out var edge))
        {
            item = edge.Remove;
            return item != null;
        }
        item = null;
        return false;
    }

    public T GetEdgeAdd(TypeCollectionKeyNoAlloc key)
    {
        return map[key].Add!;
    }

    public T GetEdgeRemove(TypeCollectionKeyNoAlloc key)
    {
        return map[key].Remove!;
    }

    public Edge this[TypeCollectionKeyNoAlloc key]
    {
        get
        {
            return map[key];
        }
    }

    public TypeCollectionMap<Edge>.TypeCollectionMapEnumerator GetEnumerator()
    {
        return map.GetEnumerator();
    }
}
