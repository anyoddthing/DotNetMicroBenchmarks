using DotNetMicroBenchmarks.Benchmarks;

namespace DotNetMicroBenchmarks;

internal class Program
{
    public static void Main(string[] args)
    {
        Runner.Run<Benchmarks.Boxing.IntCache.Benchmark>();
    }
}