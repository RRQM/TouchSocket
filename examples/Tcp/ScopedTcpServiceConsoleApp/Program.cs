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

using Microsoft.Extensions.DependencyInjection;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ScopedTcpServiceConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service1 = await CreateService_AspNetCoreContainer(8848);
        var service2 = await CreateService_Container(8849);

        while (true)
        {
            Console.ReadKey();
        }
    }

    private static async Task<TcpService> CreateService_AspNetCoreContainer(int port)
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
             .SetListenIPHosts(port)//同时监听两个地址
             .UseAspNetCoreContainer(iocServices)
             .ConfigurePlugins(a =>
             {
                 a.Add<MyScopedPlugin>();
             }));
        await service.StartAsync();//启动
        return service;
    }

    private static async Task<TcpService> CreateService_Container(int port)
    {
        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts(port)//同时监听两个地址
             .ConfigureContainer(a =>
             {
                 a.RegisterScoped<MyScopedPlugin>();
                 a.AddLogger(logger =>
                 {
                     logger.AddConsoleLogger();
                     logger.AddFileLogger();
                 });
             })
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
