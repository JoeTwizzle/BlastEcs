using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Builtin;

public struct Relation
{
    public int Kind;
    public int Target;

    public Relation(int kind, int target)
    {
        Kind = kind;
        Target = target;
    }
}
