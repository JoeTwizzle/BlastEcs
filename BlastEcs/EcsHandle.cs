using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public readonly struct EcsHandle
{
    readonly ulong _id;

    public EcsHandle(ulong id)
    {
        _id = id;
    }

    public EcsHandle(uint entity, short gen, byte world)
    {
        uint low = entity;
        uint high = (uint)((ushort)gen | (world << 16));
        _id = low | ((ulong)high << 32);
    }

    public ulong Id => _id;

    public uint Entity => (uint)(_id & 0xFFFFFFFFul);
    public short Generation => (short)((_id >> 32) & 0xFFFFul);
    public byte World => (byte)((_id >> 48) & 0xFFul);

    public uint Target => (uint)((_id >> 32) & 0xFFFFFFFFul);
}
