using BlastEcs.Collections;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private int archetypeCount;

    readonly TypeCollectionMap<int> _archetypeMap;
    readonly GrowList<Archetype> _archetypes;
    readonly LongKeyMap<GrowList<int>> _archetypeTypeMap;
    readonly Archetype _entityArchetype;
    readonly Archetype _componentArchetype;

    private Archetype CreateArchetype(TypeCollectionKey key)
    {
        int id = archetypeCount++;
        _archetypeMap.Add(key, id);

        var arch = new Archetype(id, GetTable(key), key);
        _archetypes.Add();
        _archetypes[id] = arch;

        var types = key.Types;

        for (int i = 0; i < types.Length; i++)
        {
            ref var growlist = ref _archetypeTypeMap.GetValueRefOrAddDefault(types[i], out var exists);
            if (!exists)
            {
                growlist = new();
            }
            int index = growlist.Count;
            growlist.Add();
            growlist[index] = id;
        }
        return arch;
    }

    private Archetype GetArchetype(TypeCollectionKey key)
    {
        if (_archetypeMap.TryGetValue(key, out var archetypeId))
        {
            return _archetypes[archetypeId];
        }
        return CreateArchetype(key);
    }

    private Archetype GetArchetypeAdd<T>(Archetype currentArchetype) where T : struct
    {
        var addedId = GetHandleToType<T>().Id;
        if (currentArchetype.Edges.TryGetEdgeAdd(addedId, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        var newTypes = new ulong[oldTypes.Length + 1];
        oldTypes.CopyTo(newTypes);
        newTypes[newTypes.Length - 1] = addedId;
        newArch = GetArchetype(new TypeCollectionKey(newTypes));
        currentArchetype.Edges.AddEdgeAdd(addedId, newArch);
        return newArch;
    }

    private Archetype GetArchetypeRemove<T>(Archetype currentArchetype) where T : struct
    {
        var removedId = GetHandleToType<T>().Id;
        if (currentArchetype.Edges.TryGetEdgeRemove(removedId, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        int skipIndex = oldTypes.IndexOf(removedId);
        var newTypes = new ulong[oldTypes.Length - 1];
        {
            Span<ulong> destA = newTypes.AsSpan().Slice(0, skipIndex);
            Span<ulong> destb = newTypes.AsSpan().Slice(skipIndex, oldTypes.Length - (skipIndex + 1));
            oldTypes.Slice(0, skipIndex).CopyTo(destA);
            oldTypes.Slice(skipIndex + 1, oldTypes.Length - (skipIndex + 1)).CopyTo(destb);
        }
        newArch = GetArchetype(new TypeCollectionKey(newTypes));
        currentArchetype.Edges.AddEdgeRemove(removedId, newArch);
        return newArch;
    }

    public void MoveEntity(EcsHandle entity, Archetype src, Archetype dest)
    {
        ref var index = ref GetEntityIndex(entity);
        var archIndex = index.ArchetypeIndex;
        var pair = src.TableIndices[archIndex];
        if (src.Table != dest.Table)
        {
            int destTableIndex = dest.Table.Add();
            src.Table.CopyComponents(pair.tableIndex, dest.Table, destTableIndex, 1);
            src.Table.RemoveAt(pair.tableIndex);
            pair = (entity.Entity, destTableIndex);
        }
        src.RemoveEntityAt(archIndex);
        archIndex = dest.AddEntity(pair);
        index.Archetype = dest;
        index.ArchetypeIndex = archIndex;
    }
}
