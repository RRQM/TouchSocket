using System.Diagnostics;
using TouchSocket.Core;

namespace FastBinaryFormatterConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //未完成
            //SerializeConvert.AddFastBinary<Arg>();
            //SerializeConvert.AddFastBinary<List<int>>();
            //SerializeConvert.AddFastBinary<List<string>>();
            //SerializeConvert.AddFastBinary<List<byte[]>>();
            //SerializeConvert.AddFastBinary<Dictionary<int,int>>();
            //SerializeConvert.AddFastBinary<Dictionary<int,string>>();
            //SerializeConvert.AddFastBinary<Dictionary<string,string>>();
            //SerializeConvert.AddFastBinary<Dictionary<int,Arg>>();

            GlobalEnvironment.DynamicBuilderType = DynamicBuilderType.Reflect;
            ShouldSerializeObjBeOk();
            Console.ReadKey();
        }

        public static void ShouldSerializeObjBeOk()
        {
            var student = new Student();
            student.P1 = 10;
            student.P2 = "若汝棋茗";
            student.P3 = 100;
            student.P4 = 0;
            student.P5 = DateTime.Now;
            student.P6 = 10;
            student.P7 = new byte[1024 * 64];
            student.P8 = new string[] { "I", "love", "you" };

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

            var byteBlock = new ByteBlock(1024 * 512);
            SerializeConvert.FastBinarySerialize(byteBlock, student);

            Console.WriteLine("序列化完成");

            var newStudent = SerializeConvert.FastBinaryDeserialize<Student>(byteBlock.Buffer, 0);
            byteBlock.Dispose();

            Console.WriteLine(Equals(student.P1, newStudent.P1));
            Console.WriteLine(Equals(student.P2, newStudent.P2));
            Console.WriteLine(Equals(student.P3, newStudent.P3));
            Console.WriteLine(Equals(student.P4, newStudent.P4));
            Console.WriteLine(Equals(student.P5, newStudent.P5));
            Console.WriteLine(Equals(student.P6, newStudent.P6));


            Console.WriteLine(newStudent.P7 != null);
            Console.WriteLine(Equals(student.P7.Length, newStudent.P7.Length));

            //Assert.NotNull(newStudent.P8);
            //Assert.Equal(student.P8.Length, newStudent.P8.Length);

            //for (var i = 0; i < student.P8.Length; i++)
            //{
            //    Assert.Equal(student.P8[i], newStudent.P8[i]);
            //}

            //for (var i = 0; i < student.P7.Length; i++)
            //{
            //    Assert.Equal(student.P7[i], newStudent.P7[i]);
            //}

            //Assert.NotNull(newStudent.List1);
            //Assert.Equal(student.List1.Count, newStudent.List1.Count);
            //for (var i = 0; i < student.List1.Count; i++)
            //{
            //    Assert.Equal(student.List1[i], newStudent.List1[i]);
            //}

            //Assert.NotNull(newStudent.List2);
            //Assert.Equal(student.List2.Count, newStudent.List2.Count);
            //for (var i = 0; i < student.List2.Count; i++)
            //{
            //    Assert.Equal(student.List2[i], newStudent.List2[i]);
            //}

            //Assert.NotNull(newStudent.List3);
            //Assert.Equal(student.List3.Count, newStudent.List3.Count);
            //for (var i = 0; i < student.List3.Count; i++)
            //{
            //    Assert.Equal(student.List3[i].Length, newStudent.List3[i].Length);
            //}

            //Assert.NotNull(newStudent.Dic1);
            //Assert.Equal(student.Dic1.Count, newStudent.Dic1.Count);

            //Assert.NotNull(newStudent.Dic2);
            //Assert.Equal(student.Dic2.Count, newStudent.Dic2.Count);

            //Assert.NotNull(newStudent.Dic3);
            //Assert.Equal(student.Dic3.Count, newStudent.Dic3.Count);

            //Assert.NotNull(newStudent.Dic4);
            //Assert.Equal(student.Dic4.Count, newStudent.Dic4.Count);
        }
    }

    public class Arg
    {
        public Arg()
        {
        }

        public Arg(int myProperty)
        {
            this.MyProperty = myProperty;
        }

        public int MyProperty { get; set; }
    }

    public class Student
    {
        public int P1 { get; set; }
        public string P2 { get; set; }
        public long P3 { get; set; }
        public byte P4 { get; set; }
        public DateTime P5 { get; set; }
        public double P6 { get; set; }
        public byte[] P7 { get; set; }
        public string[] P8 { get; set; }

        public List<int> List1 { get; set; }
        public List<string> List2 { get; set; }
        public List<byte[]> List3 { get; set; }

        public Dictionary<int, int> Dic1 { get; set; }
        public Dictionary<int, string> Dic2 { get; set; }
        public Dictionary<string, string> Dic3 { get; set; }
        public Dictionary<int, Arg> Dic4 { get; set; }
    }


}