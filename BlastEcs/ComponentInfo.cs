namespace BlastEcs;

public readonly struct ComponentInfo
{
    public readonly EcsHandle MarkerComponent;
    public readonly Type Type;

    public ComponentInfo(EcsHandle markerComponent, Type type)
    {
        MarkerComponent = markerComponent;
        Type = type;
    }
}
