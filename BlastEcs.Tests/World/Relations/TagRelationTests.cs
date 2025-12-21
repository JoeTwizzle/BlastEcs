using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Relations;

internal class TagRelationTests
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
    public async Task AddTagRelation()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity<TestTagRelation>();
            var target = world.CreateEntity<TestComponent>();
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestTagRelation>(e)).IsTrue();
            await Assert.That(() => world.GetRef<TestTagRelation>(e)).ThrowsNothing();
            world.AddRelation<TestTagRelation>(e, target);
            await Assert.That(world.Has<TestTagRelation>(e, target)).IsTrue();
            Assert.Throws<ArgumentException>(() => world.GetRef<TestTagRelation>(e, target));
            await Assert.That(world.Has<TestTagRelation>(e, target)).IsTrue();
        }
    }
}
