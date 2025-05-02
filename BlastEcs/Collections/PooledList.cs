using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Collections;

public sealed class PooledList<T> : IDisposable where T : IEquatable<T>
{
    int count;
    private T[] _items;

    public Span<T> Items => _items.AsSpan(0, count);

    public PooledList(int capacity = 4)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
        _items = ArrayPool<T>.Shared.Rent(capacity);
    }

    private void Resize(int capacity)
    {
        var oldItems = _items;
        _items = ArrayPool<T>.Shared.Rent(capacity);
        oldItems.AsSpan(0, count).CopyTo(_items);
        ArrayPool<T>.Shared.Return(oldItems, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    public void Add(T item)
    {
        if (count >= _items.Length)
        {
            Resize(_items.Length << 1);
        }
        _items[count++] = item;
    }

    public void AddUnique(T item)
    {
#if DEBUG
        if (Items.Contains(item))
        {
            ThrowHelper.ThrowArgumentException("Cannot add duplicate item to pooled list");
        }
#endif
        if (count >= _items.Length)
        {
            Resize(_items.Length << 1);
        }
        _items[count++] = item;
    }

    public void AddRange(ReadOnlySpan<T> items)
    {
        if (count + items.Length >= _items.Length)
        {
            var size = BitOperations.RoundUpToPowerOf2((nuint)(count + items.Length));
            Resize((int)size);
        }
        items.CopyTo(_items.AsSpan(count));
        count += items.Length;
    }

    public void RemoveAt(int index)
    {
        _items[index] = _items[--count];
        //We have 4x as much space as items are used
        if (_items.Length << 2 > count)
        {
            //Reserve only 2x as much space as used
            Resize(_items.Length << 1);
        }
    }

    //public void RemoveAtStable(int index)
    //{
    //    if (index < count)
    //    {
    //        _items.AsSpan(index + 1).CopyTo(_items.AsSpan(index));
    //    }
    //}

    public void Dispose()
    {
        ArrayPool<T>.Shared.Return(_items, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }

    public void UnionRange(ReadOnlySpan<T> span)
    {
        var items = Items;
        for (int i = 0; i < items.Length; i++)
        {
            if (!span.Contains(items[i]))
            {
                RemoveAt(i);
                i--;
            }
        }
    }

    public void ExcludeRange(ReadOnlySpan<T> span)
    {
        var items = Items;
        for (int i = 0; i < items.Length; i++)
        {
            if (span.Contains(items[i]))
            {
                RemoveAt(i);
                i--;
            }
        }
    }
}
