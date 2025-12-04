using System.Diagnostics;
using System.Runtime.InteropServices;
using static BlastEcs.EcsWorld.Query;

namespace BlastEcs;

public delegate void EntityAction(EcsHandle handle);
public delegate void EntityAction<T0>(EcsHandle handle, ref T0 component0) where T0 : struct;
public delegate void EntityAction<T0, T1>(EcsHandle handle, ref T0 component0, ref T1 component1) where T0 : struct where T1 : struct;
public delegate void EntityAction<T0, T1, T2>(EcsHandle handle, ref T0 component0, ref T1 component1, ref T2 component2) where T0 : struct where T1 : struct where T2 : struct;
internal readonly struct Term
{
    //Normal/Relation
    //- Match
    //Named Relation
    //- Match
    //- KindSource OR TargetSource
    //Anonymous Up/Self Traversal
    //- Match (treat like normal, if false check match on RelationshipToTraverse up until true or empty) 
    //- RelationshipToTraverse
    //Named Up traversal
    //- RelationshipToTraverse
    //- TargetSource
    //Anonymous Down/Self Traversal
    //- Match (treat like normal, if false check match on RelationshipToTraverse down until true or empty) 
    //- RelationshipToTraverse
    public readonly EcsHandle Match;
    public readonly EcsHandle RelationshipToTraverse;
    public readonly int? KindSource;
    public readonly int? TargetSource;
    public readonly OperationTarget Target;
    public readonly bool Exclude;

    public Term(EcsHandle match, EcsHandle relationshipToTraverse, OperationTarget target, bool exclude)
    {
        Match = match;
        RelationshipToTraverse = relationshipToTraverse;
        KindSource = null;
        TargetSource = null;
        Target = target;
        Exclude = exclude;
    }

    public Term(EcsHandle match, EcsHandle relationshipToTraverse, int? src, int? dest, OperationTarget target, bool exclude)
    {
        Match = match;
        RelationshipToTraverse = relationshipToTraverse;
        KindSource = src;
        TargetSource = dest;
        Target = target;
        Exclude = exclude;
    }

    public Term(Term template, OperationTarget target)
    {
        Match = template.Match;
        RelationshipToTraverse = template.RelationshipToTraverse;
        KindSource = template.KindSource;
        TargetSource = template.TargetSource;
        Target = target;
        Exclude = template.Exclude;
    }

    public Term(Term template, EcsHandle relationshipToTraverse, OperationTarget target)
    {
        Match = template.Match;
        RelationshipToTraverse = relationshipToTraverse;
        KindSource = template.KindSource;
        TargetSource = template.TargetSource;
        Target = target;
        Exclude = template.Exclude;
    }
};

struct TAarm
{
    EcsHandle Match;
    EcsHandle RelationshipToTraverse;
    int KindSource;
    int TargetSource;
    OperationTarget OperationTarget;



    //Traversing Up/Self with DFS X
    //For archetype in validSet:
    //  Self = archetype
    //  Self has Match -> Exit
    //  Self has not (Kind, *) -> Reject
    //  Push(Self)
    //  While(stack > 0):
    //    targets = Self(Kind, *)
    //    For target in targets:
    //      archetype = arch(target)
    //      Self = archetype
    //      Self has Match -> Exit
    //      Self has not (Kind, *) -> Self = Pop(Self); Continue;
    //      Push(Self)

    //Traversing Up/Self with BFS O
    //For archetype in validSet:
    //Traverse(ref State, archetype):
    //    Self = archetype
    //    Self has Match/In named Set -> Exit
    //    Self has not (Kind, *) -> Reject
    //    Push(Self)
    //    While(stack > 0):
    //      targets = Self(Kind, *)
    //      For target in targets:
    //        archetype = arch(target)
    //        archetype has Match/In named Set -> Exit
    //        archetype has not (Kind, *) -> Continue;
    //        Push(archetype)
    //      Self = Pop()

    //Traversing Down/Self with BFS O
    //For archetype in validSet:
    //Traverse(ref State, archetype):
    //  Self = archetype
    //  Self has Match -> Exit
    //  While(stack > 0):
    //      For entity in Self:
    //          archetypes with (Kind, entity) do not exist -> continue;
    //          For archetype with (Kind, entity):
    //              archetype has Match/In named Set -> Exit
    //              Push(archetype)
    //      Self = Pop()

