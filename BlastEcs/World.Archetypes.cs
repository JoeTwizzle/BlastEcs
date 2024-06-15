using BlastEcs.Builtin;
using BlastEcs.Collections;
using System.Diagnostics;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private int archetypeCount;

    readonly TypeCollectionMap<int> _archetypeMap;
    readonly MaskedGrowList<Archetype> _archetypes;
    readonly GrowList<int> _deadArchetypes;
    readonly LongKeyMap<GrowList<int>> _archetypeTypeMap;
    readonly LongKeyMap<HashSet<ulong>> _archetypePairMap;
    readonly Archetype _entityArchetype;
    readonly Archetype _componentArchetype;

    private int GetArchetypeId()
    {
        if (_deadArchetypes.Count > 0)
        {
            int id = _deadArchetypes[_deadArchetypes.Count - 1];
            _deadArchetypes.RemoveAtDense(_deadArchetypes.Count - 1);
            return id;
        }
        return archetypeCount++;
    }

    private Archetype CreateArchetype(TypeCollectionKey key)
    {

        int id = GetArchetypeId();
        _archetypeMap.Add(key, id);

        var arch = new Archetype(id, GetTable(key), key);
        Debug.Assert(_archetypes.Count == id);
        _archetypes.Add(arch);

        var types = key.Types;

        for (int i = 0; i < types.Length; i++)
        {
            var handle = new EcsHandle(types[i]);
            //The type is a pair
            if ((handle.Flags & EntityFlags.IsPair) != 0)
            {
                ref GrowList<int> growlist = ref _archetypeTypeMap.GetValueRefOrAddDefault(types[i], out var exists);
                if (!exists)
                {
                    growlist = new();
                }
                int index = growlist.Count;
                growlist.Add();
                growlist[index] = id;
                //Kind
                ref HashSet<ulong> pairList = ref _archetypePairMap.GetValueRefOrAddDefault(handle.Entity, out exists);
                if (!exists)
                {
                    pairList = new();
                }
                index = pairList.Count;
                pairList.Add(types[i]);
                //target
                pairList = ref _archetypePairMap.GetValueRefOrAddDefault(handle.Target, out exists);
                if (!exists)
                {
                    pairList = new();
                }
                index = pairList.Count;
                pairList.Add(types[i]);
            }
            else
            {
                ref GrowList<int> growlist = ref _archetypeTypeMap.GetValueRefOrAddDefault(handle.Entity, out var exists);
                if (!exists)
                {
                    growlist = new();
                }
                int index = growlist.Count;
                growlist.Add();
                growlist[index] = id;
            }
        }
        return arch;
    }

    public Archetype GetArchetype()
    {
        return GetArchetype(new([]));
    }

    [Variadic(nameof(T0), 10)]
    public Archetype GetArchetype<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        var handle_T0 = GetHandleToType<T0>().Id;
        // [Variadic: CopyArgs(handle)]
        var key = new TypeCollectionKeyNoAlloc([handle_T0]);
        return GetArchetype(key);
    }

    public Archetype GetArchetype(TypeCollectionKeyNoAlloc key)
    {
        if (_archetypeMap.TryGetValue(key, out var archetypeId))
        {
            return _archetypes[archetypeId];
        }
        return CreateArchetype(new(key));
    }

    [Variadic(nameof(T0), 10)]
    private Archetype GetArchetypeAdd<T0>(Archetype currentArchetype) where T0 : struct
    {
        // [Variadic: CopyLines()]
        var addedId_T0 = GetHandleToType<T0>().Id;
        // [Variadic: CopyArgs(addedId)]
        var key = new TypeCollectionKeyNoAlloc([addedId_T0]);
        if (currentArchetype.Edges.TryGetEdgeAdd(key, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        // [Variadic: CopyArgs(addedId)]
        Span<ulong> newTypes = [.. oldTypes, addedId_T0];
        newArch = GetArchetype(new TypeCollectionKeyNoAlloc(newTypes));
        var key2 = new TypeCollectionKey(key);
        currentArchetype.Edges.AddEdgeAdd(key2, newArch);
        newArch.Edges.AddEdgeRemove(key2, currentArchetype);
        return newArch;
    }

    [Variadic(nameof(T0), 10)]
    private Archetype GetArchetypeRemove<T0>(Archetype currentArchetype) where T0 : struct
    {
        // [Variadic: CopyLines()]
        var removedId_T0 = GetHandleToType<T0>().Id;
        // [Variadic: CopyArgs(removedId)]
        var key = new TypeCollectionKeyNoAlloc([removedId_T0]);
        return RemoveArchetypeShared(key, currentArchetype);
    }

    private Archetype RemoveArchetypeShared(TypeCollectionKeyNoAlloc key, Archetype currentArchetype)
    {
        if (currentArchetype.Edges.TryGetEdgeRemove(key, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;

        Span<ulong> newTypes = stackalloc ulong[oldTypes.Length - key.Length];
        var removedTypes = key.Types;
        int count = 0;
        for (int i = 0; i < oldTypes.Length; i++)
        {
            bool isOk = true;

            for (int j = 0; j < removedTypes.Length && isOk; j++)
            {
                isOk = removedTypes[j] != oldTypes[i];
            }

            if (isOk)
            {
                newTypes[count++] = oldTypes[i];
            }
        }
        newArch = GetArchetype(new(newTypes));
        var key2 = new TypeCollectionKey(key);
        currentArchetype.Edges.AddEdgeRemove(key2, newArch);
        newArch.Edges.AddEdgeAdd(key2, currentArchetype);
        return newArch;
    }

    private Archetype GetArchetypeAdd(Archetype currentArchetype, EcsHandle addedComponent)
    {
        var addedId = addedComponent.Id;
        var key = new TypeCollectionKeyNoAlloc([addedId]);
        if (currentArchetype.Edges.TryGetEdgeAdd(key, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        var newTypes = new ulong[oldTypes.Length + 1];
        oldTypes.CopyTo(newTypes);
        newTypes[newTypes.Length - 1] = addedId;
        newArch = GetArchetype(new TypeCollectionKeyNoAlloc(newTypes));
        var key2 = new TypeCollectionKey(key);
        currentArchetype.Edges.AddEdgeAdd(key2, newArch);
        newArch.Edges.AddEdgeRemove(key2, currentArchetype);
        return newArch;
    }

    private Archetype GetArchetypeRemove(Archetype currentArchetype, EcsHandle removedComponent)
    {
        var removedId = removedComponent.Id;
        Debug.Assert(currentArchetype.Has(removedComponent));
        var key = new TypeCollectionKeyNoAlloc([removedId]);
        if (currentArchetype.Edges.TryGetEdgeRemove(key, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        int skipIndex = oldTypes.IndexOf(removedId);
        Span<ulong> newTypes = stackalloc ulong[oldTypes.Length - 1];

        Span<ulong> destA = newTypes.Slice(0, skipIndex);
        Span<ulong> destb = newTypes.Slice(skipIndex, oldTypes.Length - (skipIndex + 1));
        oldTypes.Slice(0, skipIndex).CopyTo(destA);
        oldTypes.Slice(skipIndex + 1, oldTypes.Length - (skipIndex + 1)).CopyTo(destb);

        newArch = GetArchetype(new(newTypes));
        var key2 = new TypeCollectionKey(key);
        currentArchetype.Edges.AddEdgeRemove(key2, newArch);
        newArch.Edges.AddEdgeAdd(key2, currentArchetype);
        return newArch;
    }

    private void MoveEntity(EcsHandle entity, Archetype src, Archetype dest)
    {
        ref var index = ref GetEntityIndex(entity);
        var archIndex = index.ArchetypeIndex;
        var pair = src.TableIndices[archIndex];
        if (src.Table != dest.Table)
        {
            int destTableIndex = dest.Table.Add();
            src.Table.CopyComponents(pair.tableIndex, dest.Table, destTableIndex, 1);
            src.Table.RemoveAt(pair.tableIndex);
            pair = (entity, destTableIndex);
        }
        src.RemoveEntityAt(archIndex);
        archIndex = dest.AddEntity(pair);
        index.Archetype = dest;
        index.ArchetypeIndex = archIndex;
    }

    private void MoveAllEntities(Archetype src, Archetype dest)
    {
        var indices = src.TableIndices.Span;
        if (src.Table != dest.Table)
        {
            int destTableIndex = dest.Table.AddRange(indices.Length);
            src.Table.CopyComponents(0, dest.Table, destTableIndex, src.TableIndices.Count);
            for (int i = 0; i < indices.Length; i++)
            {
                int newArchIndex = dest.AddEntity(indices[i].entity, destTableIndex + i);
                ref var ent = ref GetEntityIndex(indices[i].entity);
                ent.ArchetypeIndex = newArchIndex + i;
                ent.Archetype = dest;
            }
        }
        else
        {
            for (int i = 0; i < indices.Length; i++)
            {
                int newArchIndex = dest.AddEntities(indices);
                ref var ent = ref GetEntityIndex(indices[i].entity);
                ent.ArchetypeIndex = newArchIndex + i;
                ent.Archetype = dest;
            }
        }
    }

    private void DestroyArchetype(Archetype arch)
    {
        _deadArchetypes.Add(arch.Id);
        _archetypes[arch.Id] = null!;
        foreach (var keyValue in arch.Edges)
        {
            keyValue.Value.Remove?.Edges.RemoveEdgeAdd(keyValue.Key);
            keyValue.Value.Add?.Edges.RemoveEdgeRemove(keyValue.Key);
        }
    }
}
