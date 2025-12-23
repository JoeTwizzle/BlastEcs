using BlastEcs.Builtin;

namespace BlastEcs;

public readonly partial struct EcsHandle
{
    public EcsWorld World => EcsWorld.s_Worlds[WorldId];

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public bool Has<T0>() where T0 : struct
    {
        return World.Has<T0>(this);
    }

    public bool Has(EcsHandle identifier, EcsHandle target)
    {
        return World.Has(this, identifier, target);
    }

    public bool Has<TKind>(EcsHandle target) where TKind : struct
    {
        return World.Has<TKind>(this, target);
    }

    public bool HasRelation<TKind, TTarget>() where TKind : struct where TTarget : struct
    {
        return World.HasRelation<TKind, TTarget>(this);
    }

    public ref T GetRef<T>() where T : struct
    {
        return ref World.GetRef<T>(this);
    }

    public ref T GetRef<T>(EcsHandle target) where T : struct
    {
        return ref World.GetRef<T>(this, target);
    }

    public ref T TryGetRef<T>(out bool exists) where T : struct
    {
        return ref World.TryGetRef<T>(this, out exists);
    }

    public ref T TryGetRef<T>(EcsHandle target, out bool exists) where T : struct
    {
        return ref World.TryGetRef<T>(this, target, out exists);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>() where T0 : struct
    {
        World.Add<T0>(this);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>(T0 value_T0) where T0 : struct
    {
        World.Add(this, value_T0);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Remove<T0>() where T0 : struct
    {
        World.Remove<T0>(this);
    }

    public void AddPair<TKind, TTarget>()
        where TKind : struct
        where TTarget : struct
    {
        World.AddPair<TKind, TTarget>(this);
    }

    public void AddRelation<TKind>(EcsHandle target)
    where TKind : struct
    {
        World.AddRelation<TKind>(this, target);
    }

    public void AddRelation(EcsHandle identifier, EcsHandle target)
    {
        World.AddRelation(this, identifier, target);
    }

    public void RemovePair<TKind, TTarget>()
        where TKind : struct
        where TTarget : struct
    {
        World.RemovePair<TKind, TTarget>(this);
    }

    public void RemoveRelation<TKind>(EcsHandle target)
        where TKind : struct
    {
        World.RemoveRelation<TKind>(this, target);
    }

    public void RemoveRelation(EcsHandle identifier, EcsHandle target)
    {
        World.RemoveRelation(this, identifier, target);
    }
}
