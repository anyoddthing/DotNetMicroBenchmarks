using DotNetMicroBenchmarks.Benchmarks;
using DotNetMicroBenchmarks.Benchmarks.Nullables;
using DotNetMicroBenchmarks.Benchmarks.Records;

namespace DotNetMicroBenchmarks;

internal class Program
{
    public static void Main(string[] args)
    {
        Runner.Run<RecordConstruction>();
        // Console.WriteLine(1.Intern().Equals(1));
        // Console.WriteLine(99.Intern().Equals(99));
        //
        // Console.WriteLine(ReferenceEquals(57.Intern(), 57.Intern()));
        //
        // Console.WriteLine((int) 27.Intern());
    }
}