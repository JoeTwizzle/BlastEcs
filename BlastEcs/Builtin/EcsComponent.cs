namespace BlastEcs.Builtin;

internal readonly struct EcsComponent
{
    public readonly Type ComponentType;

    public EcsComponent(Type componentType)
    {
        ComponentType = componentType;
    }
}
