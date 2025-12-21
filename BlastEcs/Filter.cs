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
    public void Inc<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        _with.Add(_world.GetHandleToType<T0>().Id);
    }

    public void Inc(EcsHandle kind, EcsHandle target)
    {
        _with.Add(new EcsHandle(kind, target).Id);
    }

    public void Inc<TKind>(EcsHandle target) where TKind : struct
    {
        _with.Add(new EcsHandle(_world.GetHandleToType<TKind>(), target).Id);
    }

    public void IncRelation<TKind, TTarget>() where TKind : struct where TTarget : struct
    {
        Inc<TKind>(_world.GetHandleToType<TTarget>());
    }

    [Variadic(nameof(T0), EcsWorld.VariadicCount)]
    public void Exc<T0>() where T0 : struct
    {
        // [Variadic: CopyLines()]
        _without.Add(_world.GetHandleToType<T0>().Id);
    }

    public void Exc(EcsHandle kind, EcsHandle target)
    {
        _without.Add(new EcsHandle(kind, target).Id);
    }

    public void Exc<TKind>(EcsHandle target) where TKind : struct
    {
        _without.Add(new EcsHandle(_world.GetHandleToType<TKind>(), target).Id);
    }

    public void ExcRelation<TKind, TTarget>() where TKind : struct where TTarget : struct
    {
        Exc<TKind>(_world.GetHandleToType<TTarget>());
    }

    public Filter Build()
    {
        return new Filter(_world, new([.. _with]), new([.. _without]));
    }
}

public sealed class Filter
{
    public readonly EcsWorld World;
    public readonly TypeCollectionKey Inc;
    public readonly TypeCollectionKey Exc;

    public Filter(EcsWorld ecsWorld, TypeCollectionKey inc, TypeCollectionKey exc)
    {
        World = ecsWorld;
        Inc = inc;
        Exc = exc;
    }

    public void Each(Action<EcsHandle> action)
    {
        World.InvokeFilter(this, action);
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
