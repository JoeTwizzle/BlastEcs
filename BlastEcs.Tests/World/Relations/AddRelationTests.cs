using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Relations;

internal class AddRelationTests
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
    public async Task AddPair()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.HasRelation<TestComponent, TestComponent>(e)).IsFalse();
            world.AddPair<TestComponent, TestComponent>(e);
            await Assert.That(world.HasRelation<TestComponent, TestComponent>(e)).IsTrue();
        }
    }

    [Test]
    public async Task AddRelation()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e, target)).IsFalse();
            world.AddRelation<TestComponent>(e, target);
            await Assert.That(world.Has<TestComponent>(e, target)).IsTrue();
            world.GetRef<TestComponent>(e, target);
        }
    }

    [Test]
    public async Task AddRelationEntity()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            var identifier = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has(e, identifier, target)).IsFalse();
            world.AddRelation(e, identifier, target);
            await Assert.That(world.Has(e, identifier, target)).IsTrue();
        }
    }
}
