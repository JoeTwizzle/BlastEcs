using BlastEcs.Builtin;
using BlastEcs.Collections;
using System.Diagnostics;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private int archetypeCount;

    /// <summary>
    /// Maps a type key to an archetype id
    /// </summary>
    private readonly TypeCollectionMap<int> _archetypeMap;
    private readonly List<Archetype> _archetypes;
    private readonly GrowList<int> _deadArchetypes;
    private readonly Archetype _emptyArchetype;
    private readonly Archetype _componentArchetype;

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
        arch.Table._archetypes.Add(id);
        Debug.Assert(_archetypes.Count == id);
        _archetypes.Add(arch);

        return arch;
    }

    [Variadic(nameof(T0), VariadicCount)]
    public Archetype GetOrCreateArchetype<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        var handle_T0 = GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyArgs(handle)]
        var key = new TypeCollectionKeyNoAlloc([handle_T0]);
        return GetOrCreateArchetype(key);
    }

    public Archetype GetOrCreateArchetype(TypeCollectionKeyNoAlloc key)
    {
        if (_archetypeMap.TryGetValue(key, out var archetypeId))
        {
            return _archetypes[archetypeId];
        }
        if (key.HasDuplicates())
        {
            throw new InvalidOperationException("An archetype cannot contain duplicate components");
        }
        return CreateArchetype(new(key));
    }

    private Archetype GetArchetypeAdd(Archetype currentArchetype, TypeCollectionKeyNoAlloc addedComponents)
    {
        var lookupKey = addedComponents;
        if (currentArchetype.Edges.TryGetEdgeAdd(lookupKey, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        Span<ulong> newTypes = [.. oldTypes, .. lookupKey.Types];
        var newKey = new TypeCollectionKeyNoAlloc(newTypes);
        newArch = GetOrCreateArchetype(newKey);
        var lookupKeyAlloc = new TypeCollectionKey(lookupKey);
        currentArchetype.Edges.AddEdgeAdd(lookupKeyAlloc, newArch);
        newArch.Edges.AddEdgeRemove(lookupKeyAlloc, currentArchetype);
        return newArch;
    }

    [Variadic(nameof(T0), VariadicCount)]
    private Archetype GetArchetypeAdd<T0>(Archetype currentArchetype) where T0 : struct
    {
        // [Variadic: CopyLines()]
        var addedId_T0 = GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyArgs(addedId)]
        var lookupKey = new TypeCollectionKeyNoAlloc([addedId_T0]);
        if (currentArchetype.Edges.TryGetEdgeAdd(lookupKey, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        // [Variadic: CopyArgs(addedId)]
        Span<ulong> newTypes = [.. oldTypes, addedId_T0];
        var newKey = new TypeCollectionKeyNoAlloc(newTypes);
        newArch = GetOrCreateArchetype(newKey);
        var lookupKeyAlloc = new TypeCollectionKey(lookupKey);
        currentArchetype.Edges.AddEdgeAdd(lookupKeyAlloc, newArch);
        newArch.Edges.AddEdgeRemove(lookupKeyAlloc, currentArchetype);
        return newArch;
    }

    private Archetype GetArchetypeRemove(Archetype currentArchetype, TypeCollectionKeyNoAlloc removedComponents)
    {
        return RemoveArchetypeImpl(removedComponents, currentArchetype);
    }

    [Variadic(nameof(T0), VariadicCount)]
    private Archetype GetArchetypeRemove<T0>(Archetype currentArchetype) where T0 : struct
    {
        // [Variadic: CopyLines()]
        var removedId_T0 = GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyArgs(removedId)]
        var key = new TypeCollectionKeyNoAlloc([removedId_T0]);
        return RemoveArchetypeImpl(key, currentArchetype);
    }

    private Archetype RemoveArchetypeImpl(TypeCollectionKeyNoAlloc lookupKey, Archetype currentArchetype)
    {
        if (currentArchetype.Edges.TryGetEdgeRemove(lookupKey, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        if (lookupKey.Length > oldTypes.Length)
        {
            throw new InvalidOperationException("Cannot remove component/s from archtype. No such component present.");
        }
        Span<ulong> newTypes = stackalloc ulong[oldTypes.Length - lookupKey.Length];
        var removedTypes = lookupKey.Types;
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
        if(count == oldTypes.Length)
        {
            throw new InvalidOperationException("Cannot remove component/s from archtype. No such component present.");
        }
        var newKey = new TypeCollectionKeyNoAlloc(newTypes);
        newArch = GetOrCreateArchetype(newKey);
        var lookupKeyAlloc = new TypeCollectionKey(lookupKey);
        currentArchetype.Edges.AddEdgeRemove(lookupKeyAlloc, newArch);
        newArch.Edges.AddEdgeAdd(lookupKeyAlloc, currentArchetype);
        return newArch;
    }

    private Archetype GetArchetypeAdd(Archetype currentArchetype, EcsHandle addedComponent)
    {
        var addedId = addedComponent.Id;
        var lookupKey = new TypeCollectionKeyNoAlloc([addedId]);
        if (currentArchetype.Edges.TryGetEdgeAdd(lookupKey, out var newArch))
        {
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        var newTypes = new ulong[oldTypes.Length + 1];
        oldTypes.CopyTo(newTypes);
        newTypes[newTypes.Length - 1] = addedId;
        var newKey = new TypeCollectionKeyNoAlloc(newTypes);
        newArch = GetOrCreateArchetype(newKey);
        var lookupKeyAlloc = new TypeCollectionKey(lookupKey);
        currentArchetype.Edges.AddEdgeAdd(lookupKeyAlloc, newArch);
        newArch.Edges.AddEdgeRemove(lookupKeyAlloc, currentArchetype);
        return newArch;
    }

    private Archetype GetArchetypeRemove(Archetype currentArchetype, EcsHandle removedComponent)
    {
        var removedId = removedComponent.Id;
        Debug.Assert(currentArchetype.Has(removedComponent));
        var lookupKey = new TypeCollectionKeyNoAlloc([removedId]);
        if (currentArchetype.Edges.TryGetEdgeRemove(lookupKey, out var newArch))
        {
            Debug.Assert(newArch != currentArchetype);
            return newArch;
        }
        var oldTypes = currentArchetype.Key.Types;
        Span<ulong> newTypes = stackalloc ulong[oldTypes.Length - 1];
        if (oldTypes.Length > 0)
        {
            int skipIndex = oldTypes.IndexOf(removedId);

            Span<ulong> destA = newTypes.Slice(0, skipIndex);
            Span<ulong> destb = newTypes.Slice(skipIndex, oldTypes.Length - (skipIndex + 1));
            oldTypes.Slice(0, skipIndex).CopyTo(destA);
            oldTypes.Slice(skipIndex + 1).CopyTo(destb);
        }
        var newKey = new TypeCollectionKeyNoAlloc(newTypes);
        newArch = GetOrCreateArchetype(newKey);
        var lookupKeyAlloc = new TypeCollectionKey(lookupKey);
        currentArchetype.Edges.AddEdgeRemove(lookupKeyAlloc, newArch);
        newArch.Edges.AddEdgeAdd(lookupKeyAlloc, currentArchetype);
        Debug.Assert(newArch != currentArchetype);
        return newArch;
    }

    /// <summary>
    /// Moves an entity from one archetype to another
    /// Moves the last entity in the src archetype to fill hole#
    /// May move tables if src.Table != dest.Table
    /// </summary>
    private void MoveEntity(EcsHandle entity, Archetype src, Archetype dest)
    {
        ref var index = ref GetEntityIndex(entity);
        var archIndex = index.ArchetypeSlotIndex;
        var pair = src.Entities[archIndex];
        if (src.Table != dest.Table)
        {
            int destTableIndex = dest.Table.AddEntity(entity);
            src.Table.CopyComponents(index.TableSlotIndex, dest.Table, destTableIndex, 1);
            src.Table.RemoveAt(index.TableSlotIndex);
            index.TableSlotIndex = destTableIndex;
            pair = entity;
        }
        src.RemoveEntityAt(archIndex);
        archIndex = dest.AddEntity(pair);
        index.Archetype = dest;
        index.ArchetypeSlotIndex = archIndex;
    }

    private void MoveAllEntities(Archetype src, Archetype dest)
    {
        var entities = src.Entities.Span;
        if (src.Table != dest.Table)
        {
            int destTableIndex = dest.Table.AddEntities(entities);
            src.Table.CopyComponents(0, dest.Table, destTableIndex, src.Entities.Count);
            for (int i = 0; i < entities.Length; i++)
            {
                int newArchIndex = dest.AddEntity(entities[i]);
                ref var ent = ref GetEntityIndex(entities[i]);
                ent.ArchetypeSlotIndex = newArchIndex + i;
                ent.TableSlotIndex = destTableIndex + i;
                ent.Archetype = dest;
            }
        }
        else
        {
            for (int i = 0; i < entities.Length; i++)
            {
                int newArchIndex = dest.AddEntities(entities);
                ref var ent = ref GetEntityIndex(entities[i]);
                ent.ArchetypeSlotIndex = newArchIndex + i;
                ent.Archetype = dest;
            }
        }
    }

    private void DestroyArchetype(Archetype arch)
    {
        _archetypeMap.Remove(arch.Key);
        _archetypes.RemoveAt(arch.Id);
        _deadArchetypes.Add(arch.Id);
        arch.Table._archetypes.RemoveAtDense(arch.Id);
        foreach (var target in arch.Edges)
        {
            target.Value.Add?.Edges.RemoveEdgeRemove(target.Key);
            target.Value.Remove?.Edges.RemoveEdgeAdd(target.Key);
        }
        arch.Edges.Clear();
    }
}
