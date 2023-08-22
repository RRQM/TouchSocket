#if NETCOREAPP3_1_OR_GREATER
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core;

namespace BenchmarkConsoleApp.Benchmark
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class BenchmarkSerializationComposite : BenchmarkBase
    {
        public BenchmarkSerializationComposite()
        {
            this.Count = 10;
        }

        [Benchmark(Baseline = true)]
        public void DirectNew()
        {
            var v = this.GetStudent();
            for (var i = 0; i < this.Count; i++)
            {
                var bytes = new byte[80 * 1024];//开辟小内存模拟序列化
                var val = this.GetStudent();//直接新建，模拟反序列化
            }
        }

        [Benchmark]
        public void NewtonsoftJson()
        {
            var v = this.GetStudent();
            for (var i = 0; i < this.Count; i++)
            {
                var str = Newtonsoft.Json.JsonConvert.SerializeObject(v);
                var val = Newtonsoft.Json.JsonConvert.DeserializeObject<Student>(str);
            }
        }

        [Benchmark]
        public void SystemTextJson()
        {
            var v = this.GetStudent();
            for (var i = 0; i < this.Count; i++)
            {
                var str = System.Text.Json.JsonSerializer.Serialize(v);
                var val = System.Text.Json.JsonSerializer.Deserialize<Student>(str);
            }
        }

        [Benchmark]
        public void FastBinarySerialize()
        {
            var v = this.GetStudent();
            using (var byteBlock = new ByteBlock(1024 * 512))
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();
                    SerializeConvert.FastBinarySerialize(byteBlock, v);
                    byteBlock.Seek(0);
                    var val = SerializeConvert.FastBinaryDeserialize<Student>(byteBlock.Buffer);
                }
            }
        }

        [Benchmark]
        public void SystemBinarySerialize()
        {
            var v = this.GetStudent();
            using (var byteBlock = new ByteBlock(1024 * 512))
            {
                for (var i = 0; i < this.Count; i++)
                {
                    byteBlock.Reset();
                    SerializeConvert.BinarySerialize(byteBlock, v);
                    byteBlock.Seek(0);
                    var val = SerializeConvert.BinaryDeserialize<Student>(byteBlock.Buffer);
                }
            }
        }


        private Student GetStudent()
        {
            var student = new Student();
            student.P1 = 10;
            student.P2 = "若汝棋茗";
            student.P3 = 100;
            student.P4 = 0;
            student.P5 = DateTime.Now;
            student.P6 = 10;
            student.P7 = new byte[1024 * 64];

            var random = new Random();
            random.NextBytes(student.P7);

            student.List1 = new List<int>();
            student.List1.Add(1);
            student.List1.Add(2);
            student.List1.Add(3);

            student.List2 = new List<string>();
            student.List2.Add("1");
            student.List2.Add("2");
            student.List2.Add("3");

            student.List3 = new List<byte[]>();
            student.List3.Add(new byte[1024]);
            student.List3.Add(new byte[1024]);
            student.List3.Add(new byte[1024]);

            student.Dic1 = new Dictionary<int, int>();
            student.Dic1.Add(1, 1);
            student.Dic1.Add(2, 2);
            student.Dic1.Add(3, 3);

            student.Dic2 = new Dictionary<int, string>();
            student.Dic2.Add(1, "1");
            student.Dic2.Add(2, "2");
            student.Dic2.Add(3, "3");

            student.Dic3 = new Dictionary<string, string>();
            student.Dic3.Add("1", "1");
            student.Dic3.Add("2", "2");
            student.Dic3.Add("3", "3");

            student.Dic4 = new Dictionary<int, Arg>();
            student.Dic4.Add(1, new Arg(1));
            student.Dic4.Add(2, new Arg(2));
            student.Dic4.Add(3, new Arg(3));

            return student;
        }
    }

    [Serializable]
    public class Student
    {
        public int P1 { get; set; }
        public string P2 { get; set; }
        public long P3 { get; set; }
        public byte P4 { get; set; }
        public DateTime P5 { get; set; }
        public double P6 { get; set; }
        public byte[] P7 { get; set; }

        public List<int> List1 { get; set; }
        public List<string> List2 { get; set; }
        public List<byte[]> List3 { get; set; }

        public Dictionary<int, int> Dic1 { get; set; }
        public Dictionary<int, string> Dic2 { get; set; }
        public Dictionary<string, string> Dic3 { get; set; }
        public Dictionary<int, Arg> Dic4 { get; set; }
    }
    [Serializable]
    public class Arg
    {
        public Arg(int myProperty)
        {
            this.MyProperty = myProperty;
        }

        public Arg()
        {
            var person = new Person();
            person.Name = "张三";
            person.Age = 18;
        }

        public int MyProperty { get; set; }
    }
    [Serializable]
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
#endif