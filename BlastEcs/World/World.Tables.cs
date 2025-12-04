using BlastEcs.Builtin;
using BlastEcs.Collections;
using BlastEcs.Utils;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private int tableCount;

    private readonly GrowList<Table> _tables;
    private readonly TypeCollectionMap<int> _tableMap;

    private Table CreateTable(TypeCollectionKey key)
    {
        int tableId = tableCount++;
        _tableMap.Add(key, tableId);

        var typeEntities = key.Types.Cast<ulong, EcsHandle>();
        Type[] types = new Type[typeEntities.Length];
        for (int i = 0; i < typeEntities.Length; i++)
        {
            types[i] = GetRef<EcsComponent>(typeEntities[i]).ComponentType;
        }

        var table = new Table(tableId, types, key);
        _tables.Add(table);

        return table;
    }

    private Table GetTable(TypeCollectionKeyNoAlloc key)
    {
        if (_tableMap.TryGetValue(key, out int id))
        {
            return _tables[id];
        }

        int count = 0;
        var components = key.Types.Cast<ulong, EcsHandle>();
        Span<ulong> handles = components.Length <= StackallocCount ? stackalloc ulong[StackallocCount] : stackalloc ulong[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            if (HandleIsComponent(components[i]))
            {
                handles[count++] = components[i].Id;
            }
        }
        handles = handles.Slice(0, count);

        if (count == components.Length)
        {
            return CreateTable(new TypeCollectionKey(key));
        }
        else
        {
            var key2 = new TypeCollectionKeyNoAlloc(handles);
            if (_tableMap.TryGetValue(key2, out id))
            {
                return _tables[id];
            }
            return CreateTable(new TypeCollectionKey(key2));
        }
    }

    private Table? GetTableOrNull(TypeCollectionKeyNoAlloc key)
    {
        if (_tableMap.TryGetValue(key, out int id))
        {
            return _tables[id];
        }
        return null;
    }
}
