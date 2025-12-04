namespace BlastEcs;

public static class EntityFlags
{
    public const int None = 0;
    public const int IsPair = 1 << 0;
    public const int IsTag = 1 << 1;
    public const int IsTagRelation = 1 << 2;


    public const ulong NoneBit = (0ul << 0) << 56;
    public const ulong IsPairBit = (1ul << 0) << 56;
    public const ulong IsTagBit = (1ul << 1) << 56;
    public const ulong IsTagRelationBit = (1ul << 2) << 56;
}
