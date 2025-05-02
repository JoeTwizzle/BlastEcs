using BlastEcs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

struct ComponentIndexInfo
{
    public Dictionary<int, int> ArchetypeMap;
    public BitMask ContainingArchetypes;

    public ComponentIndexInfo()
    {
        ArchetypeMap = new();
        ContainingArchetypes = new();
    }
}
