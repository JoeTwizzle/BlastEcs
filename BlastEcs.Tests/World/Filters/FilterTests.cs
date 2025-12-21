using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastEcs.Tests.World.Filters;

internal class FilterTests
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
    public async Task IterateFilter()
    {
        var fb = new FilterBuilder(world);
        for (int i = 0; i < EntityCount; i++)
        {
            world.CreateEntity<TestComponent>();
            world.CreateEntity<TestComponent, TestTag>();
        }
        fb.Inc<TestComponent>();
        var filter = fb.Build();
        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent>()).IsTrue();
            count++;
        });
        await Assert.That(count).IsEqualTo(EntityCount * 2);
    }
}
