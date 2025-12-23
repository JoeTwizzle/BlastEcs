using System.Numerics;

namespace BlastEcs.Collections;

public sealed class SparseSet<T>
{
    private int[] _sparse;
    private int[] _denseKeys;
    private T[] _denseValues;

    private int _count;

    public int Count => _count;
    public int Capacity => _sparse.Length;

    public SparseSet(int capacity = 16)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        _sparse = new int[capacity];
        _denseKeys = new int[capacity];
        _denseValues = new T[capacity];
    }

    public bool Contains(int key)
    {
        if ((uint)key >= (uint)_sparse.Length)
        {
            return false;
        }

        int index = _sparse[key];
        return index < _count && _denseKeys[index] == key;
    }

    public ref T Get(int key)
    {
        return ref _denseValues[_sparse[key]];
    }

    public ref T this[int key]
    {
        get => ref _denseValues[_sparse[key]];
    }

    public void Add(int key, T value)
    {
        EnsureCapacity(key + 1);

        int index = _sparse[key];

        if (index < _count && _denseKeys[index] == key)
        {
            _denseValues[index] = value;
            return;
        }

        int denseIndex = _count++;

        _sparse[key] = denseIndex;
        _denseKeys[denseIndex] = key;
        _denseValues[denseIndex] = value;
    }

    public bool Remove(int key)
    {
        if ((uint)key >= (uint)_sparse.Length)
        {
            return false;
        }

        int index = _sparse[key];

        if (index >= _count || _denseKeys[index] != key)
        {
            return false;
        }

        int lastIndex = --_count;
        int lastKey = _denseKeys[lastIndex];

        _denseKeys[index] = lastKey;
        _denseValues[index] = _denseValues[lastIndex];
        _sparse[lastKey] = index;

        return true;
    }

    public void Clear()
    {
        DenseValues.Clear();
        _count = 0;
    }

    public Span<T> DenseValues => new(_denseValues, 0, _count);

    public ReadOnlySpan<int> DenseKeys => new(_denseKeys, 0, _count);

    private void EnsureCapacity(int requiredKeyCapacity)
    {
        if (requiredKeyCapacity <= _sparse.Length)
        {
            return;
        }

        int newCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)requiredKeyCapacity);

        Array.Resize(ref _sparse, newCapacity);
        Array.Resize(ref _denseKeys, newCapacity);
        Array.Resize(ref _denseValues, newCapacity);
    }
}