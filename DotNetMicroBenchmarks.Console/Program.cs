using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using DotNetMicroBenchmarks.Benchmarks;
using DotNetMicroBenchmarks.Benchmarks.Boxing;
using DotNetMicroBenchmarks.Benchmarks.Dictionary;
using DotNetMicroBenchmarks.Benchmarks.Reflection;
using DotNetMicroBenchmarks.Benchmarks.Sorting;
using DotNetMicroBenchmarks.Benchmarks.Types;
using DotNetMicroBenchmarks.Benchmarks.ValuePassing;
using DotNetMicroBenchmarks.Boxing;
using DotNetMicroBenchmarks.Dictionary;
using DotNetMicroBenchmarks.Memory;

namespace DotNetMicroBenchmarks;

internal class Program
{
    private static void Run<T>()
    {
        Run(typeof(T));
    }

    private static void Run(Type suiteType)
    {
        if (UseBenchmarkDotNet)
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run(suiteType, ManualConfig.CreateMinimumViable()
                .AddDiagnoser(new MemoryDiagnoser(new())));
        }
        else
        {
            BenchmarkRunner.Run(suiteType, 4, Console.WriteLine);
        }
    }

    private static bool UseBenchmarkDotNet { get; set; } = true;

    public static void Main(string[] args)
    {
        Console.WriteLine($"{1:0000}");
        Console.WriteLine(string.CompareOrdinal("0", "1"));
        Console.WriteLine(string.CompareOrdinal("A", "Aa"));
        Console.WriteLine(string.CompareOrdinal("AA", "Aa"));
        Console.WriteLine(string.CompareOrdinal("A", "Ab"));
        Console.WriteLine(string.CompareOrdinal("A", "Aba"));
        Console.WriteLine(string.CompareOrdinal("A", "Aca"));
        // var dict = new PartitionedMixedValueDictionary();
        // dict.Set("string", "string");
        // dict.Set("double", 1.0);
        // dict.Set("int", 1);
        // var before = GC.GetAllocatedBytesForCurrentThread();
        // dict.TryGet<int>("int", out var result);
        // dict.Set("int", 10);
        // var after = GC.GetAllocatedBytesForCurrentThread();
        // Console.WriteLine($"Result: {result} ({after - before} bytes)");
        
        // UseBenchmarkDotNet = false;
        // Run<BoxedIntCacheBenchmark>();

        // foreach (var benchmarkSuite in Runner.GetAllBenchmarkSuites())
        // {
        //     Console.WriteLine($"Running {benchmarkSuite.Name}");
        // }
        // Console.WriteLine(1.Intern().Equals(1));
        // Console.WriteLine(99.Intern().Equals(99));
        //
        // Console.WriteLine(ReferenceEquals(57.Intern(), 57.Intern()));
        //
        // Console.WriteLine((int) 27.Intern());
    }
}