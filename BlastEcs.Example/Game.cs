using System.Numerics;

namespace BlastEcs.Example;

struct Position { Vector2 pos; }

sealed class Game
{
    EcsWorld world;

    public Game()
    {
        world = new EcsWorld();
    }

    public void Run()
    {
        var world = new EcsWorld();
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity<Position>();
            var kind = world.CreateEntity<Position>();
            var target = world.CreateEntity<Position>();
            world.AddRelation(e, kind, target);
            world.DestroyEntity(kind);
            world.DestroyEntity(target);
        }
    }

    //var filter = World.Filter<Position>().SetSource(SceneGlobals).With<Score>();
    //returns (ref Position, ref Score)

    //var filter = World.Filter<Position>()
    //.Up<ChildOf>()    //check for that the current entity is pointed at by a ChildOf relation
    //.With<Position>() //check the entity pointing at the current entity has a position component
    //returns  (ref Position, ref Position) where position 2 is from the parent

    //var filter = World.Filter<Position>()
    //.Down<ParentOf>() //check for a child with a ParentOf relation to the current entity
    //.With<Position>() //check the entity pointing at the current entity has a position component
    //returns  (ref Position, ref Position) where position 1 is from the parent
    void BounceSystem()
    {

        //var filter = World.Filter<Position, Velocity>().Not<Frozen>();
        //foreach (var archetype in filter)
        //{
        //
        //}

        //
        //
    }
}
