using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BlastEcs.Builtin;
namespace BlastEcs.Benchmarks;


public struct TestTag : ITag;
public struct TestComponent;
public struct TestComponent2;
public struct TestComponent3;
public struct TestComponent4;
public struct TestComponent5;

[MemoryDiagnoser]
public class WorldBenchmarks
{
    [Benchmark(Baseline = true)]
    public void WorldAddRemoveRelation2()
    {
        var world = new EcsWorld();
        for (int i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([]);
            var kind = world.CreateEntity([]);
            var target = world.CreateEntity([]);
            world.AddRelation(e, kind, target);
            world.DestroyEntity(kind);
            world.DestroyEntity(target);
        }
    }

    [Benchmark()]
    public void WorldAddTag()
    {
        var world = new EcsWorld();
        for (int i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([]);
            world.Add<TestTag>(e);
        }
    }

    [Benchmark()]
    public void WorldAddComponent()
    {
        var world = new EcsWorld();
        for (int i = 0; i < 10000; i++)
        {
            var e = world.CreateEntity([]);
            world.Add<TestComponent>(e);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<WorldBenchmarks>();
    }
}
