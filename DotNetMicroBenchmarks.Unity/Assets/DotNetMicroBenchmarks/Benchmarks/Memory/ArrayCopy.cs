using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace DotNetMicroBenchmarks.Memory
{
    public struct Struct8
    {
        public string Value1;
    }

    public struct Struct16
    {
        public string Value1;
        public string Value2;
    }
    
    public struct Struct24
    {
        public string Value1;
        public string Value2;
        public string Value3;
    }

    public struct Struct32
    {
        public string Value1;
        public string Value2;
        public string Value3;
        public string Value4;
    }

    public struct Struct64
    {
        public string Value1;
        public string Value2;
        public string Value3;
        public string Value4;
        public string Value5;
        public string Value6;
        public string Value7;
        public string Value8;
    }
    
    public struct Struct128
    {
        public string Value1;
        public string Value2;
        public string Value3;
        public string Value4;
        public string Value5;
        public string Value6;
        public string Value7;
        public string Value8;
        public string Value9;
        public string Value10;
        public string Value11;
        public string Value12;
        public string Value13;
        public string Value14;
        public string Value15;
        public string Value16;
    }

    public class ArrayCopyBenchmark
    {
        private const int Count = 1024 * 64;
        private static List<string> _stringList = new(Count);
        private static List<Struct8> _struct8List = new List<Struct8>(Count);
        private static List<Struct16> _struct16List = new List<Struct16>(Count);
        private static List<Struct24> _struct24List = new List<Struct24>(Count);
        private static List<Struct32> _struct32List = new List<Struct32>(Count);
        private static List<Struct64> _struct64List = new List<Struct64>(Count);
        private static List<Struct128> _struct128List = new List<Struct128>(Count);

        public ArrayCopyBenchmark()
        {
            for (var i = 0; i < Count; i++)
            {
                _stringList.Add(string.Empty);
                _struct8List.Add(new Struct8());
                _struct16List.Add(new Struct16());
                _struct24List.Add(new Struct24());
                _struct32List.Add(new Struct32());
                _struct64List.Add(new Struct64());
                _struct128List.Add(new Struct128());
            }
        }


        [Benchmark(Baseline = true)]
        public void FillStrings()
        {
            _stringList.Insert(0, string.Empty);
        }

        [Benchmark]
        public void FillStruct8()
        {
            var str = new Struct8();
            _struct8List.Insert(0, str);
        }

        [Benchmark]
        public void FillStruct16()
        {
            var str = new Struct16();
            _struct16List.Insert(0, str);
        }
        
        [Benchmark]
        public void FillStruct24()
        {
            var str = new Struct24();
            _struct24List.Insert(0, str);
        }

        [Benchmark]
        public void FillStruct32()
        {
            var str = new Struct32();
            _struct32List.Insert(0, str);
        }
        
        [Benchmark]
        public void FillStruct64()
        {
            var str = new Struct64();
            _struct64List.Insert(0, str);
        }
        
        [Benchmark]
        public void FillStruct128()
        {
            var str = new Struct128();
            _struct128List.Insert(0, str);
        }
    }
}