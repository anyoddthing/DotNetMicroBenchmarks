using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.ValuePassing
{
    public interface IContainer
    {
        bool TryGet<TValue>(out TValue value);
        bool TryGet(out int value);
    }

    public class Container<T> : IContainer
    {
        private readonly T _value;

        public Container(T value)
        {
            _value = value;
        }

        public bool TryGet<TValue>(out TValue value)
        {
            // if (typeof(TValue).IsAssignableFrom(typeof(T)))
            // {
            //     value = (TValue)(object)_value;
            //     return true;
            // }
            // if (typeof(TValue) == typeof(T))
            // {
            //     value = (TValue)(object)_value;
            //     return true;
            // }

            if (_value is TValue tValue)
            {
                value = tValue;
                return true;
            }
            //
            value = default!;
            return false;
        }

        public bool TryGet(out int value)
        {
            if (_value is int intValue)
            {
                value = intValue;
                return true;
            }

            value = default;
            return false;
        }
    }

    public class IntContainer : IContainer
    {
        private readonly int _value;

        public IntContainer(int value)
        {
            _value = value;
        }

        public bool TryGet(out int value)
        {
            value = _value;
            return true;
        }
    
        public bool TryGet<TValue>(out TValue value)
        {
            if (typeof(TValue) == typeof(int))
            {
                value = (TValue)(object)_value;
                return true;
            }

            value = default;
            return false;
        }
    }

    public class ObjectContainer : IContainer
    {
        private object _value;

        public ObjectContainer(object value)
        {
            _value = value;
        }

        public bool TryGet<TValue>(out TValue value)
        {
            if (_value is TValue tValue)
            {
                value = tValue;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGet(out int value)
        {
            return TryGet<int>(out value);
        }
    }

    
    public class GenericReturnType
    {
        private readonly IContainer _intContainer = new IntContainer(47);
    
        private readonly IContainer _genIntContainer = new Container<int>(47);

        [Benchmark]
        public int Baseline()
        {
            _intContainer.TryGet(out int value);
            return value;
        }

        [Benchmark]
        public int GenericContainer()
        {
            _genIntContainer.TryGet(out int value);
            return value;
        }
    }
}