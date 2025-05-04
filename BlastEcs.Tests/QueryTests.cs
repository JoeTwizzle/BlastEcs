namespace BlastEcs.Tests;

class QueryTests
{
    EcsWorld world;

    [SetUp]
    public void Setup()
    {
        world = new();
    }
    record struct Faction;
    record struct AlliedWith;
    record struct AtWar;
    record struct MemberOf;
    record struct MarriedTo;
    [Test]
    public void TestMeDaddy()
    {
        var other1 = world.CreateEntity();
        var other2 = world.CreateEntity();



        var ally1 = world.CreateEntity();
        var ally2 = world.CreateEntity();

        var ally3 = world.CreateEntity();
        ally3.AddRelation<AtWar>(other1);
        var ally4 = world.CreateEntity();
        ally4.AddRelation<AtWar>(other1);
        var ally5 = world.CreateEntity();
        ally5.AddRelation<AtWar>(other1);
        ally5.AddRelation<AtWar>(other2);
        var ally6 = world.CreateEntity();
        ally6.AddRelation<AtWar>(other2);

        var e1 = world.CreateEntity<Faction>();
        e1.AddRelation<AlliedWith>(ally1);
        e1.AddRelation<AlliedWith>(ally4);
        var e2 = world.CreateEntity<Faction>();
        e2.AddRelation<AlliedWith>(ally2);
        e2.AddRelation<AlliedWith>(ally3);
        var e3 = world.CreateEntity<Faction>();
        e3.AddRelation<AlliedWith>(ally4);
        e3.AddRelation<AlliedWith>(ally5);

        var m1 = world.CreateEntity();
        var m2 = world.CreateEntity();
        m2.AddRelation<MemberOf>(e2);
        var m3 = world.CreateEntity();
        m3.AddRelation<MemberOf>(e3);
        var m4 = world.CreateEntity();
        m4.AddRelation<MemberOf>(e2);

        var p1 = world.CreateEntity();
        p1.AddRelation<MemberOf>(other2);
        p1.AddRelation<MarriedTo>(m3);
        var p2 = world.CreateEntity();
        p2.AddRelation<MemberOf>(other1);
        p2.AddRelation<MarriedTo>(m1);


        var query = world.CreateFilter()
               .With<Faction>()
               .With<AlliedWith>("ally")
               .SetSource("ally")
               .With<AtWar>("other")
               .SetSource("person")
               .With<MemberOf>("other")
               .With<MarriedTo>("marriedTo")
               .SetSource("marriedTo")
               .With<MemberOf>("this").Build();

        query.Init();
        query.StartMatch();
    }
}
