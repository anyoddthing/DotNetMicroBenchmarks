using System;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Records
{
    public record KeyValueRecord<TKey, TValue>(TKey Key, TValue Value)
        where TKey : IComparable
    {
        public TKey Key { get; } = Key;
        public TValue Value { get; } = Value;
    }

    public class KeyValueClass<TKey, TValue> 
        where TKey : IComparable
    {
        public KeyValueClass(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; }
        public TValue Value { get; }
    }

    public class RecordConstruction
    {
        [Benchmark]
        public void ConstructClass()
        {
            for (var i = 0; i < 100; i++)
            {
                new KeyValueClass<int, string>(i, "value");
            }
        }
    
        [Benchmark]
        public void ConstructRecord()
        {
            for (var i = 0; i < 100; i++)
            {
                new KeyValueRecord<int, string>(i, "value");
            }
        }
    }
}