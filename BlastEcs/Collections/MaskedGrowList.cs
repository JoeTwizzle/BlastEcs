//using BlastEcs.Helpers;
//using System.Numerics;

//namespace BlastEcs.Collections;

//public sealed class MaskedGrowList<T>
//{
//    T[] _array;
//    ulong[] _mask;
//    int _count;

//    public int Count => _count;
//    public int Capacity => _array.Length;
//    public ReadOnlySpan<T> Span => _array.AsSpan(0, _count);
//    public ReadOnlySpan<ulong> OccupancyMask => _mask;

//    public MaskedGrowList(int initialSize = 4)
//    {
//        _array = new T[initialSize];
//        _mask = new ulong[1];
//    }

//    public void Add(T value)
//    {
//        if (_count == _array.Length)
//        {
//            Resize();
//        }
//        _count++;
//        for (int i = 0; i < _mask.Length; i++)
//        {
//            //There is a free slot here
//            if (_mask[i] != ulong.MaxValue)
//            {
//                int indexOfFirstZeroBit = BitOperations.TrailingZeroCount(~_mask[i]);
//                _mask[i] |= 1ul << indexOfFirstZeroBit;
//                _array[i * 64 + indexOfFirstZeroBit] = value;
//                break;
//            }
//        }
//    }

//    private void Resize()
//    {
//        int newSize = _array.Length * 2;
//        Array.Resize(ref _array, _array.Length * 2);
//        Array.Resize(ref _mask, (newSize + 63) / 64);
//    }

//    public void RemoveAt(int index)
//    {
//        if (_count == 0)
//        {
//            ThrowHelper.ThrowInvalidOperationException();
//        }
//        _count--;
//        _array[index] = _array[_count];
//        int maskIndex = _count / 64;
//        int remainder = _count & 5;
//        _mask[maskIndex] &= ~(1ul << remainder);
//    }

//    public void RemoveAtDense(int index)
//    {
//        if (_count == 0)
//        {
//            ThrowHelper.ThrowInvalidOperationException();
//        }
//        _count--;
//        _array[index] = _array[_count];
//        int maskIndex = _count / 64;
//        int remainder = _count & 5;
//        _mask[maskIndex] &= ~(1ul << remainder);
//    }

//    public void RemoveAtDenseOrdered(int index)
//    {
//        if (_count == 0)
//        {
//            ThrowHelper.ThrowInvalidOperationException();
//        }
//        _count--;
//        int maskIndex = _count / 64;
//        int remainder = _count & 5;
//        _mask[maskIndex] &= ~(1ul << remainder);
//        if (index < _count)
//        {
//            Array.Copy(_array, index + 1, _array, index, _count - index);
//        }
//    }

//    public ref T this[int index]
//    {
//        get
//        {
//            return ref _array[index];
//        }
//    }

//    public ref T this[uint index]
//    {
//        get
//        {
//            return ref _array[index];
//        }
//    }
//}