    //Matching multiple Source terms X
    //Init():
    //  For source in Sources:
    //      validSets[source] = GetValidSet(source)
    //StartMatch():
    //  validSet = validSets[this]
    //  state.targetSets[this] = validSet
    //  StartMatch(this, validSet)
    //  For empty in state.targetSets:
    //      
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
}

internal enum OperationTarget
{
    Default = 0,
    Self = 1,
    Up = 2,
    Down = 4,
    Cascade = 8,
}
internal struct Node
{
    public TypeCollectionKey WithKey;
    public TypeCollectionKey WithoutKey;
    public Term[] ComplexTerms;

    public Node(TypeCollectionKey withKey, TypeCollectionKey withoutKey, Term[] complexTerms)
    {
        WithKey = withKey;
        WithoutKey = withoutKey;
        ComplexTerms = complexTerms;
    }
}
public sealed partial class EcsWorld
{
    public FilterBuilder CreateFilter()
    {
        return new FilterBuilder(this);
    }

    public sealed class FilterBuilder
    {
        sealed class SourceNode
        {
            public List<Term> Terms;

            public SourceNode()
            {
                Terms = new();
            }
        }

        const int defaultSourceId = 0;
        readonly EcsWorld _world;
        int _targets;
        int currentSource;
        readonly Dictionary<string, int> _targetMap;
        readonly List<SourceNode> _nodes;

        public FilterBuilder(EcsWorld world)
        {
            _world = world;
            _targets = 1;
            _targetMap = new()
            {
                { "", defaultSourceId },
                { "this", defaultSourceId },
                { "self", defaultSourceId }
            };
            _nodes = [new()];
        }
        //modifiers

        internal FilterBuilder Self()
        {
            var term = _nodes[currentSource].Terms[^1];
            _nodes[currentSource].Terms[^1] = new Term(term, term.Target | OperationTarget.Self);
            return this;
        }

        //Anonymous up traversal
        internal FilterBuilder Up<TReletion>() where TReletion : struct
        {
            var term = _nodes[currentSource].Terms[^1];
            _nodes[currentSource].Terms[^1] = new Term(term, _world.GetHandleToType<TReletion>(), term.Target | OperationTarget.Up);
            return this;
        }

        //Named up traversal
        internal FilterBuilder Up<TRelation>(string targetSource) where TRelation : struct
        {
            return Up(_world.GetHandleToType<TRelation>(), targetSource);
        }

        internal FilterBuilder Up(EcsHandle relationKind, string targetSource)
        {
            RegisterIndefiniteTargetRelation(relationKind, targetSource);
            var term = _nodes[currentSource].Terms[^1];
            _nodes[currentSource].Terms[^1] = new Term(term, relationKind, term.Target | OperationTarget.Up);
            return this;
        }

        private void RegisterSimple<T0>(bool exclude = false) where T0 : struct
        {
            _nodes[currentSource].Terms.Add(new Term(_world.GetHandleToType<T0>(), _world._emptyEntity, OperationTarget.Default, exclude));
        }

        private void RegisterRelation(EcsHandle kind, EcsHandle target, bool exclude = false)
        {
            _nodes[currentSource].Terms.Add(new Term(_world.GetHandleToPair(kind, target), _world._emptyEntity, OperationTarget.Default, exclude));
        }

        private void RegisterIndefiniteTargetRelation(EcsHandle kind, string id, bool exclude = false)
        {
            int index = GetOrRegisterIdentifier(id);
            _nodes[currentSource].Terms.Add(new Term(_world.GetHandleToPair(kind, _world.AnyEntity), _world._emptyEntity, null, index, OperationTarget.Default, exclude));
        }

        private void RegisterIndefiniteKindRelation(string id, EcsHandle target, bool exclude = false)
        {
            int index = GetOrRegisterIdentifier(id);
            _nodes[currentSource].Terms.Add(new Term(_world.GetHandleToPair(_world.AnyEntity, target), _world._emptyEntity, index, null, OperationTarget.Default, exclude));
        }

