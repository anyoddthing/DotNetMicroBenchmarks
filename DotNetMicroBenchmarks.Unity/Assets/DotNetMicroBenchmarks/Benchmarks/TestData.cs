using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetMicroBenchmarks.Benchmarks
{
    public static class TestData
    {
        public static Random Random = new Random();
        
        public static readonly string[] RandomWords = new string[]
        {
            "Ephemeral",
            "Quizzical",
            "Exuberant",
            "Spectacular",
            "Mysterious",
            "Incredible",
            "Fascinating",
            "Enigmatic",
            "Ambiguous",
            "Captivating",
            "Effervescent",
            "Harmonious",
            "Mesmerizing",
            "Resplendent",
            "Mellifluous",
            "Sensational",
            "Serendipity",
            "Extraordinary",
            "Enchanted",
            "Wonderstruck"        
        };
        
        public static string RandomWord() => RandomWords[Random.Next(RandomWords.Length)];

        public static List<string> GenerateSortedStrings(int numberOfStrings, int minLength, int variance = 5)
        {
            var uniqueStrings = new HashSet<string>();
            while (uniqueStrings.Count < numberOfStrings)
            {
                uniqueStrings.Add(GenerateRandomString(minLength));
            }

            var sortedStrings = uniqueStrings.ToList();
            sortedStrings.Sort();
            return sortedStrings;
        }

        private static string GenerateRandomString(int minLength, int variance = 5)
        {
            // Adjust the length to be at least `minLength`
            int length = Random.Next(minLength, minLength + variance);

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(chars[Random.Next(chars.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}