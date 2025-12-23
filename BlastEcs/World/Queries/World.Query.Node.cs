using BlastEcs.Collections;
using System.Numerics;

namespace BlastEcs;


public sealed partial class EcsWorld
{
    //Matching multiple Source terms X
    //Init():
    //  For source in Sources:
    //      validSets[source] = GetValidSet(source)
    //StartMatch():
    //  validSet = validSets[this]
    //  state.targetSets[this] = validSet
    //  StartMatch(this, validSet)
    //      

    public sealed partial class Query
    {
        internal sealed class Source
        {
            public readonly Term[] ComplexTerms;
            public readonly TypeCollectionKey WithKey;
            public readonly TypeCollectionKey WithoutKey;

            public Source(Term[] complexTerms, TypeCollectionKey withKey, TypeCollectionKey withoutKey)
            {
                ComplexTerms = complexTerms;
                WithKey = withKey;
                WithoutKey = withoutKey;
            }
        }

        internal sealed class State
        {
            public int[] ActiveMembers;
            public BitMask[] TargetSets;
            public bool[] TargetSetInit;

            public State(int[] activeMembers, BitMask[] targetSets)
            {
                ActiveMembers = activeMembers;
                TargetSets = targetSets;
                TargetSetInit = new bool[targetSets.Length];
            }
        }

        readonly EcsWorld _world;
        readonly Source[] _sources;
        readonly BitMask[] _potentiallyValidSets;
        readonly State _state;

        internal Query(EcsWorld world, Source[] sources)
        {
            _world = world;
            _sources = sources;
            _potentiallyValidSets = new BitMask[sources.Length];
            _state = new State(new int[sources.Length], new BitMask[sources.Length]);
        }

        public void Init()
        {
            for (int i = 0; i < _sources.Length; i++)
            {
                var validSet = new BitMask(false);
                if (_sources[i].WithKey.Types.Length == 0 && _sources[i].WithoutKey.Types.Length == 0)
                {
                    validSet.SetRange(0, _world._archetypes.Count);
                }
                //_world.GetArchetypesWith(_sources[i].WithKey, validSet, true);
                //_world.FilterArchetypesWithout(_sources[i].WithoutKey, validSet);
                _potentiallyValidSets[i] = validSet;
            }
        }

        public void StartMatch()
        {
            var mismatch = StartMatch(0, _potentiallyValidSets[0]);
            if (mismatch.HasValue)
            {
                Console.WriteLine("mismatch at source: " + mismatch.Value);
            }
        }
        //StartMatch(Source, validSet, ref state):
        //  For archetype,i in validSet:
        //      state.activeMembers[Source] = i
        //      For complexTerm in Source:
        //          state.targetSets[complexTerm.TargetSource] ??= new(); clear(); //archetypes targeted by this relation
        //          For target in archetype(complexTerm.match):
        //              t = arch(target)
        //              state.targetSets[complexTerm.TargetSource].set(t.id) //Set archetypes targeted by this relation for this archetype
        //          state.targetSets[complexTerm.TargetSource].and(validSets[complexTerm.TargetSource]) //mask to only include valid archetypes by component filter
        //          If state.targetSets[complexTerm.TargetSource] is empty: return complexTerm.TargetSource //return error value
        //          If !validSets[complexTerm.TargetSource].IsSet(state.activeMembers[complexTerm.TargetSource]): return complexTerm.TargetSource
        //          Mismatch = StartMatch(complexTerm.TargetSource, targetSet, ref state)
        //          If mismatch not none AND mismatch != source: return Mismatch //Short circuit 
        //          If mismatch == source: validset.clear(i); Goto next; //mark as invalid and go to next valid archetype 
        //      For set,j in state.targetSets where empty:
        //          set = state.targetSets[j] = validSets[j].copy();
        //          Mismatch = StartMatch(j, set, ref state);
        //          If mismatch not none AND mismatch != source: return Mismatch //Short circuit 
        //      message=""
        //      For active,j in state.activeMembers.Length:
        //          message += "Source "+ j + ": " + active;
        //      CW(Message)
        //      next:
        internal int? StartMatch(int sourceId, BitMask validSet)
        {
            Source source = _sources[sourceId];
            _state.TargetSetInit[sourceId] = true;
            var archetypes = validSet.Bits;
            for (int i = 0; i < archetypes.Length; i++)
            {
                long rem = (long)archetypes[i];
                while (rem != 0)
                {
                    int archetypeId = i * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(rem);
                    rem ^= rem & -rem;

                    _state.ActiveMembers[sourceId] = archetypeId;
                    var archetype = _world._archetypes[archetypeId];

                    for (int j = 0; j < source.ComplexTerms.Length; j++)
                    {
                        var complexTerm = source.ComplexTerms[j];
                        if (complexTerm.TargetSource != null)
                        {

                            BitMask targetMask = _state.TargetSets[complexTerm.TargetSource.Value] ??= new(false);
                            targetMask.ClearAll();

                            archetype.Key.ForeachTarget(complexTerm.Match, (target) =>
                            {
                                targetMask
                                    .SetBit(_world.GetEntityIndex(target).ArchetypeSlotIndex);
                            });

                            targetMask //mask to only include valid archetypes by component filter
                                .AndBits(_potentiallyValidSets[complexTerm.TargetSource.Value]);

                            if (_state.TargetSetInit[complexTerm.TargetSource.Value])
                            {
                                if (!targetMask.HasAnySet())
                                {
                                    _state.TargetSetInit[sourceId] = false;
                                    return complexTerm.TargetSource; //return index of problematic source
                                }

                                if (!_potentiallyValidSets[complexTerm.TargetSource.Value]
                                    .IsSet(_state.ActiveMembers[complexTerm.TargetSource.Value]))
                                {
                                    _state.TargetSetInit[sourceId] = false;
                                    return complexTerm.TargetSource; //invalid archetype selected at source
                                }
                            }
                            else
                            {
                                if (!targetMask.HasAnySet())
                                {
                                    goto next;
                                }
                            }

                            var mismatch = StartMatch(complexTerm.TargetSource.Value, _potentiallyValidSets[complexTerm.TargetSource.Value]);
                            if (mismatch.HasValue && mismatch.Value != sourceId)
                            {
                                if (mismatch.Value != sourceId)
                                {
                                    _state.TargetSetInit[sourceId] = false;
                                    return mismatch.Value; //Short circuit 
                                }
                                else
                                {
                                    _state.TargetSets[sourceId].ClearBit(archetypeId); //mark as invalid and go to next valid archetype 
                                    goto next;
                                }
                            }
                        }
                    }

                    for (int j = 0; j < _state.TargetSets.Length; j++)
                    {
                        var set = _state.TargetSets[j];
                        if (set != null || _state.TargetSetInit[j])
                        {
                            continue;
                        }
                        set = new(_potentiallyValidSets[j], false);
                        var mismatch = StartMatch(j, set);
                        if (mismatch.HasValue && mismatch.Value == sourceId)
                        {
                            _state.TargetSetInit[sourceId] = false;
                            return mismatch;
                        }
                        //TODO?
                    }
                    string message = "";
                    for (int j = 0; j < _state.ActiveMembers.Length; j++)
                    {
                        message += " Src " + j + ": " + _state.ActiveMembers[j];
                    }
                    Console.WriteLine(message);
                next:
                    ;
                }
            }
            _state.TargetSetInit[sourceId] = false;
            return default;
        }
    }
}
