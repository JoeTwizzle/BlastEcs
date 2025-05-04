using BlastEcs.Collections;

namespace BlastEcs;

readonly struct ComponentIndexInfo : IDisposable
{
    public readonly Dictionary<int, int> ArchetypeMap;
    public readonly BitMask ContainingArchetypes;

    public ComponentIndexInfo()
    {
        ArchetypeMap = new();
        ContainingArchetypes = new();
    }

    public readonly void Dispose()
    {
        ContainingArchetypes.Dispose();
    }
}
