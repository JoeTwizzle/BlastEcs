namespace BlastEcs;

struct EntityIndex
{
    public short Generation;
    public Archetype Archetype;
    public int ArchetypeIndex;

    public EntityIndex(short generation, Archetype archetype, int archetypeIndex)
    {
        Generation = generation;
        Archetype = archetype;
        ArchetypeIndex = archetypeIndex;
    }
}
