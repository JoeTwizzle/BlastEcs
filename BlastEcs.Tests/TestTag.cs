using BlastEcs.Builtin;

namespace BlastEcs.Tests;

public struct TestTag : ITag;
public struct TestTagRelation : ITagRelation
{
    public int Data;
}
