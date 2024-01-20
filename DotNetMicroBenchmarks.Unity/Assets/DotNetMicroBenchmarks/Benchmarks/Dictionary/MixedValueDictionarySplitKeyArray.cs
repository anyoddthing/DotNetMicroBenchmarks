using System;
using System.Runtime.InteropServices;

namespace DotNetMicroBenchmarks.Dictionary
{
    public class MixedValueDictionarySplitKeyArray : IMixedValueDictionary
    {
        private int _length = 0;
        private string[] _keys = Array.Empty<string>();
        private Property[] _properties = Array.Empty<Property>();

        public int Count => _length;

        public void Set<TValue>(string key, TValue value)
        {
            var insertionIndex = GetInsertionIndex(key);
            if (insertionIndex < 0)
            {
                _length++;
                if (_length > _keys.Length)
                {
                    var newCapacity = Math.Max(8, _keys.Length * 2);
                    Array.Resize(ref _keys, newCapacity);
                    Array.Resize(ref _properties, newCapacity);
                }

                insertionIndex = ~insertionIndex;
                Array.Copy(
                    _keys, insertionIndex,
                    _keys, insertionIndex + 1,
                    _length - insertionIndex
                );
                Array.Copy(
                    _properties, insertionIndex,
                    _properties, insertionIndex + 1,
                    _length - insertionIndex
                );
            }

            _keys[insertionIndex] = key;
            _properties[insertionIndex].Set(value);
        }

        public bool TryGet<T>(string key, out T value)
        {
            var insertionIndex = GetInsertionIndex(key);
            if (insertionIndex < 0)
            {
                value = default;
                return false;
            }

            return _properties[insertionIndex].TryGet<T>(out value);
        }

        public bool Remove(string key)
        {
            var insertionIndex = GetInsertionIndex(key);
            if (insertionIndex < 0)
            {
                return false;
            }

            _length--;
            Array.Copy(
                _keys, insertionIndex + 1,
                _keys, insertionIndex,
                _length - insertionIndex
            );
            Array.Copy(
                _properties, insertionIndex + 1,
                _properties, insertionIndex,
                _length - insertionIndex
            );

            _keys[_length] = null!;
            _properties[_length] = default;

            return true;
        }

        public int GetInsertionIndex(string key)
        {
            return _keys.Length == 0 ? -1 : Array.BinarySearch(_keys, 0, _length, key, StringComparer.Ordinal);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Property
        {
            [FieldOffset(0)]
            public Type Type;

            [FieldOffset(8)]
            public object ObjectValue;

            [FieldOffset(16)]
            public int IntValue;

            [FieldOffset(16)]
            public double DoubleValue;

            public void Set<T>(T value)
            {
                Type = typeof(T);
                if (Type == typeof(int))
                {
                    IntValue = (int) (object) value;
                }
                else if (Type == typeof(double))
                {
                    DoubleValue = (double) (object) value;
                }
                else
                {
                    ObjectValue = value;
                }
            }

            public bool TryGet<T>(out T value)
            {
                if (Type == typeof(int) && typeof(T) == typeof(int))
                {
                    value = (T) (object) IntValue;
                    return true;
                }
                else if (Type == typeof(double) && typeof(T) == typeof(double))
                {
                    value = (T) (object) DoubleValue;
                    return true;
                }
                else if (Type == typeof(T))
                {
                    value = (T) ObjectValue;
                    return true;
                }

                value = default;
                return false;
            }
        }
    }
}