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

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace CreateDmtpConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var tcpDmtpService = await CreateTcpDmtpService();
    }

    private static async Task<TcpDmtpService> CreateTcpDmtpService()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   //添加插件
                   //a.Add<MyPlugin>();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Dmtp";//设定连接口令，作用类似账号密码
                   options.VerifyTimeout = TimeSpan.FromSeconds(3);//设定账号密码验证超时时间
               });

        await service.SetupAsync(config);
        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }

    static async Task<UdpDmtp> CreateUdpDmtpService()
    {

        var udpDmtp = new UdpDmtp();

        var config = new TouchSocketConfig();
        config.SetBindIPHost(new IPHost(7789))
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             });

        await udpDmtp.SetupAsync(config);

        await udpDmtp.StartAsync();

        udpDmtp.Logger.Info($"{udpDmtp.GetType().Name}已启动");
        return udpDmtp;
    }
    static async Task<HttpDmtpService> CreateHttpDmtpService()
    {
        var service = new HttpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Dmtp";
               });

        await service.SetupAsync(config);

        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }

    static async Task<NamedPipeDmtpService> CreateNamedPipeDmtpService()
    {
        var service = new NamedPipeDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetPipeName("TouchSocketPipe")//设置管道名称
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Dmtp";
               });

        await service.SetupAsync(config);

        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }
    static async Task CreateAspNetCoreWebSocketDmtpService(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.ConfigureContainer(container =>
        {
            container.AddAspNetCoreLogger();
        });

        builder.Services.AddWebSocketDmtpService(config =>
        {
            config.SetDmtpOption(options =>
                {
                    options.VerifyToken = "Dmtp";
                })
                .ConfigurePlugins(a =>
                {
                });
        });

        var app = builder.Build();
        app.UseWebSockets();
        app.UseWebSocketDmtp("/WebSocketDmtp");//WebSocketDmtp必须在UseWebSockets之后使用。
    }

    static async Task CreateAspNetCoreHttpDmtpService(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureContainer(container =>
        {
            container.AddAspNetCoreLogger();
        });

        builder.Services.AddHttpMiddlewareDmtpService(config =>
        {
            config.SetDmtpOption(options=>
            {
                options.VerifyToken = "Dmtp";
            })
            .ConfigurePlugins(a =>
            {
                //添加插件
            });
        });

        var app = builder.Build();
        app.UseHttpDmtp(); //HttpDmtp可以单独直接使用。不需要其他。
    }
}
