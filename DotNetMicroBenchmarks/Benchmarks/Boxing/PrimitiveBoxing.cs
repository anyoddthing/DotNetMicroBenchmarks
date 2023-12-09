using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Boxing;

public interface IKeyValuePair
{
    IComparable Key { get; }
    object Value { get; }
}

public interface IKeyValuePair<out TKey, out TValue> : IKeyValuePair
    where TKey : IComparable
{
    TKey Key { get; }
    TValue Value { get; }
}

public class KeyValuePair<TKey, TValue> : IKeyValuePair<TKey, TValue>
    where TKey : IComparable
{
    public KeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public TKey Key { get; }
    public TValue Value { get; }

    IComparable IKeyValuePair.Key => Key;
    object IKeyValuePair.Value => Value;
}

public class Container<TKey>
    where TKey : IComparable
{
    private List<IKeyValuePair<TKey, object>> _items = new List<IKeyValuePair<TKey, object>>();
    
    public void Add<TValue>(TKey key, TValue value)
        where TValue : class
    {
        _items.Add(new KeyValuePair<TKey, TValue>(key, value));
    }
    
    public IEnumerable<IKeyValuePair> Items1 => _items;
    public IEnumerable<IKeyValuePair<TKey, object>> Items2 => _items;
}

public class PrimitiveBoxing
{
    private Container<int> _container = new Container<int>();

    public PrimitiveBoxing()
    {
        var numElements = 1000;
        for (var i = 0; i < numElements; i++)
        {
            _container.Add(i, i.ToString());
        }
    }
    
    [Benchmark]
    public int Baseline()
    {
        var t = 0;
        var value = 1;
        foreach (var pair in _container.Items2)
        {
            if (pair.Key.CompareTo(value) == 0)
            {
                t++;
            }
        }

        return t;
    }
    
    [Benchmark]
    public int IntBoxing()
    {
         var t = 0;
         IComparable value = 1;
         foreach (var pair in _container.Items1)
         {
             if (pair.Key.CompareTo(value) == 0)
             {
                 t++;
             }
         }
 
         return t;
    }
}