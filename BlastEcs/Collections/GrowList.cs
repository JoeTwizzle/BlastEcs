namespace BlastEcs.Collections;

public sealed class GrowList<T>
{
    T[] _array;

    int _count;

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

    public void InvalidateAt(int index)
    {
        _array[index] = default!;
    }


    public void InvalidateRangeAt(int index, int length)
    {
        Array.Clear(_array, index, length);
    }

    public void RemoveAtDense(int index)
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty GrowList");
        }
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

    public void RemoveAtDenseOrdered(int index)
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty GrowList");
        }
        _count--;
        if (index < _count)
        {
            Array.Copy(_array, index + 1, _array, index, _count - index);
        }
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
