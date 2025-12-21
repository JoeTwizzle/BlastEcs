using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Components;

internal class RemoveComponentTests
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
    public async Task RemoveComponent()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([typeof(TestComponent)]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e)).IsTrue();
            world.Remove<TestComponent>(e);
            await Assert.That(world.Has<TestComponent>(e)).IsFalse();
        }
    }
}
