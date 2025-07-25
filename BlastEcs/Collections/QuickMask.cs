using System.Numerics;
using System.Runtime.CompilerServices;

namespace BlastEcs.Collections;


[InlineArray(QuickMask.QuickRangeLength)]
public struct QuickInlineArray<T>
{
    public T Value;
}

public sealed class QuickMask
{
    public const int QuickRangeLength = 4;

    internal QuickInlineArray<ulong> QuickBits;
    internal HashSet<int>? Mask;

    public void Add(int key)
    {
        if (key >= 64 * QuickRangeLength)
        {
            if (Mask == null)
            {
                Mask = new(1);
            }
            Mask.Add(key);
        }
        else
        {
            int bitIndex = key >>> 6;
            int remainder = key & (63);
            QuickBits[bitIndex] |= (1uL << remainder);
        }
    }

    public bool Has(int key)
    {
        if (key >= 64 * QuickRangeLength)
        {
            if (Mask == null)
            {
                Mask = new(1);
            }
            return Mask.Contains(key);
        }
        else
        {
            int bitIndex = key >>> 6;
            int remainder = key & (63);
            return (QuickBits[bitIndex] & (1uL << remainder)) != 0;
        }
    }

    public void Remove(int key)
    {
        if (key >= 64 * QuickRangeLength)
        {
            if (Mask == null)
            {
                Mask = new(1);
            }
            Mask.Remove(key);
        }
        else
        {
            int bitIndex = key >>> 6;
            int remainder = key & (63);
            QuickBits[bitIndex] &= ~(1uL << remainder);
        }
    }

    public void Clear()
    {
        if (Mask == null)
        {
            Mask = new(1);
        }
        Mask.Clear();
        new Span<ulong>(ref QuickBits.Value).Clear();
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    public struct Enumerator
    {
        private readonly QuickMask _source;
        private HashSet<int>.Enumerator _hashEnumerator;
        private ulong _currentChunk;
        private int _chunkIndex;
        private int _phase;

        internal Enumerator(QuickMask source)
        {
            _source = source;
            _hashEnumerator = source.Mask?.GetEnumerator() ?? default;
            _currentChunk = 0;
            _chunkIndex = 0;
            _phase = 0; // 0=not started, 1=quick bits, 2=hash set, 3=ended
        }

        public int Current => _phase switch
        {
            1 => (_chunkIndex << 6) | BitOperations.TrailingZeroCount(_currentChunk),
            2 => _hashEnumerator.Current,
            _ => throw new InvalidOperationException("Enumeration has not started or has finished.")
        };

        public bool MoveNext()
        {
            switch (_phase)
            {
                case 0: // Start enumeration
                    _phase = 1;
                    _chunkIndex = 0;
                    _currentChunk = _source.QuickBits[0];
                    goto case 1;

                case 1: // QuickBits phase
                    while (_chunkIndex < QuickRangeLength)
                    {
                        if (_currentChunk != 0)
                        {
                            // Isolate the lowest set bit and clear it for next iteration
                            _currentChunk &= _currentChunk - 1;
                            return true;
                        }

                        if (++_chunkIndex >= QuickRangeLength)
                            break;

                        _currentChunk = _source.QuickBits[_chunkIndex];
                    }
                    _phase = 2;
                    goto case 2;

                case 2: // HashSet phase
                    if (_hashEnumerator.MoveNext())
                        return true;
                    _phase = 3;
                    break;
            }
            return false;
        }
    }
}
