using BlastEcs.Collections;
using BlastEcs.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace BlastEcs
{

    public sealed partial class EcsWorld
    {
        public sealed class SimpleQuery
        {
            EcsWorld _world;
            SimpleQueryKey _filter;

            bool cacheValid;
            List<int> _archetypeCache;
            public SimpleQuery(EcsWorld world, SimpleQueryKey filter)
            {
                _world = world;
                _filter = filter;
                _archetypeCache = [];
            }

            public void Each(Action<EcsHandle> action)
            {
                var allArchetypes = _world._archetypes;
                if (!cacheValid)
                {
                    UpdateCache();
                }

                for (int i = 0; i < _archetypeCache.Count; i++)
                {
                    IterateArchetype(action, allArchetypes[_archetypeCache[i]]);
                }
            }

            public void MarkDirty()
            {
                cacheValid = false;
            }

            void UpdateCache()
            {
                _archetypeCache.Clear();
                var with = _filter.Inc.Types;
                var without = _filter.Exc.Types;
                var componentIndex = _world._componentIndex;
                var allArchetypes = _world._archetypes;

                BitMask candidates = new(false);
                if (with.Length == 0 && without.Length == 0)
                {
                    _archetypeCache.AddRange(allArchetypes.DenseKeys);
                    cacheValid = true;
                    return;
                }

                for (int i = 0; i < with.Length; i++)
                {
                    if (i == 0)
                    {
                        candidates.OrBits(componentIndex[with[i]]);
                    }
                    else
                    {
                        candidates.AndBits(componentIndex[with[i]]);
                    }
                }

                if (with.Length == 0)
                {
                    candidates.SetBits(allArchetypes.DenseKeys);
                }

                for (int i = 0; i < without.Length; i++)
                {
                    candidates.ClearBits(componentIndex[without[i]]);
                }

                var bits = candidates.Bits;
                for (int idx = 0; idx < bits.Length; idx++)
                {
                    long bitItem = (long)bits[idx];
                    while (bitItem != 0)
                    {
                        int id = idx * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(bitItem);
                        bitItem ^= bitItem & -bitItem;
                        _archetypeCache.Add(id);
                    }
                }
                candidates.Dispose();
                cacheValid = true;
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

                    var newlyUnlockedArchetype = _world._archetypes[archetypes[j]];
                    var entities = newlyUnlockedArchetype._queuedEntities.Span;
                    foreach (var handle in entities)
                    {
                        ref EntityIndex entityIndex = ref _world.GetEntityIndex(handle);
                        entityIndex.TableSlotIndex = archetype.Table.AddEntity(handle);
                        entityIndex.ArchetypeSlotIndex = archetype.AddEntity(handle);
                    }
                    newlyUnlockedArchetype._queuedEntities.Clear();
                }
            }
        }

        Dictionary<SimpleQueryKey, SimpleQuery> _queryCache;
        public SimpleQuery GetQuery(SimpleQueryKey filter)
        {
            ref var query = ref _queryCache.GetRefOrAddDefault(filter, out var exists);
            if (!exists)
            {
                query = new SimpleQuery(this, filter);
            }
            return query;
        }

        internal void InvokeFilter(SimpleQueryKey filter, Action<EcsHandle> action)
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

        internal void InvokeFilter2(SimpleQueryKey filter, Action<EcsHandle> action)
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
