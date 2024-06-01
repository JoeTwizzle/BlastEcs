using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentException()
    {
        throw new ArgumentException();
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentException(string message)
    {
        throw new ArgumentException(message);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException()
    {
        throw new InvalidOperationException();
    }
}
