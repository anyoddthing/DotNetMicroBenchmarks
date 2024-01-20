using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Boxing
{
    public static class BoxedInt
    {
        private static IComparable[] _values = Array.Empty<IComparable>();

        static BoxedInt()
        {
            Resize(64 * 1000);
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
        private readonly List<int> _values = Enumerable.Range(0, 100).ToList();

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


    public class BoxedIntCacheBenchmark
    {
        private readonly Container _container = new Container();

        [Benchmark]
        public void GetMaxBaseline()
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
        }

        [Benchmark]
        public void GetMaxComparables()
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
        }

        [Benchmark]
        public void GetMaxCachedComparables()
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
        }
    }
}