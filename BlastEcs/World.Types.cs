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

    private bool IsComponent(EcsHandle entity)
    {
        return GetEntityIndex(entity).Archetype.Has(_componentHandle);
    }

    private static bool IsTag(Type type)
    {
        return typeof(ITag).IsAssignableFrom(type);
    }

    private static bool IsTagRelation(Type type)
    {
        return typeof(ITagRelation).IsAssignableFrom(type);
    }

    private EcsHandle CreateHandle(Type type)
    {
        EcsHandle markerEntity;
        byte flags = 0;
        if (IsTagRelation(type))
        {
            flags |= EntityFlags.IsTagRelation;
        }
        if (IsTag(type))
        {
            markerEntity = CreateEntity(_entityArchetype, flags);
        }
        else
        {
            markerEntity = CreateEntity(_componentArchetype, flags);
            GetRef<EcsComponent>(markerEntity) = new(type);
        }

        _typeMap.Add(type, markerEntity);
        return markerEntity;
    }

    //TODO
    private void RemoveHandle(Type type)
    {
        var entity = _typeMap[type];
        DestroyEntity(entity);
        _typeMap.Remove(type);
    }

    private EcsHandle CreateHandle(EcsHandle identifier, EcsHandle target)
    {
        EcsHandle markerEntity;

        if (identifier.IsTagRelation || (!IsComponent(identifier) && !IsComponent(target)))
        {
            markerEntity = CreatePair(identifier, target, _entityArchetype);
        }
        else if (IsComponent(identifier))
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(identifier);
        }
        else if (IsComponent(target))
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(target);
        }
        else
        {
            markerEntity = CreatePair(identifier, target, _componentArchetype);
            GetRef<EcsComponent>(markerEntity) = GetRef<EcsComponent>(identifier);
        }
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

    public EcsHandle GetHandleToInstantiableType(Type type)
    {
        if (type == typeof(Any))
        {
            ThrowHelper.ThrowArgumentException("The type \"Any\" is only valid for queries");
        }

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

    public EcsHandle GetHandleToInstantiableType<T>() where T : struct
    {
        if (typeof(T) == typeof(Any))
        {
            ThrowHelper.ThrowArgumentException("The type \"Any\" is only valid for queries");
        }

        return GetHandleToType(typeof(T));
    }

    public EcsHandle GetHandleToType(EcsHandle identifier, EcsHandle target)
    {
        var pair = new EcsHandle(identifier, target); // TODO: fixme??
        if (_entities.GetOrCreateRefAt(pair.Id).Generation > 0)
        {
            return pair;
        }
        return CreateHandle(identifier, target);
    }

    public EcsHandle GetKindHandle(EcsHandle pair)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(pair.Entity);
        return new EcsHandle(pair.Entity, entityIndex.Generation, _worldId, entityIndex.Flags);
    }

    public EcsHandle GetTargetHandle(EcsHandle pair)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(pair.Target);
        return new EcsHandle(pair.Target, entityIndex.Generation, _worldId, entityIndex.Flags);
    }

    public EcsHandle GetEntity(uint id)
    {
        ref EntityIndex entityIndex = ref GetEntityIndex(id);
        return new EcsHandle(id, entityIndex.Generation, _worldId, entityIndex.Flags);
    }

    public EcsHandle GetRelationWithIndefiniteTarget(EcsHandle handle)
    {
        return GetHandleToType(GetKindHandle(handle), AnyEntity);
    }

    public EcsHandle GetRelationWithIndefiniteKind(EcsHandle handle)
    {
        return GetHandleToType(AnyEntity, GetTargetHandle(handle));
    }    
    
    public EcsHandle GetHandleToInstantiableType(uint kind, uint target)
    {
        return GetHandleToType(GetEntity(kind), GetEntity(target));
    }
}
