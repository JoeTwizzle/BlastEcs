using BlastEcs.Collections;

namespace BlastEcs;

readonly struct ComponentIndexInfo
{
    //public readonly Dictionary<int, int> ArchetypeMap;
    public readonly QuickMask ContainingArchetypes;

    public ComponentIndexInfo()
    {
        //ArchetypeMap = new();
        ContainingArchetypes = new();
    }
}
