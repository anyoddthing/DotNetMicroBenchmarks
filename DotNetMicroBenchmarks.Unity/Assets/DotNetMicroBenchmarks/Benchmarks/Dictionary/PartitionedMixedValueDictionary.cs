using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNetMicroBenchmarks.Dictionary
{
    public class PartitionedMixedValueDictionary : IMixedValueDictionary
    {
        private const int PartitionMaxSize = 256;

        private int _count = 0;

        private Partition _firstPartition = new() {Properties = Array.Empty<Property>()};

        private int _partitionCount = 1;

        private Partition[]? _extPartitions;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref Partition GetPartition(int index)
        {
            return ref index == 0 ? ref _firstPartition : ref _extPartitions![index - 1];
        }

        public int Count => _count;

        public void Set<TValue>(string key, TValue value)
        {
            var breadcrumps = new List<object>();
            
            var index = GetInsertionIndex(key);

            if (!index.ElementExists)
            {
                if (index.PartitionIndex < 0)
                {
                    breadcrumps.Add(0);
                    var newPartitionCount = _partitionCount + 1;
                    var newPartitionIndex = ~index.PartitionIndex;
                    if (_extPartitions == null)
                    {
                        breadcrumps.Add(1);
                        _extPartitions = new[]
                        {
                            Partition.NewPartition(PartitionMaxSize)
                        };
                    }
                    else
                    {
                        breadcrumps.Add(2);
                        
                        var extPartitionIndex = newPartitionIndex - 1;
                        var extPartitionCount = newPartitionCount - 1;
                        
                        if (extPartitionCount > _extPartitions.Length)
                        {
                            breadcrumps.Add($"Growing ext partitions to fit {newPartitionCount}");
                            breadcrumps.Add(3);
                            Array.Resize(ref _extPartitions, 2 * _extPartitions.Length);
                        }
                        
                        if (extPartitionIndex < extPartitionCount - 1)
                        {
                            breadcrumps.Add(4);
                            Array.Copy(
                                _extPartitions, extPartitionIndex,
                                _extPartitions, extPartitionIndex + 1,
                                extPartitionCount - extPartitionIndex
                            );
                        }

                        _extPartitions[extPartitionIndex] = Partition.NewPartition(PartitionMaxSize);
                    }
                    
                    _partitionCount = newPartitionCount;
                    
                    // split the content of the 2 partitions
                    var oldPartitionInsertionIndex = ~index.PropertyIndex;
                    
                    ref var oldPartition = ref GetPartition(newPartitionIndex - 1);
                    ref var newPartition = ref GetPartition(newPartitionIndex);
                    
                    if (false && oldPartitionInsertionIndex * 3 / PartitionMaxSize == 1)
                    {
                        breadcrumps.Add(5);
                        // element in the middle: just slice at this position
                        Array.Copy(
                            oldPartition.Properties, oldPartitionInsertionIndex,
                            newPartition.Properties, 0,
                            PartitionMaxSize - oldPartitionInsertionIndex
                        );
                        
                        Array.Clear(oldPartition.Properties, oldPartitionInsertionIndex, 
                            PartitionMaxSize - oldPartitionInsertionIndex);
                        
                        oldPartition.Count = oldPartitionInsertionIndex + 1;
                        newPartition.Count = PartitionMaxSize - oldPartitionInsertionIndex;
                        
                        _count++;
                        oldPartition.Properties[oldPartitionInsertionIndex].Set(key, value);
                    }
                    else 
                    {
                        breadcrumps.Add(6);
                        breadcrumps.Add($"old partition length: {oldPartition.Count}");
                        
                        Array.Copy(
                            oldPartition.Properties, PartitionMaxSize / 2,
                            newPartition.Properties, 0,
                            PartitionMaxSize / 2
                        );
    
                        Array.Clear(oldPartition.Properties, PartitionMaxSize / 2, PartitionMaxSize / 2);
                        
                        oldPartition.Count = PartitionMaxSize / 2;
                        newPartition.Count = PartitionMaxSize / 2;

                        if (oldPartitionInsertionIndex < PartitionMaxSize / 2)
                        {
                            breadcrumps.Add(7);
                            Array.Copy(
                                oldPartition.Properties, oldPartitionInsertionIndex,
                                oldPartition.Properties, oldPartitionInsertionIndex + 1,
                                PartitionMaxSize / 2 - oldPartitionInsertionIndex
                            );
                        
                            oldPartition.Properties[oldPartitionInsertionIndex].Set(key, value);
                            _count++;
                            oldPartition.Count++;
                            
                            ValidatePartitions();
                        }
                        else
                        {
                            breadcrumps.Add(8);
                            var newPartitionInsertionIndex = oldPartitionInsertionIndex - PartitionMaxSize / 2;
                            Array.Copy(
                                newPartition.Properties, newPartitionInsertionIndex,
                                newPartition.Properties, newPartitionInsertionIndex + 1,
                                PartitionMaxSize / 2 - newPartitionInsertionIndex
                            );
                        
                            newPartition.Properties[newPartitionInsertionIndex].Set(key, value);
                            _count++;
                            newPartition.Count++;
                            
                            ValidatePartitions();
                        }
                    }
                }
                else
                {
                    ref var partition = ref GetPartition(index.PartitionIndex);
                    if (partition.IsFull)
                    {
                        ref var nextPartition = ref GetPartition(index.PartitionIndex + 1);
                        var elementsToMove = Math.Max(1, (PartitionMaxSize - nextPartition.Count) / 2);
                        
                        Array.Copy(
                            nextPartition.Properties, 0, 
                            nextPartition.Properties, elementsToMove, 
                            nextPartition.Count
                        );
                        
                        Array.Copy(
                            partition.Properties, PartitionMaxSize - elementsToMove,
                            nextPartition.Properties, 0,
                            elementsToMove
                        );
                    }
                    
                    var newLength = partition.Count + 1;
                    if (newLength > partition.Capacity)
                    {
                        Array.Resize(ref partition.Properties, 2 * Math.Max(8, partition.Capacity));
                    }

                    var oldPartitionIndex = ~index.PropertyIndex;
                    if (oldPartitionIndex < partition.Count)
                    {
                        Array.Copy(
                            partition.Properties, oldPartitionIndex,
                            partition.Properties, oldPartitionIndex + 1,
                            partition.Count - oldPartitionIndex
                        );
                    }
                    
                    partition.Properties[oldPartitionIndex].Set(key, value);
                    _count++;
                    partition.Count = newLength;
                }
            }
            else
            {
                GetPartition(index.PartitionIndex).Properties[index.PropertyIndex].Update(value);
            }
            
            ValidatePartitions();
        }

        private void ValidatePartitions()
        {
            if (_extPartitions != null)
            {
                for (var i = 0; i < _partitionCount - 1; i++)
                {
                    var compareTo = string.CompareOrdinal(GetPartition(i).Last, GetPartition(i + 1).First);
                    if (compareTo >= 0)
                    {
                        throw new Exception($"Partitions {i + 1} last element is not smaller than partition {i + 2}");
                    }
                }
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            var readIndex = GetReadIndex(key);
            if (!readIndex.ElementExists)
            {
                value = default;
                return false;
            }
            
            return GetPartition(readIndex.PartitionIndex).Properties[readIndex.PropertyIndex].TryGet(out value);
        }

        public bool Remove(string key)
        {
            var insertionIndex = GetReadIndex(key);
            if (insertionIndex.ElementExists is false)
            {
                return false;
            }

            _count--;
            ref var partition = ref GetPartition(insertionIndex.PartitionIndex);
            
            partition.Count--;
            
            Array.Copy(
                partition.Properties, insertionIndex.PropertyIndex + 1,
                partition.Properties, insertionIndex.PropertyIndex,
                partition.Count - insertionIndex.PropertyIndex
            );

            partition.Properties[partition.Count] = default;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Index GetReadIndex(string key)
        {
            var partitionIndex = GetPartitionIndex(key);

            ref var partition = ref GetPartition(partitionIndex);
            var propertyIndex = GetProppertyIndex(ref partition, key);
            return new() {PartitionIndex = partitionIndex, PropertyIndex = propertyIndex};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Index GetInsertionIndex(string key)
        {
            var partitionIndex = GetPartitionIndex(key);

            ref var partition = ref GetPartition(partitionIndex);
            var propertyIndex = GetProppertyIndex(ref partition, key);

            if (propertyIndex < 0)
            {
                if (partition.Count < PartitionMaxSize)
                {
                    return new() {PartitionIndex = partitionIndex, PropertyIndex = propertyIndex};
                }

                var nextPartitionIndex = partitionIndex + 1;
                if (_partitionCount > nextPartitionIndex)
                {
                    ref var nextPartition = ref GetPartition(nextPartitionIndex);
                    if (nextPartition.Count < PartitionMaxSize)
                    {
                        // next partition has room
                        return new() {PartitionIndex = partitionIndex, PropertyIndex = -1};
                    }
                }

                // need to create a new partition after the natural one
                return new() {PartitionIndex = ~nextPartitionIndex, PropertyIndex = propertyIndex};
            }

            return new() {PartitionIndex = partitionIndex, PropertyIndex = propertyIndex};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPartitionIndex(string key)
        {
            if (_partitionCount == 1)
            {
                return 0;
            }

            int index;
            for (index = _partitionCount - 1; index >= 0; index--)
            {
                if (string.CompareOrdinal(key, GetPartition(index).First) >= 0)
                {
                    return index;
                }
            }

            return ++index;

            // if (_partitionCount == 2)
            // {
            //     return string.CompareOrdinal(key, _extPartitions![0].First) < 0 ? 0 : 1;
            // }
            //
            // for (var i = _partitionCount - 2; i >= 0; i--)
            // {
            //     if (string.CompareOrdinal(key, _extPartitions![i].First) >= 0)
            //     {
            //         return i + 1;
            //     }
            // }
            //
            // return 0;

            // var left = 0;
            // var right = _partitionCount - 2;
            //
            // while (left <= right)
            // {
            //     var mid = left + (right - left) / 2;
            //
            //     switch (string.CompareOrdinal(_extPartitions[mid].First, key))
            //     {
            //         case 0:
            //             return mid + 1;
            //         case < 0:
            //             left = mid + 1;
            //             break;
            //         default:
            //             right = mid - 1;
            //             break;
            //     }
            // }
            //
            // return left;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetProppertyIndex(ref Partition partition, string key)
        {
            int left = 0;
            int right = partition.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (ReferenceEquals(partition.Properties[mid].Key, key))
                {
                    return mid;
                }

                int comparison = string.CompareOrdinal(partition.Properties[mid].Key, key);

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

        public struct Index
        {
            public int PartitionIndex;
            public int PropertyIndex;

            public bool ElementExists => PropertyIndex >= 0;

            public override string ToString()
            {
                return $"[{Complements(PartitionIndex)}, {Complements(PropertyIndex)}]";

                string Complements(int index)
                {
                    return index < 0 ? $"~{~index}" : index.ToString();
                }
            }
        }

        public struct Partition
        {
            public static Partition NewPartition(int size)
            {
                return new() {Properties = new Property[size]};
            }

            public int Count;
            public Property[] Properties;

            public string First => Properties[0].Key;
            public string Last => Properties[Count - 1].Key;
            public int Capacity => Properties.Length;
            public bool IsFull => Count == PartitionMaxSize;
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
                Update(value);
            }

            public void Update<T>(T value)
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
            
            public override string ToString()
            {
                return $"[{Key}, {Type.Name}]";
            }
        }
    }
}