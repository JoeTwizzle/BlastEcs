using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Diagnostics.CodeAnalysis;

namespace BlastEcs.Utils;

public static class SpanExtensions
{
    public static void MaskBits(this Span<ulong> span, [ConstantExpected] ulong maskValue)
    {
        var mask = Vector256.Create(maskValue);

        int vectorSize = Vector256<ulong>.Count; // Number of ulongs that fit in a Vector256<ulong>
        int i = 0;

        // Process vectors
        while (i <= span.Length - vectorSize)
        {
            // Load current vector from span
            var vector = Vector256.LoadUnsafe(ref span[i]);

            // Apply mask
            var maskedVector = Vector256.BitwiseAnd(vector, mask);

            // Store the result back
            maskedVector.StoreUnsafe(ref span[i]);

            i += vectorSize;
        }

        // Process remaining elements (tail)
        for (; i < span.Length; i++)
        {
            span[i] &= maskValue;
        }
    }
}
