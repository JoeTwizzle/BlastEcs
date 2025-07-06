namespace BlastEcs;

struct EntityIndex
{
    public Archetype Archetype;
    public byte Flags;
    public short Generation;
    public int ArchetypeSlotIndex;
    public int TableSlotIndex;
}
