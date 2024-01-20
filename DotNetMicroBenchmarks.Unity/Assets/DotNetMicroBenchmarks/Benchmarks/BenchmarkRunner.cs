using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Benchmarks
{
    public static class BenchmarkRunner
    {
        public static void Run<T>(int runs = 3, Action<string> logger = null)
        {
            Run(typeof(T), runs, logger ?? Console.WriteLine);
        }

        public static void Run(Type suiteType, int runs, Action<string> logger)
        {
            for (var i = 0; i < runs; i++)
            {
                RunBenchmark(suiteType, i, logger);
            }
        }

        public static EventWaitHandle RunAsync(Type suiteType, int runs, Action<string> logger)
        {
            return null;
        }

        private static void RunBenchmark(Type suiteType, int run, Action<string> logger)
        {
            GC.Collect();

            // logger($"[{run}] Total Memory: {GC.GetTotalMemory(true)}");
#if !UNITY_2017_1_OR_NEWER
            if (!GC.TryStartNoGCRegion(1000 * 1000))
            {
                logger($"[{run}] Could not suspend GC");
            }
#endif
            var stopwatch = new Stopwatch();

            var callOverhead = GetCallOverhead(stopwatch);
            // logger($"[{run}] Call overhead: {callOverhead}");

            var benchmarks = GetBenchmarks(suiteType);
            var suite = Activator.CreateInstance(suiteType);
            logger($"[{run}] {suiteType.Name}");

            foreach (var benchmark in benchmarks)
            {
                var result = RunBenchmark(suite!, benchmark, stopwatch).DeductOverheadTime(callOverhead);
                if (benchmark.ReturnType != typeof(void)) 
                    result = result.DeductBoxingAllocation(callOverhead);
                
                logger($"[{run}]   {benchmark.Name} took {result}");
            }

#if !UNITY_2017_1_OR_NEWER
            if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            {
                GC.EndNoGCRegion();
            }
#endif
        }

        public static IEnumerable<Type> GetAllBenchmarkSuites(params Assembly[] assemblies)
        {
            var assembliesToSearch = assemblies.Length > 0 ? assemblies : GetAllAssemblies();
            return assembliesToSearch
                .SelectMany(assembly => assembly
                    .GetTypes()
                    .Where(t => t.GetMethods().Any(info => info.GetCustomAttributes<BenchmarkAttribute>().Any()))
                );
        }

        private static Measurement GetCallOverhead(Stopwatch stopwatch)
        {
            var tester = new MethodCallOverheadTester();
            var methodInfo = GetBenchmarks(tester.GetType())[0];

            return RunBenchmark(tester, methodInfo, stopwatch);
        }

        private static Measurement RunBenchmark(object suite, MethodInfo benchmark, Stopwatch stopwatch)
        {
            var operations = EstimateNecessaryOperations(suite, benchmark, stopwatch);

            var allocatedBytes = GetAllocatedBytes();

            stopwatch.Restart();
            for (var i = 0; i < operations; i++)
            {
                benchmark.Invoke(suite, null);
            }

            stopwatch.Stop();

            return new(operations, stopwatch.Elapsed, GetAllocatedBytes() - allocatedBytes);
        }

        private static long GetAllocatedBytes()
        {
#if UNITY_2017_1_OR_NEWER
            return -1;
#else
            return GC.GetAllocatedBytesForCurrentThread();
#endif
        }

        private static int EstimateNecessaryOperations(object suite, MethodInfo benchmark, Stopwatch stopwatch)
        {
            var result = 0;
            stopwatch.Restart();

            while (stopwatch.ElapsedMilliseconds < 100)
            {
                result++;
                benchmark.Invoke(suite, null);
            }

            return result * 10;
        }

        private static List<MethodInfo> GetBenchmarks(Type suiteType)
        {
            var benchmarks = suiteType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(info => info.GetCustomAttribute(typeof(BenchmarkAttribute)) != null)
                .ToList();
            return benchmarks;
        }

        private static IEnumerable<Assembly> GetAllAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            var queue = new HashSet<Assembly>();
            if (Assembly.GetEntryAssembly() != null)
            {
                queue.Add(Assembly.GetEntryAssembly()!);
            }

            queue.Add(Assembly.GetCallingAssembly());
            queue.Add(Assembly.GetExecutingAssembly());

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                queue.Add(assembly);
            }

            var visited = new HashSet<string>();
            while (queue.Any())
            {
                var assembly = queue.First();
                assemblies.Add(assembly);
                queue.Remove(assembly);
                visited.Add(assembly.FullName!);

                var references = assembly.GetReferencedAssemblies();
                foreach (var reference in references)
                {
                    if (!visited.Contains(reference.FullName) && TryLoadAssembly(reference, out var loadedAssembly))
                    {
                        queue.Add(loadedAssembly!);
                    }
                }
            }

            return assemblies;
        }

        private static bool TryLoadAssembly(AssemblyName reference, out Assembly? assembly)
        {
            try
            {
                assembly = Assembly.Load(reference);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not load assembly: {reference.FullName}: {e.Message}");
                assembly = null;
                return false;
            }
        }

        public readonly struct Measurement
        {
            public readonly long Operations;
            public readonly TimeSpan Elapsed;
            public readonly long AllocatedBytes;

            public Measurement(long operations, TimeSpan elapsed, long allocatedBytes)
            {
                Operations = operations;
                Elapsed = elapsed;
                AllocatedBytes = allocatedBytes;
            }

            public override string ToString()
            {
                double scale = 1;
                if (Operations < scale * 1000)
                {
                    return
                        $"{(scale * Elapsed.TotalMilliseconds / Operations):F} ms ({FormatNumber(AllocatedBytes / Operations)})";
                }

                scale *= 1000;
                if (Operations < scale * 1000)
                {
                    return
                        $"{(scale * Elapsed.TotalMilliseconds / Operations):F} μs ({FormatNumber(AllocatedBytes / Operations)})";
                }

                scale *= 1000;
                if (Operations < scale * 1000)
                {
                    return
                        $"{(scale * Elapsed.TotalMilliseconds / Operations):F} ns ({FormatNumber(AllocatedBytes / Operations)})";
                }

                return $"{Elapsed} / {Operations}";
            }

            public Measurement DeductOverheadTime(Measurement other)
            {
                return new(
                    Operations,
                    TimeSpan.FromTicks(
                        Elapsed.Ticks - (long) (other.Elapsed.Ticks * ((double) Operations / other.Operations))),
                    AllocatedBytes
                );
            }
            
            public Measurement DeductBoxingAllocation(Measurement other)
            {
                return new(
                    Operations,
                    Elapsed,
                    AllocatedBytes - (long) (other.AllocatedBytes * ((double) Operations / other.Operations))
                );
            }

            static string FormatNumber(long num)
            {
#if UNITY_2017_1_OR_NEWER
                    return "NA";
#endif

                if (num >= 100000000)
                {
                    return (num / 1000000D).ToString("0.#M");
                }

                if (num >= 1000000)
                {
                    return (num / 1000000D).ToString("0.##M");
                }

                if (num >= 100000)
                {
                    return (num / 1000D).ToString("0.#k");
                }

                if (num >= 10000)
                {
                    return (num / 1000D).ToString("0.##k");
                }

                return num.ToString("0 B");
            }
        }

        class MethodCallOverheadTester
        {
            [Benchmark]
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            public bool BaseLine()
            {
                return true;
            }
        }

        class BenchmarkThread
        {
            private readonly object _lock = new();

            private readonly object _benchmarkSuite;
            private readonly Stopwatch _stopwatch = new();

            private readonly Thread _thread;

            private MethodInfo? _benchmark;

            public BenchmarkThread(object benchmarkSuite)
            {
                _benchmarkSuite = benchmarkSuite;
                _thread = new Thread(Consume);
            }

            public void Run(MethodInfo methodInfo)
            {
                lock (_lock)
                {
                    _benchmark = methodInfo;
                    Monitor.Pulse(_lock);
                }
            }

            private void Consume()
            {
                MethodInfo? benchmark = null;

                lock (_lock)
                {
                    while (_benchmark == null)
                    {
                        Monitor.Wait(_lock);
                    }

                    benchmark = _benchmark;
                }
            }
        }
    }
}