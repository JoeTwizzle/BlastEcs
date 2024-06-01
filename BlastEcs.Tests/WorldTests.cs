using BlastEcs.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlastEcs.Tests;

internal class WorldTests
{
    EcsWorld world;

    [SetUp]
    public void Setup()
    {
        world = new();
    }

    [Test]
    public void WorldCreateEmptyEntityTest()
    {
        var e = world.CreateEntity();
        Assert.That(world.IsAlive(e), Is.True);
    }

    [Test]
    public void WorldCreateManyEmptyEntitiesTest()
    {
        for (var i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity();
            Assert.That(world.IsAlive(e), Is.True);
        }
    }

    [Test]
    public void WorldCreateTestComponentTest()
    {
        var e = world.CreateEntity([typeof(TestComponent)]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestComponent>(e), Is.True);
        Assert.That(world.GetRef<TestComponent>(e).TestValue, Is.EqualTo(0));
    }

    [Test]
    public void WorldCreateManyTestComponentTest()
    {
        for (var i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([typeof(TestComponent)]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestComponent>(e), Is.True);
            Assert.That(world.GetRef<TestComponent>(e).TestValue, Is.EqualTo(0));
        }
    }

    [Test]
    public void WorldCreateTestTagTest()
    {
        var e = world.CreateEntity([typeof(TestTag)]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestTag>(e), Is.True);
        Assert.Throws<ArgumentException>(() => world.GetRef<TestTag>(e));
    }

    [Test]
    public void WorldCreateManyTestTagTest()
    {
        for (var i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([typeof(TestTag)]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestTag>(e), Is.True);
            Assert.Throws<ArgumentException>(() => world.GetRef<TestTag>(e));
        }
    }

    [Test]
    public void WorldAddTestTagTest()
    {
        var e = world.CreateEntity([]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestTag>(e), Is.False);
        world.Add<TestTag>(e);
        Assert.That(world.Has<TestTag>(e), Is.True);
    }

    [Test]
    public void WorldAddManyTestTagTest()
    {
        for (var i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestTag>(e), Is.False);
            world.Add<TestTag>(e);
            Assert.That(world.Has<TestTag>(e), Is.True);
        }
    }

    [Test]
    public void WorldRemoveTestTagTest()
    {
        var e = world.CreateEntity([typeof(TestTag)]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestTag>(e), Is.True);
        world.Remove<TestTag>(e);
        Assert.That(world.Has<TestTag>(e), Is.False);
    }

    [Test]
    public void WorldAddTestComponentTest()
    {
        var e = world.CreateEntity([]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestComponent>(e), Is.False);
        world.Add<TestComponent>(e);
        Assert.That(world.Has<TestComponent>(e), Is.True);
    }

    [Test]
    public void WorldAddManyTestComponentTest()
    {
        for (int i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestComponent>(e), Is.False);
            world.Add<TestComponent>(e);
            Assert.That(world.Has<TestComponent>(e), Is.True);
        }
    }

    [Test]
    public void WorldRemoveTestComponentTest()
    {
        var e = world.CreateEntity([typeof(TestComponent)]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestComponent>(e), Is.True);
        world.Remove<TestComponent>(e);
        Assert.That(world.Has<TestComponent>(e), Is.False);
    }

    [Test]
    public void WorldKillEmptyEntityTest()
    {
        var e = world.CreateEntity();
        Assert.That(world.IsAlive(e), Is.True);
        world.DestroyEntity(e);
        Assert.That(world.IsAlive(e), Is.False);
    }
}
