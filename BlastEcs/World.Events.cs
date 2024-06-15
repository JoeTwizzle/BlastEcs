using BlastEcs.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class EcsWorld
{
    event Action<EcsHandle>? OnEntityDestroyed;
    event Action<EcsHandle>? OnEntityCreated;

    
    public void Subscribe<T0>()
    {
        
    }
}
