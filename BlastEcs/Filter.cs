using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed class Filter
{
    readonly HashSet<ulong> include = new();
    readonly HashSet<ulong> exclude = new();


}
