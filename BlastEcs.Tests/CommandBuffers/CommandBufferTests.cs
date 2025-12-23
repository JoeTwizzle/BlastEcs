using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Tests.CommandBuffers;

internal class CommandBufferTests
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
    public async Task CommandCreate()
    {
        FilterBuilder fb = new(world);
        fb.Inc<TestComponent, TestTag>();
        var filter = fb.Build();

        EcsCommandBuffer cmd = new(world);
        cmd.Begin();
        for (int i = 0; i < EntityCount; i++)
        {
            cmd.Create<TestComponent, TestTag>();
            cmd.Create<TestComponent>();
        }
        cmd.End();
        cmd.Execute();

        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });

        await Assert.That(count).IsEqualTo(EntityCount);
    }

    [Test]
    public async Task CommandCreateBulk()
    {
        FilterBuilder fb = new(world);
        fb.Inc<TestComponent, TestTag>();
        var filter = fb.Build();

        EcsCommandBuffer cmd = new(world);
        cmd.Begin();
        cmd.Create<TestComponent, TestTag>(EntityCount);
        cmd.Create<TestComponent>(EntityCount);
        cmd.End();
        cmd.Execute();

        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });

        await Assert.That(count).IsEqualTo(EntityCount);
    }

    [Test]
    public async Task CommandAdd()
    {
        FilterBuilder fb = new(world);
        fb.Inc<TestComponent, TestTag>();
        var filter = fb.Build();

        EcsCommandBuffer cmd = new(world);
        cmd.Begin();
        for (int i = 0; i < EntityCount; i++)
        {
            cmd.Create<TestComponent, TestTag>();
            var ent = cmd.Create<TestComponent>();
            cmd.Add<TestTag>(ent);
        }
        cmd.End();
        cmd.Execute();

        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });

        await Assert.That(count).IsEqualTo(EntityCount * 2);
    }

    [Test]
    public async Task CommandAddBulk()
    {
        FilterBuilder fb = new(world);
        fb.Inc<TestComponent, TestTag>();
        var filter = fb.Build();

        EcsCommandBuffer cmd = new(world);
        cmd.Begin();
        cmd.Create<TestComponent, TestTag>(EntityCount);
        var ent = cmd.Create<TestComponent>(EntityCount);
        cmd.Add<TestTag>(ent);
        cmd.End();
        cmd.Execute();

        int count = 0;
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });

        await Assert.That(count).IsEqualTo(EntityCount * 2);
    }

    [Test]
    public async Task CommandIterate()
    {
        EcsCommandBuffer cmd = new(world);
        FilterBuilder fb = new(world);
        fb.Inc<TestComponent, TestTag>();
        world.CreateEntity<TestComponent, TestTag>();
        var filter = fb.Build();
        int count = 0;
        cmd.Begin();
        filter.Each(async (e) =>
        {
            cmd.Create<TestComponent, TestTag>();
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });
        await Assert.That(count).IsEqualTo(1);
        cmd.End();
        cmd.Execute();
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task CommandIterateModifyReal()
    {
        EcsCommandBuffer cmd = new(world);
        FilterBuilder fb = new(world);
        fb.Inc<TestComponent, TestTag>();
        world.CreateEntity<TestComponent, TestTag>();
        var filter = fb.Build();
        int count = 0;
        cmd.Begin();
        filter.Each(async (e) =>
        {
            var vEnt = cmd.Create(e);
            cmd.Remove<TestComponent>(vEnt);
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });
        await Assert.That(count).IsEqualTo(1);
        cmd.End();
        cmd.Execute();
        filter.Each(async (e) =>
        {
            await Assert.That(e.Has<TestComponent, TestTag>()).IsTrue();
            count++;
        });
        await Assert.That(count).IsEqualTo(1);
    }
}
