using BlastEcs.Builtin;
using BlastEcs.Collections;
using BlastEcs.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private int archetypeCount;

    /// <summary>
    /// Maps a type key to an archetype id
    /// </summary>
    private readonly TypeCollectionMap<int> _archetypeMap;
    private readonly SparseMap<Archetype> _archetypes;
    private readonly GrowList<int> _deadArchetypes;
    /// <summary>
    /// For every entity which is used as a type in an Archetype
    /// Write id of the archetype that contains it
    /// Store ArchetypeId mapped to indices
    /// </summary>
    private readonly LongKeyMap<ComponentIndexInfo> _componentIndex;
    /// <summary>
    /// For every pair which is used as a type in an Archetype
    /// Write an entry for Kind, containing Target
    /// Write an entry for Target, containing Kind
    /// </summary>
    private readonly Dictionary<uint, SparseSet> _pairTypeMap;
    private readonly Archetype _entityArchetype;
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
        Debug.Assert(_archetypes.Count == id);
        _archetypes.Add((uint)arch.Id, arch);

        //Register presence of components
        var types = key.Types;
        for (int i = 0; i < types.Length; i++)
        {
            var handle = new EcsHandle(types[i]);
            ref var info = ref _componentIndex.GetValueRefOrAddDefault(handle.Id, out var exists);
            if (!exists)
            {
                info = new();
            }
            info.ArchetypeMap.Add(id, i);
            info.ContainingArchetypes.SetBit(id);
            if (handle.IsPair)
            {
                //Add refrence for Kind
                ref SparseSet list = ref _pairTypeMap.GetRefOrAddDefault(handle.Entity, out exists);
                if (!exists)
                {
                    list = new();
                }
                list.Add(handle.Target);

                //Add refrence for Target
                list = ref _pairTypeMap.GetRefOrAddDefault(handle.Target, out exists);
                if (!exists)
                {
                    list = new();
                }
                list.Add(handle.Entity);

                //Add refrence for indefinite Kind
                var indefiniteKind = GetRelationWithIndefiniteKind(handle);
                info = ref _componentIndex.GetValueRefOrAddDefault(indefiniteKind.Id, out exists);
                if (!exists)
                {
                    info = new();
                }
                info.ContainingArchetypes.SetBit(id);

                //Add refrence for indefinite Target
                var indefiniteTarget = GetRelationWithIndefiniteTarget(handle);
                info = ref _componentIndex.GetValueRefOrAddDefault(indefiniteTarget.Id, out exists);
                if (!exists)
                {
                    info = new();
                }
                info.ContainingArchetypes.SetBit(id);
            }
        }
        return arch;
    }

    public Archetype GetArchetype()
    {
        return GetArchetype(new([]));
    }

    [Variadic(nameof(T0), VariadicCount)]
    public Archetype GetArchetype<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        var handle_T0 = GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyArgs(handle)]
        var key = new TypeCollectionKeyNoAlloc([handle_T0]);
        return GetArchetype(key);
    }

    public Archetype GetArchetype(TypeCollectionKeyNoAlloc key)
    {
        if (_archetypeMap.TryGetValue(key, out var archetypeId))
        {
            return _archetypes[(uint)archetypeId];
        }
        return CreateArchetype(new(key));
    }

    public void GetArchetypesWith(TypeCollectionKeyNoAlloc key, BitMask validArchetypes, bool init)
    {
        for (int i = 0; i < key.Length; i++)
        {
            var handle = new EcsHandle(key[i]);
            var archetypes = _componentIndex[handle.Id];
            if (i == 0 && init)
            {
                validArchetypes.OrBits(archetypes.ContainingArchetypes);
            }
            else
            {
                validArchetypes.AndBits(archetypes.ContainingArchetypes);
            }
        }
    }

    internal void GetArchetypesWith(Term term, BitMask validArchetypes, bool init)
    {
        var archetypes = _componentIndex[term.Match.Id];
        if (init)
        {
            validArchetypes.OrBits(archetypes.ContainingArchetypes);
        }
        else
        {
            if (term.Exclude)
            {
                validArchetypes.ClearBits(archetypes.ContainingArchetypes);
            }
            else
            {
                validArchetypes.AndBits(archetypes.ContainingArchetypes);
            }
        }
    }

    public void FilterArchetypesWithout(TypeCollectionKeyNoAlloc key, BitMask validArchetypes)
    {
        for (int i = 0; i < key.Length; i++)
        {
            var handle = new EcsHandle(key[i]);
            var archetypes = _componentIndex[handle.Id];
            validArchetypes.ClearBits(archetypes.ContainingArchetypes);
        }
    }

    [Variadic(nameof(T0), VariadicCount)]
    private Archetype GetArchetypeAdd<T0>(Archetype currentArchetype) where T0 : struct
    {
        // [Variadic: CopyLines()]
        var addedId_T0 = GetHandleToInstantiableType<T0>().Id;
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

    [Variadic(nameof(T0), VariadicCount)]
    private Archetype GetArchetypeRemove<T0>(Archetype currentArchetype) where T0 : struct
    {
        // [Variadic: CopyLines()]
        var removedId_T0 = GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyArgs(removedId)]
        var key = new TypeCollectionKeyNoAlloc([removedId_T0]);
        return RemoveArchetypeImpl(key, currentArchetype);
    }

    private Archetype RemoveArchetypeImpl(TypeCollectionKeyNoAlloc key, Archetype currentArchetype)
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
        Span<ulong> newTypes = stackalloc ulong[oldTypes.Length - 1];
        if (oldTypes.Length > 0)
        {
            int skipIndex = oldTypes.IndexOf(removedId);

            Span<ulong> destA = newTypes.Slice(0, skipIndex);
            Span<ulong> destb = newTypes.Slice(skipIndex, oldTypes.Length - (skipIndex + 1));
            oldTypes.Slice(0, skipIndex).CopyTo(destA);
            oldTypes.Slice(skipIndex + 1).CopyTo(destb);
        }
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
        _archetypeMap.Remove(arch.Key);
        _archetypes.Remove((uint)arch.Id);
        _deadArchetypes.Add(arch.Id);
        foreach (var keyValue in arch.Edges)
        {
            keyValue.Value.Remove?.Edges.RemoveEdgeAdd(keyValue.Key);
            keyValue.Value.Add?.Edges.RemoveEdgeRemove(keyValue.Key);
        }
        var types = arch.Key.Types;
        for (int i = 0; i < types.Length; i++)
        {
            var handle = new EcsHandle(types[i]);
            //The type is a pair
            ref var info = ref _componentIndex.GetValueRefOrNullRef(handle.Id, out bool exists);
            if (exists)
            {
                info.ArchetypeMap.Remove(arch.Id);
                info.ContainingArchetypes.ClearBit(arch.Id);
            }

            if (!handle.IsPair)
            {
                continue;
            }

            ref SparseSet set = ref _pairTypeMap.GetRefOrNullRef(handle.Entity);
            if (!Unsafe.IsNullRef(ref set))
            {
                set.Remove(handle.Target);
                _pairTypeMap.Remove(handle.Entity);
            }

            set = ref _pairTypeMap.GetRefOrNullRef(handle.Target);
            if (!Unsafe.IsNullRef(ref set))
            {
                set.Remove(handle.Entity);
                _pairTypeMap.Remove(handle.Target);
            }

            var indefiniteTarget = GetRelationWithIndefiniteTarget(handle);
            info = ref _componentIndex.GetValueRefOrNullRef(indefiniteTarget.Id, out exists);
            if (exists)
            {
                info.ContainingArchetypes.ClearBit(arch.Id);
            }

            var indefiniteKind = GetRelationWithIndefiniteKind(handle);
            info = ref _componentIndex.GetValueRefOrNullRef(indefiniteKind.Id, out exists);
            if (exists)
            {
                info.ContainingArchetypes.ClearBit(arch.Id);
            }
        }
    }
}
