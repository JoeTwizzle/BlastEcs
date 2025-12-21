using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
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

                if (!archetype.Key.Contains(filter.Inc) || 
                    archetype.Key.Contains(filter.Exc)) 
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
    }
}