        private void RegisterIndefinitePair(string id, string id2, bool exclude = false)
        {
            var index1 = GetOrRegisterIdentifier(id);
            var index2 = GetOrRegisterIdentifier(id2);
            _nodes[currentSource].Terms.Add(new Term(_world.GetHandleToPair(_world.AnyEntity, _world.AnyEntity), _world._emptyEntity, index1, index2, OperationTarget.Default, exclude));
        }

        private int GetOrRegisterIdentifier(string target)
        {
            ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault(_targetMap, target, out bool exists);
            if (!exists)
            {
                index = _targets++;
                _nodes.Add(new());
            }
            return index;
        }

        //With
        public FilterBuilder With<T0>() where T0 : struct
        {
            RegisterSimple<T0>();
            return this;
        }

        //Pairs
        public FilterBuilder With<TKind, TTarget>() where TKind : struct where TTarget : struct
        {
            return With(_world.GetHandleToType<TKind>(), _world.GetHandleToType<TTarget>());
        }

        public FilterBuilder With<TKind>(EcsHandle target) where TKind : struct
        {
            return With(_world.GetHandleToType<TKind>(), target);
        }

        public FilterBuilder With(EcsHandle kind, EcsHandle target)
        {
            RegisterRelation(kind, target);
            return this;
        }

        //Open pairs
        public FilterBuilder With<TKind>(string id) where TKind : struct
        {
            return With(_world.GetHandleToType<TKind>(), id);
        }

        public FilterBuilder With(EcsHandle kind, string id)
        {
            RegisterIndefiniteTargetRelation(kind, id);
            return this;
        }

        public FilterBuilder With(string id, EcsHandle target)
        {
            RegisterIndefiniteKindRelation(id, target);
            return this;
        }

        public FilterBuilder With(string id, string id2)
        {
            RegisterIndefinitePair(id, id2);
            return this;
        }

        //Without
        public FilterBuilder Without<T0>() where T0 : struct
        {
            RegisterSimple<T0>(true);
            return this;
        }

        //Pairs
        public FilterBuilder Without<TKind, TTarget>() where TKind : struct where TTarget : struct
        {
            return Without(_world.GetHandleToType<TKind>(), _world.GetHandleToType<TTarget>());
        }

        public FilterBuilder Without<TKind>(EcsHandle target) where TKind : struct
        {
            return Without(_world.GetHandleToType<TKind>(), target);
        }

        public FilterBuilder Without(EcsHandle kind, EcsHandle target)
        {
            RegisterRelation(kind, target, true);
            return this;
        }

        public FilterBuilder Without(EcsHandle kind, string id)
        {
            RegisterIndefiniteTargetRelation(kind, id, true);
            return this;
        }

        public FilterBuilder Without(string id, EcsHandle target)
        {
            RegisterIndefiniteKindRelation(id, target, true);
            return this;
        }

        public FilterBuilder Without(string id, string id2)
        {
            RegisterIndefinitePair(id, id2, true);
            return this;
        }

        public FilterBuilder ResetSource()
        {
            currentSource = defaultSourceId;
            return this;
        }

        public FilterBuilder SetSource(string target)
        {
            currentSource = GetOrRegisterIdentifier(target);
            return this;
        }

        public Query Build()
        {
            var sources = new Source[_nodes.Count];
            for (int i = 0; i < _nodes.Count; i++)
            {
                var terms = _nodes[i].Terms;
                int inc = 0;
                int exc = 0;
                int complex = 0;
                for (int j = 0; j < terms.Count; j++)
                {
                    if (terms[j].Exclude)
                    {
                        exc++;
                    }
                    else
                    {
                        inc++;
                    }
                    if (terms[j].TargetSource != null || terms[j].KindSource != null)
                    {
                        complex++;
                    }
                }
                ulong[] include = new ulong[inc];
                ulong[] exclude = new ulong[exc];
                Term[] complexTerms = new Term[complex];
                for (int j = terms.Count - 1; j >= 0; j--)
                {
                    if (terms[j].Exclude)
                    {
                        exclude[--exc] = terms[j].Match.Id;
                    }
                    else
                    {
                        include[--inc] = terms[j].Match.Id;
                    }
                    if (terms[j].TargetSource != null || terms[j].KindSource != null)
                    {
                        complexTerms[--complex] = terms[j];
                    }
                }
                Debug.Assert(inc == 0);
                Debug.Assert(exc == 0);
                Debug.Assert(complex == 0);


                sources[i] = new Source(complexTerms, new TypeCollectionKey(include), new TypeCollectionKey(exclude));
            }
            return new Query(_world, sources);
        }
    }



