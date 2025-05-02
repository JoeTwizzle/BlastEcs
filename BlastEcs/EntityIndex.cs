namespace BlastEcs;

struct EntityIndex
{
    public short Generation;
    public Archetype Archetype;
    public int ArchetypeIndex;
    public byte Flags;

    public EntityIndex(short generation, Archetype archetype, int archetypeIndex, byte flags)
    {
        Generation = generation;
        Archetype = archetype;
        ArchetypeIndex = archetypeIndex;
        Flags = flags;
    }
}
