using BlastEcs.Builtin;
using BlastEcs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    readonly Dictionary<Type, EcsHandle> _typeMap;
    readonly LongKeyMap<EcsHandle> _handleMap;

    private bool IsComponent(EcsHandle entity)
    {
        return GetEntityIndex(entity).Archetype == _componentArchetype;
    }

    private static bool IsTag(Type type)
    {
        return type.GetInterface("ITag") != null;
    }

    private EcsHandle CreateHandle(Type type)
    {
        EcsHandle markerEntity;
        if (IsTag(type))
        {
            markerEntity = CreateEntity();
        }
        else
        {
            markerEntity = CreateEntity([typeof(EcsComponent)]);
            GetRef<EcsComponent>(markerEntity) = new(type);
        }

        _typeMap.Add(type, markerEntity);
        return markerEntity;
    }

    private void RemoveHandle(Type type)
    {
        var entity = _typeMap[type];
        DestroyEntity(entity);
        _typeMap.Remove(type);
    }

    private void RemoveHandle(EcsHandle handle)
    {
        ref var pairEntity = ref _handleMap.GetValueRefOrNullRef(handle.Id, out var exists);
    }

    private EcsHandle CreateHandle(EcsHandle identifier, EcsHandle target)
    {
        EcsHandle markerEntity;

        if (!IsComponent(identifier) && !IsComponent(target))
        {
            markerEntity = CreatePair(identifier, target, _entityArchetype.Key);
        }
        else if (IsComponent(identifier))
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype.Key);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(identifier);
        }
        else if (IsComponent(target))
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype.Key);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(target);
        }
        else
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype.Key);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(identifier);
        }
        var pair = new EcsHandle(identifier, target);
        _handleMap.Add(pair.Id, markerEntity);
        return markerEntity;
    }

    public EcsHandle GetHandleToType(Type type)
    {
        if (_typeMap.TryGetValue(type, out var handle))
        {
            return handle;
        }
        return CreateHandle(type);
    }

    public EcsHandle GetHandleToType<T>() where T : struct
    {
        return GetHandleToType(typeof(T));
    }

    public EcsHandle GetHandleToType(EcsHandle identifier, EcsHandle target)
    {
        var pair = new EcsHandle(identifier, target);
        if (_handleMap.TryGetValue(pair.Id, out var handle))
        {
            return handle;
        }
        return CreateHandle(identifier, target);
    }
}
