using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Collections;

public sealed class SparseSet<TValue>
{
    int[] _indirect;
    TValue[] _values;
    int _denseCount;

    public int Count => _denseCount;

    public SparseSet()
    {
        _indirect = new int[4];
        _values = new TValue[4];
    }

    public void Add(int key, TValue value)
    {
        if (key >= _indirect.Length) { Array.Resize(ref _indirect, _indirect.Length * 2); }
        if (_indirect[key] != 0) ThrowHelper.ThrowArgumentException("Key already present in sparse set");

        _indirect[key] = ++_denseCount;

        if (_denseCount >= _values.Length) { Array.Resize(ref _values, _values.Length * 2); }
        _values[_denseCount] = value;
    }

    public void Remove(int key)
    {
        if (!Contains(key)) { return; }

        //get hole in dense array
        int denseIndex = _indirect[key];
        //remove from entry
        _indirect[key] = 0;
        if (denseIndex == _denseCount-1)
        {
            _values[denseIndex] = default!;
            return;
        }
        //override with last element
        _values[denseIndex] = _values[_denseCount - 1];
        // get key for previously last element
        int changedIndex = _indirect.AsSpan().IndexOf(_denseCount - 1);
        //update sparse pointer to new dense index
        _indirect[changedIndex] = denseIndex;
    }

    public bool Contains(int key)
    {
        if (key < 0 || key >= _indirect.Length) return false;

        return _indirect[key] != 0;
    }

    public ref TValue this[int index]
    {
        get
        {
            return ref _values[index];
        }
    }

    public Span<TValue> Values => _values.AsSpan(0, _denseCount);
    public ReadOnlySpan<int> RawKeys => _indirect;

    public SparseSetEnumerator GetEnumerator()
    {
        return new SparseSetEnumerator(this);
    }

    public void Clear()
    {
        _indirect.AsSpan().Clear();
        _values.AsSpan(0, _denseCount).Clear();
        _denseCount = 0;
    }

    public ref struct SparseSetEnumerator(SparseSet<TValue> set)
    {
        readonly SparseSet<TValue> _set = set;
        readonly ReadOnlySpan<int> _keys = set.RawKeys;
        int key = -1;

        public readonly KeyValueRef<int, TValue> Current
        {
            get
            {
                return new KeyValueRef<int, TValue>(key, ref _set.Values[_keys[key]]);
            }
        }

        public bool MoveNext()
        {
            key++;
            for (; key < _keys.Length;)
            {
                int denseIndex = _keys[key];
                if (denseIndex != 0)
                {
                    return true;
                }
                key++;
            }
            return false;
        }
    }
}

