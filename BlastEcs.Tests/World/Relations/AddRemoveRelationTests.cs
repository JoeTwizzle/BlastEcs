using BlastEcs.Tests.Data;

namespace BlastEcs.Tests.World.Relations;

internal class AddRemoveRelationTests
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
    public async Task AddRemoveRelation()
    {       
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has<TestComponent>(e, target)).IsFalse();
            world.AddRelation<TestComponent>(e, target);
            await Assert.That(world.Has<TestComponent>(e, target)).IsTrue();
            world.DestroyEntity(target);
            await Assert.That(world.Has<TestComponent>(e, target)).IsFalse();
        }
    }

    [Test]
    public async Task AddRemoveRelationEntity()
    {
        for (int i = 0; i < EntityCount; i++)
        {
            var e = world.CreateEntity([]);
            var kind = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            await Assert.That(world.IsAlive(e)).IsTrue();
            await Assert.That(world.Has(e, kind, target)).IsFalse();
            world.AddRelation(e, kind, target);
            await Assert.That(world.Has(e, kind, target)).IsTrue();
            world.DestroyEntity(kind);
            world.DestroyEntity(target);
            await Assert.That(world.Has(e, kind, target)).IsFalse();
        }
    }
}
