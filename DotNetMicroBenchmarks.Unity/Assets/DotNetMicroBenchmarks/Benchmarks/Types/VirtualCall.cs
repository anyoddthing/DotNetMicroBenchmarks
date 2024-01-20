using System;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Types
{
    public class Container
    {
        private readonly string _key;

        public Container(string key)
        {
            _key = key;
        }

        public IComparable Key => _key;
        
        public string StringKey => _key;
    }
    
    public class VirtualCallBenchmark
    {
        private readonly Container[] _containers = 
        {
            new ("Ephemeral"),
            new ("Quizzical"),
        };
        
        [Benchmark(Baseline = true)]
        public void VirtualCall()
        {
            _containers[0].Key.CompareTo(_containers[1].Key);
        }

        [Benchmark]
        public void DirectCall()
        {
            _containers[0].StringKey.CompareTo(_containers[1].StringKey);
        }
    }
}