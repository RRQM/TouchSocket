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
using System;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc.WebApi;
using TouchSocket.Rpc.XmlRpc;
using TouchSocket.Sockets;
using Xunit;

namespace XUnitTest.RPC
{
    public class RpcStoreTest
    {
        [Fact]
        public void TcpTouchRpcServiceShouleBeOk()
        {
            TcpTouchRpcService service = new TcpTouchRpcService();

            service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(GetPort()) })
                .SetVerifyToken("TouchRpc"))
                .Start();
            service.RegisterServer<MyServer>();

            Assert.NotNull(service.RpcStore);
            Assert.True(service.RpcStore.ServerTypes.Length==1);

            service.SafeDispose();
        }

        [Fact]
        public void WebApiShouleBeOk()
        {
            WebApiParserPlugin service = new WebApiParserPlugin(new RpcStore(new Container()));
            service.RegisterServer<MyServer>();

            Assert.NotNull(service.RpcStore);
            Assert.True(service.RpcStore.ServerTypes.Length == 1);

            service.SafeDispose();
        }

        [Fact]
        public void XmlRpcShouleBeOk()
        {
            XmlRpcParserPlugin service = new XmlRpcParserPlugin(new RpcStore(new Container()));
            service.RegisterServer<MyServer>();

            Assert.NotNull(service.RpcStore);
            Assert.True(service.RpcStore.ServerTypes.Length == 1);

            service.SafeDispose();
        }

        [Fact]
        public void JsonRpcShouleBeOk()
        {
            JsonRpcParserPlugin service = new JsonRpcParserPlugin(new RpcStore(new Container()));
            service.RegisterServer<MyServer>();

            Assert.NotNull(service.RpcStore);
            Assert.True(service.RpcStore.ServerTypes.Length == 1);

            service.SafeDispose();
        }

        [Fact]
        public void TcpTouchRpcClientShouleBeOk()
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig());
            client.RegisterServer<MyServer>();

            Assert.NotNull(client.RpcStore);
            Assert.True(client.RpcStore.ServerTypes.Length == 1);

            client.SafeDispose();
        }

        [Fact]
        public void HttpTouchRpcServiceShouleBeOk()
        {
            var service = new HttpTouchRpcService();

            service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(GetPort()) })
                .SetVerifyToken("TouchRpc"))
                .Start();

            service.RegisterServer<MyServer>();
            Assert.NotNull(service.RpcStore);
            Assert.True(service.RpcStore.ServerTypes.Length == 1);
            service.SafeDispose();
        }

        [Fact]
        public void HttpTouchRpcClientShouleBeOk()
        {
            HttpTouchRpcClient client = new HttpTouchRpcClient();
            client.Setup(new TouchSocketConfig());
            client.RegisterServer<MyServer>();

            Assert.NotNull(client.RpcStore);
            Assert.True(client.RpcStore.ServerTypes.Length == 1);

            client.SafeDispose();
        }

        [Fact]
        public void AllTouchRpcShouleBeOk()
        {
            RpcStore rpcStore = new RpcStore(new Container());

            TcpTouchRpcService service1 = new TcpTouchRpcService();
            service1.Setup(new TouchSocketConfig()
                .ConfigureRpcStore(null,rpcStore));

            HttpTouchRpcService service2 = new HttpTouchRpcService();

            TcpTouchRpcClient client1 = new TcpTouchRpcClient();
            HttpTouchRpcClient client2 = new HttpTouchRpcClient();

            WebApiParserPlugin service3 = new WebApiParserPlugin(rpcStore);
            JsonRpcParserPlugin service4 = new JsonRpcParserPlugin(rpcStore);
            XmlRpcParserPlugin service5 = new XmlRpcParserPlugin(rpcStore);

            rpcStore.AddRpcParser(nameof(service1), service1);
            rpcStore.AddRpcParser(nameof(service2), service2);
            rpcStore.AddRpcParser(nameof(client1), client1);
            rpcStore.AddRpcParser(nameof(client2), client2);

            rpcStore.AddRpcParser(nameof(service3), service3);
            rpcStore.AddRpcParser(nameof(service4), service4);
            rpcStore.AddRpcParser(nameof(service5), service5);

            rpcStore.RegisterServer<MyServer>();

            Assert.NotNull(client1.RpcStore);
            Assert.NotNull(client2.RpcStore);
            Assert.NotNull(service1.RpcStore);
            Assert.NotNull(service2.RpcStore);

            Assert.NotNull(service3.RpcStore);
            Assert.NotNull(service4.RpcStore);
            Assert.NotNull(service5.RpcStore);

            Assert.True(client1.RpcStore.ServerTypes.Length == 1);
            Assert.True(client2.RpcStore.ServerTypes.Length == 1);
            Assert.True(service1.RpcStore.ServerTypes.Length == 1);
            Assert.True(service2.RpcStore.ServerTypes.Length == 1);

            Assert.True(service3.RpcStore.ServerTypes.Length == 1);
            Assert.True(service4.RpcStore.ServerTypes.Length == 1);
            Assert.True(service5.RpcStore.ServerTypes.Length == 1);

            //foreach (var item in client1.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
            //foreach (var item in client2.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
            //foreach (var item in service1.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
            //foreach (var item in service2.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
            //foreach (var item in service3.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
            //foreach (var item in service4.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
            //foreach (var item in service5.RpcStore.ServerProviders)
            //{
            //    Assert.Equal(provider, item);
            //}
        }

        private int GetPort()
        {
            return new Random((int)DateTime.Now.Ticks).Next(1024, 60000);
        }
    }

    internal class MyServer : RpcServer
    {
    }
}