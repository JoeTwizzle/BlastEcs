using BlastEcs.Builtin;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BlastEcs;

public readonly partial struct VirtualEntity : IEquatable<VirtualEntity>
{
    public readonly EcsCommandBuffer cmd;
    public readonly ulong RealHandle;
    public readonly int Id;
    public readonly int Count;
    public bool IsReal => RealHandle != 0;

    public VirtualEntity(EcsCommandBuffer cmd, int id)
    {
        this.cmd = cmd;
        Id = id;
        Count = 1;
    }

    public VirtualEntity(EcsCommandBuffer cmd, int id, int count)
    {
        this.cmd = cmd;
        Id = id;
        Count = count;
    }

    public VirtualEntity(EcsCommandBuffer cmd, int id, ulong realHandle)
    {
        this.cmd = cmd;
        RealHandle = realHandle;
        Id = id;
        Count = 1;
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>() where T0 : struct
    {
        cmd.Add<T0>(this);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>(T0 data_T0) where T0 : struct
    {
        cmd.Add(this, data_T0);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Remove<T0>() where T0 : struct
    { 
        cmd.Remove<T0>(this);
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

internal readonly struct CommandBufferRecord
{
    public readonly HashSet<ulong> HandlesRemoved;
    public readonly HashSet<ulong> HandlesAdded;
    public readonly Dictionary<ulong, object> ComponentValuesSet;
    public CommandBufferRecord()
    {
        HandlesAdded = [];
        HandlesRemoved = [];
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

    public VirtualEntity Create()
    {
        return Create(1);
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public VirtualEntity Create<T0>() where T0 : struct
    {
        return Create<T0>(1);
    }

    public VirtualEntity Create(int count)
    {
        ValidateRecording();
        var ent = new VirtualEntity(this, _records.Count, count);
        _entities.Add(ent);
        _records.Add(new CommandBufferRecord());
        return ent;
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public VirtualEntity Create<T0>(int count) where T0 : struct
    {
        ValidateRecording();
        var ent = new VirtualEntity(this, _records.Count, count);
        _entities.Add(ent);
        _records.Add(new CommandBufferRecord());
        var handlesAdded = _records[ent.Id].HandlesAdded;
        // [Variadic: CopyLines()]
        var idT0 = _ecsWorld.GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyLines()]
        handlesAdded.Add(idT0);
        return ent;
    }

    public VirtualEntity Create(EcsHandle entity)
    {
        ValidateRecording();
        var ent = new VirtualEntity(this, _records.Count, entity.Id);
        _entities.Add(ent);
        _records.Add(new CommandBufferRecord());
        return ent;
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>(VirtualEntity entity) where T0 : struct
    {
        ValidateRecording();
        var handlesAdded = _records[entity.Id].HandlesAdded;
        var componentValuesSet = _records[entity.Id].ComponentValuesSet;
        var handlesRemoved = _records[entity.Id].HandlesRemoved;
        // [Variadic: CopyLines()]
        var idT0 = _ecsWorld.GetHandleToInstantiableType<T0>().Id;
        // [Variadic: CopyLines()]
        if (!handlesRemoved.Remove(idT0)) { handlesAdded.Add(idT0); } else if (entity.IsReal && !new EcsHandle(entity.RealHandle).IsTag) { componentValuesSet.Add(idT0, default(T0)); }
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Add<T0>(VirtualEntity entity, T0 data_T0) where T0 : struct
    {
        ValidateRecording();
        var handlesAdded = _records[entity.Id].HandlesAdded;
        var componentValuesSet = _records[entity.Id].ComponentValuesSet;
        var handlesRemoved = _records[entity.Id].HandlesRemoved;

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
        var handlesAdded = _records[entity.Id].HandlesAdded;
        var componentValuesSet = _records[entity.Id].ComponentValuesSet;
        var handlesRemoved = _records[entity.Id].HandlesRemoved;
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
                _ecsWorld.Remove(entity, new TypeCollectionKeyNoAlloc([.. record.HandlesRemoved]));
                _ecsWorld.Add(entity, new TypeCollectionKeyNoAlloc([.. record.HandlesAdded]));
                _ecsWorld.SetValues(entity, record.ComponentValuesSet);
            }
            else //We are dealing with a new entity 
            {
                if (record.HandlesRemoved.Count != 0) throw new InvalidOperationException("Cannot remove non existant component from entity");
                for (int c = 0; c < virtualEntity.Count; c++)
                {
                    entity = _ecsWorld.CreateEntity(new TypeCollectionKeyNoAlloc([.. record.HandlesAdded]));
                    _ecsWorld.SetValues(entity, record.ComponentValuesSet);
                }
            }
        }
    }
}
