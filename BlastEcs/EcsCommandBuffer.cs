using BlastEcs.Builtin;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs;

public readonly struct VirtualEntity : IEquatable<VirtualEntity>
{
    public readonly int Id;
    public readonly ulong RealHandle;
    public readonly int Count;
    public bool IsReal => RealHandle != 0;

    public VirtualEntity(int id)
    {
        Id = id;
        Count = 1;
    }

    public VirtualEntity(int id, int count)
    {
        Id = id;
        Count = count;
    }

    public VirtualEntity(int id, ulong realHandle)
    {
        RealHandle = realHandle;
        Id = id;
        Count = 1;
    }

    public override bool Equals(object? obj)
    {
        return obj is VirtualEntity e && Equals(e);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(VirtualEntity left, VirtualEntity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(VirtualEntity left, VirtualEntity right)
    {
        return !(left == right);
    }

    public bool Equals(VirtualEntity other)
    {
        return Id == other.Id;
    }
}

readonly struct CommandBufferRecord
{
    public readonly HashSet<ulong> handlesRemoved;
    public readonly HashSet<ulong> handlesAdded;
    public readonly Dictionary<ulong, object> ComponentValuesSet;
    public CommandBufferRecord()
    {
        handlesAdded = [];
        handlesRemoved = [];
        ComponentValuesSet = [];
    }
}

public sealed partial class EcsCommandBuffer
{
    private readonly EcsWorld _ecsWorld;
    private readonly List<CommandBufferRecord> _records = [];
    private readonly List<VirtualEntity> _entities = [];
    private bool _isRecording;

    public EcsCommandBuffer(EcsWorld ecsWorld)
    {
        _ecsWorld = ecsWorld;
    }

    public void Clear()
    {
        _records.Clear();
        _entities.Clear();
    }

    public void Begin()
    {
        if (_isRecording)
        {
            throw new InvalidOperationException("Recording already in progress");
        }
        _isRecording = true;
        Clear();
    }

    public void End()
    {
        ValidateRecording();
        _isRecording = false;
    }

    private void ValidateRecording()
    {
        if (!_isRecording)
        {
            throw new InvalidOperationException("Must call Begin before recording commands");
        }
    }

    public VirtualEntity Create(int count)
    {
        ValidateRecording();
        var ent = new VirtualEntity(_records.Count, count);
        _entities.Add(ent);
        _records.Add(new CommandBufferRecord());
        return ent;
    }

    public VirtualEntity Create(EcsHandle entity)
    {
        ValidateRecording();
        var ent = new VirtualEntity(_records.Count, entity.Id);
        _entities.Add(ent);
        _records.Add(new CommandBufferRecord());
        return ent;
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>(VirtualEntity entity) where T0 : struct
    {
        ValidateRecording();
        var handlesAdded = _records[entity.Id].handlesAdded;
        var componentValuesSet = _records[entity.Id].ComponentValuesSet;
        var handlesRemoved = _records[entity.Id].handlesRemoved;
        // [Variadic: CopyLines()]
        var idT0 = _ecsWorld.GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyLines()]
        if (!handlesRemoved.Remove(idT0)) { handlesAdded.Add(idT0); } else if (entity.IsReal && !new EcsHandle(entity.RealHandle).IsTag) { componentValuesSet.Add(idT0, default(T0)); }
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>(VirtualEntity entity, T0 data_T0) where T0 : struct
    {
        ValidateRecording();
        var handlesAdded = _records[entity.Id].handlesAdded;
        var componentValuesSet = _records[entity.Id].ComponentValuesSet;
        var handlesRemoved = _records[entity.Id].handlesRemoved;

        // [Variadic: CopyLines()]
        var idT0 = _ecsWorld.GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyLines()]
        if (!handlesRemoved.Remove(idT0)) { handlesAdded.Add(idT0); }
        // [Variadic: CopyLines()]
        componentValuesSet.Add(idT0, data_T0);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Remove<T0>(VirtualEntity entity) where T0 : struct
    {
        ValidateRecording();
        var handlesAdded = _records[entity.Id].handlesAdded;
        var componentValuesSet = _records[entity.Id].ComponentValuesSet;
        var handlesRemoved = _records[entity.Id].handlesRemoved;
        // [Variadic: CopyLines()]
        var idT0 = _ecsWorld.GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyLines()]
        if (!handlesAdded.Remove(idT0)) { handlesRemoved.Add(idT0); } else { componentValuesSet.Remove(idT0); }
    }

    public void Execute()
    {
        if (_isRecording) throw new InvalidOperationException("Recording must be stopped before a command buffer may be executed");

        var records = CollectionsMarshal.AsSpan(_records);
        var entites = CollectionsMarshal.AsSpan(_entities);

        if (records.Length != entites.Length) throw new InvalidOperationException($"There was a mismatch in internal data. Please file an issue on Github. Additional info: e={entites.Length} r={records.Length}");

        for (int i = 0; i < records.Length; i++)
        {
            var virtualEntity = entites[i];
            var record = records[i];

            EcsHandle entity;
            if (virtualEntity.IsReal) //We are dealing with an entity that already exists
            {
                entity = new EcsHandle(virtualEntity.RealHandle);
                _ecsWorld.Remove(entity, new TypeCollectionKeyNoAlloc([.. record.handlesRemoved]));
                _ecsWorld.Add(entity, new TypeCollectionKeyNoAlloc([.. record.handlesAdded]));
            }
            else //We are dealing with a new entity 
            {
                if (record.handlesRemoved.Count != 0) throw new InvalidOperationException("Cannot remove non existant component from entity");
                entity = _ecsWorld.CreateEntity(new TypeCollectionKeyNoAlloc([.. record.handlesAdded]));
            }
            _ecsWorld.SetValues(entity, record.ComponentValuesSet);
        }
    }
}
