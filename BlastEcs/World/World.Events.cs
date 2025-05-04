namespace BlastEcs;

public sealed partial class EcsWorld
{
    public event Action<EcsHandle>? OnEntityDestroyed;
    public event Action<EcsHandle>? OnEntityCreated;
    public bool EntityEventsEnabled { get; set; }

    public void Subscribe<T0>()
    {

    }
}
