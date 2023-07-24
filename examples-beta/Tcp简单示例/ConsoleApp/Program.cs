using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ServiceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = CreateService();
            var client = CreateClient();
            Console.WriteLine("输入任意内容，回车发送");
            while (true)
            {
                client.Send(Console.ReadLine());
            }
        }

        private static TcpService CreateService()
        {
            TcpService service = new TcpService();
            service.Connecting = (client, e) => { };//有客户端正在连接
            service.Connected = (client, e) => { };//有客户端成功连接
            service.Disconnected = (client, e) => { };//有客户端断开连接
            service.Received = (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");

                client.Send(mes);//将收到的信息直接返回给发送方

                //client.Send("id",mes);//将收到的信息返回给特定ID的客户端

                var ids = service.GetIds();
                foreach (var clientId in ids)//将收到的信息返回给在线的所有客户端。
                {
                    if (clientId != client.Id)//不给自己发
                    {
                        service.Send(clientId, mes);
                    }
                }
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("tcp://127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                    a.RegisterSingleton(service);//将服务器以单例注入。便于插件或其他地方获取。
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyServicePluginClass>();
                    //a.Add();//此处可以添加插件
                }))
                .Start();//启动
            return service;
        }

        private static TcpClient CreateClient()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connected = (client, e) => { };//成功连接到服务器
            tcpClient.Disconnected = (client, e) => { };//从服务器断开连接，当连接不成功时不会触发。
            tcpClient.Received = (client, byteBlock, requestInfo) =>
            {
                //从服务器收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                tcpClient.Logger.Info($"客户端接收到信息：{mes}");
            };

            //载入配置
            tcpClient.Setup(new TouchSocketConfig()
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
                }))
                .Connect();
            tcpClient.Logger.Info("客户端成功连接");
            return tcpClient;
        }
    }

    class MyServicePluginClass : PluginBase, IServerStartedPlugin, IServerStopedPlugin
    {
        Task IServerStartedPlugin<IService>.OnServerStarted(IService sender, ServiceStateEventArgs e)
        {
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

        Task IServerStopedPlugin<IService>.OnServerStoped(IService sender, ServiceStateEventArgs e)
        {
            Console.WriteLine("服务已停止");
            return e.InvokeNext();
        }
    }
}