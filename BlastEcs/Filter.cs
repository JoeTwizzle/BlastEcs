//using BlastEcs.Collections;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace BlastEcs;



//public sealed class Filter
//{
//    struct FilterSource
//    {
//        public readonly ulong[] IncludedIds;
//        public readonly ulong[] ExcludedIds;
//    }
//    EcsWorld world;
//    FilterSource[] filterSources;
//    struct FilterLink
//    {
//        public readonly ulong PartialLinkId;
//        public readonly bool IsFullyPartial;
//        public readonly bool IsKindPartial;
//    }

//    public Filter()
//    {
//        world.
//    }

//    public Iterator GetEnumerator()
//    {
//        return new Iterator();
//    }

//    public struct Iterator
//    {
//        public Iterator()
//        {
//            world.
//        }

//        public EcsHandle Current => ;

//        public bool MoveNext()
//        {

//        }
//    }
//}


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
