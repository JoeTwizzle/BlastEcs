using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

internal static class EntityFlags
{
    public const int None = 0; 
    public const int IsPair = 1 << 0; 
    public const int IDK = 1 << 1; 
}
