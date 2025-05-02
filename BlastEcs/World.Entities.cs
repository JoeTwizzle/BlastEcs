using BlastEcs.Builtin;
using BlastEcs.Collections;
using System.Buffers.Binary;
using System.Numerics;

namespace BlastEcs;
public sealed partial class EcsWorld
{
    private uint entityCount;
    readonly EcsHandle emptyEntity;
    readonly FastMap<EntityIndex> _entities;
    readonly GrowList<uint> _deadEntities;

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
            typeHandles[i] = GetHandleToInstantiableType(types[i]).Id;
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
        entityIndex.ArchetypeIndex = archetype.AddEntity(entity, tableIndex);
        OnEntityCreated?.Invoke(entity);
        return entity;
    }

    private EcsHandle CreatePair(EcsHandle kind, EcsHandle target, Archetype archetype)
    {
        var handle = new EcsHandle(kind, target);
        ref EntityIndex entityIndex = ref GetEntityIndex(handle);
        entityIndex.Archetype = archetype;
        entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var tableIndex = archetype.Table.Add();
        entityIndex.ArchetypeIndex = archetype.AddEntity(handle, tableIndex);
        return handle;
    }

    public void DestroyEntity(EcsHandle entity)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(entity);
        var arch = entityIndex.Archetype;
        var pair = arch.TableIndices[entityIndex.ArchetypeIndex];
        arch.RemoveEntityAt(entityIndex.ArchetypeIndex);
        arch.Table.RemoveAt(pair.tableIndex);
        entityIndex.Generation = (short)-entityIndex.Generation;

        //Recycle id if entity is not a pair
        if (!entity.IsPair)
        {
            _deadEntities.Add(entity.Entity);
        }
        RemoveRefrencesTo(entity);
        OnEntityDestroyed?.Invoke(entity);
    }

    private void RemoveRefrencesTo(EcsHandle handle)
    {
        //Get archetypes that contain this entity
        if (_componentIndex.TryGetValue(handle.Id, out var archetypes))
        {
            RemoveEntityFromArchetypes(handle, archetypes.ContainingArchetypes.Bits);
            _componentIndex.Remove(handle.Id);
        }
        if (!handle.IsPair)
        {
            if (_pairTypeMap.TryGetValue(handle.Entity, out var types))
            {
                var items = types.Items;
                foreach (var item in items)
                {
                    var pairA = GetHandleToType(GetEntity(handle.Entity), GetEntity(item));
                    if (_componentIndex.TryGetValue(pairA.Id, out var archetypess))
                    {
                        RemoveEntityFromArchetypes(pairA, archetypess.ContainingArchetypes.Bits);
                        _componentIndex.Remove(pairA.Id);
                    }
                    var pairB = GetHandleToType(GetEntity(item), GetEntity(handle.Entity));
                    if (_componentIndex.TryGetValue(pairB.Id, out var archetypesss))
                    {
                        RemoveEntityFromArchetypes(pairB, archetypesss.ContainingArchetypes.Bits);
                        _componentIndex.Remove(pairB.Id);
                    }
                }
            }
        }
    }

    private void RemoveEntityFromArchetypes(EcsHandle entity, Dictionary<int, int> archetypes)
    {
        foreach (var ids in archetypes.Keys)
        {
            var oldArch = _archetypes[ids];

            var newArch = GetArchetypeRemove(oldArch, entity);
            MoveAllEntities(oldArch, newArch);
            DestroyArchetype(oldArch);
        }
    }
    
    private void RemoveEntityFromArchetypes(EcsHandle entity, ReadOnlySpan<int> archetypes)
    {
        foreach (var ids in archetypes)
        {
            var oldArch = _archetypes[ids];
            var newArch = GetArchetypeRemove(oldArch, entity);
            MoveAllEntities(oldArch, newArch);
            DestroyArchetype(oldArch);
        }
    }
    
    private void RemoveEntityFromArchetypes(EcsHandle entity, ReadOnlySpan<ulong> archetypesMask)
    {
        for (int idx = 0; idx < archetypesMask.Length; idx++)
        {
            long bitItem = (long)archetypesMask[idx];
            while (bitItem != 0)
            {
                int id = idx * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(bitItem);
                bitItem ^= bitItem & -bitItem;

                var oldArch = _archetypes[id];
                var newArch = GetArchetypeRemove(oldArch, entity);
                MoveAllEntities(oldArch, newArch);
                DestroyArchetype(oldArch);
            }
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
        uint id = ++entityCount;
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

    private ref EntityIndex GetEntityIndex(EcsHandle entity)
    {
        if (entity.IsPair)
        {
            return ref _entities.GetOrCreateRefAt(entity.Id);
        }
        return ref _entities.GetOrCreateRefAt(entity.Entity);
    }

    private ref EntityIndex GetEntityIndex(uint index)
    {
        return ref _entities.GetOrCreateRefAt(index);
    }
}
