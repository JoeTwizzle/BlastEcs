using BlastEcs.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed class Edges<T> where T : class
{
    public struct Edge
    {
        public T Add;
        public T Remove;
    }

    readonly LongKeyMap<Edge> map;
    public Edges()
    {
        map = new LongKeyMap<Edge>();
    }

    internal void AddEdgeAdd(ulong key, T item)
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

    internal void AddEdgeRemove(ulong key, T item)
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

    public bool TryGetEdgeAdd(ulong key, [NotNullWhen(true)] out T? item)
    {
        if (map.TryGetValue(key, out var edge))
        {
            item = edge.Add;
            return item != null;
        }
        item = null;
        return false;
    }

    public bool TryGetEdgeRemove(ulong key, [NotNullWhen(true)] out T? item)
    {
        if (map.TryGetValue(key, out var edge))
        {
            item = edge.Remove;
            return item != null;
        }
        item = null;
        return false;
    }

    public T GetEdgeAdd(ulong key)
    {
        return map[key].Add;
    }

    public T GetEdgeRemove(ulong key)
    {
        return map[key].Remove;
    }

    public Edge this[ulong key]
    {
        get
        {
            return map[key];
        }
    }
}
