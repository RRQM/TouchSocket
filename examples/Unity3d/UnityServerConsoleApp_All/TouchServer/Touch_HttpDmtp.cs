using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using UnityRpcProxy;

namespace UnityServerConsoleApp_All.TouchServer
{
    /// <summary>
    /// HTTP_Dmtp 网络服务
    /// </summary>
    public class Touch_HttpDmtp : BaseTouchServer
    {
        HttpDmtpService dmtpService = new HttpDmtpService();
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
                 .SetDmtpOption(new DmtpOption()
                 {
                     VerifyToken = "Dmtp"//设置验证token
                 });

            await dmtpService.SetupAsync(config);
            await dmtpService.StartAsync();


            dmtpService.Logger.Info($"Dmtp已启动，监听端口：{port}");
        }

        /// <summary>
        /// 状态日志打印插件
        /// </summary>
       internal class Touch_Dmtp_Log_Plguin : PluginBase, IDmtpHandshakedPlugin, IDmtpClosedPlugin, IDmtpCreatedChannelPlugin
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
                        //此判断主要是探测是否有Hold操作
                        while (channel.CanMoveNext)
                        {
                            long count = 0;
                            foreach (var byteBlock in channel)
                            {
                                //这里处理数据
                                count += byteBlock.Length;
                                client.DmtpActor.Logger.Info($"通道已接收：{count}字节");
                            }

                            client.DmtpActor.Logger.Info($"通道接收结束，状态={channel.Status}，短语={channel.LastOperationMes}，共接收{count / (1048576.0):0.00}Mb字节");
                        }
                    }
                }

                await e.InvokeNext();
            }

            public async Task OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
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
        internal class HttpDmtpSessionClient : TouchSocket.Dmtp.HttpDmtpSessionClient {

            Timer timer;
            internal void StartReverseRPC() {
                timer = new Timer(ClientReverseRPC, null, 1 * 1000, 10 * 1000);
            }

            Random Random = new Random();
            async void ClientReverseRPC(object? client)
            {
                if (Online)
                {
                    var a = Random.Next(100000000);
                    var b = Random.Next(100000000);
                    var c = a + b;
                    try
                    {
                        var d = await this.GetDmtpRpcActor().RandomNumberAsync(a, b);
                        if (c != d)
                        {
                            Logger.Info("客户端计算数据不对");
                        }
                    }
                    catch (Exception e)
                    {
                        StopReverseRPC();
                    }

                }
                else
                {
                    StopReverseRPC();
                }
            }

            internal void StopReverseRPC() {
                timer.Dispose();
                timer = null;
            }
        }
    }

}
