using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Collections;

//TODO: Replace many operations with tensorprimitives class
public sealed class BitMask : IEquatable<BitMask>, IDisposable
{
    private ulong[] _bits;
    private int _count;

    public ReadOnlySpan<ulong> Bits
    {
        get
        {
            return _bits.AsSpan(0, _count);
        }
    }

    public BitMask()
    {
        _bits = ArrayPool<ulong>.Shared.Rent(1);
        Array.Clear(_bits);
        _count = 1;
    }

    public BitMask(BitMask componentMask)
    {
        _bits = ArrayPool<ulong>.Shared.Rent(componentMask._count);
        componentMask.Bits.CopyTo(_bits);
        _count = componentMask._count;
        _bits.AsSpan(_count, _bits.Length - _count).Clear();
    }

    public bool IsAllZeros()
    {
        return !Bits.ContainsAnyExcept(0ul);
    }

    public bool HasAnySet()
    {
        return Bits.ContainsAnyExcept(0ul);
    }

    public bool IsSet(int index)
    {
        int bitIndex = index >>> 6;
        if (bitIndex < _count)
        {
            int remainder = index & (63);
            return (_bits[bitIndex] & (1uL << remainder)) != 0;
        }
        return false;
    }

    public void SetBit(int index)
    {
        int bitIndex = index >>> 6;
        ResizeIfNeeded(bitIndex);
        int remainder = index & (63);
        _bits[bitIndex] |= (1uL << remainder);
    }

    public void ClearBit(int index)
    {
        int bitIndex = index >>> 6;
        int remainder = index & (63);
        if (_count > bitIndex)
        {
            _bits[bitIndex] &= ~(1uL << remainder);
        }
    }

    public void SetRange(int index, int count)
    {
        int start = index;
        int end = start + count;

        int startByteIndex = index >>> 6;
        int endByteIndex = end >>> 6;

        ResizeIfNeeded(endByteIndex);

        ulong mask = ulong.MaxValue >>> (64 - (start & 63)); //mask off bits in start long value
        _bits[startByteIndex] |= (mask << ((end - 1) & 63)); //shift mask to correct for starting bit offset
        int byteLength = endByteIndex - startByteIndex;
        if (byteLength > 0) //start and end long values are not the same
        {
            ulong mask2 = ulong.MaxValue >>> (64 - (end & (63))); //mask off bits in end long value
            _bits[endByteIndex] |= mask2;
            if (byteLength > 1) //fill middle between start end end long values
            {
                Array.Fill(_bits, ulong.MaxValue, startByteIndex + 1, byteLength - 1);
            }
        }
    }

    public void ClearRange(int index, int count)
    {
        int start = index;
        int end = start + count;

        int startByteIndex = index >>> 6;
        int endByteIndex = end >>> 6;

        ResizeIfNeeded(endByteIndex);

        ulong mask = ulong.MaxValue >>> (64 - (start & 63)); //mask off bits in start long value
        _bits[startByteIndex] &= ~(mask << ((end - 1) & 63)); //shift mask to correct for starting bit offset
        int byteLength = endByteIndex - startByteIndex;
        if (byteLength > 0) //start and end long values are not the same
        {
            ulong mask2 = ulong.MaxValue >>> (64 - (end & (63))); //mask off bits in end long value
            _bits[endByteIndex] &= ~mask2;
            if (byteLength > 1) //fill middle between start end end long values
            {
                Array.Fill(_bits, 0uL, startByteIndex + 1, byteLength - 1);
            }
        }
    }


    public void FlipBit(int index)
    {
        int bitIndex = index >>> 6;
        ResizeIfNeeded(bitIndex);
        int remainder = index & (63);
        _bits[bitIndex] ^= (1uL << remainder);
    }


    public void OrBits(BitMask mask)
    {
        ResizeIfNeeded(mask._count);
        TensorPrimitives.BitwiseOr(Bits, mask.Bits, _bits);
    }
    
    public void OrBits(ReadOnlySpan<ulong> mask)
    {
        ResizeIfNeeded(mask.Length);
        TensorPrimitives.BitwiseOr(Bits, mask, _bits);
    }


    public void OrFilteredBits(BitMask mask, BitMask filter)
    {
        int length = Math.Min(filter._count, mask._count);
        ResizeIfNeeded(length);

        for (int i = 0; i < length; i++)
        {
            _bits[i] |= (mask._bits[i] & filter._bits[i]);
        }
    }


