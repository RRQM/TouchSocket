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

using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeClientConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var client = await CreateClientAsync();


        while (true)
        {
            #region NamedPipe客户端发送数据
            await client.SendAsync(Console.ReadLine());
            #endregion

        }
    }

    static async Task RunAsyncReceived()
    {
        #region NamedPipe客户端异步阻塞接收
        var client = new NamedPipeClient();

        await client.ConnectAsync("touchsocketpipe");

        using (var receiver = client.CreateReceiver())
        {
            while (true)
            {
                await client.SendAsync(Console.ReadLine());

                var cts = new CancellationTokenSource(1000 * 10);
                using (var receiverResult = await receiver.ReadAsync(cts.Token))
                {
                    var memory = receiverResult.Memory;

                    var mes = memory.Span.ToString(Encoding.UTF8);
                    client.Logger.Info($"客户端接收到信息：{mes}");

                    if (receiverResult.IsCompleted)
                    {
                        //断开连接
                        break;
                    }
                }
            }
        }
        #endregion


        #region NamedPipe重连插件暂停重连
        client.SetPauseReconnection(true);//暂停重连
        client.SetPauseReconnection(false);//恢复重连
        #endregion
    }

    private static async Task<NamedPipeClient> CreateClientAsync()
    {
        #region 创建NamedPipe客户端
        var client = new NamedPipeClient();

        #region NamedPipe客户端使用Received异步委托接收数据
        client.Received = (client, e) =>
        {
            //从服务器收到信息
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"客户端接收到信息：{mes}");

            return Task.CompletedTask;
        };
        #endregion


        var config = GetConfig();

        //载入配置
        await client.SetupAsync(config);

        //连接服务器
        await client.ConnectAsync();
        #endregion

        client.Logger.Info("客户端成功连接");
        return client;
    }

    private static TouchSocketConfig GetConfig()
    {
        TouchSocketConfig config = new TouchSocketConfig();

        config.SetPipeServerName(".");

        #region 客户端设置命名管道名称
        config.SetPipeName("touchsocketpipe");
        #endregion

        config.SetPipeServerName(".");

        #region NamedPipe客户端配置插件
        config.ConfigurePlugins(a =>
        {
            a.Add<MyNamedPipeReceived>();
        });
        #endregion

        #region NamedPipe客户端启用断线重连
        config.ConfigurePlugins(a =>
        {
            a.UseReconnection<NamedPipeClient>();
        });
        #endregion

        config.ConfigureContainer(a =>
        {
            a.AddConsoleLogger();
        });


        return config;
    }
}

#region NamedPipe客户端使用插件接收
class MyNamedPipeReceived : PluginBase, INamedPipeReceivedPlugin
{
    public async Task OnNamedPipeReceived(INamedPipeSession client, ReceivedDataEventArgs e)
    {
        //从服务器收到信息
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        Console.WriteLine($"客户端接收到信息：{mes}");

        await e.InvokeNext();
    }
}
#endregion


#region 从继承创建NamedPipe客户端
class MyNamedPipeClient : NamedPipeClient
{
    protected override async Task OnNamedPipeReceived(ReceivedDataEventArgs e)
    {
        //从服务器收到信息
        var mes = e.Memory.Span.ToString(Encoding.UTF8);
        this.Logger.Info($"客户端接收到信息：{mes}");
        await base.OnNamedPipeReceived(e);
    }
}
#endregion
