using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Boxing.IntCache;

public static class BoxedInt
{
    private static IComparable[] _values = Array.Empty<IComparable>();

    static BoxedInt()
    {
        Resize(64);
    }

    private static void Resize(int i)
    {
        if (i < _values.Length) return;

        var lastI = _values.Length;
        Array.Resize(ref _values, i);
        for (; lastI < i; lastI++)
        {
            _values[lastI] = lastI;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IComparable Intern(this int value)
    {
        if (value < _values.Length)
        {
            return _values[value];
        }

        Resize(_values.Length * 2);
        return _values[value];
    }
}

public class Container
{
    private readonly List<int> _values = Enumerable.Range(0, 10).ToList();

    public int Count => _values.Count;

    public int GetValue(int index)
    {
        return _values[index];
    }

    public IComparable GetComparable(int index)
    {
        return _values[index];
    }

    public IComparable GetCachedComparable(int index)
    {
        return _values[index].Intern();
    }
}

[MemoryDiagnoser(true)]
public class Benchmark
{
    private readonly Container _container = new Container();

    [Benchmark]
    public int GetMaxBaseline()
    {
        var first = _container.GetValue(0);
        for (var i = 0; i < _container.Count; i++)
        {
            var value = _container.GetValue(i);
            if (value.CompareTo(first) > 0)
            {
                first = value;
            }
        }

        return first;
    }

    [Benchmark]
    public IComparable GetMaxComparables()
    {
        var first = _container.GetComparable(0);
        for (var i = 0; i < _container.Count; i++)
        {
            var value = _container.GetComparable(i);
            if (value.CompareTo(first) > 0)
            {
                first = value;
            }
        }
        return first;
    }

    [Benchmark]
    public IComparable GetMaxCachedComparables()
    {
        var first = _container.GetCachedComparable(0);
        for (var i = 0; i < _container.Count; i++)
        {
            var value = _container.GetCachedComparable(i);
            if (value.CompareTo(first) > 0)
            {
                first = value;
            }
        }
        
        return first;
    }
}