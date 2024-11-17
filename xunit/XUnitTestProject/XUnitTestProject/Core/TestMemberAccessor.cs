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

using TouchSocket.Core;

namespace XUnitTestProject.Core
{
    
    public class TestMemberAccessor
    {
        [Fact]
        public void ClassShouldBeOk()
        {
            var accessor = new DynamicMethodMemberAccessor();

            var p1 = 10;
            var p2 = "RR";
            var p3 = 100;
            var p4 = "QM";

            var myMemberClass = new MyMemberClass();
            accessor.SetValue(myMemberClass, nameof(MyMemberClass.P1), p1);
            accessor.SetValue(myMemberClass, nameof(MyMemberClass.P2), p2);
            accessor.SetValue(myMemberClass, nameof(MyMemberClass.P3), p3);
            accessor.SetValue(myMemberClass, nameof(MyMemberClass.P4), p4);
            Assert.Equal(p1, myMemberClass.P1);
            Assert.Equal(p2, myMemberClass.P2);
            Assert.Equal(p3, myMemberClass.P3);
            Assert.Equal(p4, myMemberClass.P4);

            Assert.Equal(p1, accessor.GetValue(myMemberClass, nameof(MyMemberClass.P1)));
            Assert.Equal(p2, accessor.GetValue(myMemberClass, nameof(MyMemberClass.P2)));
            Assert.Equal(p3, accessor.GetValue(myMemberClass, nameof(MyMemberClass.P3)));
            Assert.Equal(p4, accessor.GetValue(myMemberClass, nameof(MyMemberClass.P4)));
        }

        [Fact]
        public void StructShouldBeOk()
        {
            var accessor = new DynamicMethodMemberAccessor();

            var p1 = 10;
            var p2 = "RR";
            var p3 = 100;
            var p4 = "QM";

            var myMemberStruct = new MyMemberStruct()
            {
                P1 = p1,
                P2 = p2,
                P3 = p3,
                P4 = p4
            };
            Assert.Equal(p1, accessor.GetValue(myMemberStruct, nameof(MyMemberClass.P1)));
            Assert.Equal(p2, accessor.GetValue(myMemberStruct, nameof(MyMemberClass.P2)));
            Assert.Equal(p3, accessor.GetValue(myMemberStruct, nameof(MyMemberClass.P3)));
            Assert.Equal(p4, accessor.GetValue(myMemberStruct, nameof(MyMemberClass.P4)));
        }
    }

    internal class MyMemberClass
    {
        public int P1 { get; set; }
        public string P2 { get; set; }

        public int P3;
        public string P4;
    }

    internal struct MyMemberStruct
    {
        public int P1 { get; set; }
        public string P2 { get; set; }

        public int P3;
        public string P4;
    }
}