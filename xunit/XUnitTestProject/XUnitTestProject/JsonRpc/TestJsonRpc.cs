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
using TouchSocket.Core;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using XUnitTestProject.Dmtp;
using XUnitTestProject.Rpc;

namespace XUnitTestProject.JsonRpc
{
    public class TestJsonRpc : UnitBase
    {
        [Fact]
        public void TcpShouldBeOk()
        {
            var client = new TouchSocketConfig()
                 .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                 .SetRemoteIPHost("127.0.0.1:7803")
                 .BuildWithTcpJsonRpcClient();

            var remoteTest = new RemoteTest(client);
            remoteTest.Test01(null);
            remoteTest.Test02(null);
            remoteTest.Test03(null);
            remoteTest.Test04(null);
            remoteTest.Test05(null);

            remoteTest.Test09(null);
            remoteTest.Test10(null);
            remoteTest.Test11(null);
            remoteTest.Test12(null);
            remoteTest.Test13(null);
            remoteTest.Test14(null);
            remoteTest.Test15(null);
            remoteTest.Test16(null);
            remoteTest.Test17(null);
            remoteTest.Test18(null);
            remoteTest.Test22(null);
        }

        [Fact]
        public void HttpShouldBeOk()
        {
            var client = new TouchSocketConfig()
                 .SetRemoteIPHost("http://127.0.0.1:7801/jsonRpc")
                 .BuildWithHttpJsonRpcClient();

            var remoteTest = new RemoteTest(client);
            remoteTest.Test01(null);
            remoteTest.Test02(null);
            remoteTest.Test03(null);
            remoteTest.Test04(null);
            remoteTest.Test05(null);

            remoteTest.Test09(null);
            remoteTest.Test10(null);
            remoteTest.Test11(null);
            remoteTest.Test12(null);
            remoteTest.Test13(null);
            remoteTest.Test14(null);
            remoteTest.Test15(null);
            remoteTest.Test16(null);
            remoteTest.Test17(null);
            remoteTest.Test18(null);
            remoteTest.Test22(null);
        }

        [Fact]
        public void WebSocketShouldBeOk()
        {
            var client = new TouchSocketConfig()
                 .SetRemoteIPHost("ws://127.0.0.1:7801/wsjsonrpc")
                 .BuildWithWebSocketJsonRpcClient();

            var remoteTest = new RemoteTest(client);
            remoteTest.Test01(null);
            remoteTest.Test02(null);
            remoteTest.Test03(null);
            remoteTest.Test04(null);
            remoteTest.Test05(null);

            remoteTest.Test09(null);
            remoteTest.Test10(null);
            remoteTest.Test11(null);
            remoteTest.Test12(null);
            remoteTest.Test13(null);
            remoteTest.Test14(null);
            remoteTest.Test15(null);
            remoteTest.Test16(null);
            remoteTest.Test17(null);
            remoteTest.Test18(null);
            remoteTest.Test22(null);
        }

        [Fact]
        public void WebSocketReseverShouldBeOk()
        {
            var client = new TouchSocketConfig()
                .ConfigurePlugins(a =>
                {
                    a.UseGlobalRpcStore()
                        .ConfigureRpcStore(store =>
                        {
                            store.RegisterServer<CallbackServer>();
                        });
                })
                .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                 .SetRemoteIPHost("127.0.0.1:7803")
                 .BuildWithTcpJsonRpcClient();

            var remoteTest = new RemoteTest(client);

            remoteTest.Test38(null, null);

        }

        [Fact]
        public void TcpReseverShouldBeOk()
        {
            var client = new TouchSocketConfig()
                .ConfigurePlugins(a =>
                {
                    a.UseGlobalRpcStore()
                        .ConfigureRpcStore(store =>
                        {
                            store.RegisterServer<CallbackServer>();
                        });
                })
                 .SetRemoteIPHost("ws://127.0.0.1:7801/wsjsonrpc")
                 .BuildWithWebSocketJsonRpcClient();

            var remoteTest = new RemoteTest(client);

            remoteTest.Test38(null, null);

        }
    }
}