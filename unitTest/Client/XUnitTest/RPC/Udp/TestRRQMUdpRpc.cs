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
using TouchSocket.Core.Config;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;
using Xunit;

namespace XUnitTest.RPC.Udp
{
    public class TestRRQMUdpRpc
    {
        [Theory]
        [InlineData("127.0.0.1:7797", 8848)]
        public void ShouldSuccessfulCallService(string ipHost, int port)
        {
            UdpTouchRpc client = new UdpTouchRpc();

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost(ipHost))
                .SetBindIPHost(new IPHost(port)));

            client.Start();

            RemoteTest remoteTest = new RemoteTest(client);
            remoteTest.Test01(null);
            remoteTest.Test02(null);
            remoteTest.Test03(null);
            remoteTest.Test04(null);
            remoteTest.Test05(null);
            remoteTest.Test06(null);
            remoteTest.Test07(null);
            remoteTest.Test08(null);
            remoteTest.Test09(null);
            remoteTest.Test10(null);
            remoteTest.Test11(null);

            if (new IPHost(ipHost).Port != 7799)
            {
                remoteTest.Test12(null);
            }

            remoteTest.Test13(null);
            remoteTest.Test14(null);
            remoteTest.Test15(null);
            remoteTest.Test16(null);
            remoteTest.Test17(null);
            remoteTest.Test18(null);
            remoteTest.Test22(null);
        }
    }
}