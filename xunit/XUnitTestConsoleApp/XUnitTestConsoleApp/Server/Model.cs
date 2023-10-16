using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnitTestConsoleApp.Server
{
    public enum MyEnum
    {
        T1 = 0,
        T2 = 100,
        T3 = 200
    }

    public struct StructArgs
    {
        public int P1 { get; set; }
    }

    public class Args
    {
        public int P1 { get; set; }
        public double P2 { get; set; }
        public string P3 { get; set; }
    }

    public class Class01
    {
        public int Age { get; set; } = 1;
        public string Name { get; set; }

        public int? P1 { get; set; } = 1;
        public string? P2 { get; set; }
        public (string a, int b)? P3 { get; set; }
        public int? P4;
        public (string a, int b)? P5;
    }

    public class Class02
    {
        public int Age { get; set; }
        public List<int> list { get; set; }
        public string Name { get; set; }
        public int[] nums { get; set; }
    }

    public class Class03 : Class02
    {
        public int Length { get; set; }
    }

    public class Class04
    {
        public int P1 { get; set; }
        public string P2 { get; set; }
        public int P3 { get; set; }
    }

    public class MyClass
    {
        public int P1 { get; set; }
    }

}
