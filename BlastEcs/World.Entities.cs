using BlastEcs.Collections;
using System.Buffers.Binary;
using System.Numerics;

namespace BlastEcs;
struct EntityIndex
{
    public short Generation;
    public Archetype Archetype;
    public int ArchetypeIndex;

    public EntityIndex(short generation, Archetype archetype, int archetypeIndex)
    {
        Generation = generation;
        Archetype = archetype;
        ArchetypeIndex = archetypeIndex;
    }
}
public sealed partial class EcsWorld
{
    private uint entityCount;
    readonly EcsHandle emptyEntity;
    readonly GrowList<EntityIndex> _entities;
    readonly GrowList<uint> _deadEntities;

    public EcsHandle CreateEntity()
    {
        return CreateEntity([]);
    }

    public EcsHandle CreateEntity(ReadOnlySpan<Type> types)
    {
        var typeHandles = new ulong[types.Length];
        for (int i = 0; i < types.Length; i++)
        {
            typeHandles[i] = GetHandleToType(types[i]).Id;
        }
        return CreateEntity(new TypeCollectionKey(typeHandles));
    }

    public EcsHandle CreateEntity(TypeCollectionKey key)
    {
        uint id = GetNextEntityId();

        ref EntityIndex entityIndex = ref _entities[id];
        var arch = entityIndex.Archetype = GetArchetype(key);
        var gen = entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var entity = new EcsHandle(id, gen, worldId);
        var tableIndex = arch.Table.Add();
        entityIndex.ArchetypeIndex = arch.AddEntity(entity.Entity, tableIndex);
        return entity;
    }

    public void DestroyEntity(EcsHandle entity)
    {
        ref EntityIndex entityIndex = ref _entities[entity.Entity];
        var arch = entityIndex.Archetype;
        var pair = arch.TableIndices[entityIndex.ArchetypeIndex];
        arch.RemoveEntityAt(entityIndex.ArchetypeIndex);
        arch.Table.RemoveAt(pair.tableIndex);
        entityIndex.Generation = (short)-entityIndex.Generation;
        _deadEntities.Add(entity.Entity);
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
        _entities.Add();
        return id;
    }

    public bool IsAlive(EcsHandle entity)
    {
        uint index = entity.Entity;
        return index < _entities.Count && entity.Generation > 0 && GetEntityIndex(entity).Generation == entity.Generation;
    }

    private ref EntityIndex GetEntityIndex(EcsHandle entity)
    {
        return ref _entities[entity.Entity];
    }
}
