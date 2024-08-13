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
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace XUnitTestProject.Rpc
{
    public class MethodInstanceTest : UnitBase
    {
        [Fact]
        public void FilterMutexAccessTypeShouleBeOk()
        {
            var methodInstance1 = new RpcMethod(typeof(IInterface).GetMethod(nameof(IInterface.Test)));

            Assert.NotNull(methodInstance1);
            Assert.True(methodInstance1.GetFilters().Count == 1);
            Assert.True(methodInstance1.HasCallContext);

            var methodInstance2 = new RpcMethod(typeof(IInterface).GetMethod(nameof(IInterface.Test2)));

            Assert.NotNull(methodInstance2);
            Assert.True(methodInstance2.GetFilters().Count == 2);
            Assert.False(methodInstance2.HasCallContext);

            var methodInstance3 = new RpcMethod(typeof(IInterface).GetMethod(nameof(IInterface.Test3)));

            Assert.NotNull(methodInstance3);
            Assert.True(methodInstance3.GetFilters().Count == 3);

            var methodInstance222 = new RpcMethod(typeof(MyClass222).GetMethod(nameof(MyClass222.Test222)));

            Assert.NotNull(methodInstance222);
            Assert.True(methodInstance222.HasCallContext);
        }

        [Fact]
        public void FilterRunShouleBeOk()
        {

        }

        class MyClass222 : TransientRpcServer
        {
            [DmtpRpc]
            [My]
            [My2]
            public void Test222()
            {

            }
        }

        [My]
        interface IInterface
        {
            [DmtpRpc]
            [My]
            [My2]
            void Test(ICallContext callContext);

            [DmtpRpc]
            [My]
            [My2]
            [My3]
            void Test2();

            [DmtpRpc]
            [My4]
            [My3]
            void Test3();
        }

        class MyClass : IInterface
        {
            [My]
            public void Test(ICallContext callContext)
            {

            }

            public void Test2()
            {

            }

            public void Test3()
            {

            }
        }

        class MyBaseAttribute : RpcActionFilterAttribute
        {
            public override Type[] MutexAccessTypes => new Type[] { typeof(MyBaseAttribute) };
        }

        class MyAttribute : MyBaseAttribute
        {

        }

        class My2Attribute : MyBaseAttribute
        {

        }

        class My3Attribute : RpcActionFilterAttribute
        {

        }

        class My4Attribute : RpcActionFilterAttribute
        {

        }
    }
}