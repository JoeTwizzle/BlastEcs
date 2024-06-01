using BlastEcs.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private static byte s_worldCounter;
    private readonly byte worldId;
    public EcsWorld()
    {
        worldId = s_worldCounter++;
        _entities = new();
        _archetypes = new();
        _tables = new();
        _deadEntities = new();

        _archetypeMap = new();
        _tableMap = new();
        _typeMap = [];
        _archetypeTypeMap = new();

        //Reserve first index for empty entity
        _entities.Add();
        emptyEntity = new EcsHandle(0);

        //We need to manually register this component as we otherwise have a circular dependency
        uint componentEntity = GetNextEntityId();
        ref EntityIndex entityIndex = ref _entities[componentEntity];
        var gen = entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var entity = new EcsHandle(componentEntity, gen, worldId);
        _typeMap.Add(typeof(EcsComponent), entity);

        //The table for the archetype also needs to be created
        var key = new TypeCollectionKey([GetHandleToType<EcsComponent>().Id]);
        int tableId = tableCount++;
        _tableMap.Add(key, tableId);
        var table = new Table(tableId, [typeof(EcsComponent)], key);
        _tables.Add();
        _tables[tableId] = table;

        var arch = entityIndex.Archetype = _componentArchetype = CreateArchetype(key);
        var tableIndex = arch.Table.Add();
        int archIndex = entityIndex.ArchetypeIndex = arch.AddEntity(entity.Entity, tableIndex);

        _entityArchetype = CreateArchetype(new([]));
    }
}
