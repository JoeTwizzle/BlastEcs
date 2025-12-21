using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Entities;

internal class DestroyEntityTests
{
    const int EntityCount = Config.EntityCount;
    EcsWorld world;

    [Before(Test)]
    public void Setup()
    {
        world = new();
    }

    [After(Test)]
    public void Cleanup()
    {
        world.Dispose();
    }

    [Test]
    public async Task DestroyEmptyEntites()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity();
            await Assert.That(world.IsAlive(e)).IsTrue();
            world.DestroyEntity(e);
            await Assert.That(world.IsAlive(e)).IsFalse();
        }
    }

    [Test]
    public async Task DestroyComponentEntites()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity<TestComponent>();
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e)).IsTrue();
            world.DestroyEntity(e);
            await Assert.That(world.IsAlive(e)).IsFalse();
            await Assert.That(world.Has<TestComponent>(e)).IsFalse();
        }
    }
}
