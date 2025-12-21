using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Tags;

internal class AddTagTests
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
    public async Task AddTag()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestTag>(e)).IsFalse();
            world.Add<TestTag>(e);
            await Assert.That(world.Has<TestTag>(e)).IsTrue();
        }
    }

    [Test]
    public async Task AddTagDuplicate()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity<TestTag>();
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestTag>(e)).IsTrue();
            Assert.Throws<InvalidOperationException>(() => world.Add<TestTag>(e));
            await Assert.That(world.Has<TestTag>(e)).IsTrue();
        }
    }
}
