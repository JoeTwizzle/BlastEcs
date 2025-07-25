using BlastEcs.Builtin;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    [Variadic(nameof(T0), VariadicCount)]
    public bool Has<T0>(EcsHandle entity) where T0 : struct
    {
        var arch = GetEntityIndex(entity).Archetype;
        // [Variadic: CopyLines()]
        if (!arch.Has(GetHandleToType<T0>())) { return false; }
        return true;
    }

    public bool Has(EcsHandle entity, EcsHandle kind, EcsHandle target)
    {
        return GetEntityIndex(entity).Archetype.Has(new EcsHandle(kind, target));
    }

    public bool Has<TKind>(EcsHandle entity, EcsHandle target) where TKind : struct
    {
        EcsHandle kindHandle = GetHandleToType<TKind>();
        return Has(entity, kindHandle, target);
    }

    public bool HasRelation<TKind, TTarget>(EcsHandle entity) where TKind : struct where TTarget : struct
    {
        EcsHandle targetHandle = GetHandleToType<TTarget>();
        return Has<TKind>(entity, targetHandle);
    }

    public ref T GetRef<T>(EcsHandle entity) where T : struct
    {
        Debug.Assert(Has<T>(entity));
        ref var entityIndex = ref GetEntityIndex(entity);
        int tableIndex = entityIndex.TableSlotIndex;
        return ref entityIndex.Archetype.Table.GetRefAt<T>(tableIndex, GetHandleToType<T>().Id);
    }

    public ref T GetRef<T>(EcsHandle entity, EcsHandle target) where T : struct
    {
        Debug.Assert(Has<T>(entity, target));
        ref var entityIndex = ref GetEntityIndex(entity);
        int tableIndex = entityIndex.TableSlotIndex;
        return ref entityIndex.Archetype.Table.GetRefAt<T>(tableIndex, GetHandleToPair(GetHandleToType<T>(), target).Id);
    }

    public ref T TryGetRef<T>(EcsHandle entity, out bool exists) where T : struct
    {
        EntityIndex entityIndex = GetEntityIndex(entity);
        EcsHandle componentHandle = GetHandleToType<T>();
        exists = entityIndex.Archetype.Table.Key.HasType(componentHandle);
        if (exists)
        {
            int tableIndex = entityIndex.TableSlotIndex;
            return ref entityIndex.Archetype.Table.GetRefAt<T>(tableIndex, GetHandleToType<T>().Id);
        }
        return ref Unsafe.NullRef<T>();
    }

    public ref T TryGetRef<T>(EcsHandle entity, EcsHandle target, out bool exists) where T : struct
    {
        EntityIndex entityIndex = GetEntityIndex(entity);
        EcsHandle componentHandle = GetHandleToType<T>();
        exists = entityIndex.Archetype.Table.Key.HasType(componentHandle);
        if (exists)
        {
            int tableIndex = entityIndex.TableSlotIndex;
            return ref entityIndex.Archetype.Table.GetRefAt<T>(tableIndex, GetHandleToPair(GetHandleToType<T>(), target).Id);
        }
        return ref Unsafe.NullRef<T>();
    }

    [Variadic(nameof(T0), VariadicCount)]
    public void Add<T0>(EcsHandle entity) where T0 : struct
    {
        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeAdd<T0>(src);
        MoveEntity(entity, src, dest);
    }

    [Variadic(nameof(T0), VariadicCount)]
    public void Remove<T0>(EcsHandle entity) where T0 : struct
    {
        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeRemove<T0>(src);
        MoveEntity(entity, src, dest);
    }

    public void AddPair<TKind, TTarget>(EcsHandle entity)
        where TKind : struct
        where TTarget : struct
    {
        var kindHandle = GetHandleToType<TKind>();
        var targetHandle = GetHandleToType<TTarget>();

        var componentHandle = GetHandleToPair(kindHandle, targetHandle);

        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeAdd(src, componentHandle);
        MoveEntity(entity, src, dest);
    }

    public void AddRelation<TKind>(EcsHandle entity, EcsHandle target)
    where TKind : struct
    {
        var kindHandle = GetHandleToType<TKind>();

        AddRelation(entity, kindHandle, target);
    }

    public void AddRelation(EcsHandle entity, EcsHandle kind, EcsHandle target)
    {
        var componentHandle = GetHandleToPair(kind, target);

        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeAdd(src, componentHandle);
        MoveEntity(entity, src, dest);
    }

    public void RemovePair<TKind, TTarget>(EcsHandle entity)
        where TKind : struct
        where TTarget : struct
    {
        var kindHandle = GetHandleToType<TKind>();
        var targetHandle = GetHandleToType<TTarget>();

        var componentHandle = GetHandleToPair(kindHandle, targetHandle);

        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeRemove(src, componentHandle);
        MoveEntity(entity, src, dest);
    }

    public void RemoveRelation<TKind>(EcsHandle entity, EcsHandle target)
        where TKind : struct
    {
        var kindHandle = GetHandleToType<TKind>();

        RemoveRelation(entity, kindHandle, target);
    }

    public void RemoveRelation(EcsHandle entity, EcsHandle kind, EcsHandle target)
    {
        var componentHandle = GetHandleToPair(kind, target);

        var src = GetEntityIndex(entity).Archetype;
        var dest = GetArchetypeRemove(src, componentHandle);
        MoveEntity(entity, src, dest);
    }
}
