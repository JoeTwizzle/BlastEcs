using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Builtin;

public readonly struct EcsComponent
{
    public readonly Type ComponentType;

    public EcsComponent(Type componentType)
    {
        ComponentType = componentType;
    }
}
