using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Nullables;

public interface IProxy 
{
    T? PassThrough<T>(T? value);
    T? PassThroughStruct<T>(T? value) where T : struct;
}

public class Proxy : IProxy
{
    public T PassThrough<T>(T? value)
    {
        if (value is null) return default;
        
        return value;
    }

    public T? PassThroughStruct<T>(T? value) where T : struct
    {
        if (value is null) return default;
        
        return value;
    }
}

[MemoryDiagnoser(true)]
public class UnconstrainedContainer
{
    private readonly IProxy _proxy = new Proxy();

    [Benchmark]
    public int Baseline()
    {
        return _proxy.PassThrough(1);
    }
    
    [Benchmark]
    public int? BaselineNullable()
    {
        return _proxy.PassThrough<int?>(1);
    }
    
    [Benchmark]
    public int? BaselineConstraint()
    {
        return _proxy.PassThroughStruct<int>(1);
    }
}