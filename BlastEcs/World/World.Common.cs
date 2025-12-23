using BlastEcs.Builtin;
using System.Diagnostics;

namespace BlastEcs;

public sealed partial class EcsWorld : IDisposable
{
    internal const int VariadicCount = 11;
    const int StackallocCount = 12;
    private static byte s_worldCounter;
    internal static EcsWorld[] s_Worlds = new EcsWorld[256];
    private readonly byte _worldId;
    internal const int AnyId = 2;
    public EcsHandle AnyEntity { get; }
    internal readonly EcsHandle _componentHandle;
    public EcsWorld(int anticipatedEntityCount = 4196)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(anticipatedEntityCount, nameof(anticipatedEntityCount));
        _worldId = s_worldCounter++;
        _entities = new((ulong)anticipatedEntityCount);
        _archetypes = new();
        _tables = new();
        _deadEntities = new();
        _archetypeMap = new();
        _tableMap = new();
        _typeRegistry = [];
        _componentIndex = [];
        _deadArchetypes = new();
        //Reserve first index for empty entity
        _emptyEntity = new EcsHandle(0, 0, _worldId);
        _emptyArchetype = CreateArchetype(new([]));
        //We need to manually register this component as we otherwise have a circular dependency
        uint componentEntityId = GetNextEntityId();
        ref EntityIndex entityIndex = ref GetEntityIndex(componentEntityId);
        var gen = entityIndex.Generation = (short)((-entityIndex.Generation) + 1);
        var componentEntity = _componentHandle = new EcsHandle(componentEntityId, gen, _worldId);
        _typeRegistry.Add(typeof(EcsComponent).TypeHandle, componentEntity);

        //The table for the archetype also needs to be created
        var key = new TypeCollectionKey([componentEntity.Id]);
        int tableId = tableCount++;
        _tableMap.Add(key, tableId);
        var table = new Table(tableId, [typeof(EcsComponent)], key);
        _tables.Add(table);

        var arch = entityIndex.Archetype = _componentArchetype = CreateArchetype(key);
        entityIndex.TableSlotIndex = arch.Table.AddEntity(componentEntity);
        entityIndex.ArchetypeSlotIndex = arch.AddEntity(componentEntity);

        //Register "Any" Special type
        AnyEntity = GetHandleToType<Any>();
        Debug.Assert(AnyEntity.Entity == AnyId);
        s_Worlds[_worldId] = this;
    }

    public void Dispose()
    {
        s_Worlds[_worldId] = null!;
    }
}
