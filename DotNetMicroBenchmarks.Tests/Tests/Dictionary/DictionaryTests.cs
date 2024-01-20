using System;
using System.Linq;
using DotNetMicroBenchmarks.Benchmarks;
using DotNetMicroBenchmarks.Dictionary;
using NUnit.Framework;

namespace DotNetMicroBenchmarks.Tests.Tests.Dictionary
{
    public class DictionaryTests
    {
        [Test]
        public void TestThatDictionaryCanStoreValue()
        {
            var sortedStrings = Enumerable.Range(0, 3 * 256).Select(i => $"{i:0000}")
                .OrderBy(s => Guid.NewGuid())
                .ToList();

            var dictionary = new PartitionedMixedValueDictionary();
            // for (var index = 0; index < 256; index++)
            for (var index = 0; index < sortedStrings.Count; index++)
            {
                var s = sortedStrings[index];
                dictionary.Set(s, s);
            }
            
            // dictionary.Set(sortedStrings[256], sortedStrings[256]);

            Assert.That(dictionary.Count, Is.EqualTo(sortedStrings.Count));
            
            Assert.That(dictionary.Get<string>("0128"), Is.EqualTo("0128"));

            for (var index = 0; index < sortedStrings.Count; index++)
            {
                var s = sortedStrings[index]; 
                Assert.That(dictionary.Get<string>(s), Is.EqualTo(s));
            }
        }
    }
}