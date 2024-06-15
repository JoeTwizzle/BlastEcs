using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public readonly struct EcsHandle
{
    readonly ulong _id;

    public EcsHandle(EcsHandle kindHandle, EcsHandle targetHandle)
    {
        _id = ((ulong)kindHandle.Entity | (((ulong)targetHandle.Entity) << 32)) | (((ulong)EntityFlags.IsPair) << 56);
    }

    public EcsHandle(ulong id)
    {
        _id = id;
    }

    public EcsHandle(uint entity, short gen, byte world, byte flags = EntityFlags.None)
    {
        uint low = entity & 0xFFFFFF | (((uint)world) << 24);
        uint high = (uint)((ushort)gen | (((uint)flags) << 24));
        _id = low | ((ulong)high << 32);
    }

    //24 Bit Id | 8 Bit WorldId | 16 Bit Generation | 8 Bit Unused | 8 Bit Flags
    //24 Bit Id | 8 Bit WorldId | 24 Bit Target Id  | 8 Bit Flags

    public ulong Id => _id;

    /// <summary>
    /// 24 Bit EntityId.
    /// If Pair flag is present then this represents the kind of relationship
    /// </summary>
    public uint Entity => (uint)(_id & 0xFFFFFFul);
    /// <summary>
    /// 8 Bit world Id.
    /// </summary>
    public byte World => (byte)((_id >> 24));
    /// <summary>
    /// 16 Bit generation Id.
    /// Only valid if Pair flag is NOT present
    /// </summary>
    public short Generation => (short)((_id >> 32) & 0xFFFFul);
    /// <summary>
    /// 24 Bit Target EntityId.
    /// Only valid if Pair flag is present
    /// </summary>
    public uint Target => (uint)((_id >> 32) & 0xFFFFFFul);
    /// <summary>
    /// 8 Bit Flags.
    /// </summary>
    public byte Flags => (byte)((_id >> 56) & 0xFFul);

    public bool IsPair => (Flags & EntityFlags.IsPair) != 0;
}
