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

    [Test]
    public async Task IterateWithoutFilter()
    {
        var fb = new FilterBuilder(world);
        for (int i = 0; i < EntityCount; i++)
        {
            world.CreateEntity<TestComponent>();
            world.CreateEntity<TestComponent, TestTag>();
        }
        fb.Exc<TestTag>();
        var filter = fb.Build();
        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestTag>()).IsFalse();
            count++;
        });
        await Assert.That(count).IsLessThan(EntityCount * 2).And.IsGreaterThan(EntityCount);
    }

    [Test]
    public async Task IterateBlankFilter()
    {
        var fb = new FilterBuilder(world);
        for (int i = 0; i < EntityCount; i++)
        {
            world.CreateEntity<TestComponent>();
            world.CreateEntity<TestComponent, TestTag>();
        }
        var filter = fb.Build();
        int count = 0;
        filter.Each(async (e) =>
        {
            count++;
        });
        await Assert.That(count).IsGreaterThan(EntityCount * 2);
    }

    [Test]
    public async Task IterateNestedFilter()
    {
        const int EntityCount = Config.EntityCount / 10;
        var fb = new FilterBuilder(world);
        var fb2 = new FilterBuilder(world);
        for (int i = 0; i < EntityCount; i++)
        {
            world.CreateEntity<TestComponent>();
            world.CreateEntity<TestComponent, TestTag>();
        }
        fb.Inc<TestComponent>();
        fb2.Inc<TestComponent, TestTag>();
        var filter = fb.Build();
        var filter2 = fb2.Build();
        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent>()).IsTrue();
            filter2.Each(async (e) =>
            {
                await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
                count++;
            });
            count++;
        });
        await Assert.That(count).IsEqualTo(EntityCount * EntityCount * 2 + EntityCount * 2);
    }
}
