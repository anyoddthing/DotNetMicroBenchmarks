using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Reflection
{
    public static class IntInterfaces
    {
        private static HashSet<Type> Interfaces = new(typeof(int).GetInterfaces());

        public static bool ImplementsLookup<T>()
        {
            return Interfaces.Contains(typeof(T));
        }
    
        public static bool ImplementsUnrolled<T>()
        {
            var type = typeof(T);
            if (type == typeof(int)) return true;
            if (type == typeof(IComparable)) return true;
            if (type == typeof(IConvertible)) return true;
            if (type == typeof(IFormattable)) return true;
            if (type == typeof(IComparable<int>)) return true;
            if (type == typeof(IEquatable<int>)) return true;

            return false;
        }
    }

    public interface IContainer
    {
        object Value { get; }
        bool IsA<TOther>();
        bool IsAUnrolled<TOther>();
        bool IsALookup<TOther>();
    }

    public class Container<T> : IContainer
    {
        private T _value;

        public T Value => _value;
    
        object IContainer.Value => _value;
    
        public Container(T value)
        {
            _value = value;
        }

        public bool IsA<TOther>()
        {
            return _value is TOther;
        }

        public bool IsAUnrolled<TOther>()
        {
            if (typeof(T) == typeof(int))
            {
                return IntInterfaces.ImplementsUnrolled<TOther>();
            }

            return IsA<TOther>();
        }
    
        public bool IsALookup<TOther>()
        {
            if (typeof(T) == typeof(int))
            {
                return IntInterfaces.ImplementsLookup<TOther>();
            }

            return IsA<TOther>();
        }
    }

    
    public class ValueTypeIsABenchmark
    {
        private IContainer _container = new Container<int>(12);
    
        [Benchmark(Baseline = true)]
        public bool IsA()
        {
            var result = false;
            result &= _container.IsA<IComparable>();
            result &= _container.IsA<IConvertible>();
            result &= _container.IsA<IFormattable>();
            result &= _container.IsA<IComparable<Int32>>();
            result &= _container.IsA<IEquatable<Int32>>();
            return result;
        }
    
        [Benchmark]
        public bool IsALookup()
        {
            var result = false;
            result &= _container.IsALookup<IComparable>();
            result &= _container.IsALookup<IConvertible>();
            result &= _container.IsALookup<IFormattable>();
            result &= _container.IsALookup<IComparable<Int32>>();
            result &= _container.IsALookup<IEquatable<Int32>>();
            return result;
        }
    
        [Benchmark]
        public bool IsAUnrolled()
        {
            var result = false;
            result &= _container.IsAUnrolled<IComparable>();
            result &= _container.IsAUnrolled<IConvertible>();
            result &= _container.IsAUnrolled<IFormattable>();
            result &= _container.IsAUnrolled<IComparable<Int32>>();
            result &= _container.IsAUnrolled<IEquatable<Int32>>();
            return result;
        }
    }
}