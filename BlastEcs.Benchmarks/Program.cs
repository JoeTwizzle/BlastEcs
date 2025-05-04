using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
namespace BlastEcs.Benchmarks
{

    public struct TestComponent { }
    public struct TestComponent2 { }
    public struct TestComponent3 { }
    public struct TestComponent4 { }
    public struct TestComponent5 { }

    [MemoryDiagnoser]
    public class TypeHandleBenchmarks
    {
        private readonly Dictionary<Type, int> _typeDict = new();
        private readonly Dictionary<RuntimeTypeHandle, int> _handleDict = new();

        [GlobalSetup]
        public void Setup()
        {
            var type = typeof(TestComponent);
            _typeDict[type] = 42;
            _handleDict[type.TypeHandle] = 42;
            var type2 = typeof(TestComponent2);
            _typeDict[type2] = 42;
            _handleDict[type2.TypeHandle] = 42;
            var type3 = typeof(TestComponent3);
            _typeDict[type3] = 42;
            _handleDict[type3.TypeHandle] = 42;
            var type4 = typeof(TestComponent4);
            _typeDict[type4] = 42;
            _handleDict[type4.TypeHandle] = 42;
            var type5 = typeof(TestComponent5);
            _typeDict[type5] = 42;
            _handleDict[type5.TypeHandle] = 42;
        }

        [Benchmark(Baseline = true)]
        public int GetFromTypeDictionary()
        {
            if (_typeDict.TryGetValue(typeof(TestComponent), out var val))
            {
                return val;
            }
            return -1;
        }

        [Benchmark]
        public int GetFromHandleDictionary()
        {
            if (_handleDict.TryGetValue(typeof(TestComponent).TypeHandle, out var val))
            {
                return val;
            }
            return -1;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<TypeHandleBenchmarks>();
        }
    }

}