    //    struct FieldAccessor<T> where T : struct
    //    {

    //    }

    //    struct QueryIterator
    //    {
    //        private readonly Query _query;

    //        public int Current => 0;

    //        public bool MoveNext()
    //        {
    //            return true;
    //        }

    //        public FieldAccessor<T> Read<T>() where T : struct
    //        {
    //            return default;
    //        }

    //        public FieldAccessor<T> ReadWrite<T>() where T : struct
    //        {
    //            return default;
    //        }
    //    }

    //    public class A
    //    {
    //        readonly EcsWorld _world;
    //        readonly Node[] _nodes;
    //        readonly BitMask[] _matchingArchetypesCache;

    //        internal A(EcsWorld world, Node[] nodes)
    //        {
    //            _world = world;
    //            _nodes = nodes;
    //            _matchingArchetypesCache = new BitMask[nodes.Length];
    //        }

    //        struct Info
    //        {
    //            public long Remainder;
    //            public int Index;
    //            public int node;
    //            public int arch;

    //            public Info(long remainder, int index, int node)
    //            {
    //                Remainder = remainder;
    //                Index = index;
    //                this.node = node;
    //            }
    //        }
    //        public void Query<T0, T1, T2>(ReadOnlySpan<int> sources, ReadOnlySpan<ulong> handles, EntityAction<T0, T1, T2> body) where T0 : struct where T1 : struct where T2 : struct
    //        {
    //            for (int i = 0; i < _nodes.Length; i++)
    //            {
    //                //init cache
    //                GetMatches(i, null);
    //            }
    //            int head = 0;
    //            Span<Info> stack = stackalloc Info[_nodes.Length];
    //            ref var current = ref stack[head];
    //            stack[head++] = new Info();

    //            var archetypes = _matchingArchetypesCache[current.node].Bits;
    //            for (current.Index = 0; current.Index < archetypes.Length; current.Index++)
    //            {
    //                current.Remainder = (long)archetypes[current.Index];
    //                while (current.Remainder != 0)
    //                {
    //                    int validArchetypeId = current.Index * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(current.Remainder);
    //                    current.Remainder ^= current.Remainder & -current.Remainder;
    //                    current.arch = validArchetypeId;
    //                    var arch = _world._archetypes[validArchetypeId];
    //                    for (int i = 0; i < _nodes[current.node].ComplexTerms.Length; i++)
    //                    {
    //                        var term = _nodes[current.node].ComplexTerms[i];
    //                        bool uncertainSource = term.Src.HasValue;
    //                        bool uncertainTarget = term.Dest.HasValue;
    //                        if (uncertainSource && uncertainTarget)
    //                        {
    //                            throw new ArgumentException("Only single uncertain term supported atm.");
    //                        }
    //                        else if (uncertainTarget)
    //                        {
    //                            var mask = _matchingArchetypesCache[term.Dest!.Value];
    //                            for (int j = 0; j < mask.Bits.Length; j++)
    //                            {
    //                                long rem = (long)archetypes[j];
    //                                while (rem != 0)
    //                                {
    //                                    int testArchId = j * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(rem);
    //                                    rem ^= rem & -rem;
    //                                    arch
    //                                }
    //                            }
    //                            arch.Key.ForeachTarget(term.Match, (e) =>
    //                            {
    //                                ref EntityIndex entityIndex = ref _world.GetEntityIndex(e);
    //                                validTargets.SetBit(entityIndex.Archetype.Id);
    //                            });
    //                        }
    //                        else if (uncertainSource)
    //                        {
    //                            arch.Key.ForeachSource(term.Match, (e) =>
    //                            {
    //                                ref EntityIndex entityIndex = ref _world.GetEntityIndex(e);
    //                                validTargets.SetBit(entityIndex.Archetype.Id);
    //                            });
    //                        }

    //                    }

