using BlastEcs.Builtin;
using BlastEcs.Collections;
using BlastEcs.Helpers;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BlastEcs;
public sealed partial class EcsWorld
{
    private uint _entityCount;
    private readonly EcsHandle _emptyEntity;
    private readonly FastMap<EntityIndex> _entities;
    private readonly GrowList<uint> _deadEntities;

    public EcsHandle CreateEntity()
    {
        return CreateEntity([]);
    }

    [Variadic(nameof(T0), VariadicCount)]
    public EcsHandle CreateEntity<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        var handle_T0 = GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyArgs(handle)]
        var key = new TypeCollectionKeyNoAlloc([handle_T0]);
        return CreateEntity(key);
    }

    public EcsHandle CreateEntity(ReadOnlySpan<Type> types)
    {
        Span<ulong> typeHandles = stackalloc ulong[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            typeHandles[i] = GetHandleToInstantiableType(types[i].TypeHandle).Id;
        }
        return CreateEntity(new TypeCollectionKeyNoAlloc(typeHandles));
    }

    public EcsHandle CreateEntity(TypeCollectionKeyNoAlloc key)
    {
        return CreateEntity(GetArchetype(key));
    }

    public EcsHandle CreateEntity(Archetype archetype)
    {
        return CreateEntity(archetype, EntityFlags.None);
    }

    private EcsHandle CreateEntity(Archetype archetype, byte flags)
    {
        uint id = GetNextEntityId();

        ref EntityIndex entityIndex = ref GetEntityIndex(id);
        entityIndex.Archetype = archetype;
        var gen = entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        entityIndex.Flags = flags;
        var entity = new EcsHandle(id, gen, _worldId, flags);
        var tableIndex = archetype.Table.Add();
        entityIndex.ArchetypeSlotIndex = archetype.AddEntity(entity, tableIndex);
        if (EntityEventsEnabled)
        {
            OnEntityCreated?.Invoke(entity);
        }
        return entity;
    }

    private EcsHandle CreatePair(EcsHandle kind, EcsHandle target, Archetype archetype)
    {
        var handle = new EcsHandle(kind, target);
        ref EntityIndex entityIndex = ref GetEntityIndex(handle);
        entityIndex.Archetype = archetype;
        entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var tableIndex = archetype.Table.Add();
        entityIndex.ArchetypeSlotIndex = archetype.AddEntity(handle, tableIndex);
        return handle;
    }

    public void DestroyEntity(EcsHandle entity)
    {
        //RemoveReferencesTo(entity);
        ref EntityIndex entityIndex = ref GetEntityIndex(entity);
        var arch = entityIndex.Archetype;
        var pair = arch.TableIndices[entityIndex.ArchetypeSlotIndex];
        arch.RemoveEntityAt(entityIndex.ArchetypeSlotIndex);
        arch.Table.RemoveAt(pair.tableIndex);
        entityIndex.Generation = (short)-entityIndex.Generation;

        //Recycle id if entity is not a pair
        if (!entity.IsPair)
        {
            _deadEntities.Add(entity.Entity);
        }
        if (EntityEventsEnabled)
        {
            OnEntityDestroyed?.Invoke(entity);
        }
    }

    //private void RemoveReferencesTo(EcsHandle handle)
    //{
    //    //Get archetypes that contain this entity
    //    if (_componentIndex.TryGetValue(handle.Id, out var compInfo))
    //    {
    //        RemoveComponentFromArchetypes(handle, compInfo.ContainingArchetypes);
    //        _componentIndex.Remove(handle.Id);
    //    }

    //    //Are there any archetypes which have this component as part of a relationship?
    //    if (handle.IsPair || !_pairTypeMap.TryGetValue(handle.Entity, out var types))
    //    {
    //        return;
    //    }

    //    foreach (var entityB in types)
    //    {
    //        //This entity is the Kind
    //        var pairA = GetHandleToPair(GetEntity(handle.Entity), GetEntity(entityB));
    //        GetArchetypesWith(pairA,,true)
    //        if (_componentIndex.TryGetValue(pairA.Id, out compInfo))
    //        {
    //            RemoveComponentFromArchetypes(pairA, compInfo.ContainingArchetypes);
    //            _componentIndex.Remove(pairA.Id);
    //        }
    //        //This entity is the Target
    //        var pairB = GetHandleToPair(GetEntity(entityB), GetEntity(handle.Entity));
    //        if (_componentIndex.TryGetValue(pairB.Id, out compInfo))
    //        {
    //            RemoveComponentFromArchetypes(pairB, compInfo.ContainingArchetypes);
    //            _componentIndex.Remove(pairB.Id);
    //        }
    //        //This entity is the Kind
    //        var indefiniteTarget = GetRelationWithIndefiniteTarget(handle);
    //        if (_componentIndex.TryGetValue(indefiniteTarget.Id, out compInfo))
    //        {
    //            _componentIndex.Remove(indefiniteTarget.Id);
    //        }
    //        //This entity is the Target
    //        var indefiniteKind = GetRelationWithIndefiniteKind(handle);
    //        if (_componentIndex.TryGetValue(indefiniteKind.Id, out compInfo))
    //        {
    //            _componentIndex.Remove(indefiniteKind.Id);
    //        }
    //    }
    //}

    private void RemoveComponentFromArchetypes(EcsHandle entity, QuickMask archetypes)
    {
        foreach (var ids in archetypes)
        {
            var oldArch = _archetypes[(uint)ids];
            var newArch = GetArchetypeRemove(oldArch, entity);
            MoveAllEntities(oldArch, newArch);
            DestroyArchetype(oldArch);
        }
    }

    private void RemoveComponentFromArchetypes(EcsHandle component, ReadOnlySpan<ulong> archetypesMask)
    {
        for (int idx = 0; idx < archetypesMask.Length; idx++)
        {
            long bitItem = (long)archetypesMask[idx];
            while (bitItem != 0)
            {
                int id = idx * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(bitItem);
                bitItem ^= bitItem & -bitItem;

                var oldArch = _archetypes[(uint)id];
                var newArch = GetArchetypeRemove(oldArch, component);
                MoveAllEntities(oldArch, newArch);
                DestroyArchetype(oldArch);
            }
        }
    }
    //TODO: Unused?
    private void RemoveComponentFromArchetypes(EcsHandle entity, Dictionary<int, int> archetypes)
    {
        foreach (var ids in archetypes.Keys)
        {
            var oldArch = _archetypes[(uint)ids];

            var newArch = GetArchetypeRemove(oldArch, entity);
            MoveAllEntities(oldArch, newArch);
            DestroyArchetype(oldArch);
        }
    }
    //TODO: Unused?
    private void RemoveComponentFromArchetypes(EcsHandle entity, ReadOnlySpan<int> archetypes)
    {
        foreach (var ids in archetypes)
        {
            var oldArch = _archetypes[(uint)ids];
            var newArch = GetArchetypeRemove(oldArch, entity);
            MoveAllEntities(oldArch, newArch);
            DestroyArchetype(oldArch);
        }
    }


    private uint GetNextEntityId()
    {
        if (_deadEntities.Count > 0)
        {
            uint ent = _deadEntities[_deadEntities.Count - 1];
            _deadEntities.RemoveAtDense(_deadEntities.Count - 1);
            return ent;
        }
        uint id = ++_entityCount;
        if (id == uint.MaxValue) ThrowHelper.ThrowInvalidOperationException("Maximum number of entities exceded");
        return id;
    }

    public bool IsAlive(EcsHandle entity)
    {
        short gen = GetEntityIndex(entity).Generation;
        if (entity.IsPair)
        {
            return gen > 0;
        }
        return entity.Generation > 0 && gen == entity.Generation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref EntityIndex GetEntityIndex(EcsHandle entity)
    {
        if (entity.IsPair)
        {
            return ref _entities.GetOrCreateRefAt(entity.Id);
        }
        return ref _entities.GetOrCreateRefAt(entity.Entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref EntityIndex GetEntityIndex(uint index)
    {
        return ref _entities.GetOrCreateRefAt(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsHandle GetHandleToPair(EcsHandle identifier, EcsHandle target)
    {
        var pair = new EcsHandle(identifier, target);
        if (GetEntityIndex(pair).Generation > 0)
        {
            return pair;
        }
        return CreatePair(identifier, target);
    }

    private EcsHandle CreatePair(EcsHandle identifier, EcsHandle target)
    {
        EcsHandle markerEntity;

        if (identifier.IsTagRelation || (!HandleIsComponent(identifier) && !HandleIsComponent(target)))
        {
            markerEntity = CreatePair(identifier, target, _entityArchetype);
        }
        else if (HandleIsComponent(identifier))
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(identifier);
        }
        else if (HandleIsComponent(target))
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(target);
        }
        else
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(identifier);
        }
        return markerEntity;
    }

    public EcsHandle GetKindHandle(EcsHandle pair)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(pair.Entity);
        return new EcsHandle(pair.Entity, entityIndex.Generation, _worldId, entityIndex.Flags);
    }

    public EcsHandle GetTargetHandle(EcsHandle pair)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(pair.Target);
        return new EcsHandle(pair.Target, entityIndex.Generation, _worldId, entityIndex.Flags);
    }

    public EcsHandle GetEntity(uint id)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(id);
        return new EcsHandle(id, entityIndex.Generation, _worldId, entityIndex.Flags);
    }

    public EcsHandle GetRelationWithIndefiniteTarget(EcsHandle handle)
    {
        return GetHandleToPair(GetKindHandle(handle), AnyEntity);
    }

    public EcsHandle GetRelationWithIndefiniteKind(EcsHandle handle)
    {
        return GetHandleToPair(AnyEntity, GetTargetHandle(handle));
    }

    public EcsHandle GetHandleToInstantiablePair(uint kind, uint target)
    {
        return GetHandleToPair(GetEntity(kind), GetEntity(target));
    }
}
