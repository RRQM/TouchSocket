using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ServiceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var consoleAction = new ConsoleAction();
            consoleAction.Add("1", "以Received委托接收", RunClientForReceived);
            consoleAction.Add("2", "以ReadAsync异步阻塞接收", () => { RunClientForReadAsync().GetFalseAwaitResult(); });

            var service = CreateService();

            consoleAction.ShowAll();
            consoleAction.RunCommandLine();
        }


        private static TcpService CreateService()
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    a.UseCheckClear()
                    .SetCheckClearType(CheckClearType.All)
                    .SetTick(TimeSpan.FromSeconds(60))
                    .SetOnClose((c, t) =>
                    {
                        c.TryShutdown();
                        c.SafeClose("超时无数据");
                    });

                    a.Add<ClosePlugin>();
                    a.Add<TcpServiceReceivedPlugin>();
                    a.Add<MyServicePluginClass>();
                }));
            service.Start();//启动
            return service;
        }


        /// <summary>
        /// 以Received异步委托接收数据
        /// </summary>
        private static void RunClientForReceived()
        {
            var client = new TcpClient();
            client.Connected = (client, e) => { return EasyTask.CompletedTask; };//成功连接到服务器
            client.Disconnected = (client, e) => { return EasyTask.CompletedTask; };//从服务器断开连接，当连接不成功时不会触发。
            client.Received = (client, e) =>
            {
                //从服务器收到信息
                var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
                client.Logger.Info($"客户端接收到信息：{mes}");
                return EasyTask.CompletedTask;
            };

            client.Setup(GetConfig());//载入配置
            client.Connect();//连接
            client.Logger.Info("客户端成功连接");

            Console.WriteLine("输入任意内容，回车发送");
            while (true)
            {
                client.Send(Console.ReadLine());
            }
        }

        private static TouchSocketConfig GetConfig()
        {
            return new TouchSocketConfig()
                    .SetRemoteIPHost(new IPHost("127.0.0.1:7789"))
                    .ConfigurePlugins(a =>
                    {
                        a.UseReconnection()
                        .SetTick(TimeSpan.FromSeconds(1))
                        .UsePolling();
                    })
                    .ConfigureContainer(a =>
                    {
                        a.AddConsoleLogger();//添加一个日志注入
                    });
        }

        private static async Task RunClientForReadAsync()
        {
            var client = new TcpClient();
            client.Setup(GetConfig());//载入配置
            client.Connect("127.0.0.1:7789");//连接
            client.Logger.Info("客户端成功连接");

            Console.WriteLine("输入任意内容，回车发送");
            //receiver可以复用，不需要每次接收都新建
            using (var receiver = client.CreateReceiver())
            {
                while (true)
                {
                    client.Send(Console.ReadLine());

                    //receiverResult必须释放
                    using (var receiverResult = await receiver.ReadAsync(CancellationToken.None))
                    {
                        if (receiverResult.IsClosed)
                        {
                            //断开连接了
                        }

                        //从服务器收到信息。
                        var mes = Encoding.UTF8.GetString(receiverResult.ByteBlock.Buffer, 0, receiverResult.ByteBlock.Len);
                        client.Logger.Info($"客户端接收到信息：{mes}");

                        //如果是适配器信息，则可以直接获取receiverResult.RequestInfo;

                    }
                }
            }
        }
    }

    class MyTcpClient : TcpClientBase
    {
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            //此处逻辑单线程处理。

            //此处处理数据，功能相当于Received委托。
            string mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
            Console.WriteLine($"已接收到信息：{mes}");
            await base.ReceivedData(e);
        }
    }



    internal class MyServicePluginClass : PluginBase, IServerStartedPlugin, IServerStopedPlugin
    {
        public Task OnServerStarted(IService sender, ServiceStateEventArgs e)
        {
            if (sender is ITcpService service)
            {
                foreach (var item in service.Monitors)
                {
                    ConsoleLogger.Default.Info($"iphost={item.Option.IpHost}");
                }
            }
            if (e.ServerState == ServerState.Running)
            {
                ConsoleLogger.Default.Info($"服务器成功启动");
            }
            else
            {
                ConsoleLogger.Default.Info($"服务器启动失败，状态：{e.ServerState}，异常：{e.Exception}");
            }
            return e.InvokeNext();
        }

        public Task OnServerStoped(IService sender, ServiceStateEventArgs e)
        {
            Console.WriteLine("服务已停止");
            return e.InvokeNext();
        }
    }

    class TcpServiceReceiveAsyncPlugin : PluginBase, ITcpConnectedPlugin<ISocketClient>
    {
        public async Task OnTcpConnected(ISocketClient client, ConnectedEventArgs e)
        {
            //receiver可以复用，不需要每次接收都新建
            using (var receiver = client.CreateReceiver())
            {
                while (true)
                {
                    //receiverResult必须释放
                    using (var receiverResult = await receiver.ReadAsync(CancellationToken.None))
                    {
                        if (receiverResult.IsClosed)
                        {
                            //断开连接了
                        }

                        //从服务器收到信息。
                        var mes = Encoding.UTF8.GetString(receiverResult.ByteBlock.Buffer, 0, receiverResult.ByteBlock.Len);
                        client.Logger.Info($"客户端接收到信息：{mes}");

                        //如果是适配器信息，则可以直接获取receiverResult.RequestInfo;

                    }
                }
            }
        }
    }

    class TcpServiceReceivedPlugin : PluginBase, ITcpReceivedPlugin<ISocketClient>
    {
        public async Task OnTcpReceived(ISocketClient client, ReceivedDataEventArgs e)
        {
            //从客户端收到信息
            var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
            if (mes == "close")
            {
                throw new CloseException(mes);
            }
            client.Logger.Info($"已从{client.GetIPPort()}接收到信息：{mes}");

            client.Send(mes);//将收到的信息直接返回给发送方

            //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

            //注意，此处是使用的当前客户端的接收线程做发送，实际使用中不可以这样做。不然一个客户端阻塞，将导致本客户端无法接收数据。
            //var ids = client.Service.GetIds();
            //foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
            //{
            //    if (clientId != client.Id)//不给自己发
            //    {
            //        await client.Service.SendAsync(clientId, mes);
            //    }
            //}

            await e.InvokeNext();
        }
    }

    /// <summary>
    /// 应一个网友要求，该插件主要实现，在接收数据时如果触发<see cref="CloseException"/>异常，则断开连接。
    /// </summary>
    class ClosePlugin : PluginBase, ITcpReceivedPlugin<ISocketClient>
    {
        private readonly ILog m_logger;

        public ClosePlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnTcpReceived(ISocketClient client, ReceivedDataEventArgs e)
        {
            try
            {
                await e.InvokeNext();
            }
            catch (CloseException ex)
            {
                m_logger.Info("拦截到CloseException");
                client.Close(ex.Message);
            }
            catch (Exception exx)
            {

            }
            finally
            {

            }
        }
    }

    class CloseException : Exception
    {
        public CloseException(string msg) : base(msg) { }
    }
}