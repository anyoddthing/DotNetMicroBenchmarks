using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNetMicroBenchmarks.Dictionary
{
    public class MixedValueDictionary : IMixedValueDictionary
    {
        private int _length = 0;
        private Property[] _properties = Array.Empty<Property>();

        public int Count => _length;

        public void Set<TValue>(string key, TValue value)
        {
            var insertionIndex = GetInsertionIndex(key);
            if (insertionIndex < 0)
            {
                _length++;
                if (_length > _properties.Length)
                {
                    var newCapacity = Math.Max(8, _properties.Length * 2);
                    Array.Resize(ref _properties, newCapacity);
                }

                insertionIndex = ~insertionIndex;
                Array.Copy(
                    _properties, insertionIndex,
                    _properties, insertionIndex + 1,
                    _length - insertionIndex
                );
            }

            _properties[insertionIndex].Set(key, value);
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
                _properties, insertionIndex + 1,
                _properties, insertionIndex,
                _length - insertionIndex
            );

            _properties[_length] = default;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexOf(string key)
        {
            int left = 0;
            int right = _length - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (ReferenceEquals(_properties[mid].Key, key))
                {
                    return mid;
                }

                int comparison = string.CompareOrdinal(_properties[mid].Key, key);

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
        
        public int GetInsertionIndex(string key)
        {
            
            return _properties.Length == 0 ? -1 : GetIndexOf(key);
        }
        
        public class PropertyComparer : IComparer
        {
            public static readonly PropertyComparer Instance = new();
            
            public int Compare(Property x, string key)
            {
                return StringComparer.Ordinal.Compare(x.Key, key);
            }

            public int Compare(object? x, object? y)
            {
                if (x is string xKey)
                {
                    return Compare((Property) y, x);
                }
                else
                {
                    return Compare((Property)x, (string)y);
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Property
        {
            [FieldOffset(0)]
            public Type Type;

            [FieldOffset(8)]
            public string Key;

            [FieldOffset(16)]
            public object ObjectValue;

            [FieldOffset(24)]
            public int IntValue;

            [FieldOffset(24)]
            public double DoubleValue;

            public void Set<T>(string key, T value)
            {
                Key = key;
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