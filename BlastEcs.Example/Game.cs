using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        //Run until any key is pressed
        while (!Console.KeyAvailable)
        {
            //var ball = world.CreateEntity<Position>();

            BounceSystem();
        }
    }

    void BounceSystem()
    {
        //world.All < B
    }
}
