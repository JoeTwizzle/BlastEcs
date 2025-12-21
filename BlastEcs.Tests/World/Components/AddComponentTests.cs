using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Components;

internal class AddComponentTests
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
    public async Task AddComponent()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e)).IsFalse();
            world.Add<TestComponent>(e);
            await Assert.That(world.Has<TestComponent>(e)).IsTrue();
        }
    }

    [Test]
    public async Task AddComponentByHandle()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e)).IsFalse();
            world.Add(e, new TypeCollectionKeyNoAlloc([world.GetHandleToInstantiableType<TestComponent>().Id]));
            await Assert.That(world.Has<TestComponent>(e)).IsTrue();
        }
    }
}
