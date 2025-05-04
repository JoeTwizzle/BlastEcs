namespace BlastEcs.Utils;

public static class Hasher
{
    public static unsafe ulong Hash(ReadOnlySpan<ulong> data)
    {
        unchecked
        {
            ulong hashCode = 17;
            for (int i = 0; i < data.Length; i++)
            {
                hashCode = hashCode * 486187739 + data[i];
            }
            return hashCode;
        }
    }
}