    //                    _world._archetypes[validArchetypeId].;
    //                    //var arch0 = _world._archetypes[id];
    //                    //var arch1 = _world._archetypes[id];
    //                    //var arch2 = _world._archetypes[id];

    //                    //foreach (var (entity0, tableIndex0) in arch0.TableIndices.Span)
    //                    //{
    //                    //    foreach (var (entity1, tableIndex1) in arch1.TableIndices.Span)
    //                    //    {
    //                    //        foreach (var (entity2, tableIndex2) in arch2.TableIndices.Span)
    //                    //        {
    //                    //            body(entity,
    //                    //                ref arch.Table.GetRefAt<T0>(tableIndex, handles[0]),
    //                    //                ref arch.Table.GetRefAt<T1>(tableIndex, handles[1]),
    //                    //                ref arch.Table.GetRefAt<T2>(tableIndex, handles[2]));
    //                    //        }
    //                    //    }
    //                    //}
    //                }
    //            }
    //        }

    //        BitMask GetMatches(int nodeId, BitMask? mask)
    //        {
    //            ref Node node = ref _nodes[nodeId];
    //            BitMask validArchetypes;
    //            if (_matchingArchetypesCache[nodeId] != null)
    //            {
    //                validArchetypes = _matchingArchetypesCache[nodeId];
    //            }
    //            else
    //            {
    //                validArchetypes = new();
    //            }


    //            _world.GetArchetypesWith(node.WithKey, validArchetypes, true);
    //            _world.FilterArchetypesWithout(node.WithoutKey, validArchetypes);
    //            //We have all archetypes that match the simple query terms
    //            for (int i = 0; i < node.ComplexTerms.Length; i++)
    //            {
    //                //Traverse the relations layed out in the complex terms
    //                //then gather the archetypes that match these terms
    //                if (node.ComplexTerms[i].Dest != null && node.ComplexTerms[i].Src != null)
    //                {
    //                    throw new ArgumentException("Only single uncertain term supported atm.");
    //                }
    //                else if (node.ComplexTerms[i].Dest != null)
    //                {
    //                    ProcessTerm(node.ComplexTerms[i], validArchetypes, node.ComplexTerms[i].Dest!.Value, true, false);
    //                }
    //                else if (node.ComplexTerms[i].Src != null)
    //                {
    //                    ProcessTerm(node.ComplexTerms[i], validArchetypes, node.ComplexTerms[i].Src!.Value, false, true);
    //                }
    //            }
    //            if (mask != null)
    //            {
    //                validArchetypes.AndBits(mask);
    //            }
    //            return validArchetypes;
    //        }

    //        private void ProcessTerm(Term term, BitMask validArchetypes, int nodeIndex, bool uncertainTarget, bool uncertainSource)
    //        {
    //            var validTargets = new BitMask();
    //            var archetypes = validArchetypes.Bits;
    //            for (int idx = 0; idx < archetypes.Length; idx++)
    //            {
    //                long bitItem = (long)archetypes[idx];
    //                while (bitItem != 0)
    //                {
    //                    int id = idx * (sizeof(ulong) * 8) + BitOperations.TrailingZeroCount(bitItem);
    //                    bitItem ^= bitItem & -bitItem;
    //                    if (uncertainSource && uncertainTarget)
    //                    {
    //                        throw new ArgumentException("Only single uncertain term supported atm.");
    //                    }
    //                    else if (uncertainTarget)
    //                    {
    //                        _world._archetypes[id].Key.ForeachTarget(term.Match, (e) =>
    //                        {
    //                            ref EntityIndex entityIndex = ref _world.GetEntityIndex(e);
    //                            validTargets.SetBit(entityIndex.Archetype.Id);
    //                        });
    //                    }
    //                    else if (uncertainSource)
    //                    {
    //                        _world._archetypes[id].Key.ForeachSource(term.Match, (e) =>
    //                        {
    //                            ref EntityIndex entityIndex = ref _world.GetEntityIndex(e);
    //                            validTargets.SetBit(entityIndex.Archetype.Id);
    //                        });
    //                    }
    //                }
    //            }
    //            var matchingArchetypes = GetMatches(nodeIndex, validTargets);
    //            _matchingArchetypesCache[nodeIndex] = matchingArchetypes;
    //        }
    //    }

