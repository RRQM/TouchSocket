using Microsoft.Extensions.DependencyInjection;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ScopedTcpServiceConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();

        while (true)
        {
            Console.ReadKey();
        }
    }

    private static async Task<TcpService> CreateService()
    {
        //创建IOC容器
        var iocServices = new ServiceCollection();
        iocServices.AddScoped<MyScopedPlugin>();//添加一个作用域插件

        //如果需要注入组件内内容，请使用此方法
        iocServices.ConfigureContainer(a =>
        {
            //例如添加一个日志服务
            a.AddLogger(logger =>
            {
                logger.AddConsoleLogger();
                logger.AddFileLogger();
            });
        });

        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .UseAspNetCoreContainer(iocServices)
             .ConfigurePlugins(a =>
             {
                 a.Add<MyScopedPlugin>();
             }));
        await service.StartAsync();//启动
        return service;
    }

    [PluginOption(FromIoc = true)]//表示该插件从IOC容器中获取
    public class MyScopedPlugin : PluginBase, ITcpConnectedPlugin, ITcpClosedPlugin, ITcpReceivedPlugin
    {
        public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
        {
            Console.WriteLine($"断开，插件HashCode={this.GetHashCode()}");
            await e.InvokeNext();
        }

        public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
        {
            Console.WriteLine($"连接，插件HashCode={this.GetHashCode()}");
            await e.InvokeNext();
        }

        public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
        {
            Console.WriteLine($"消息{e.Memory.Span.ToString(Encoding.UTF8)}，插件HashCode={this.GetHashCode()}");
            await e.InvokeNext();
        }
    }
}
