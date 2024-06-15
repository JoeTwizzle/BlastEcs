using BlastEcs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

    private readonly int _id;
    private readonly Type[] _componentTypes;
    private readonly TypeCollectionKey _key;
    private readonly Array[] _components;
    private readonly Dictionary<Type, int> _typeIndices;
    //private readonly Edges<Table> _edges;
    private int _count;
    private int _capacity;

    public Table(int id, Type[] componentTypes, TypeCollectionKey key, int initialCapacity = 4)
    {
        _id = id;
        _componentTypes = componentTypes;
        _key = key;
        _components = new Array[componentTypes.Length];
        _capacity = initialCapacity;
        _typeIndices = new(_components.Length);
        //_edges = new Edges<Table>();
        for (int i = 0; i < _components.Length; i++)
        {
            _typeIndices.Add(componentTypes[i], i);
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

    public void RemoveAt(int index)
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

    public ref T GetRefAt<T>(int index) where T : struct
    {
        if (_typeIndices.TryGetValue(typeof(T), out int i))
        {
            return ref ((T[])_components[i])[index];
        }
        ThrowHelper.ThrowArgumentException();
        return ref Unsafe.NullRef<T>();
    }

    private void Resize()
    {
        _capacity *= 2;
        for (int i = 0; i < _components.Length; i++)
        {
            var newArr = Array.CreateInstance(_componentTypes[i], _capacity);
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