    //    public sealed partial class Query<T0, T1> : Query where T0 : struct where T1 : struct
    //    {
    //        readonly EcsWorld _world;
    //        readonly Node[] _nodes;
    //        readonly int[][] _caches;


    //        internal Query(EcsWorld world, Node[] nodes)
    //        {
    //            _world = world;
    //            _nodes = nodes;
    //            _caches = new int[nodes.Length][];
    //            Init();
    //        }



    //        void No()
    //        {
    //            bool initial = true;
    //            PooledList<int> pooledList = new PooledList<int>();

    //            _world.GetArchetypesWith(, pooledList, initial)
    //                if (initial)
    //            {
    //                pooledList.
    //                initial = false;
    //            }
    //        }

    //        void LMAOUUUU()
    //        {
    //            List<Archetype> arches = [.. _world._archetypes.Span];

    //            bool complete = false;
    //            int currentNodeIndex = 0;
    //            int currentTermIndex = 0;
    //            while (currentNodeIndex < _nodes.Length && !complete)
    //            {
    //                var term = _nodes[currentNodeIndex].Terms[currentTermIndex];
    //                if (!IsValid(term,))
    //                {

    //                    continue;
    //                }
    //                if (currentTermIndex >= _nodes[currentNodeIndex].Terms.Length)
    //                {
    //                    currentNodeIndex++;
    //                    currentTermIndex = 0;
    //                }
    //                else
    //                {
    //                    currentTermIndex++;
    //                }
    //            }
    //        }

    //        bool IsValid(Term term, Archetype archetype)
    //        {
    //            return archetype.Has(term.Match);
    //        }

    //        public void Foreach2(EntityAction<T0, T1> body, ReadOnlySpan<int> sources)
    //        {
    //            int[] trail = new int[_nodes.Length];
    //            int[] backingArray = ArrayPool<int>.Shared.Rent(_world.archetypeCount);
    //            int componentCount = FilterNodeSelf(_nodes[0], backingArray);
    //            var span = backingArray.AsSpan(0, componentCount);
    //            var validArchetypes = _caches[0] = span.ToArray();
    //            DoWork(trail, 0, span);
    //            ArrayPool<int>.Shared.Return(backingArray);
    //        }

    //        void DoWork(int currentNode, HashSet<int> validArchetypes)
    //        {
    //            Span<uint> ids = stackalloc uint[1];

    //            foreach (int id in validArchetypes)
    //            {
    //                var arch = _world._archetypes[id];
    //                var terms = _nodes[currentNode].Terms;
    //                for (int j = 0; j < terms.Length; j++)
    //                {
    //                    var term = terms[j];
    //                    if (!IsComplex(term))
    //                    {
    //                        continue;
    //                    }
    //                    int count;
    //                    while (!arch.Key.TryGetTargets(term.Match, ids, out count))
    //                    {
    //                        //span was too small
    //                        if (count != 0)
    //                        {
    //#pragma warning disable CA2014 // Do not use stackalloc in loops
    //                            ids = stackalloc uint[ids.Length * 2];
    //#pragma warning restore CA2014 // Do not use stackalloc in loops
    //                        }
    //                        else
    //                        {
    //                            break;
    //                        }
    //                    }
    //                    //Term did not match archetype (Should never be the case)
    //                    if (count == 0)
    //                    {
    //                        break;
    //                    }
    //                    Dictionary<int, HashSet<int>> candidateArchetypes = new();
    //                    for (int k = 0; k < count; k++)
    //                    {
    //                        ref var ent = ref _world._entities.TryGetRefAt(ids[k]);
    //                        if (Unsafe.IsNullRef(ref ent))
    //                        {
    //                            continue;
    //                        }
    //                        candidateArchetypes.Add(, ent.ArchetypeIndex);
    //                    }
    //                }
    //            }



    //        }

    //        void Init()
    //        {
    //            for (int i = 0; i < _nodes.Length; i++)
    //            {
    //                int[] backingArray = ArrayPool<int>.Shared.Rent(_world.archetypeCount);
    //                int componentCount = FilterNodeSelf(_nodes[i], backingArray);
    //                var span = backingArray.AsSpan(0, componentCount);
    //                if (_caches[i] != null)
    //                {
    //                    Query<T0, T1>.FilterAnd(ref componentCount, ref span, _caches[i]);
    //                }
    //                _caches[i] = backingArray.AsSpan(0, componentCount).ToArray();
    //                ArrayPool<int>.Shared.Return(backingArray);
    //            }
    //        }

