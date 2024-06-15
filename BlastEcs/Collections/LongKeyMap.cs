using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Collections;

public sealed class LongKeyMap<TValue>
{
    private struct Entry
    {
        public ulong Key;
        [AllowNull] public TValue Value;
        public int Next;
        public ulong HashCode;
    }

    private int[] _buckets;
    private Entry[] _entries;
    private int _count;
    private int _freeList;
    private int _freeCount;
    private const int StartOfFreeList = -3;

    public LongKeyMap(int capacity = 16)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        int size = HashHelpers.GetPrime(capacity);
        _buckets = new int[size];
        Array.Fill(_buckets, -1);
        _entries = new Entry[size];
        _freeList = -1;
    }

    public ref TValue this[ulong key]
    {
        get
        {
            int i = FindEntry(key);
            if (i >= 0) { return ref _entries[i].Value; }
            ThrowHelper.ThrowArgumentException("Key is not present in dictionary");
            return ref Unsafe.NullRef<TValue>();
        }
    }

    public int Count => _count - _freeCount;

    public void Add(ulong key, [AllowNull] TValue value)
    {
        Insert(key, value);
    }

    public bool Contains(ulong key)
    {
        return FindEntry(key) >= 0;
    }

    public bool TryGetValue(ulong key, [NotNullWhen(true)] out TValue? value)
    {
        int i = FindEntry(key);
        if (i >= 0)
        {
            value = _entries[i].Value!;
            return true;
        }
        value = default;
        return false;
    }

    public ref TValue GetValueRefOrAddDefault(ulong key, out bool existed)
    {
        int i = FindEntry(key);
        if (i < 0)
        {
            existed = false;
            i = Insert(key, default);
        }
        else
        {
            existed = true;
        }
        return ref _entries[i].Value!;
    }

    public ref TValue GetValueRefOrNullRef(ulong key, out bool existed)
    {
        int i = FindEntry(key);
        existed = i >= 0;
        if (existed)
        {
            return ref _entries[i].Value!;
        }
        else
        {
            return ref Unsafe.NullRef<TValue>();
        }
    }

    public bool Remove(ulong key)
    {
        ulong bucket = key % (ulong)_buckets.Length;
        int last = -1;
        for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].Next)
        {
            if (_entries[i].Key == key)
            {
                if (last < 0)
                {
                    _buckets[bucket] = _entries[i].Next;
                }
                else
                {
                    _entries[last].Next = _entries[i].Next;
                }

                _entries[i].Next = StartOfFreeList - _freeList;
                _entries[i].Key = 0;
                _entries[i].Value = default!;
                _freeList = i;
                _freeCount++;
                return true;
            }
        }
        return false;
    }

    private int Insert(ulong key, [AllowNull] TValue value)
    {
        ulong targetBucket = key % (ulong)_buckets.Length;

        //Find last free bucket
        for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
        {
            if (_entries[i].Key == key)
            {
                ThrowHelper.ThrowArgumentException("Adding duplicate key");
                return -1;
            }
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            _freeList = StartOfFreeList - _entries[index].Next;
            _freeCount--;
        }
        else
        {
            if (_count == _entries.Length)
            {
                Resize();
                targetBucket = key % (ulong)_buckets.Length;
            }
            index = _count;
            _count++;
        }

        ref Entry entry = ref _entries[index];

        entry.HashCode = key;
        entry.Next = _buckets[targetBucket];
        entry.Key = key;
        entry.Value = value;
        _buckets[targetBucket] = index;
        return index;
    }

    private void Resize()
    {
        int newSize = HashHelpers.ExpandPrime(_count);
        int[] newBuckets = new int[newSize];
        Array.Fill(newBuckets, -1);
        Entry[] newEntries = new Entry[newSize];
        Array.Copy(_entries, 0, newEntries, 0, _count);
        for (int i = 0; i < _count; i++)
        {
            if (newEntries[i].HashCode >= 0)
            {
                ulong bucket = newEntries[i].HashCode % (ulong)newSize;
                newEntries[i].Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
        }
        _buckets = newBuckets;
        _entries = newEntries;
    }

    private int FindEntry(ulong key)
    {
        for (int i = _buckets[key % (ulong)_buckets.Length]; i >= 0; i = _entries[i].Next)
        {
            if (_entries[i].Key == key)
            {
                return i;
            }
        }
        return -1;
    }

    public void Clear()
    {
        _buckets.AsSpan().Fill(-1);
        _entries.AsSpan().Clear();
        _freeList = -1;
    }

    public LongKeyMapEnumerator GetEnumerator()
    {
        return new LongKeyMapEnumerator(this);
    }

    public struct LongKeyMapEnumerator(LongKeyMap<TValue> map)
    {
        readonly LongKeyMap<TValue> _map = map;
        int i = -1;

        public readonly KeyValueRef<ulong, TValue> Current => new(_map._entries[i].Key, ref _map._entries[i].Value);
        public bool MoveNext()
        {
            i++;
            for (; i < _map._count;)
            {
                if (_map._entries[i].HashCode >= 0)
                {
                    return true;
                }
                i++;
            }
            return false;
        }
    }
}



internal static class HashHelpers
{
    private static ReadOnlySpan<int> Primes =>
        [
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239,
            293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
            4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293,
            36353, 43627, 52361, 62851, 75521, 90617, 108743, 130363, 156437, 187751, 225307,
            270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        ];

    public static int GetPrime(int min)
    {
        if (min < 0) throw new ArgumentException(null, nameof(min));

        foreach (int prime in Primes)
        {
            if (prime >= min) return prime;
        }

        for (int i = min | 1; i < int.MaxValue; i += 2)
        {
            if (IsPrime(i)) return i;
        }

        return min;
    }

    public static int ExpandPrime(int oldSize)
    {
        int newSize = 2 * oldSize;

        if ((uint)newSize > int.MaxValue && int.MaxValue > oldSize)
        {
            return int.MaxValue;
        }

        return GetPrime(newSize);
    }

    private static bool IsPrime(int candidate)
    {

        if ((candidate & 1) != 0)
        {
            int limit = (int)Math.Sqrt(candidate);
            for (int divisor = 3; divisor <= limit; divisor += 2)
            {
                if (candidate % divisor == 0) return false;
            }
            return true;
        }
        return candidate == 2;
    }
}