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

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcStreamConsoleApp;

internal class Program
{
    public static int Count { get; set; } = 1000;//测试100Mb数据。

    private static async Task Main(string[] args)
    {
        StartServer();
        await TestRpcPullChannel();
        await TestRpcPushChannel();
        Console.ReadKey();
    }

    private static TcpDmtpClient CreateClient()
    {
        var client = new TcpDmtpClient();
        client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            })
            .SetDmtpOption(new DmtpOption()
            {
                VerifyToken = "Dmtp"
            }));
        client.ConnectAsync();
        return client;
    }

    private static void StartServer()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<MyRpcServer>();
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Rpc"
               });

        service.SetupAsync(config);
        service.StartAsync();

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
        var channel = await client.CreateChannelAsync();//创建通道
        var task = Task.Run(async () =>//这里必须用异步
        {
            using (channel)
            {
                await foreach (var byteBlock in channel)
                {
                    size += byteBlock.Length;
                }
            }
        });

        //此处是直接调用，真正使用时，可以生成代理调用。
        var result = await client.GetDmtpRpcActor().InvokeTAsync<int>("RpcPullChannel", InvokeOption.WaitInvoke, channel.Id);
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
        var channel = await client.CreateChannelAsync();//创建通道
        var task = Task.Run(async () =>//这里必须用异步
        {
            for (var i = 0; i < Program.Count; i++)
            {
                size += package;
                await channel.WriteAsync(new byte[package]);
            }
            await channel.CompleteAsync();//必须调用指令函数，如Complete，Cancel，Dispose
        });

        //此处是直接调用，真正使用时，可以生成代理调用。
        var result = await client.GetDmtpRpcActor().InvokeTAsync<int>("RpcPushChannel", InvokeOption.WaitInvoke, channel.Id);
        await task;//等待异步接收完成

        channel.Dispose();
        Console.WriteLine($"状态：{channel.Status}，result={result}");
    }

    public class MyRpcServer : SingletonRpcServer
    {
        /// <summary>
        /// "测试ServiceToClient创建通道，从而实现流数据的传输"
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="channelID"></param>
        [Description("测试ServiceToClient创建通道，从而实现流数据的传输")]
        [DmtpRpc(MethodInvoke = true)]//此处设置直接使用方法名调用
        public async Task<int> RpcPullChannel(ICallContext callContext, int channelID)
        {
            var size = 0;
            var package = 1024 * 1024;
            if (callContext.Caller is ITcpDmtpSessionClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    for (var i = 0; i < Program.Count; i++)
                    {
                        size += package;
                        await channel.WriteAsync(new byte[package]);
                    }
                    await channel.CompleteAsync();//必须调用指令函数，如HoldOn，Complete，Cancel，Dispose
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
        [DmtpRpc(MethodInvoke = true)]//此处设置直接使用方法名调用
        public int RpcPushChannel(ICallContext callContext, int channelID)
        {
            var size = 0;

            if (callContext.Caller is TcpDmtpSessionClient socketClient)
            {
                if (socketClient.TrySubscribeChannel(channelID, out var channel))
                {
                    foreach (var byteBlock in channel)
                    {
                        size += byteBlock.Length;
                    }
                    Console.WriteLine($"服务器接收结束，状态：{channel.Status}，长度：{size}");
                }
            }
            return size;
        }
    }
}