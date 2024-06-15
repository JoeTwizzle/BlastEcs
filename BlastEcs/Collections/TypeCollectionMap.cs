using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Collections;

public sealed class TypeCollectionMap<TValue>
{
    private struct Entry
    {
        public TypeCollectionKey Key;
        public TValue Value;
        public int Next;
        public ulong HashCode;
    }

    private int[] _buckets;
    private Entry[] _entries;
    private int _count;
    private int _freeList;
    private int _freeCount;
    private const int StartOfFreeList = -3;

    public TypeCollectionMap(int capacity = 16)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        int size = HashHelpers.GetPrime(capacity);
        _buckets = new int[size];
        Array.Fill(_buckets, -1);
        _entries = new Entry[size];
        _freeList = -1;
    }

    public ref TValue this[TypeCollectionKeyNoAlloc key]
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

    public void Add(TypeCollectionKey key, TValue value)
    {
        Insert(key, value);
    }

    public ref TValue GetValueRefOrAddDefault(TypeCollectionKeyNoAlloc key, out bool existed)
    {
        int i = FindEntry(key);
        if (i < 0)
        {
            existed = false;
            i = Insert(new(key), default!);
        }
        else
        {
            existed = true;
        }
        return ref _entries[i].Value!;
    }

    public ref TValue GetValueRefOrNullRef(TypeCollectionKeyNoAlloc key, out bool existed)
    {
        int i = FindEntry(key);
        if (i < 0)
        {
            existed = false;
            return ref Unsafe.NullRef<TValue>();
        }
        else
        {
            existed = true;
            return ref _entries[i].Value!;
        }
    }

    public bool Contains(TypeCollectionKeyNoAlloc key)
    {
        return FindEntry(key) >= 0;
    }

    public bool TryGetValue(TypeCollectionKeyNoAlloc key, out TValue? value)
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

    public bool Remove(TypeCollectionKeyNoAlloc key)
    {
        ulong bucket = key.GetHash() % (ulong)_buckets.Length;
        int last = -1;
        for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].Next)
        {
            if (key == _entries[i].Key)
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
                _entries[i].Key = default;
                _entries[i].Value = default!;
                _freeList = i;
                _freeCount++;
                return true;
            }
        }
        return false;
    }

    private int Insert(TypeCollectionKey key, TValue value)
    {
        var rawHash = key.GetHash();
        ulong targetBucket = rawHash % (ulong)_buckets.Length;

        //Find last free bucket
        for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
        {
            if (key == _entries[i].Key)
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
                targetBucket = rawHash % (ulong)_buckets.Length;
            }
            index = _count;
            _count++;
        }

        ref Entry entry = ref _entries[index];

        entry.HashCode = rawHash;
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

    private int FindEntry(TypeCollectionKeyNoAlloc key)
    {
        for (int i = _buckets[key.GetHash() % (ulong)_buckets.Length]; i >= 0; i = _entries[i].Next)
        {
            if (key == _entries[i].Key)
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

    public TypeCollectionMapEnumerator GetEnumerator()
    {
        return new TypeCollectionMapEnumerator(this);
    }

    public struct TypeCollectionMapEnumerator(TypeCollectionMap<TValue> map)
    {
        readonly TypeCollectionMap<TValue> _map = map;
        int i = -1;

        public readonly KeyValueRef<TypeCollectionKey, TValue> Current => new(_map._entries[i].Key, ref _map._entries[i].Value);
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