namespace DotNetMicroBenchmarks.Benchmarks.Boxing;

public static class BoxedInt
{
    private static object[] _values = Array.Empty<object>();

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

    public static object Intern(this int value)
    {
        if (value < _values.Length)
        {
            return _values[value];
        }
        
        Resize(_values.Length * 2);
        return _values[value];
    }
}

public class BoxedIntCache
{
    
}