    //        public void Foreach(EntityAction<T0, T1> body, ReadOnlySpan<int> sources)
    //        {
    //            var originArchetypes = _caches[0];
    //            for (int i = 0; i < originArchetypes.Length; i++)
    //            {
    //                var arch = _world._archetypes[originArchetypes[i]];
    //                var terms = _nodes[0].Terms;
    //                for (int j = 0; j < terms.Length; j++)
    //                {
    //                    var term = terms[j];
    //                    if (!IsComplex(term))
    //                    {
    //                        continue;
    //                    }
    //                    //Match all complex terms (up/down traversal, specific sources)
    //                    if (term.Dest.HasValue)
    //                    {
    //                        var destNodeGroup = term.Dest.Value;
    //                        var cache = _caches[destNodeGroup];
    //                        for (int n = 0; n < cache.Length; n++)
    //                        {
    //                            var archId = cache[n];
    //                            var arch2 = _world._archetypes[archId];


    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        static bool IsComplex(Term term)
    //        {
    //            return term.Src != null || term.Dest != null || (term.Target != Target.Self || term.Target != Target.Default);
    //        }

    //        //void Run(EntityAction<T0> action)
    //        //{
    //        //    int[] backingArray = ArrayPool<int>.Shared.Rent(_world.archetypeCount);
    //        //    int count = FilterNode(_nodes[0], backingArray);
    //        //    var span = backingArray.AsSpan(0, count);
    //        //    var handle0 = _world.GetHandleToType<T0>();
    //        //    foreach (var arch in span)
    //        //    {
    //        //        var archetype = _world._archetypes[arch];
    //        //        var indices = archetype.TableIndices.Span;
    //        //        var componentIndex0 = archetype.Table.TypeIndices[handle0.Id];
    //        //        var componentArray0 = archetype.Table.GetComponentArray<T0>(componentIndex0);
    //        //        for (int i = 0; i < indices.Length; i++)
    //        //        {

    //        //            action(indices[i].entity, ref componentArray0[indices[i].tableIndex]);
    //        //        }
    //        //    }
    //        //    ArrayPool<int>.Shared.Return(backingArray);
    //        //}

    //        int FilterNodeSelf(Node node, int[] backingArray)
    //        {
    //            //Initialize as all possible archetypes            
    //            int count = _world.archetypeCount;
    //            Span<int> possibleArchetypes = backingArray.AsSpan(0, count);
    //            for (int i = 0; i < possibleArchetypes.Length; i++)
    //            {
    //                possibleArchetypes[i] = i;
    //            }

    //            //constrain based on terms on the current node
    //            for (int i = 0; i < node.Terms.Length; i++)
    //            {
    //                FilterSimpleTerm(node, ref count, ref possibleArchetypes, i);
    //            }
    //            return count;
    //        }

