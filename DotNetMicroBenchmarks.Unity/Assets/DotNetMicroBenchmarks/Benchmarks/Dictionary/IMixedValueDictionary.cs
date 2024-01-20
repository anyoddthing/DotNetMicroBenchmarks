using System.Collections.Generic;

namespace DotNetMicroBenchmarks.Dictionary
{
    public interface IMixedValueDictionary
    {
        int Count { get; }
        void Set<TValue>(string key, TValue value);
        bool TryGet<T>(string key, out T value);
        bool Remove(string key);
    }

    public static class MixedValueDictionaryExtensions
    {
        public static T Get<T>(this IMixedValueDictionary dictionary, string key)
        {
            if (!dictionary.TryGet(key, out T value))
            {
                throw new KeyNotFoundException(key);
            }

            return value;
        }
    }
}