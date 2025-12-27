using BlastEcs.Builtin;
using BlastEcs.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs;

public sealed partial class FilterBuilder
{
    private readonly EcsWorld _world;
    private readonly HashSet<ulong> _with;
    private readonly HashSet<ulong> _without;

    public FilterBuilder(EcsWorld world)
    {
        _world = world;
        _with = [];
        _without = [];
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public FilterBuilder Inc<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        _with.Add(_world.GetHandleToType<T0>().Id);
        return this;
    }

    public FilterBuilder Inc(EcsHandle kind, EcsHandle target)
    {
        _with.Add(new EcsHandle(kind, target).Id);
        return this;
    }

    public FilterBuilder Inc<TKind>(EcsHandle target) where TKind : struct
    {
        _with.Add(new EcsHandle(_world.GetHandleToType<TKind>(), target).Id);
        return this;
    }

    public FilterBuilder IncRelation<TKind, TTarget>() where TKind : struct where TTarget : struct
    {
        Inc<TKind>(_world.GetHandleToType<TTarget>());
        return this;
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public FilterBuilder Exc<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        _without.Add(_world.GetHandleToType<T0>().Id);
        return this;
    }

    public FilterBuilder Exc(EcsHandle kind, EcsHandle target)
    {
        _without.Add(new EcsHandle(kind, target).Id);
        return this;
    }

    public FilterBuilder Exc<TKind>(EcsHandle target) where TKind : struct
    {
        _without.Add(new EcsHandle(_world.GetHandleToType<TKind>(), target).Id);
        return this;
    }

    public FilterBuilder ExcRelation<TKind, TTarget>() where TKind : struct where TTarget : struct
    {
        Exc<TKind>(_world.GetHandleToType<TTarget>());
        return this;
    }

    public SimpleQueryKey Build()
    {
        return new SimpleQueryKey(_world, new([.. _with]), new([.. _without]));
    }
}

public sealed class SimpleQueryKey : IEquatable<SimpleQueryKey>
{
    public readonly EcsWorld World;
    public readonly TypeCollectionKey Inc;
    public readonly TypeCollectionKey Exc;

    public SimpleQueryKey(EcsWorld ecsWorld, TypeCollectionKey inc, TypeCollectionKey exc)
    {
        World = ecsWorld;
        Inc = inc;
        Exc = exc;
    }

    public bool Matches(Archetype archetype)
    {
        var key = archetype.Key;
        return key.Contains(Inc) && !key.Contains(Exc);
    }

    public void Each(Action<EcsHandle> action)
    {
        World.InvokeFilter(this, action);
    }

    public void Each2(Action<EcsHandle> action)
    {
        World.InvokeFilter2(this, action);
    }

    public bool Equals(SimpleQueryKey? other)
    {
        return other?.Inc == Inc && other.Exc == Exc;
    }

    public override bool Equals(object? obj)
    {
        return obj is SimpleQueryKey filter && Equals(filter);
    }

    public override int GetHashCode()
    {
        int hashCode = World.WorldId;
        hashCode = hashCode * 486187739 + Inc.GetHashCode();
        hashCode = hashCode * 486187739 + Exc.GetHashCode();
        return hashCode;
    }
}


//public class Query
//{
//    private readonly Filter filter;

//    public Query(Filter filter)
//    {
//        this.filter = filter;
//    }

//    public Iterator GetEnumerator()
//    {
//        return new Iterator();
//    }

//    public ref T Get<T>(EcsHandle ecsHandle) where T : struct
//    {
//        return ref ;
//    }

//    public struct Iterator
//    {


//        public EcsHandle Current => ;

//        public bool MoveNext()
//        {

//        }
//    }
//}

//class temp
//{
//    struct Faction;
//    struct AlliedWith;
//    struct TradesWith;
//    struct AtWar;
//    struct MemberOf;
//    struct MarriedTo;
//    struct Style;
//    struct Widget;
//    struct ChildOf;
//    void a()
//    {
//        new FilterBuilder(new())
//            .With<Faction>()
//            .With<AlliedWith>("ally")
//            .SetSource("ally")
//            .With<TradesWith>("");


//        /* Faction($this),                    // Find all factions
//         * AlliesWith($this, $ally),          // with an ally $ally
//         * AtWar($ally, $other),              // which is at war with $other 
//         * MemberOf($person, $other),         // which has a family member $person
//         * MarriedTo($person, $marriedTo),    // who is married to $marriedTo
//         * MemberOf($marriedTo, $this)        // where $marriedTo is of the faction
//         */
//        var filter = new FilterBuilder(new())
//             .With<Faction>()                  // Find all factions
//             .With<AlliedWith>("ally")         // with an ally $ally
//             .SetSource("ally")
//             .With<AtWar>("other")            // which is at war with $other 
//             .SetSource("person")
//             .With<MemberOf>("other")         // which has a family member $person
//             .With<MarriedTo>("marriedTo")    // who is married to $marriedTo
//             .SetSource("marriedTo")
//             .With<MemberOf>("").Build();     // where $marriedTo is of the faction

//        var query = new Query(filter);

//(ref Position pos, ref Velocity vel) => 
//{
// 
//}

//        new FilterBuilder(new())
//            .With<Widget>()
//            .TraverseUp<ChildOf>()
//            .With<Style>()
//            .Build();
//    }
//}
