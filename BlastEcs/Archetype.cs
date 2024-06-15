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
    public GrowList<(EcsHandle entity, int tableIndex)> TableIndices => _tableIndices;

    private readonly Edges<Archetype> _edges;
    private readonly Table _table;
    private readonly int _id;
    private readonly GrowList<(EcsHandle entity, int tableIndex)> _tableIndices;

    public Archetype(int id, Table table, TypeCollectionKey key)
    {
        Key = key;
        _edges = new();
        _table = table;
        _id = id;
        _tableIndices = new();
    }

    public int AddEntity(EcsHandle entity, int tableIndex)
    {
        int index = _tableIndices.Count;
        _tableIndices.Add((entity, tableIndex));
        return index;
    }    

    public int AddEntity((EcsHandle entity, int tableIndex) item)
    {
        int index = _tableIndices.Count;
        _tableIndices.Add(item);
        return index;
    }

    public int AddEntities(ReadOnlySpan<(EcsHandle entity, int tableIndex)> items)
    {
        int index = _tableIndices.Count;
        _tableIndices.AddRange(items);
        return index;
    }

    public void RemoveEntityAt(int index)
    {
        _tableIndices.RemoveAtDense(index);
    }

    public bool Has(EcsHandle componentType)
    {
        return Key.HasType(componentType);
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
