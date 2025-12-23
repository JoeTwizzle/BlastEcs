using BlastEcs.Tests.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
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

    [Test]
    public async Task RemoveComponent2()
    {
        var _world = new EcsWorld();

        var _entities = new List<EcsHandle>(EntityCount);
        for (var index = 0; index < EntityCount; index++)
        {
            _entities.Add(_world.CreateEntity<Matrix4x4, Vector3>());
        }

        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            _world.Add<Vector2>(entity);
        }

        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            _world.Remove<Matrix4x4>(entity);
        }
    }
}
