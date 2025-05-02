using BlastEcs.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Tests;

public struct TestTag : ITag;
public struct TestTagRelation : ITagRelation
{
    public int Data;
}
