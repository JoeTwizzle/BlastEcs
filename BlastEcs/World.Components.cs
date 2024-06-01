using BlastEcs.Builtin;
using System.Runtime.CompilerServices;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private bool IsComponent(EcsHandle entity)
    {
        return GetEntityIndex(entity).Archetype == _componentArchetype;
    }

    public bool Has<T>(EcsHandle entity) where T : struct
    {
        EcsHandle componentHandle = GetHandleToType<T>();
        return GetEntityIndex(entity).Archetype.Has(componentHandle);
    }

    public ref T GetRef<T>(EcsHandle entity) where T : struct
    {
        ref var entityIndex = ref GetEntityIndex(entity);
        int tableIndex = entityIndex.Archetype.TableIndices[entityIndex.ArchetypeIndex].tableIndex;
        return ref entityIndex.Archetype.Table.GetRefAt<T>(tableIndex);
    }

    public ref T TryGetRef<T>(EcsHandle entity, out bool exists) where T : struct
    {
        EntityIndex entityIndex = GetEntityIndex(entity);
        EcsHandle componentHandle = GetHandleToType<T>();
        exists = entityIndex.Archetype.Has(componentHandle);
        if (exists)
        {
            int tableIndex = entityIndex.Archetype.TableIndices[entityIndex.ArchetypeIndex].tableIndex;
            return ref entityIndex.Archetype.Table.GetRefAt<T>(tableIndex);
        }
        return ref Unsafe.NullRef<T>();
    }

    public void Add<T>(EcsHandle entity) where T : struct
    {
        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeAdd<T>(src);
        MoveEntity(entity, src, dest);
    }

    public void Remove<T>(EcsHandle entity) where T : struct
    {
        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeRemove<T>(src);
        MoveEntity(entity, src, dest);
    }

    public void Pair<TKind, TTarget>(EcsHandle entity)
        where TKind : struct
        where TTarget : struct
    {
        Type? storedDatatype;
        if (!IsTag(typeof(TKind)))
        {
            storedDatatype = typeof(TKind);
        }
        else if (!IsTag(typeof(TTarget)))
        {
            storedDatatype = typeof(TTarget);
        }

        var kindHandle = GetHandleToType<TKind>();
        var targetHandle = GetHandleToType<TTarget>();


    }
}
