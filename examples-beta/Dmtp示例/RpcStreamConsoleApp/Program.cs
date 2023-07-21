using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcStreamConsoleApp
{
    public class MyRpcServer : RpcServer
    {
        /// <summary>
        /// "测试ServiceToClient创建通道，从而实现流数据的传输"
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="channelID"></param>
        [Description("测试ServiceToClient创建通道，从而实现流数据的传输")]
        [DmtpRpc(true, MethodFlags = MethodFlags.IncludeCallContext)]//此处设置直接使用方法名调用
        public int RpcPullChannel(ICallContext callContext, int channelID)
        {
            var size = 0;
            var package = 1024 * 1024;
            if (callContext.Caller is ITcpDmtpSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    for (var i = 0; i < Program.Count; i++)
                    {
                        size += package;
                        channel.Write(new byte[package]);
                    }
                    channel.Complete();//必须调用指令函数，如HoldOn，Complete，Cancel，Dispose
                }
            }
            return size;
        }

        /// <summary>
        /// "测试推送"
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="channelID"></param>
        [Description("测试ServiceToClient创建通道，从而实现流数据的传输")]
        [DmtpRpc(true, MethodFlags = MethodFlags.IncludeCallContext)]//此处设置直接使用方法名调用
        public int RpcPushChannel(ICallContext callContext, int channelID)
        {
            var size = 0;

            if (callContext.Caller is TcpDmtpSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    foreach (var byteBlock in channel)
                    {
                        size += byteBlock.Len;
                    }
                    Console.WriteLine($"服务器接收结束，状态：{channel.Status}，长度：{size}");
                }
            }
            return size;
        }
    }

    internal class Program
    {
        private static TcpDmtpClient CreateClient()
        {
            var client = new TcpDmtpClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetBufferLength(1024 * 1024)
                .ConfigurePlugins(a =>
                {
                    a.UseDmtpRpc();
                })
                .SetVerifyToken("Rpc"));
            client.Connect();
            return client;
        }

        public static int Count { get; set; } = 1000;//测试100Mb数据。
        private static async Task Main(string[] args)
        {
            StartServer();
            await TestRpcPullChannel();
            await TestRpcPushChannel();
            Console.ReadKey();
        }

        private static void StartServer()
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .SetBufferLength(1024*1024)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRpc()
                       .ConfigureRpcStore(store =>
                       {
                           store.RegisterServer<MyRpcServer>();
                       });
                   })
                   .SetVerifyToken("Rpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }

        /// <summary>
        /// 测试Rpc客户端向服务器请求大数据
        /// </summary>
        private static async Task TestRpcPullChannel()
        {
            var client = CreateClient();
            //测试客户端持续请求数据
            var size = 0;
            var channel = client.CreateChannel();//创建通道
            var task = Task.Run(() =>//这里必须用异步
            {
                using (channel)
                {
                    foreach (var byteBlock in channel)
                    {
                        size += byteBlock.Len;
                    }
                }
            });

            //此处是直接调用，真正使用时，可以生成代理调用。
            var result = client.GetDmtpRpcActor().InvokeT<int>("RpcPullChannel", InvokeOption.WaitInvoke, channel.Id);
            await task;//等待异步接收完成
            Console.WriteLine($"客户端接收结束，状态：{channel.Status}，size={size}");
            //测试客户端持续请求数据
        }

        /// <summary>
        /// 测试Rpc客户端向服务器推送大数据
        /// </summary>
        private static async Task TestRpcPushChannel()
        {
            var client = CreateClient();
            var size = 0;
            var package = 1024 * 1024;
            var channel = client.CreateChannel();//创建通道
            var task = Task.Run(() =>//这里必须用异步
            {
                for (var i = 0; i < Program.Count; i++)
                {
                    size += package;
                    channel.Write(new byte[package]);
                }
                channel.Complete();//必须调用指令函数，如Complete，Cancel，Dispose
            });

            //此处是直接调用，真正使用时，可以生成代理调用。
            var result = client.GetDmtpRpcActor().InvokeT<int>("RpcPushChannel", InvokeOption.WaitInvoke, channel.Id);
            await task;//等待异步接收完成

            channel.Dispose();
            Console.WriteLine($"状态：{channel.Status}，result={result}");
        }
    }
}