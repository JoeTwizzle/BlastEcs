using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BlastEcs;

public readonly partial struct EcsHandle
{
    readonly ulong _id;

    public EcsHandle(EcsHandle kindHandle, EcsHandle targetHandle, byte flags = EntityFlags.None)
    {
        flags |= EntityFlags.IsPair;
        _id = ((ulong)targetHandle.Entity | (((ulong)kindHandle.WorldId) << 24) | (((ulong)kindHandle.Entity) << 32)) | (((ulong)flags) << 56);
    }

    public EcsHandle(ulong id)
    {
        _id = id;
    }

    public EcsHandle(uint entity, short gen, byte world, byte flags = EntityFlags.None)
    {
        uint high = (entity & 0xFFFFFF) | (((uint)flags) << 24);
        uint low = (((ushort)gen) | (((uint)world) << 24));
        _id = low | ((ulong)high << 32);
    }

    //LOW -> HIGH
    //ENTITY|: 16 Bit Generation | 8 Bit Unused | 8 Bit WorldId | 24 Bit Entity Id  | 8 Bit Flags
    //PAIR  |: 24 Bit Target Id                 | 8 Bit WorldId | 24 Bit Entity Id  | 8 Bit Flags
    public ulong Id => _id;

    /// <summary>
    /// 24 Bit Target EntityId.
    /// Only valid if Pair flag is present
    /// </summary>
    public uint Target => (uint)(_id & 0xFFFFFFul);
    /// <summary>
    /// 8 Bit world Id.
    /// </summary>
    public byte WorldId => (byte)((_id >> 24) & 0xFFul);
    /// <summary>
    /// 16 Bit generation Id.
    /// Only valid if Pair flag is NOT present
    /// </summary>
    public short Generation => (short)(_id & 0xFFFFul);
    /// <summary>
    /// 24 Bit EntityId.
    /// If Pair flag is present then this represents the kind of relationship
    /// </summary>
    public uint Entity => (uint)((_id >> 32) & 0xFFFFFFul);
    /// <summary>
    /// 8 Bit Flags.
    /// </summary>
    public byte Flags => (byte)((_id >> 56) & 0xFFul);
    /// <summary>
    /// Whether the current handle is pair of two handles
    /// </summary>
    public bool IsPair => (Flags & EntityFlags.IsPair) != 0;
    /// <summary>
    /// Whether the current handle forces the pair to act as a tag when used in a relation
    /// </summary>
    public bool IsTagRelation => (Flags & EntityFlags.IsTagRelation) != 0;
}
