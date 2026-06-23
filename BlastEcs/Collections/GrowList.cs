namespace BlastEcs.Collections;

public sealed class GrowList<T>
{
    private T[] _array;
    private int _count;

    public int Count => _count;
    public int Capacity => _array.Length;
    public ReadOnlySpan<T> Span => _array.AsSpan(0, _count);

    public GrowList(int initialSize = 4)
    {
        _array = new T[initialSize];
    }

    public void Add(T value)
    {
        if (_count == _array.Length)
        {
            Resize();
        }
        _array[_count++] = value;
    }

    public void AddRange(ReadOnlySpan<T> value)
    {
        while (_count + value.Length > _array.Length)
        {
            Resize();
        }
        value.CopyTo(_array.AsSpan(_count));
        _count += value.Length;
    }

    private void Resize()
    {
        Array.Resize(ref _array, _array.Length * 2);
    }

    /// <summary>
    /// Set item at index to <c>default(T)</c>.
    /// </summary>
    /// <param name="index"></param>
    public void InvalidateAt(int index)
    {
        _array[index] = default!;
    }


    public void InvalidateRangeAt(int index, int length)
    {
        Array.Clear(_array, index, length);
    }

    /// <summary>
    /// Removes the item at <paramref name="index"/> and puts the last item of the array into the now empty spot, to keep the list dense.
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    /// <exception cref="InvalidOperationException">The list is empty</exception>
    public void RemoveAtDense(int index)
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty GrowList");
        }
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
        _count--;
        _array[index] = _array[_count];
    }

    public void RemoveRangeDense(int index, int count)
    {
        _count -= count;
        Array.Copy(_array, _count, _array, index, count);
    }

    public void RemoveRangeDenseOrdered(int index, int count)
    {
        Array.Copy(_array, index + count, _array, index, _count - (index + count));
        _count -= count;
    }

    /// <summary>
    /// Removes the item at <paramref name="index"/> and shifts all items after it by one, to keep the list dense.
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    /// <exception cref="InvalidOperationException">The list is empty</exception>
    public void RemoveAtDenseOrdered(int index)
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty GrowList");
        }
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
        _count--;
        if (index < _count)
        {
            Array.Copy(_array, index + 1, _array, index, _count - index);
        }
    }

    public void Clear()
    {
        _array.AsSpan().Clear();
    }

    public ref T this[int index]
    {
        get
        {
            return ref _array[index];
        }
    }

    public ref T this[uint index]
    {
        get
        {
            return ref _array[index];
        }
    }
}
