using BlastEcs.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

namespace BlastEcs
{
    public sealed partial class EcsWorld
    {
        internal void InvokeFilter(Filter filter, Action<EcsHandle> action)
        {
            for (int i = 0; i < _archetypes.Count; i++)
            {
                var archetype = _archetypes[i];

                if ((filter.Inc.Types.Length > 0 && !archetype.Key.Contains(filter.Inc)) ||
                    (filter.Exc.Types.Length > 0 && archetype.Key.Contains(filter.Exc)))
                    continue;

                //Invoke callback for all entities in archetype
                archetype.Lock();
                archetype.Table.Lock();
                var ents = archetype.Entities.Span;
                for (int j = 0; j < ents.Length; j++)
                {
                    action.Invoke(ents[j]);
                }
                archetype.Table.Unlock();
                archetype.Unlock();

                if (archetype.Table.IsLocked) continue;

                //Create entites for locked archetypes/tables
                var archetypes = archetype.Table._archetypes.Span;
                for (int j = 0; j < archetypes.Length; j++)
                {
                    if (archetype.IsLocked) continue;

                    var newlyUnlockedArchetype = _archetypes[archetypes[j]];
                    var entities = newlyUnlockedArchetype._queuedEntities.Span;
                    foreach (var handle in entities)
                    {
                        ref EntityIndex entityIndex = ref GetEntityIndex(handle);
                        entityIndex.TableSlotIndex = archetype.Table.AddEntity(handle);
                        entityIndex.ArchetypeSlotIndex = archetype.AddEntity(handle);
                    }
                    newlyUnlockedArchetype._queuedEntities.Clear();
                }
            }
        }

        internal void InvokeFilter2(Filter filter, Action<EcsHandle> action)
        {
            var with = filter.Inc.Types;
            var without = filter.Exc.Types;

            BitMask candidates = new(false);
            if (with.Length == 0 && without.Length == 0)
            {
                var archetypes = _archetypes.DenseValues;
                for (int i = 0; i < archetypes.Length; i++)
                {
                    IterateArchetype(action, archetypes[i]);
                }
                return;
            }

            for (int i = 0; i < with.Length; i++)
            {
                if (i == 0)
                {
                    candidates.OrBits(_componentIndex[with[i]]);
                }
                else
                {
                    candidates.AndBits(_componentIndex[with[i]]);
                }
            }
            if (with.Length == 0)
            {
                candidates.SetBits(_archetypes.DenseKeys);
            }

            for (int i = 0; i < without.Length; i++)
            {
                candidates.ClearBits(_componentIndex[without[i]]);
            }

            var bits = candidates.Bits;
            for (int idx = 0; idx < bits.Length; idx++)
            {
                long bitItem = (long)bits[idx];
                while (bitItem != 0)
                {
                    int id = idx * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(bitItem);
                    bitItem ^= bitItem & -bitItem;
                    var archetype = _archetypes[id];
                    IterateArchetype(action, archetype);
                }
            }
            candidates.Dispose();
        }

        private void IterateArchetype(Action<EcsHandle> action, Archetype archetype)
        {
            //Invoke callback for all entities in archetype
            archetype.Lock();
            archetype.Table.Lock();
            var ents = archetype.Entities.Span;
            for (int j = 0; j < ents.Length; j++)
            {
                action.Invoke(ents[j]);
            }
            archetype.Table.Unlock();
            archetype.Unlock();

            if (archetype.Table.IsLocked) return;

            //Create entites for locked archetypes/tables
            var archetypes = archetype.Table._archetypes.Span;
            for (int j = 0; j < archetypes.Length; j++)
            {
                if (archetype.IsLocked) continue;

                var newlyUnlockedArchetype = _archetypes[archetypes[j]];
                var entities = newlyUnlockedArchetype._queuedEntities.Span;
                foreach (var handle in entities)
                {
                    ref EntityIndex entityIndex = ref GetEntityIndex(handle);
                    entityIndex.TableSlotIndex = archetype.Table.AddEntity(handle);
                    entityIndex.ArchetypeSlotIndex = archetype.AddEntity(handle);
                }
                newlyUnlockedArchetype._queuedEntities.Clear();
            }
        }
    }
}
