using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Dictionary;

public static class XComparer<T>
{
    public static readonly IComparer<T> Default = CreateComparer();

    private static IComparer<T> CreateComparer()
    {
        if (typeof(T) == typeof(string))
        {
            return (IComparer<T>)StringComparer.Ordinal;
        }

        return Comparer<T>.Default;
    }
}

public struct Property<K, V> where K : IComparable
{
    public K Key { get; }
    public V Value { get; }

    public Property(K key, V value)
    {
        Key = key;
        Value = value;
    }
}

public class SortedMap<K, V> where K : IComparable
{
    private static readonly IComparer<K> _comparer = XComparer<K>.Default;

    private List<Property<K, V>> _items = new();

    public void Clear()
    {
        _items.Clear();
    }

    public void Add(K key, V value)
    {
        var index = GetIndexOf(key);
        if (index >= 0)
        {
            _items[index] = new Property<K, V>(key, value);
            return;
        }
        else
        {
            index = ~index;
            _items.Insert(index, new Property<K, V>(key, value));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public V? Get(K key)
    {
        var index = GetIndexOf(key);
        return index >= 0 ? _items[index].Value : default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndexOf(K key)
    {
        int left = 0;
        int right = _items.Count - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (ReferenceEquals(_items[mid].Key, key))
            {
                return mid;
            }

            int comparison = _comparer.Compare(_items[mid].Key, key);

            if (comparison == 0)
            {
                return mid;
            }
            else if (comparison < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return ~left;
    }
}


public class Benchmark
{
    readonly int _count = 10;
    readonly int _keyLength = 10;

    readonly List<string> _list = new ();
    readonly SortedMap<string, string> _sortedList = new ();
    readonly Dictionary<string, string> _dictionary = new ();

    public Benchmark()
    {
        for (int i = 0; i < _count; i++)
        {
            var key = RandomString(_keyLength);
            _list.Add(key);
            _sortedList.Add(key, key);
            _dictionary.Add(key, key);
        }
    }

    private string RandomString(int length)
    {
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }

    [Benchmark]
    public void ReadFromDictionary()
    {
        for (int i = 0; i < _count; i++)
        {
            _dictionary.TryGetValue(_list[i], out var value);
        }
    }

    [Benchmark]
    public void ReadFromSortedList()
    {
        for (int i = 0; i < _count; i++)
        {
            _sortedList.Get(_list[i]);
        }
    }   
}