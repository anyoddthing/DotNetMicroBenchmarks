using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks.Sorting
{
    public static class StringHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int CompareOrdinalHelper(string strA, string strB)
        {
            if (ReferenceEquals(strA, strB))
            {
                return 0;
            }
            
            int order;
            int length = Math.Min(strA.Length, strB.Length);

            if (length > 0)
            {
                if ((order = strA[0] - strB[0]) != 0)
                {
                    return order;
                }
            }
            
            int diffOffset = -1;

            fixed (char* ap = strA)
            fixed (char* bp = strB)
            {
                char* a = ap;
                char* b = bp;

                // unroll the loop
                while (length >= 10)
                {
                    if (*(int*) a != *(int*) b)
                    {
                        diffOffset = 0;
                        break;
                    }

                    if (*(int*) (a + 2) != *(int*) (b + 2))
                    {
                        diffOffset = 2;
                        break;
                    }

                    if (*(int*) (a + 4) != *(int*) (b + 4))
                    {
                        diffOffset = 4;
                        break;
                    }

                    if (*(int*) (a + 6) != *(int*) (b + 6))
                    {
                        diffOffset = 6;
                        break;
                    }

                    if (*(int*) (a + 8) != *(int*) (b + 8))
                    {
                        diffOffset = 8;
                        break;
                    }

                    a += 10;
                    b += 10;
                    length -= 10;
                }

                if (diffOffset != -1)
                {
                    // we already see a difference in the unrolled loop above
                    a += diffOffset;
                    b += diffOffset;
                    if ((order = (int) *a - (int) *b) != 0)
                    {
                        return order;
                    }

                    Contract.Assert(*(a + 1) != *(b + 1), "This byte must be different if we reach here!");
                    return ((int) *(a + 1) - (int) *(b + 1));
                }

                // now go back to slower code path and do comparison on 4 bytes one time.
                // Following code also take advantage of the fact strings will 
                // use even numbers of characters (runtime will have a extra zero at the end.)
                // so even if length is 1 here, we can still do the comparsion.  
                while (length > 0)
                {
                    if (*(int*) a != *(int*) b)
                    {
                        break;
                    }

                    a += 2;
                    b += 2;
                    length -= 2;
                }

                if (length > 0)
                {
                    int c;
                    // found a different int on above loop
                    if ((c = (int) *a - (int) *b) != 0)
                    {
                        return c;
                    }

                    Contract.Assert(*(a + 1) != *(b + 1), "This byte must be different if we reach here!");
                    return ((int) *(a + 1) - (int) *(b + 1));
                }

                // At this point, we have compared all the characters in at least one string.
                // The longer string will be larger.
                return strA.Length - strB.Length;
            }
        }
    }

    public class StringSortingBenchmark
    {
        private readonly string[] randomWords = new string[]
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

        [Benchmark(Baseline = true)]
        public void CompareOrdinal()
        {
            for (int i = 0; i < randomWords.Length; i++)
            {
                for (int j = 0; j < randomWords.Length; j++)
                {
                    StringComparer.Ordinal.Compare(randomWords[i], randomWords[j]);
                }
            }
        }

        [Benchmark]
        public void CompareInlined()
        {
            for (int i = 0; i < randomWords.Length; i++)
            {
                for (int j = 0; j < randomWords.Length; j++)
                {
                    StringHelper.CompareOrdinalHelper(randomWords[i], randomWords[j]);
                }
            }
        }
    }
}