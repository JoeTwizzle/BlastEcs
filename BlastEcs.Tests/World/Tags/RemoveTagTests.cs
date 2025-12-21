using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Tags;

internal class RemoveTagTests
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
    public async Task RemoveTag()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([typeof(TestTag)]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestTag>(e)).IsTrue();
            world.Remove<TestTag>(e);
            await Assert.That(world.Has<TestTag>(e)).IsFalse();
        }
    }

    [Test]
    public async Task RemoveTagDuplicate()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([typeof(TestTag)]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestTag>(e)).IsTrue();
            world.Remove<TestTag>(e);
            await Assert.That(world.Has<TestTag>(e)).IsFalse();
            Assert.Throws<InvalidOperationException>(() => world.Remove<TestTag>(e));
            await Assert.That(world.Has<TestTag>(e)).IsFalse();
        }
    }
}
