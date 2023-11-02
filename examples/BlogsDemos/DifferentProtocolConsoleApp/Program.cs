using TouchSocket.Core;
using TouchSocket.Sockets;

namespace DifferentProtocolConsoleApp
{
    /// <summary>
    /// C# Tcp服务器实现多端口、多协议解析，博客<see href="https://blog.csdn.net/qq_40374647/article/details/128641766"/>
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(new IPHost[] { new IPHost("tcp://127.0.0.1:7789"), new IPHost(7790) })//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<DifferentProtocolPlugin>();
                }));
            service.Start();//启动

            service.Logger.Info("服务器成功启动");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 此插件实现，按照不同端口，使用不同适配器。
    /// <list type="bullet">
    /// <item>7789端口:使用"**"结尾的数据</item>
    /// <item>7790端口:使用"##"结尾的数据</item>
    /// </list>
    /// </summary>
    internal class DifferentProtocolPlugin : PluginBase, ITcpConnectingPlugin<ISocketClient>, ITcpReceivedPlugin<ISocketClient>
    {
        public Task OnTcpConnecting(ISocketClient client, ConnectingEventArgs e)
        {
            if (client.ServicePort == 7789)
            {
                client.SetDataHandlingAdapter(new TerminatorPackageAdapter("**"));
            }
            else
            {
                client.SetDataHandlingAdapter(new TerminatorPackageAdapter("##"));
            }
            return e.InvokeNext();
        }

        public Task OnTcpReceived(ISocketClient client, ReceivedDataEventArgs e)
        {
            //如果是自定义适配器，此处解析时，可以判断e.RequestInfo的类型

            client.Logger.Info($"{client.GetInfo()}收到数据，服务器端口：{client.ServicePort},数据：{e.ByteBlock}");
            return e.InvokeNext();
        }
    }
}