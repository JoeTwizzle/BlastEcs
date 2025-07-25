using BlastEcs.Collections;
using BlastEcs.Helpers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs;

/// <summary>
/// A table contains entities that share the same component types
/// </summary>
public sealed class Table : IEquatable<Table>
{
    public TypeCollectionKey Key => _key;
    //public Edges<Table> Edges => _edges;
    public int Count => _count;
    public int Capacity => _capacity;
    public int Id => _id;

    internal LongKeyMap<int> TypeIndices => _typeIndices;

    private readonly int _id;
    private readonly TypeCollectionKey _key;
    private readonly Array[] _components;
    private readonly LongKeyMap<int> _typeIndices;
    //private readonly Edges<Table> _edges;
    private int _count;
    private int _capacity;

    internal Table(int id, Type[] componentTypes, TypeCollectionKey key, int initialCapacity = 4)
    {
        _id = id;
        _key = key;
        _components = new Array[componentTypes.Length];
        _capacity = initialCapacity;
        _typeIndices = new(_components.Length);
        Debug.Assert(key.Types.Length == componentTypes.Length);
        for (int i = 0; i < _components.Length; i++)
        {
            _typeIndices.Add(key.Types[i], i);
            _components[i] = Array.CreateInstance(componentTypes[i], initialCapacity);
        }
    }

    public int Add()
    {
        if (_count >= _capacity)
        {
            Resize();
        }
        int index = _count;
        _count++;
        return index;
    }

    public int AddRange(int amount)
    {
        if (amount <= 0)
        {
            ThrowHelper.ThrowArgumentException();
        }
        while (_count + amount > _capacity)
        {
            Resize();
        }
        int index = _count;
        _count += amount;
        return index;
    }

    public void FillHoleAt(int index)
    {
        _count--;
        for (int i = 0; i < _components.Length; i++)
        {
            Array.Copy(_components[i], _count, _components[i], index, 1);
        }
    }

    public void RemoveRange(int index, int count)
    {
        if (count <= 0)
        {
            ThrowHelper.ThrowArgumentException();
        }
        _count -= count;
        for (int i = 0; i < _components.Length; i++)
        {
            Array.Copy(_components[i], _count, _components[i], index, count);
        }
    }

    internal void CopyComponents(int srcIndex, Table dest, int destIndex, int count)
    {
        var srcTypes = _key.Types;
        var destTypes = dest._key.Types;
        for (int i = 0; i < srcTypes.Length; i++)
        {
            ulong type = srcTypes[i];
            for (int j = 0; j < destTypes.Length; j++)
            {
                if (type == destTypes[j])
                {
                    Array.Copy(_components[i], srcIndex, dest._components[j], destIndex, count);
                    break;
                }
            }
        }
    }

    internal ref T GetRefAt<T>(int index, ulong handle) where T : struct
    {
        if (_typeIndices.TryGetValue(handle, out int i))
        {
            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(_components[i])), index);
        }
        ThrowHelper.ThrowArgumentException();
        return ref Unsafe.NullRef<T>();
    }

    internal T[] GetComponentArray<T>(int componentIndex) where T : struct
    {
        return (T[])_components[componentIndex];
    }

    internal ref T GetRawRefAt<T>(int entityIndex, int componentIndex) where T : struct
    {
        return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(_components[componentIndex])), entityIndex);
    }

    private void Resize()
    {
        _capacity *= 2;
        for (int i = 0; i < _components.Length; i++)
        {
            var newArr = Array.CreateInstanceFromArrayType(_components[i].GetType(), _capacity);
            _components[i].CopyTo(newArr, 0);
            _components[i] = newArr;
        }
    }

    public bool Equals(Table? other)
    {
        return other != null && other._id == _id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Table);
    }

    public override int GetHashCode()
    {
        return _id;
    }
}
