using BlastEcs.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    readonly Dictionary<Type, EcsHandle> _typeMap;

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

    //public EcsHandle GetHandleToPair(EcsHandle kindHandle, EcsHandle targetHandle)
    //{

    //    return 
    //}
}
