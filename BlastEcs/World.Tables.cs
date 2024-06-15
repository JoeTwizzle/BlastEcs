using BlastEcs.Builtin;
using BlastEcs.Collections;
using BlastEcs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private int tableCount;

    readonly GrowList<Table> _tables;
    readonly TypeCollectionMap<int> _tableMap;

    private Table CreateTable(TypeCollectionKey key)
    {
        int tableId = tableCount++;
        _tableMap.Add(key, tableId);

        var typeEntities = key.Types.Cast<ulong, EcsHandle>();
        Type[] types = new Type[typeEntities.Length];
        for (int i = 0; i < typeEntities.Length; i++)
        {
            //This is wrong for pairs          
            types[i] = GetRef<EcsComponent>(typeEntities[i]).ComponentType;
        }

        var table = new Table(tableId, types, key);
        _tables.Add();
        _tables[tableId] = table;

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
        Span<ulong> handles = components.Length <= 64 ? stackalloc ulong[64] : stackalloc ulong[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            if (IsComponent(components[i]))
            {
                handles[count++] = components[i].Id;
            }
        }
        handles = handles.Slice(0, count);

        if (count == components.Length)
        {
            return CreateTable(new(key));
        }
        else
        {
            var key2 = new TypeCollectionKeyNoAlloc(handles);
            if (_tableMap.TryGetValue(key2, out id))
            {
                return _tables[id];
            }
            return CreateTable(new(key2));
        }
    }

    private Table? TryGetTable(TypeCollectionKeyNoAlloc key)
    {
        if (_tableMap.TryGetValue(key, out int id))
        {
            return _tables[id];
        }
        return null;
    }
}
