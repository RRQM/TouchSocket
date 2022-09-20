//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RpcProxy;
using System;
using System.Threading.Tasks;
using TouchSocket.Rpc.WebApi;
using Xunit;

namespace XUnitTest.RPC.WebApi
{
    public class TestWebApi
    {
        [Fact]
        public async void ShouldSuccessfulCallService()
        {
            WebApiClient client = new WebApiClient();
            client.Setup("127.0.0.1:7801");
            client.Connect();

            await Task.Run(() =>
             {
                 IXUnitTestController controller = new XUnitTestController(client);

                 controller.GET_Test01_Performance();

                 var p1 = controller.GET_Test03_GetProxyClass();
                 Assert.Equal(10, p1.P1);
                 Assert.Equal(100, p1.P2.P1);
                 Assert.Equal(100, p1.P2.P1);
                 Assert.Equal(1000, p1.P2.P2.P1);

                 int len = new Random().Next(5, 20);
                 var list = controller.GET_Test14_ListClass01(len);
                 Assert.True(len == list.Count);
                 for (int i = 0; i < len; i++)
                 {
                     Assert.Equal(i, list[i].Age);
                 }

                 var m = controller.POST_Test29_TestPost(10, new MyClass() { P1 = 20 });
                 Assert.Equal(30, m.P1);

                 Assert.Equal(30, controller.GET_Sum(10, 20));
             });
        }
    }
}