using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Boxing.PrimitiveBoxing;

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

public class IntValuePair<TValue> : IKeyValuePair<int, TValue>
{
    public IntValuePair(int key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public int Key { get; }
    public TValue Value { get; }

    IComparable IKeyValuePair.Key => Key;
    object IKeyValuePair.Value => Value;
}

public class Container<TKey>
    where TKey : IComparable
{
    private List<IKeyValuePair<TKey, object>> _items = new();
    private List<IntValuePair<object>> _intItems = new();
    
    public void Add<TValue>(TKey key, TValue value)
        where TValue : class
    {
        _items.Add(new KeyValuePair<TKey, TValue>(key, value));
        if (key is int intKey)
        {
            _intItems.Add(new IntValuePair<object>(intKey, value));
        }
    }
    
    public IEnumerable<IKeyValuePair> TypeErasedItems => _items;
    public IEnumerable<IKeyValuePair<TKey, object>> GenericItems => _items;
    
}

[MemoryDiagnoser(true)]
public class Benchmark
{
    private Container<int> _container = new Container<int>();

    public Benchmark()
    {
        var numElements = 1000;
        for (var i = 0; i < numElements; i++)
        {
            _container.Add(i, i.ToString());
        }
    }
    
    [Benchmark]
    public int CompareBaseline()
    {
        var t = 0;
        var value = 1;
        foreach (var pair in _container.GenericItems)
        {
            if (pair.Key.CompareTo(value) == 0)
            {
                t++;
            }
        }

        return t;
    }
    
    [Benchmark]
    public int CompareIntBoxing()
    {
         var t = 0;
         IComparable value = 1;
         foreach (var pair in _container.TypeErasedItems)
         {
             if (pair.Key.CompareTo(value) == 0)
             {
                 t++;
             }
         }
 
         return t;
    }
    
    [Benchmark]
    public int EqualsBaseline()
    {
        var t = 0;
        var value = 1;
        foreach (var pair in _container.GenericItems)
        {
            if (pair.Key.Equals(value))
            {
                t++;
            }
        }

        return t;
    }
    
    [Benchmark]
    public int EqualsIntBoxing()
    {
         var t = 0;
         IComparable value = 1;
         foreach (var pair in _container.TypeErasedItems)
         {
             if (pair.Key.Equals(value))
             {
                 t++;
             }
         }
 
         return t;
    }
}
