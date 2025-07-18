using BlastEcs.Builtin;
using BlastEcs.Helpers;
using System.Runtime.CompilerServices;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    private readonly Dictionary<RuntimeTypeHandle, EcsHandle> _typeRegistry;

    private bool HandleIsComponent(EcsHandle entity)
    {
        return GetEntityIndex(entity).Archetype.Has(_componentHandle);
    }

    private static bool TypeIsTag(Type type)
    {
        return typeof(ITag).IsAssignableFrom(type);
    }

    private static bool TypeIsTagRelation(Type type)
    {
        return typeof(ITagRelation).IsAssignableFrom(type);
    }

    private EcsHandle CreateHandle(RuntimeTypeHandle typeHandle)
    {
        EcsHandle markerEntity;
        byte flags = 0;
        var type = Type.GetTypeFromHandle(typeHandle)!;
        if (TypeIsTagRelation(type))
        {
            flags |= EntityFlags.IsTagRelation;
        }
        if (TypeIsTag(type))
        {
            markerEntity = CreateEntity(_entityArchetype, flags);
        }
        else
        {
            markerEntity = CreateEntity(_componentArchetype, flags);
            GetRef<EcsComponent>(markerEntity) = new(type);
        }

        _typeRegistry.Add(type.TypeHandle, markerEntity);
        return markerEntity;
    }

    //TODO: Test that this works
    private void RemoveHandle(RuntimeTypeHandle type)
    {
        var entity = _typeRegistry[type];
        DestroyEntity(entity);
        _typeRegistry.Remove(type);
    }

    public EcsHandle GetHandleToType(RuntimeTypeHandle type)
    {
        if (_typeRegistry.TryGetValue(type, out var handle))
        {
            return handle;
        }
        return CreateHandle(type);
    }

    public EcsHandle GetHandleToInstantiableType(RuntimeTypeHandle type)
    {
        if (type == typeof(Any))
        {
            ThrowHelper.ThrowArgumentException("The type \"Any\" is only valid for queries");
        }

        if (_typeRegistry.TryGetValue(type, out var handle))
        {
            return handle;
        }
        return CreateHandle(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsHandle GetHandleToType<T>() where T : struct
    {
        return GetHandleToType(typeof(T).TypeHandle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsHandle GetHandleToInstantiableType<T>() where T : struct
    {
        if (typeof(T) == typeof(Any))
        {
            ThrowHelper.ThrowArgumentException("The type \"Any\" is only valid for queries");
        }

        return GetHandleToType<T>();
    }
}
