using System.Diagnostics;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace DotNetMicroBenchmarks.Benchmarks;

public static class Runner
{
    public static void Run<T>()
    {
        BenchmarkRunner.Run<T>();
    }
    
    public static void Test<T>()
    {
        var benchmarks = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(info => info.GetCustomAttribute(typeof(BenchmarkDotNet.Attributes.BenchmarkAttribute)) != null)
            .ToList();

        var suite = Activator.CreateInstance<T>();
        var stopWatch = new Stopwatch();
        
        foreach (var benchmark in benchmarks)
        {
            stopWatch.Restart();
            benchmark.Invoke(suite, null);
            stopWatch.Stop();
            
            Console.WriteLine($"{benchmark.Name} took {stopWatch.ElapsedMilliseconds}ms");
        }
    }
}