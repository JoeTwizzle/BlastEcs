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
        uint id = GetNextEntityId();

        ref EntityIndex entityIndex = ref GetEntityIndex(id);
        var arch = entityIndex.Archetype = archetype;
        var gen = entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var entity = new EcsHandle(id, gen, _worldId, EntityFlags.None);
        var tableIndex = arch.Table.Add();
        entityIndex.ArchetypeIndex = arch.AddEntity(entity, tableIndex);
        OnEntityCreated?.Invoke(entity);
        return entity;
    }

    private EcsHandle CreatePair(EcsHandle kind, EcsHandle target, TypeCollectionKeyNoAlloc key)
    {
        var handle = new EcsHandle(kind, target);
        ref EntityIndex entityIndex = ref GetEntityIndex(handle);
        var arch = entityIndex.Archetype = GetArchetype(key);
        entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var tableIndex = arch.Table.Add();
        entityIndex.ArchetypeIndex = arch.AddEntity(handle, tableIndex);
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

    private void RemoveRefrencesTo(EcsHandle entity)
    {
        if (!entity.IsPair)
        {
            if (_archetypeTypeMap.TryGetValue(entity.Entity, out var archetypes))
            {
                RemoveEntityFromArchetypes(entity, archetypes);
                _archetypeTypeMap.Remove(entity.Entity);
            }
            if (_archetypePairMap.TryGetValue(entity.Entity, out var pairs))
            {
                foreach (var pair in pairs)
                {
                    RemoveRefrencesTo(new(pair));
                }
                _archetypePairMap.Remove(entity.Entity);
            }
        }
        else
        {
            //Get archetypes that contain this pair
            if (_archetypeTypeMap.TryGetValue(entity.Id, out var archetypes))
            {
                RemoveEntityFromArchetypes(entity, archetypes);
            }
            _archetypeTypeMap.Remove(entity.Id);
        }
    }

    private void RemoveEntityFromArchetypes(EcsHandle entity, GrowList<int> archetypes)
    {
        var list = archetypes.Span;
        for (int i = 0; i < list.Length; i++)
        {
            var oldArch = _archetypes[list[i]];
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
        uint id = ++entityCount;
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
