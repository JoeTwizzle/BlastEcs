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
    private readonly GrowList<EcsHandle> _entities;
    public TypeCollectionKey Key => _key;
    //public Edges<Table> Edges => _edges;
    public int Count => _count;
    public int Capacity => _capacity;
    public int Id => _id;
    public GrowList<EcsHandle> Entities => _entities;

    internal LongKeyMap<int> TypeIndices => _typeIndices;

    private readonly int _id;
    private readonly TypeCollectionKey _key;
    private readonly Array[] _componentArrays;
    private readonly LongKeyMap<int> _typeIndices;
    //private readonly Edges<Table> _edges;
    private int _count;
    private int _capacity;

    internal Table(int id, Type[] componentTypes, TypeCollectionKey key, int initialCapacity = 4)
    {
        _entities = new();
        _id = id;
        _key = key;
        _componentArrays = new Array[componentTypes.Length];
        _capacity = initialCapacity;
        _typeIndices = new(_componentArrays.Length);
        Debug.Assert(key.Types.Length == componentTypes.Length);
        for (int i = 0; i < _componentArrays.Length; i++)
        {
            _typeIndices.Add(key.Types[i], i);
            _componentArrays[i] = Array.CreateInstance(componentTypes[i], initialCapacity);
        }
    }

    internal int AddEntity(EcsHandle entity)
    {
        _entities.Add(entity);
        if (_count >= _capacity)
        {
            Resize();
        }
        int index = _count;
        _count++;
        return index;
    }

    internal int AddEntities(ReadOnlySpan<EcsHandle> entities)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(entities.Length, 0);
        _entities.AddRange(entities);
        while (_count + entities.Length > _capacity)
        {
            Resize();
        }
        int index = _count;
        _count += entities.Length;
        return index;
    }

    internal void RemoveAt(int index)
    {
        _count--;
        _entities.RemoveAtDense(index);
        for (int i = 0; i < _componentArrays.Length; i++)
        {
            Array.Copy(_componentArrays[i], _count, _componentArrays[i], index, 1);
        }
    }

    internal void RemoveRange(int index, int count)
    {
        if (count <= 0)
        {
            ThrowHelper.ThrowArgumentException();
        }
        _count -= count;
        _entities.RemoveRangeDense(index, count);
        for (int i = 0; i < _componentArrays.Length; i++)
        {
            Array.Copy(_componentArrays[i], _count, _componentArrays[i], index, count);
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
                    Array.Copy(_componentArrays[i], srcIndex, dest._componentArrays[j], destIndex, count);
                    break;
                }
            }
        }
    }

    internal ref T GetRefAt<T>(int index, ulong handle) where T : struct
    {
        if (_typeIndices.TryGetValue(handle, out int i))
        {
            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(_componentArrays[i])), index);
        }
        ThrowHelper.ThrowArgumentException();
        return ref Unsafe.NullRef<T>();
    }

    internal T[] GetComponentArray<T>(int componentIndex) where T : struct
    {
        return (T[])_componentArrays[componentIndex];
    }

    internal ref T GetRawRefAt<T>(int entityIndex, int componentIndex) where T : struct
    {
        return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(_componentArrays[componentIndex])), entityIndex);
    }

    private void Resize()
    {
        _capacity *= 2;
        for (int i = 0; i < _componentArrays.Length; i++)
        {
            var newArr = Array.CreateInstanceFromArrayType(_componentArrays[i].GetType(), _capacity);
            _componentArrays[i].CopyTo(newArr, 0);
            _componentArrays[i] = newArr;
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
