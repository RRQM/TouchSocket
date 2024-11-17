//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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