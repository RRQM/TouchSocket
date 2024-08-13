using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    public class TestPackage
    {
        [Fact]
        public void GeneratorPackageShouldBeOk()
        {
            var myPackage = new MyPackage()
            {
                Property1 = true,
                Property2 = sbyte.MaxValue,
                Property3 = byte.MaxValue,
                Property4 = sbyte.MaxValue,
                Property5 = ushort.MaxValue,
                Property6 = int.MaxValue,
                Property7 = uint.MaxValue,
                Property8 = long.MaxValue,
                Property9 = ulong.MaxValue,
                Property10 = float.MaxValue,
                Property11 = double.MaxValue,
                Property12 = decimal.MaxValue,
                Property13 = DateTime.MaxValue,
                Property14 = TimeSpan.MaxValue,
                Property15 = "RRQM",
                Property16 = char.MaxValue,
                Property17 = new MyPackageClass() { Property1 = 10, Property2 = 20 },
                Property18 = new MyStructPackageClass() { Property1 = 10, Property2 = 20 },
                Property19 = new int[] { 0, 1, 2, 3, 4 },
                Property20 = new List<int>() { 0, 1, 2, 3, 4 },
                Property21 = new Dictionary<int, string>() { { 0, "0" }, { 1, "1" } }
            };

            using (var byteBlock = new ByteBlock())
            {
                myPackage.Package(byteBlock);

                byteBlock.Seek(0, SeekOrigin.Begin);

                var myNewPackage = new MyPackage();
                myNewPackage.Unpackage(byteBlock);

                Assert.Equal(myPackage.Property1, myNewPackage.Property1);
                Assert.Equal(myPackage.Property2, myNewPackage.Property2);
                Assert.Equal(myPackage.Property3, myNewPackage.Property3);
                Assert.Equal(myPackage.Property4, myNewPackage.Property4);
                Assert.Equal(myPackage.Property5, myNewPackage.Property5);
                Assert.Equal(myPackage.Property6, myNewPackage.Property6);
                Assert.Equal(myPackage.Property7, myNewPackage.Property7);
                Assert.Equal(myPackage.Property8, myNewPackage.Property8);
                Assert.Equal(myPackage.Property9, myNewPackage.Property9);
                Assert.Equal(myPackage.Property10, myNewPackage.Property10);
                Assert.Equal(myPackage.Property11, myNewPackage.Property11);
                Assert.Equal(myPackage.Property12, myNewPackage.Property12);
                Assert.Equal(myPackage.Property13, myNewPackage.Property13);
                Assert.Equal(myPackage.Property14, myNewPackage.Property14);
                Assert.Equal(myPackage.Property15, myNewPackage.Property15);
                Assert.Equal(myPackage.Property16, myNewPackage.Property16);

                Assert.NotNull(myPackage.Property17);
                Assert.Equal(myPackage.Property17.Property1, myNewPackage.Property17.Property1);
                Assert.Equal(myPackage.Property17.Property2, myNewPackage.Property17.Property2);

                Assert.Equal(myPackage.Property18.Property1, myNewPackage.Property18.Property1);
                Assert.Equal(myPackage.Property18.Property2, myNewPackage.Property18.Property2);

                Assert.NotNull(myPackage.Property19);
                Assert.Equal(5, myPackage.Property19.Length);
                Assert.True(myPackage.Property19.SequenceEqual(new int[] { 0, 1, 2, 3, 4 }));

                Assert.NotNull(myPackage.Property20);
                Assert.Equal(5, myPackage.Property20.Count);
                Assert.True(myPackage.Property20.SequenceEqual(new int[] { 0, 1, 2, 3, 4 }));

                Assert.NotNull(myPackage.Property21);
                Assert.Equal(2, myPackage.Property21.Count);
                Assert.True(myPackage.Property21.TryGetValue(0, out var v1) && v1 == "0");
                Assert.True(myPackage.Property21.TryGetValue(1, out var v2) && v2 == "1");
            }
        }

        [Fact]
        public void GeneratorStructPackageShouldBeOk()
        {
            var myPackage = new MyStructPackage()
            {
                Property1 = true,
                Property2 = sbyte.MaxValue,
                Property3 = byte.MaxValue,
                Property4 = sbyte.MaxValue,
                Property5 = ushort.MaxValue,
                Property6 = int.MaxValue,
                Property7 = uint.MaxValue,
                Property8 = long.MaxValue,
                Property9 = ulong.MaxValue,
                Property10 = float.MaxValue,
                Property11 = double.MaxValue,
                Property12 = decimal.MaxValue,
                Property13 = DateTime.MaxValue,
                Property14 = TimeSpan.MaxValue,
                Property15 = "RRQM",
                Property16 = char.MaxValue,
                Property17 = new MyPackageClass() { Property1 = 10, Property2 = 20 },
                Property18 = new MyStructPackageClass() { Property1 = 10, Property2 = 20 },
                Property19 = new int[] { 0, 1, 2, 3, 4 },
                Property20 = new List<int>() { 0, 1, 2, 3, 4 },
                Property21 = new Dictionary<int, string>() { { 0, "0" }, { 1, "1" } }
            };

            using (var byteBlock = new ByteBlock())
            {
                myPackage.Package(byteBlock);

                byteBlock.Seek(0, SeekOrigin.Begin);

                var myNewPackage = new MyPackage();
                myNewPackage.Unpackage(byteBlock);

                Assert.Equal(myPackage.Property1, myNewPackage.Property1);
                Assert.Equal(myPackage.Property2, myNewPackage.Property2);
                Assert.Equal(myPackage.Property3, myNewPackage.Property3);
                Assert.Equal(myPackage.Property4, myNewPackage.Property4);
                Assert.Equal(myPackage.Property5, myNewPackage.Property5);
                Assert.Equal(myPackage.Property6, myNewPackage.Property6);
                Assert.Equal(myPackage.Property7, myNewPackage.Property7);
                Assert.Equal(myPackage.Property8, myNewPackage.Property8);
                Assert.Equal(myPackage.Property9, myNewPackage.Property9);
                Assert.Equal(myPackage.Property10, myNewPackage.Property10);
                Assert.Equal(myPackage.Property11, myNewPackage.Property11);
                Assert.Equal(myPackage.Property12, myNewPackage.Property12);
                Assert.Equal(myPackage.Property13, myNewPackage.Property13);
                Assert.Equal(myPackage.Property14, myNewPackage.Property14);
                Assert.Equal(myPackage.Property15, myNewPackage.Property15);
                Assert.Equal(myPackage.Property16, myNewPackage.Property16);

                Assert.NotNull(myPackage.Property17);
                Assert.Equal(myPackage.Property17.Property1, myNewPackage.Property17.Property1);
                Assert.Equal(myPackage.Property17.Property2, myNewPackage.Property17.Property2);

                Assert.Equal(myPackage.Property18.Property1, myNewPackage.Property18.Property1);
                Assert.Equal(myPackage.Property18.Property2, myNewPackage.Property18.Property2);

                Assert.NotNull(myPackage.Property19);
                Assert.Equal(5, myPackage.Property19.Length);
                Assert.True(myPackage.Property19.SequenceEqual(new int[] { 0, 1, 2, 3, 4 }));

                Assert.NotNull(myPackage.Property20);
                Assert.Equal(5, myPackage.Property20.Count);
                Assert.True(myPackage.Property20.SequenceEqual(new int[] { 0, 1, 2, 3, 4 }));

                Assert.NotNull(myPackage.Property21);
                Assert.Equal(2, myPackage.Property21.Count);
                Assert.True(myPackage.Property21.TryGetValue(0, out var v1) && v1 == "0");
                Assert.True(myPackage.Property21.TryGetValue(1, out var v2) && v2 == "1");
            }
        }

        [Fact]
        public void GeneratorListPackageShouldBeOk()
        {
            var myPackage = new MyListPackage()
            {
                Property1 = new List<MyPackageClass>() {new MyPackageClass(),null,new MyPackageClass(),null },
                Property2 = new List<MyStructPackageClass>() { new MyStructPackageClass(),default,new MyStructPackageClass(),default }
            };

            using (var byteBlock = new ByteBlock())
            {
                myPackage.Package(byteBlock);

                byteBlock.Seek(0, SeekOrigin.Begin);

                var myNewPackage = new MyListPackage();
                myNewPackage.Unpackage(byteBlock);

                Assert.NotNull(myNewPackage.Property1);
                Assert.True(myNewPackage.Property1.Count==4);
                Assert.NotNull(myNewPackage.Property1[0]);
                Assert.Null(myNewPackage.Property1[1]);
                Assert.NotNull(myNewPackage.Property1[2]);
                Assert.Null(myNewPackage.Property1[3]);

                Assert.NotNull(myNewPackage.Property2);
                Assert.True(myNewPackage.Property2.Count == 4);
            }
        }

        [Fact]
        public void StudentPackage()
        {
            var studentPackage = GetStudentPackage();

            using (var byteBlock = new ByteBlock())
            {
                studentPackage.Package(byteBlock);

                byteBlock.SeekToStart();

                var studentPackageNew = new StudentPackage();
                studentPackageNew.Unpackage(byteBlock);
            }
        }

        private StudentPackage GetStudentPackage()
        {
            var student = new StudentPackage();
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

            student.Dic4 = new Dictionary<int, ArgPackage>();
            student.Dic4.Add(1, new ArgPackage() { MyProperty = 1 });
            student.Dic4.Add(2, new ArgPackage() { MyProperty = 2 });
            student.Dic4.Add(3, new ArgPackage() { MyProperty = 3 });

            return student;
        }

    }

    [GeneratorPackage]
    partial class MyPackage2 : PackageBase
    {
        public Dictionary<string, MyPackage> Property1 { get; set; }
        public Dictionary<string, MyStructPackage> Property2 { get; set; }
        public List<MyPackage> Property3 { get; set; }
    }

    [GeneratorPackage]
    internal partial class MyPackage : PackageBase
    {
        public bool Property1 { get; set; }
        public sbyte Property2 { get; set; }
        public byte Property3 { get; set; }
        public short Property4 { get; set; }
        public ushort Property5 { get; set; }
        public int Property6 { get; set; }
        public uint Property7 { get; set; }
        public long Property8 { get; set; }
        public ulong Property9 { get; set; }
        public float Property10 { get; set; }
        public double Property11 { get; set; }
        public decimal Property12 { get; set; }
        public DateTime Property13 { get; set; }
        public TimeSpan Property14 { get; set; }
        public string Property15 { get; set; }
        public char Property16 { get; set; }
        public MyPackageClass Property17 { get; set; }
        public MyStructPackageClass Property18 { get; set; }
        public int[] Property19 { get; set; }
        public List<int> Property20 { get; set; }
        public Dictionary<int, string> Property21 { get; set; }
    }

    [GeneratorPackage]
    partial struct MyStructPackage : IPackage
    {
        public bool Property1 { get; set; }
        public sbyte Property2 { get; set; }
        public byte Property3 { get; set; }
        public short Property4 { get; set; }
        public ushort Property5 { get; set; }
        public int Property6 { get; set; }
        public uint Property7 { get; set; }
        public long Property8 { get; set; }
        public ulong Property9 { get; set; }
        public float Property10 { get; set; }
        public double Property11 { get; set; }
        public decimal Property12 { get; set; }
        public DateTime Property13 { get; set; }
        public TimeSpan Property14 { get; set; }
        public string Property15 { get; set; }
        public char Property16 { get; set; }
        public MyPackageClass Property17 { get; set; }
        public MyStructPackageClass Property18 { get; set; }
        public int[] Property19 { get; set; }
        public List<int> Property20 { get; set; }
        public Dictionary<int, string> Property21 { get; set; }
    }

    [GeneratorPackage]
    partial class MyPackageClass : PackageBase
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
    }

    [GeneratorPackage]
    partial struct MyStructPackageClass : IPackage
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
    }

    [GeneratorPackage]
    partial class MyListPackage : PackageBase
    {
        public List<MyPackageClass> Property1 { get; set; }
        public List<MyStructPackageClass> Property2 { get; set; }
    }

    #region MyRegion
    
    [GeneratorPackage]
    [Serializable]
    public partial class StudentPackage : PackageBase
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
        public Dictionary<int, ArgPackage> Dic4 { get; set; }
    }

    
    [GeneratorPackage]
    [Serializable]
    public partial class ArgPackage : PackageBase
    {
        public int MyProperty { get; set; }
        public PersonPackage MyProperty2 { get; set; }
    }

    [GeneratorPackage]
    [Serializable]
    public partial class PersonPackage : PackageBase
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    #endregion
}
