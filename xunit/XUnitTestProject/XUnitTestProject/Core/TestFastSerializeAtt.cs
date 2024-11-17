using System.Data;
using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestFastSerializeAtt
    {
        [Fact]
        public void ShouldSerializeReadonlyObjBeOk()
        {
            var processInfo = new ReadonlyProcessInfoAtt("name", 10, "winName", "location");
            var data = SerializeConvert.FastBinarySerialize(processInfo);

            var sobj = SerializeConvert.FastBinaryDeserialize<ProcessInfoAtt>(data, 0);
            Assert.True(processInfo.Name == sobj.Name);
            Assert.True(processInfo.PID == sobj.PID);
            Assert.Null(sobj.WinName);
            Assert.Null(sobj.Location);
        }

        [Fact]
        public void ShouldSerializeObjBeOk()
        {
            var student = new StudentAtt();
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

            student.Dic4 = new Dictionary<int, ArgAtt>();
            student.Dic4.Add(1, new ArgAtt(1));
            student.Dic4.Add(2, new ArgAtt(2));
            student.Dic4.Add(3, new ArgAtt(3));

            var byteBlock = new ByteBlock(1024 * 512);
            SerializeConvert.FastBinarySerialize(byteBlock, student);
            var newStudent = SerializeConvert.FastBinaryDeserialize<StudentAtt>(byteBlock.Buffer, 0);
            byteBlock.Dispose();

            Assert.Equal(student.P1, newStudent.P1);
            Assert.Equal(student.P2, newStudent.P2);
            Assert.Equal(student.P3, newStudent.P3);
            Assert.Equal(student.P4, newStudent.P4);
            Assert.Equal(student.P5, newStudent.P5);
            Assert.Equal(student.P6, newStudent.P6);

            Assert.NotNull(newStudent.P7);
            Assert.Equal(student.P7.Length, newStudent.P7.Length);

            Assert.NotNull(newStudent.P8);
            Assert.Equal(student.P8.Length, newStudent.P8.Length);

            for (var i = 0; i < student.P8.Length; i++)
            {
                Assert.Equal(student.P8[i], newStudent.P8[i]);
            }

            for (var i = 0; i < student.P7.Length; i++)
            {
                Assert.Equal(student.P7[i], newStudent.P7[i]);
            }

            Assert.NotNull(newStudent.List1);
            Assert.Equal(student.List1.Count, newStudent.List1.Count);
            for (var i = 0; i < student.List1.Count; i++)
            {
                Assert.Equal(student.List1[i], newStudent.List1[i]);
            }

            Assert.NotNull(newStudent.List2);
            Assert.Equal(student.List2.Count, newStudent.List2.Count);
            for (var i = 0; i < student.List2.Count; i++)
            {
                Assert.Equal(student.List2[i], newStudent.List2[i]);
            }

            Assert.NotNull(newStudent.List3);
            Assert.Equal(student.List3.Count, newStudent.List3.Count);
            for (var i = 0; i < student.List3.Count; i++)
            {
                Assert.Equal(student.List3[i].Length, newStudent.List3[i].Length);
            }

            Assert.NotNull(newStudent.Dic1);
            Assert.Equal(student.Dic1.Count, newStudent.Dic1.Count);

            Assert.NotNull(newStudent.Dic2);
            Assert.Equal(student.Dic2.Count, newStudent.Dic2.Count);

            Assert.NotNull(newStudent.Dic3);
            Assert.Equal(student.Dic3.Count, newStudent.Dic3.Count);

            Assert.NotNull(newStudent.Dic4);
            Assert.Equal(student.Dic4.Count, newStudent.Dic4.Count);
        }

        [Fact]
        public void ShouldSerializeProcessInfoesBeOk()
        {
            var infos = new List<ProcessInfoAtt>();
            for (var i = 0; i < 200; i++)
            {
                infos.Add(new ProcessInfoAtt() { Location = i.ToString(), Name = i.ToString(), PID = i, WinName = i.ToString() });
            }

            var byteBlock = new ByteBlock(10240);
            SerializeConvert.FastBinarySerialize(byteBlock, infos.ToArray());
            var datas = byteBlock.ToArray();
            var newinfos = SerializeConvert.FastBinaryDeserialize<ProcessInfoAtt[]>(datas, 0);
            Assert.NotNull(newinfos);
            Assert.Equal(infos.Count, newinfos.Length);

            for (var i = 0; i < infos.Count; i++)
            {
                Assert.Equal(infos[i].Location, newinfos[i].Location);
                Assert.Equal(infos[i].Name, newinfos[i].Name);
                Assert.Equal(infos[i].WinName, newinfos[i].WinName);
                Assert.Equal(infos[i].PID, newinfos[i].PID);
            }
        }

        [Fact]
        public void ShouldSerializeNullableBeOk()
        {
            var test = new TestNullableAtt();
            test.P1 = 10;
            test.P2 = "RRQM";
            test.P3 = null;
            test.P4 = new ProcessInfoAtt() { Location = "中国", Name = "RRQM", PID = 100, WinName = "RRQM" };

            var data = SerializeConvert.FastBinarySerialize(test);
            var newTest = SerializeConvert.FastBinaryDeserialize<TestNullableAtt>(data);

            Assert.NotNull(newTest);
            Assert.Equal(test.P1, newTest.P1);
            Assert.Equal(test.P2, newTest.P2);
            Assert.Equal(test.P3, newTest.P3);
            Assert.Equal(test.P4.Location, newTest.P4.Location);
            Assert.Equal(test.P4.Name, newTest.P4.Name);
            Assert.Equal(test.P4.PID, newTest.P4.PID);
            Assert.Equal(test.P4.WinName, newTest.P4.WinName);
        }

        [Fact]
        public void ShouldSerializeShortPerObjBeOk()
        {
            var processInfo = new ProcessInfoAtt()
            {
                Location = "location",
                Name = "name",
                PID = 10,
                WinName = "winName",
            };
            var data = SerializeConvert.FastBinarySerialize(processInfo);

            var sobj = SerializeConvert.FastBinaryDeserialize<ProcessInfoShortAtt>(data, 0);
            Assert.True(processInfo.Name == sobj.Name);
            Assert.Null(sobj.Location);
        }
    }

    #region ClassAtt

    [FastSerialized(EnableIndex = true)]
    public class TestNullableAtt
    {
        [FastMember(0)]
        public int? P1 { get; set; }

        [FastMember(1)]
        public string? P2 { get; set; }

        [FastMember(2)]
        public char? P3 { get; set; }

        [FastMember(3)]
        public ProcessInfoAtt? P4 { get; set; }
    }

    [FastSerialized(EnableIndex = true)]
    public class ProcessInfoAtt
    {
        [FastMember(0)]
        public string Name { get; set; }

        [FastMember(1)]
        public int PID { get; set; }

        [FastMember(2)]
        public string WinName { get; set; }

        [FastMember(3)]
        public string Location { get; set; }
    }

    [FastSerialized(EnableIndex = true)]
    public class ReadonlyProcessInfoAtt
    {
        public ReadonlyProcessInfoAtt(string name, int pID, string winName, string location)
        {
            this.Name = name;
            this.PID = pID;
            this.WinName = winName;
            this.Location = location;
        }

        [FastMember(0)]
        [FastSerialized]
        public string Name { get; }

        [FastMember(1)]
        [FastSerialized]
        public int PID { get; }

        [FastMember(2)]
        public string WinName { get; }

        [FastMember(3)]
        public string Location { get; }
    }

    [FastSerialized(EnableIndex = true)]
    public class ProcessInfoShortAtt
    {
        [FastMember(0)]
        public string Name { get; set; }

        [FastMember(3)]
        public string Location { get; set; }
    }

    [FastSerialized(EnableIndex = true)]
    public class ArgAtt
    {
        public ArgAtt()
        {
        }

        public ArgAtt(int myProperty)
        {
            this.MyProperty = myProperty;
        }

        [FastMember(0)]
        public int MyProperty { get; set; }
    }

    [FastSerialized(EnableIndex = true)]
    public class StudentAtt
    {
        [FastMember(0)]
        public int P1 { get; set; }

        [FastMember(1)]
        public string P2 { get; set; }

        [FastMember(2)]
        public long P3 { get; set; }

        [FastMember(3)]
        public byte P4 { get; set; }

        [FastMember(4)]
        public DateTime P5 { get; set; }

        [FastMember(5)]
        public double P6 { get; set; }

        [FastMember(6)]
        public byte[] P7 { get; set; }

        [FastMember(7)]
        public string[] P8 { get; set; }

        [FastMember(8)]
        public List<int> List1 { get; set; }

        [FastMember(9)]
        public List<string> List2 { get; set; }

        [FastMember(10)]
        public List<byte[]> List3 { get; set; }

        [FastMember(11)]
        public Dictionary<int, int> Dic1 { get; set; }

        [FastMember(12)]
        public Dictionary<int, string> Dic2 { get; set; }

        [FastMember(13)]
        public Dictionary<string, string> Dic3 { get; set; }

        [FastMember(14)]
        public Dictionary<int, ArgAtt> Dic4 { get; set; }
    }
    #endregion
}
