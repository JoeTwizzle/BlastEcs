namespace BlastEcs;

internal static class EntityFlags
{
    public const int None = 0;
    public const int IsPair = 1 << 0;
    public const int IsTagRelation = 1 << 1;
}