    //        private void FilterSimpleTerm(Node node, ref int count, ref Span<int> possibleArchetypes, int currentTerm)
    //        {
    //            EcsHandle match = node.Terms[currentTerm].Match;
    //            bool exclude = node.Terms[currentTerm].Exclude;
    //            Target target = node.Terms[currentTerm].Target;
    //            if (target.HasFlag(Target.Self))
    //            {
    //                //The term is not a pair or is a definite relation
    //                if (!match.IsPair || (match.Entity != _world.AnyEntity.Entity && match.Target != _world.AnyEntity.Entity))
    //                {
    //                    FilterId(ref count, ref possibleArchetypes, match.Id, exclude);
    //                }
    //                else //The term is a relation with "ANY" term
    //                {
    //                    if (match.Entity != _world.AnyEntity.Entity)
    //                    {
    //                        if (_world._archetypePairMap.TryGetValue(match.Entity, out var ids))
    //                        {
    //                            //ids contains all types that contain the source of this term in a relation
    //                            foreach (var item in ids)
    //                            {
    //                                FilterId(ref count, ref possibleArchetypes, item, exclude);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            count = 0;
    //                            possibleArchetypes = possibleArchetypes.Slice(0, count);
    //                        }
    //                    }
    //                    if (match.Target != _world.AnyEntity.Entity)
    //                    {
    //                        if (_world._archetypePairMap.TryGetValue(match.Target, out var ids))
    //                        {
    //                            //ids contains all types that contain the target of this term in a relation
    //                            foreach (var item in ids)
    //                            {
    //                                FilterId(ref count, ref possibleArchetypes, item, exclude);
    //                            }
    //                        }
    //                        else
    //                        {
    //                            count = 0;
    //                            possibleArchetypes = possibleArchetypes.Slice(0, count);
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        private static void FilterAnd(ref int count, ref Span<int> possibleArchetypes, Span<int> possibleArchetypes2)
    //        {
    //            for (int j = 0; j < count; j++)
    //            {
    //                if (!possibleArchetypes2.Contains(possibleArchetypes[j]))
    //                {
    //                    possibleArchetypes[j] = possibleArchetypes[--count];
    //                    j--;
    //                }
    //            }
    //            possibleArchetypes = possibleArchetypes.Slice(0, count);
    //        }

    //        private void FilterId(ref int count, ref Span<int> possibleArchetypes, ulong id, bool exclude)
    //        {
    //            var other = _world._archetypeTypeMap[id].Span;
    //            for (int j = 0; j < count; j++)
    //            {
    //                if (exclude)
    //                {
    //                    if (other.Contains(possibleArchetypes[j]))
    //                    {
    //                        possibleArchetypes[j] = possibleArchetypes[--count];
    //                        j--;
    //                    }
    //                }
    //                else
    //                {
    //                    if (!other.Contains(possibleArchetypes[j]))
    //                    {
    //                        possibleArchetypes[j] = possibleArchetypes[--count];
    //                        j--;
    //                    }
    //                }
    //            }
    //            possibleArchetypes = possibleArchetypes.Slice(0, count);
    //        }
    //    }



    //    //public Filter Filter(FilterBuilder builder)
    //    //{
    //    //	_archetypePairMap
    //    //}


    //    record struct Position;
    //    record struct Velocity;
    //    record struct Faction;
    //    record struct ChildOf;
    //    record struct Style;
    //    record struct Leader;
    //    record struct Spaceship;
    //    record struct DockedTo;
    //    record struct ParentOf;
    //    void Test()
    //    {
    //        var filter = CreateFilter().With<Position>().With<Velocity>().Without<Faction>().Build();
    //        filter.Foreach((EcsHandle handle, ref Position pos, ref Velocity vel) =>
    //        {

    //        });

    //        var earth = CreateEntity<Position>();

    //        var sp = CreateEntity<Spaceship>();
    //        sp.AddRelation<DockedTo>(earth);

    //        var dockedSpaceships = CreateFilter()
    //           .With<Spaceship>()
    //           .With<DockedTo>("location")
    //           .Build();

    //        var elementFilter = CreateFilter()
    //            .With<Position>()
    //            .With<Style>().Self().Up<ChildOf>()
    //            .TraverseUp<ChildOf>("parent") //any parent with faction and style
    //            .SetSource("parent")
    //            .With<Faction>()
    //            .Build();

    //        var elementFilter2 = CreateFilter()
    //            .With<Position>()
    //            .TraverseUp<ChildOf>("parent")
    //            .SetSource("parent") //same parent with faction and style
    //            .With<Faction>()
    //            .With<Style>()
    //            .Build();

    //        //Working rn
    //        var elementFilter3 = CreateFilter()
    //            .With<Position>()
    //            .With<Style>().Self()
    //            .Without<ChildOf>()
    //            .With<Leader, Any>()
    //            .Build();

    //        var elementFilter4 = CreateFilter()
    //          .With<Position>() //my pos
    //          .With<Position>().UpCascade<ChildOf>() //parent pos
    //          .Build();

    //        //start at 
    //        var elementFilter5 = CreateFilter()
    //          .With<Position>() //my pos
    //          .With<Position>().DownCascade<ChildOf>() //children pos
    //          .Build();

    //        Term[] terms = [new()];
    //    }
}
