using System.ComponentModel;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Rpc;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace RpcStreamConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StartServer();

            TestRpcPullChannel();
            TestRpcPushChannel();
            Console.ReadKey();
        }

        /// <summary>
        /// 测试Rpc客户端向服务器推送大数据
        /// </summary>
        static async void TestRpcPushChannel()
        {
            var client = CreateClient();
            int size = 0;
            int package = 1024 * 1024;
            Channel channel = client.CreateChannel();//创建通道
            Task task = Task.Run(() =>//这里必须用异步
            {
                for (int i = 0; i < 100; i++)
                {
                    size += package;
                    channel.Write(new byte[package]);
                }
                channel.Complete();//必须调用指令函数，如Complete，Cancel，Dispose
            });

            //此处是直接调用，真正使用时，可以生成代理调用。
            int result = client.Invoke<int>("RpcPushChannel",InvokeOption.WaitInvoke,channel.ID);
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{channel.Status}，result={result}");
        }

        /// <summary>
        /// 测试Rpc客户端向服务器请求大数据
        /// </summary>
        static async void TestRpcPullChannel()
        {
            var client = CreateClient();
            //测试客户端持续请求数据
            int size = 0;
            Channel channel = client.CreateChannel();//创建通道
            Task task = Task.Run(() =>//这里必须用异步
            {
                using (channel)
                {
                    while (channel.MoveNext())
                    {
                        byte[] data = channel.GetCurrent();
                        size += data.Length;
                    }
                }
            });

            //此处是直接调用，真正使用时，可以生成代理调用。
            int result = client.Invoke<int>("RpcPullChannel", InvokeOption.WaitInvoke, channel.ID);
            await task;//等待异步接收完成
            Console.WriteLine($"状态：{channel.Status}，size={size}");
            //测试客户端持续请求数据
        }

        static TcpTouchRpcClient CreateClient()
        {
            TcpTouchRpcClient client = new TcpTouchRpcClient();
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetVerifyToken("TouchRpc"));
            client.Connect();
            return client;
        }

        static void StartServer()
        {
            var service = new TcpTouchRpcService();
            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigureRpcStore(a => 
                   {
                       a.RegisterServer<MyRpcServer>();
                   })
                   .SetVerifyToken("TouchRpc");

            service.Setup(config)
                .Start();

            service.Logger.Info($"{service.GetType().Name}已启动");
        }
    }

    public class MyRpcServer : RpcServer
    {
        /// <summary>
        /// "测试ServiceToClient创建通道，从而实现流数据的传输"
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="channelID"></param>
        [Description("测试ServiceToClient创建通道，从而实现流数据的传输")]
        [TouchRpc(true,MethodFlags = MethodFlags.IncludeCallContext)]//此处设置直接使用方法名调用
        public int RpcPullChannel(ICallContext callContext, int channelID)
        {
            int size = 0;
            int package = 1024 * 1024;
            if (callContext.Caller is TcpTouchRpcSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out Channel channel))
                {
                    for (int i = 0; i < 100; i++)
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
        [TouchRpc(true,MethodFlags = MethodFlags.IncludeCallContext)]//此处设置直接使用方法名调用
        public int RpcPushChannel(ICallContext callContext, int channelID)
        {
            int size = 0;

            if (callContext.Caller is TcpTouchRpcSocketClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out Channel channel))
                {
                    while (channel.MoveNext())
                    {
                        size += channel.GetCurrent().Length;
                    }

                    Console.WriteLine($"服务器接收结束，状态：{channel.Status}，长度：{size}");
                }
            }
            return size;
        }
    }
}