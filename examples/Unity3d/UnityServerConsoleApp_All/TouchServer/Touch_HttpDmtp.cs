// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using UnityRpcProxy;

namespace UnityServerConsoleApp_All.TouchServer;

/// <summary>
/// HTTP_Dmtp 网络服务
/// </summary>
public class Touch_HttpDmtp : BaseTouchServer
{
    private readonly HttpDmtpService dmtpService = new HttpDmtpService();
    public async Task StartService(int port)
    {
        var config = new TouchSocketConfig()//配置
             .SetListenIPHosts(port)

             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();//注册一个日志组

                 //注册rpc服务
                 a.AddRpcStore(store =>
                 {
                     store.RegisterServer<UnityRpcStore>();
#if DEBUG
                     var code = store.GetProxyCodes("UnityRpcProxy_HttpDmtp", typeof(DmtpRpcAttribute));
                     File.WriteAllText("../../../RPCStore/UnityRpcProxy_HttpDmtp.cs", code);
#endif
                 });
             })
             .ConfigurePlugins(a =>
             {
                 //启用dmtp rpc插件
                 a.UseDmtpRpc();

                 a.Add<Touch_Dmtp_Log_Plguin>();

             })
             .SetDmtpOption(options=>
             {
                 options.VerifyToken = "Dmtp";//设置验证token
             });

        await this.dmtpService.SetupAsync(config);
        await this.dmtpService.StartAsync();


        this.dmtpService.Logger.Info($"HttpDmtp已启动，监听端口：{port}");
    }

    /// <summary>
    /// 状态日志打印插件
    /// </summary>
    internal class Touch_Dmtp_Log_Plguin : PluginBase, IDmtpConnectedPlugin, IDmtpClosedPlugin, IDmtpCreatedChannelPlugin
    {
        public async Task OnDmtpClosed(IDmtpActorObject client, ClosedEventArgs e)
        {
            if (client is HttpDmtpSessionClient clientSession)
            {
                clientSession.Logger.Info($"HTTP_DMTP:客户端{clientSession.IP}已断开");
                clientSession.StopReverseRPC();
            }
            await e.InvokeNext();
        }

        public async Task OnDmtpCreatedChannel(IDmtpActorObject client, CreateChannelEventArgs e)
        {
            if (client.TrySubscribeChannel(e.ChannelId, out var channel))
            {
                //设定读取超时时间
                //channel.Timeout = TimeSpan.FromSeconds(30);
                using (channel)
                {
                    client.DmtpActor.Logger.Info("通道开始接收");
                    long count = 0;
                    await foreach (var byteBlock in channel)
                    {
                        //这里处理数据
                        count += byteBlock.Length;
                        client.DmtpActor.Logger.Info($"通道已接收：{count}字节");
                    }

                    client.DmtpActor.Logger.Info($"通道接收结束，状态={channel.Status}，短语={channel.LastOperationMes}，共接收{count / (1048576.0):0.00}Mb字节");
                }
            }

            await e.InvokeNext();
        }

        public async Task OnDmtpConnected(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            if (client is HttpDmtpSessionClient clientSession)
            {
                clientSession.Logger.Info($"HTTP_DMTP:客户端{clientSession.IP}已连接");
                //有新的客户端连接后，调用执行RandomNumber函数
                clientSession.StartReverseRPC();


            }
            await e.InvokeNext();
        }


    }

    /// <summary>
    /// 自定义HttpDmtpService
    /// </summary>
    internal class HttpDmtpService : TouchSocket.Dmtp.HttpDmtpService<HttpDmtpSessionClient>
    {
        protected override HttpDmtpSessionClient NewClient()
        {
            return new HttpDmtpSessionClient();
        }
    }

    /// <summary>
    /// 自定义HttpDmtpSessionClient
    /// </summary>
    internal class HttpDmtpSessionClient : TouchSocket.Dmtp.HttpDmtpSessionClient
    {

        private Timer timer;
        internal void StartReverseRPC()
        {
            this.timer = new Timer(this.ClientReverseRPC, null, 1 * 1000, 10 * 1000);
        }

        private readonly Random Random = new Random();
        private async void ClientReverseRPC(object? client)
        {
            if (this.Online)
            {
                var a = this.Random.Next(100000000);
                var b = this.Random.Next(100000000);
                var c = a + b;
                try
                {
                    var d = await this.GetDmtpRpcActor().RandomNumberAsync(a, b);
                    if (c != d)
                    {
                        this.Logger.Info("客户端计算数据不对");
                    }
                }
                catch (Exception)
                {
                    this.StopReverseRPC();
                }

            }
            else
            {
                this.StopReverseRPC();
            }
        }

        internal void StopReverseRPC()
        {
            this.timer.Dispose();
            this.timer = null;
        }
    }
}
