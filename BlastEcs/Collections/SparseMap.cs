using System.Runtime.CompilerServices;

namespace BlastEcs.Collections;

public sealed class SparseMap<T>
{
    private int[] _sparse;   // Maps key to index in _dense/_values; -1 = not present
    private uint[] _dense;    // Contains keys in insertion order (parallel to _values)
    private T[] _values;     // Contains values (parallel to _dense)
    private int _count;      // Number of elements
    public int Count => _count;

    public SparseMap(int initialSparseCapacity = 16, int initialValuesCapacity = 4)
    {
        _sparse = new int[initialSparseCapacity];
        Array.Fill(_sparse, -1);
        _dense = new uint[initialValuesCapacity];
        _values = new T[initialValuesCapacity];
        _count = 0;
    }

    public void Add(uint key, T value)
    {
        EnsureSparseCapacity(key + 1);
        ref int index = ref _sparse[key];
        if (index != -1)
        {
            _values[index] = value;
            return;
        }

        EnsureDenseCapacity();
        index = _count;
        _dense[_count] = key;
        _values[_count] = value;
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
            throw new ArgumentException("Index was out of range.", nameof(index));
        }

        // Swap with last element
        uint lastKey = _dense[_count - 1];
        uint removedKey = _dense[index];

        _dense[index] = lastKey;
        _values[index] = _values[_count - 1];

        // Update sparse for swapped key
        _sparse[lastKey] = index;
        _sparse[removedKey] = -1;

        _count--;
    }

    public bool Contains(uint key) =>
        key < (uint)_sparse.Length && _sparse[key] != -1;

    public bool TryGetValue(uint key, out T value)
    {
        if (key < (uint)_sparse.Length)
        {
            int index = _sparse[key];
            if (index != -1)
            {
                value = _values[index];
                return true;
            }
        }
        value = default!;
        return false;
    }

    public ref T this[uint key]
    {
        get
        {
            if (key >= (uint)_sparse.Length)
            {
                ThrowKeyNotFound();
            }

            ref int index = ref _sparse[key];
            if (index == -1)
                ThrowKeyNotFound();
            return ref _values[index];
        }
    }

    public ref T GetValueOrAddDefault(uint key)
    {
        EnsureSparseCapacity(key + 1);
        ref int index = ref _sparse[key];
        if (index == -1)
        {
            EnsureDenseCapacity();
            index = _count;
            _dense[_count] = key;
            _values[_count] = default!;
            _count++;
        }
        return ref _values[index];
    }

    public ref T GetValueOrNullRef(uint key)
    {
        if (key < (uint)_sparse.Length)
        {
            int index = _sparse[key];
            if (index != -1)
                return ref _values[index];
        }
        return ref Unsafe.NullRef<T>();
    }

    private void EnsureSparseCapacity(uint requiredCapacity)
    {
        uint currentLength = (uint)_sparse.Length;
        if (requiredCapacity <= currentLength)
            return;

        // Handle capacity overflow
        if (requiredCapacity > int.MaxValue)
        {
            throw new ArgumentException(
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
        return (currentLength <= int.MaxValue / 2)
            ? (int)currentLength * 2
            : int.MaxValue;
    }

    private void EnsureDenseCapacity()
    {
        if (_count < _dense.Length)
            return;

        int newCapacity = _dense.Length == 0 ? 4 : _dense.Length * 2;
        Array.Resize(ref _dense, newCapacity);
        Array.Resize(ref _values, newCapacity);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowKeyNotFound() => throw new KeyNotFoundException();
}