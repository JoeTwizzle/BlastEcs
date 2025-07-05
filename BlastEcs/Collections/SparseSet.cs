using BlastEcs.Helpers;

namespace BlastEcs.Collections;

public sealed class SparseSet
{
    private int[] _sparse;   // Maps key to index in _dense; -1 = not present
    private uint[] _dense;    // Contains keys in insertion order
    private int _count;      // Number of elements

    public SparseSet(int initialSparseCapacity = 16, int initialDenseCapacity = 4)
    {
        _sparse = new int[initialSparseCapacity];
        Array.Fill(_sparse, -1);
        _dense = new uint[initialDenseCapacity];
        _count = 0;
    }

    public int Count => _count;

    public void Add(uint key)
    {
        EnsureSparseCapacity(key + 1);
        ref int index = ref _sparse[key];
        if (index != -1)
            return;  // Key already exists

        EnsureDenseCapacity();
        index = _count;
        _dense[_count] = key;
        _count++;
    }

    public bool Remove(uint key)
    {
        if (key >= (uint)_sparse.Length)
            return false;

        ref int index = ref _sparse[key];
        if (index == -1)
            return false;

        RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)_count)
        {
            ThrowHelper.ThrowArgumentException("Index was out of range.", nameof(index));
            return;
        }

        // Swap with last element
        uint lastKey = _dense[_count - 1];
        uint removedKey = _dense[index];

        _dense[index] = lastKey;

        // Update sparse for swapped key
        _sparse[lastKey] = index;
        _sparse[removedKey] = -1;

        _count--;
    }

    public bool Contains(uint key) =>
        key < (uint)_sparse.Length && _sparse[key] != -1;

    public uint GetKeyAt(int index) => _dense[index];

    public ReadOnlySpan<uint> GetDense() => new ReadOnlySpan<uint>(_dense, 0, _count);

    private void EnsureSparseCapacity(uint requiredCapacity)
    {
        uint currentLength = (uint)_sparse.Length;
        if (requiredCapacity <= currentLength)
            return;

        // Handle capacity overflow
        if (requiredCapacity > int.MaxValue)
        {
            ThrowHelper.ThrowArgumentException(
                $"Required capacity {requiredCapacity} exceeds maximum array size",
                nameof(requiredCapacity)
            );
        }

        int newCapacity = Math.Max((int)requiredCapacity, DoubledCapacity(currentLength));
        int[] newSparse = new int[newCapacity];
        Array.Copy(_sparse, newSparse, _sparse.Length);
        Array.Fill(newSparse, -1, _sparse.Length, newCapacity - _sparse.Length);
        _sparse = newSparse;
    }

    private static int DoubledCapacity(uint currentLength)
    {
        if (currentLength <= int.MaxValue / 2)
        {
            return (int)currentLength * 2;
        }
        else
        {
            return int.MaxValue;
        }
    }

    private void EnsureDenseCapacity()
    {
        if (_count < _dense.Length)
        {
            return;
        }

        int newCapacity = _dense.Length == 0 ? 4 : _dense.Length * 2;
        Array.Resize(ref _dense, newCapacity);
    }
}
