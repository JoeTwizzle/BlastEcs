using BlastEcs.Builtin;
using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Relations;

internal class AnyRelationTests
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
    public async Task HasAnyRelationToEntity()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            var identifier = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();

            await Assert.That(world.Has<Any>(e)).IsFalse();
            await Assert.That(world.Has<Any>(e, target)).IsFalse();
            await Assert.That(world.Has(e, identifier, world.AnyEntity)).IsFalse();
            await Assert.That(world.Has(e, identifier, target)).IsFalse();

            world.AddRelation(e, identifier, target);

            await Assert.That(world.Has<Any>(e)).IsTrue();
            await Assert.That(world.Has<Any>(e, target)).IsTrue();
            await Assert.That(world.Has(e, identifier, world.AnyEntity)).IsTrue();
            await Assert.That(world.Has(e, identifier, target)).IsTrue();

            world.DestroyEntity(target);

            await Assert.That(world.Has<Any>(e)).IsFalse();
            await Assert.That(world.Has<Any>(e, target)).IsFalse();
            await Assert.That(world.Has(e, identifier, world.AnyEntity)).IsFalse();
            await Assert.That(world.Has(e, identifier, target)).IsFalse();
        }
    }
}
