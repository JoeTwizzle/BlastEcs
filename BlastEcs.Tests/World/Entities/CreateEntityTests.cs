using BlastEcs.Builtin;
using BlastEcs.Tests.Data;


namespace BlastEcs.Tests.World.Entities;

public class CreateEntityTests
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
    public async Task CreateEmpty()
    {
        for (var i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity();
            await Assert.That(world.IsAlive(e)).IsTrue();
        }
    }

    [Test]
    public async Task CreateWithTestComponent()
    {
        for (var i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([typeof(TestComponent)]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e)).IsTrue();
            await Assert.That(world.GetRef<TestComponent>(e).TestValue).EqualTo(0);
        }
    }

    [Test]
    public async Task CreateWithTag()
    {
        for (var i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([typeof(TestTag)]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestTag>(e)).IsTrue();
            Assert.Throws<ArgumentException>(() => world.GetRef<TestTag>(e));
        }
    }
}
