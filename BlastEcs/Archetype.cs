using BlastEcs.Collections;

namespace BlastEcs;

/// <summary>
/// An Archetype contains all entities that share the same components and tags
/// </summary>
public sealed class Archetype : IEquatable<Archetype>
{
    public readonly TypeCollectionKey Key;
    public int Id => _id;
    public Table Table => _table;
    public Edges<Archetype> Edges => _edges;
    public bool IsLocked => _lockCount != 0;
    private int _lockCount;

    //Index of an entity in a table's component arrays
    public GrowList<EcsHandle> Entities => _entities;

    private readonly Edges<Archetype> _edges;
    private readonly Table _table;
    private readonly int _id;
    private readonly GrowList<EcsHandle> _entities;

    public Archetype(int id, Table table, TypeCollectionKey key)
    {
        Key = key;
        _edges = new();
        _table = table;
        _id = id;
        _entities = new();
    }

    public int AddEntity(EcsHandle entity)
    {
        int index = _entities.Count;
        _entities.Add(entity);
        return index;
    }

    public int AddEntities(ReadOnlySpan<EcsHandle> items)
    {
        int index = _entities.Count;
        _entities.AddRange(items);
        return index;
    }

    public void RemoveEntityAt(int index)
    {
        _entities.RemoveAtDense(index);
    }

    public bool Has(EcsHandle componentType)
    {
        return Key.HasType(componentType);
    }

    public void Lock()
    {
        _lockCount++;
    }

    public void Unlock()
    {
        _lockCount--;
    }


    public bool Equals(Archetype? other)
    {
        return other != null && _id == other._id;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Archetype);
    }

    public override int GetHashCode()
    {
        return _id;
    }
}
