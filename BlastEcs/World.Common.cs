using BlastEcs.Builtin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    internal const int VariadicCount = 11;
    const int StackallocCount = 12;
    private static byte s_worldCounter;
    internal static EcsWorld[] s_Worlds = new EcsWorld[256];
    private readonly byte _worldId;
    internal const int AnyId = 2;
    public EcsHandle AnyEntity { get; }
    internal readonly EcsHandle _componentHandle;
    public EcsWorld()
    {
        _worldId = s_worldCounter++;
        _entities = new();
        _archetypes = new();
        _tables = new();
        _deadEntities = new();
        _archetypeMap = new();
        _tableMap = new();
        _typeMap = [];
        _pairTypeMap = new();
        _componentIndex = new();
        _deadArchetypes = new();
        //Reserve first index for empty entity
        emptyEntity = new EcsHandle(0, 0, _worldId);
        _entityArchetype = CreateArchetype(new([]));
        //We need to manually register this component as we otherwise have a circular dependency
        uint componentEntityId = GetNextEntityId();
        ref EntityIndex entityIndex = ref GetEntityIndex(componentEntityId);
        var gen = entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var componentEntity = _componentHandle = new EcsHandle(componentEntityId, gen, _worldId);
        _typeMap.Add(typeof(EcsComponent), componentEntity);

        //The table for the archetype also needs to be created
        var key = new TypeCollectionKey([componentEntity.Id]);
        int tableId = tableCount++;
        _tableMap.Add(key, tableId);
        var table = new Table(tableId, [typeof(EcsComponent)], key);
        _tables.Add();
        _tables[tableId] = table;

        var arch = entityIndex.Archetype = _componentArchetype = CreateArchetype(key);
        var tableIndex = arch.Table.Add();
        entityIndex.ArchetypeIndex = arch.AddEntity(componentEntity, tableIndex);

        //Register "Any" Special type
        AnyEntity = GetHandleToType<Any>();
        Debug.Assert(AnyEntity.Entity == AnyId);
        s_Worlds[_worldId] = this;
    }
}
