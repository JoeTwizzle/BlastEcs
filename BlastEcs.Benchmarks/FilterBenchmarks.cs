using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace BlastEcs.Benchmarks;

[HtmlExporter]
[MemoryDiagnoser]
public class FilterBenchmarks
{
    [Params(10000, 100000, 1000000)] public int Amount;

    private static EcsWorld _world;
    private static SimpleQueryKey filter;

    [GlobalSetup]
    public void Setup()
    {
        _world = new EcsWorld();

        for (var index = 0; index < Amount; index++)
        {
            var entity = _world.CreateEntity();
            _world.Add(entity, new Transform { X = 0, Y = 0 }, new Velocity { X = 1, Y = 1 });
        }
        filter = new FilterBuilder(_world).Inc<Transform, Velocity>().Build();
    }
    [GlobalCleanup]
    public void Cleanup()
    {
        _world.Dispose();
    }

    [Benchmark]
    public void WorldEntityQuery()
    {
        filter.Each2(static entity =>
        {
            var ref1 = _world.GetRef<Transform>(entity);
            var ref2 = _world.GetRef<Velocity>(entity);

            ref1.X += ref2.X;
            ref1.Y += ref2.Y;
        });
    }
}