    public void AndBits(BitMask mask)
    {
        TensorPrimitives.BitwiseAnd(Bits, mask.Bits, _bits);
    }

    public void AndBits(ReadOnlySpan<ulong> mask)
    {
        TensorPrimitives.BitwiseAnd(Bits, mask, _bits);
    }

    public void ClearBits(BitMask mask)
    {
        ResizeIfNeeded(mask._count);
        for (int i = 0; i < mask._count; i++)
        {
            _bits[i] &= ~mask._bits[i];
        }
    }


    public void ClearMatchingBits(BitMask mask, BitMask filter)
    {
        for (int i = 0; i < _count; i++)
        {
            _bits[i] &= ~(mask._bits[i] & filter._bits[i]);
        }
    }



    public void ClearAll()
    {
        Array.Clear(_bits); //Fill with all 0s
    }


    public void SetAll()
    {
        Array.Fill(_bits, ulong.MaxValue); //Fill with all 1s
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ResizeIfNeeded(int index)
    {
        if (_bits.Length <= index)
        {
            Resize(index);
        }
        _count = Math.Max(_count, index);
    }

    void Resize(int index)
    {
        var oldBits = _bits;
        _bits = ArrayPool<ulong>.Shared.Rent(index + 1);
        oldBits.AsSpan(0, _count).CopyTo(_bits.AsSpan(0, _count));
        ArrayPool<ulong>.Shared.Return(oldBits);
        _bits.AsSpan(_count, _bits.Length - _count).Clear();
    }

    /// <summary>
    /// Tests if all set bits of this ComponentMask match the other ComponentMask
    /// </summary>
    /// <param name="other"></param>
    /// <returns>true if all set bits of this ComponentMask match the other ComponentMask otherwise false</returns>

    public bool AllMatch(BitMask other)
    {
        if (other._count > _count)
        {
            return false;
        }
        for (int i = 0; i < _count; i++)
        {
            if ((_bits[i] & other._bits[i]) != _bits[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Tests if any set bits of this ComponentMask match the other ComponentMask
    /// </summary>
    /// <param name="other"></param>
    /// <returns>true if any set bits of this ComponentMask match the other ComponentMask otherwise false</returns>

    public bool AnyMatch(BitMask other)
    {
        int length = Math.Min(_count, other._count);
        for (int i = 0; i < length; i++)
        {
            if ((_bits[i] & other._bits[i]) != 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Tests if all bits of this ComponentMask match the other ComponentMask
    /// </summary>
    /// <param name="other"></param>
    /// <returns>true if all bits of this ComponentMask match the other ComponentMask otherwise false</returns>

    public bool EqualMatch(BitMask other)
    {
        return _bits.AsSpan().SequenceEqual(other._bits);
    }

    /// <summary>
    /// Tests if all bits of this ComponentMask match the other ComponentMask
    /// </summary>
    /// <param name="other"></param>
    /// <returns>true if all bits of this ComponentMask match the other ComponentMask otherwise false</returns>

    public bool EqualMatchExact(BitMask other)
    {
        int length = Math.Min(_count, other._count);
        for (int i = 0; i < length; i++)
        {
            if (_bits[i] != other._bits[i])
            {
                return false;
            }
        }
        for (int i = length; i < _count; i++)
        {
            if (_bits[i] != 0)
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is BitMask b && Equals(b);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < _bits.Length; i++)
            {
                hash = hash * 486187739 + (int)(_bits[i] & 0xffffffff);
                hash = hash * 486187739 + (int)(_bits[i] << 32);
            }
            return hash;
        }
    }

    public bool Equals(BitMask? other)
    {
        if (other == null)
        {
            return false;
        }
        if (_count != other._count)
        {
            return false;
        }
        return _bits.AsSpan().SequenceEqual(other._bits);
    }

    public override string ToString()
    {
        unchecked
        {
            if (_count <= 0)
            {
                return "0b: 0";
            }
            string agg = "0b: " + Convert.ToString((long)_bits[0], 2);
            for (int i = 1; i < _count; i++)
            {
                agg += Convert.ToString((long)_bits[i], 2);
            }
            return agg;
        }
    }

    public void Dispose()
    {
        ArrayPool<ulong>.Shared.Return(_bits);
    }

    public void Invert()
    {
        for (int i = 0; i < _count; i++)
        {
            _bits[i] = ~_bits[i];
        }
    }
}
