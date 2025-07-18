using BlastEcs.Builtin;

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
        var arch = world.GetOrCreateArchetype<TestComponent>();
        for (var i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity(arch);
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
        var arch = world.GetOrCreateArchetype<TestTag>();
        for (var i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity(arch);
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
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity([typeof(TestTag)]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestTag>(e), Is.True);
            world.Remove<TestTag>(e);
            Assert.That(world.Has<TestTag>(e), Is.False);
        }
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
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity([typeof(TestComponent)]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestComponent>(e), Is.True);
            world.Remove<TestComponent>(e);
            Assert.That(world.Has<TestComponent>(e), Is.False);
        }
    }

    [Test]
    public void WorldKillEmptyEntityTest()
    {
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity();
            Assert.That(world.IsAlive(e), Is.True);
            world.DestroyEntity(e);
            Assert.That(world.IsAlive(e), Is.False);
        }
    }

    [Test]
    public void WorldAddPairTest()
    {
        var e = world.CreateEntity([]);
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.HasRelation<TestComponent, TestComponent>(e), Is.False);
        world.AddPair<TestComponent, TestComponent>(e);
        Assert.That(world.HasRelation<TestComponent, TestComponent>(e), Is.True);
    }

    [Test]
    public void WorldAddRelationTest()
    {
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestComponent>(e, target), Is.False);
            world.AddRelation<TestComponent>(e, target);
            Assert.That(world.Has<TestComponent>(e, target), Is.True);
            world.GetRef<TestComponent>(e, target);
        }
    }

    [Test]
    public void WorldAddRemoveRelationTest()
    {
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has<TestComponent>(e, target), Is.False);
            world.AddRelation<TestComponent>(e, target);
            Assert.That(world.Has<TestComponent>(e, target), Is.True);
            world.DestroyEntity(target);
            Assert.That(world.Has<TestComponent>(e, target), Is.False);
        }
    }

    [Test]
    public void WorldAddRemoveRelationTest2()
    {
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity([]);
            var kind = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has(e, kind, target), Is.False);
            world.AddRelation(e, kind, target);
            Assert.That(world.Has(e, kind, target), Is.True);
            world.DestroyEntity(kind);
            world.DestroyEntity(target);
            Assert.That(world.Has(e, kind, target), Is.False);
        }
    }

    [Test]
    public void WorldAddRelationEntityTest()
    {
        for (int i = 0; i < 10; i++)
        {
            var e = world.CreateEntity([]);
            var identifier = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            Assert.That(world.IsAlive(e), Is.True);
            Assert.That(world.Has(e, identifier, target), Is.False);
            world.AddRelation(e, identifier, target);
            Assert.That(world.Has(e, identifier, target), Is.True);
        }
    }

    [Test]
    public void WorldHasAnyRelationEntityTest()
    {
        var e = world.CreateEntity([]);
        var identifier = world.CreateEntity([]);
        var target = world.CreateEntity([]);
        Assert.That(world.IsAlive(e), Is.True);

        Assert.That(world.Has<Any>(e), Is.False);
        Assert.That(world.Has<Any>(e, target), Is.False);
        Assert.That(world.Has(e, identifier, world.AnyEntity), Is.False);
        Assert.That(world.Has(e, identifier, target), Is.False);

        world.AddRelation(e, identifier, target);

        Assert.That(world.Has<Any>(e), Is.True);
        Assert.That(world.Has<Any>(e, target), Is.True);
        Assert.That(world.Has(e, identifier, world.AnyEntity), Is.True);
        Assert.That(world.Has(e, identifier, target), Is.True);
    }

    [Test]
    public void WorldCreateTestTagRelationTest()
    {
        var e = world.CreateEntity<TestTagRelation>();
        var target = world.CreateEntity<TestComponent>();
        Assert.That(world.IsAlive(e), Is.True);
        Assert.That(world.Has<TestTagRelation>(e), Is.True);
        Assert.DoesNotThrow(() => world.GetRef<TestTagRelation>(e));
        world.AddRelation<TestTagRelation>(e, target);
        Assert.That(world.Has<TestTagRelation>(e, target), Is.True);
        Assert.Throws<ArgumentException>(() => world.GetRef<TestTagRelation>(e, target));
    }
}