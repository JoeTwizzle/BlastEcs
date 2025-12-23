using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BlastEcs.Benchmarks;

[HtmlExporter]
[MemoryDiagnoser]
public class AddRemoveBenchmark
{
    [Params(1000000, 10000000)] public int Amount;

    private static EcsWorld _world;
    private static List<EcsHandle> _entities;

    [IterationSetup]
    public void Setup()
    {
        _world = new EcsWorld();

        _entities = new List<EcsHandle>(Amount);
        for (var index = 0; index < Amount; index++)
        {
            _entities.Add(_world.CreateEntity<Matrix4x4, Vector3>());
        }
    }
    [IterationCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }

    [Benchmark]
    public void Add()
    {
        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            _world.Add<Vector2>(entity);
        }
    }

    [Benchmark]
    public void Remove()
    {
        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            _world.Remove<Matrix4x4>(entity);
        }
    }
}
