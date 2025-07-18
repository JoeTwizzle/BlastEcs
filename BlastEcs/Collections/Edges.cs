using BlastEcs.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
class MyClass
{
    //O(N) -> O(M)
    Dictionary<int, Dictionary<TypeCollectionKey, (int, int)>> a;
    //O(M) -> O(N)
    Dictionary<ulong, Dictionary<int, (int, int)>> b;
    //O(M + K) 
    // ulong -> ulong -> ulong -> ... -> (int, int)
    //Delta, 
    LinkedList<Archetype> edges;
    struct KeyId;
    Dictionary<TypeCollectionKey, KeyId> keys;
    Dictionary<KeyId, Dictionary<Archetype, Archetype>> Edges;
    Dictionary<Archetype, List<KeyId>> SrcDestMap;
}
struct Entry
{
    public int AddId;
    public int RemoveId;
}

public sealed class TrieNode
{
    public readonly ulong Id;
    readonly Dictionary<ulong, TrieNode> edges;
    Dictionary<int, Entry>? _values;
    public TrieNode(ulong id)
    {
        Id = id;
        edges = [];
    }

    private static ref Entry GetValueRefOrAddDefault(TrieNode root, ReadOnlySpan<ulong> sequence, int archId, out bool exists)
    {
        TrieNode currentNode = root;
        int index = 0;
        int length = sequence.Length;

        while (index < length)
        {
            ulong current = sequence[index];
            Dictionary<ulong, TrieNode> edges = currentNode.edges;

            if (edges.TryGetValue(current, out var next))
            {
                currentNode = next;
            }
            else
            {
                break;
            }


            index++;
        }

        while (index < length)
        {
            ulong current = sequence[index];
            var newNode = new TrieNode(current);
            currentNode.edges.Add(current, newNode);
            currentNode = newNode;
            index++;
        }

        currentNode._values ??= new Dictionary<int, Entry>();
        return ref CollectionsMarshal.GetValueRefOrAddDefault(currentNode._values, archId, out exists);
    }

    private static ref Entry GetValueRefOrNullRef(TrieNode root, ReadOnlySpan<ulong> sequence, int archId)
    {
        TrieNode currentNode = root;
        int index = 0;
        int length = sequence.Length;

        while (index < length)
        {
            ulong current = sequence[index];
            Dictionary<ulong, TrieNode> edges = currentNode.edges;

            if (edges.TryGetValue(current, out var next))
            {
                currentNode = next;
            }
            else
            {
                break;
            }

            index++;
        }

        while (index < length)
        {
            ulong current = sequence[index];
            var newNode = new TrieNode(current);
            currentNode.edges.Add(current, newNode);
            currentNode = newNode;
            index++;
        }

        // Handle leaf node
        if (currentNode._values == null)
        {
            return ref Unsafe.NullRef<Entry>();
        }
        return ref CollectionsMarshal.GetValueRefOrNullRef(currentNode._values, archId);
    }

    public void AddEdgeAdd(ReadOnlySpan<ulong> sequence, int srcArch, int destArch)
    {
        ref var entry = ref GetValueRefOrAddDefault(this, sequence, srcArch, out var exists);
        entry.AddId = destArch;
        entry = ref GetValueRefOrAddDefault(this, sequence, destArch, out exists);
        entry.RemoveId = srcArch;
    }

    public void AddEdgeRemove(ReadOnlySpan<ulong> sequence, int srcArch, int destArch)
    {
        ref var entry = ref GetValueRefOrAddDefault(this, sequence, srcArch, out var exists);
        entry.RemoveId = destArch;
        entry = ref GetValueRefOrAddDefault(this, sequence, destArch, out exists);
        entry.AddId = srcArch;
    }

    public void RemoveEdgeAdd(ReadOnlySpan<ulong> sequence, int srcArch)
    {
        ref var entry = ref GetValueRefOrNullRef(this, sequence, srcArch);
        if (!Unsafe.IsNullRef(ref entry))
        {
            var destArch = entry.AddId;
            entry.AddId = 0;
            entry = ref GetValueRefOrNullRef(this, sequence, destArch);
            if (!Unsafe.IsNullRef(ref entry))
            {
                entry.RemoveId = 0;
            }
        }
    }

    public void RemoveEdgeRemove(ReadOnlySpan<ulong> sequence, int srcArch)
    {
        ref var entry = ref GetValueRefOrNullRef(this, sequence, srcArch);
        if (!Unsafe.IsNullRef(ref entry))
        {
            var destArch = entry.RemoveId;
            entry.RemoveId = 0;
            entry = ref GetValueRefOrNullRef(this, sequence, destArch);
            if (!Unsafe.IsNullRef(ref entry))
            {
                entry.AddId = 0;
            }
        }
    }

    public bool TryGetEdgeAdd(ReadOnlySpan<ulong> sequence, int srcArch, out int destId)
    {
        ref var entry = ref GetValueRefOrNullRef(this, sequence, srcArch);
        if (!Unsafe.IsNullRef(ref entry))
        {
            destId = entry.AddId;
            return destId != 0;
        }
        destId = 0;
        return false;
    }

    public bool TryGetEdgeRemove(ReadOnlySpan<ulong> sequence, int srcArch, out int destId)
    {
        ref var entry = ref GetValueRefOrNullRef(this, sequence, srcArch);
        if (!Unsafe.IsNullRef(ref entry))
        {
            destId = entry.RemoveId;
            return destId != 0;
        }
        destId = 0;
        return false;
    }
}

