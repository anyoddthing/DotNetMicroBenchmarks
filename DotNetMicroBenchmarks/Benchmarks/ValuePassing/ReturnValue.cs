using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.ValuePassing;

public class ReturnValue
{
    public int ReturnByValue()
    {
        return 42;
    }

    public void ReturnByOutParameter(out int value)
    {
        value = 42;
    }

    public void ReturnByRefParameter(ref int value)
    {
        value = 42;
    }
}

public class Benchmark
{
    private readonly ReturnValue _returnValue = new();

    [Benchmark]
    public int ReturnByValue()
    {
        return _returnValue.ReturnByValue();
    }

    [Benchmark]
    public int ReturnByOutParameter()
    {
        _returnValue.ReturnByOutParameter(out int value);
        return value;
    }

    [Benchmark]
    public int ReturnByRefParameter()
    {
        var value = 0;
        _returnValue.ReturnByRefParameter(ref value);
        return value;
    }
}