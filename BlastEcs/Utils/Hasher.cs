using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Utils;

public static class Hasher
{
    public static unsafe ulong Hash(Span<ulong> data)
    {
        unchecked
        {
            data.Sort();
            ulong hashCode = 17;
            for (int i = 0; i < data.Length; i++)
            {
                hashCode = hashCode * 486187739 + data[i];
            }
            return hashCode;
        }
    }
}
