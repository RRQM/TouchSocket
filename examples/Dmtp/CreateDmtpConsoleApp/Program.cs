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
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace CreateDmtpConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var tcpDmtpService = await CreateTcpDmtpService();
    }

    private static async Task<TcpDmtpService> CreateTcpDmtpService()
    {
        #region 创建TcpDmtpService
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
        #region Dmtp服务器基础配置
               .SetDmtpOption(options =>
               {
                   options.VerifyToken = "Dmtp";//设定连接口令，作用类似账号密码
                   options.VerifyTimeout = TimeSpan.FromSeconds(3);//设定账号密码验证超时时间
               })
        #endregion
               ;

        await service.SetupAsync(config);
        await service.StartAsync();
        #endregion

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }

    private static async Task<UdpDmtp> CreateUdpDmtpService()
    {
        #region 创建UdpDmtp
        var udpDmtp = new UdpDmtp();

        var config = new TouchSocketConfig();
        config.SetBindIPHost(new IPHost(7789))
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             });

        await udpDmtp.SetupAsync(config);

        await udpDmtp.StartAsync();
        #endregion

        udpDmtp.Logger.Info($"{udpDmtp.GetType().Name}已启动");
        return udpDmtp;
    }
    private static async Task<HttpDmtpService> CreateHttpDmtpService()
    {
        #region 创建HttpDmtpService
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
        #endregion

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }

    private static async Task<NamedPipeDmtpService> CreateNamedPipeDmtpService()
    {
        #region 创建NamedPipeDmtpService
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
        #endregion

        service.Logger.Info($"{service.GetType().Name}已启动");
        return service;
    }
    private static async Task CreateAspNetCoreWebSocketDmtpService(string[] args)
    {
        #region 创建WebSocket协议的Dmtp服务器
        var builder = WebApplication.CreateBuilder(args);

        #region AspNetCore统一配置容器
        builder.Services.ConfigureContainer(container =>
        {
            container.AddAspNetCoreLogger();
        });
        #endregion


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
        #endregion

        await app.RunAsync();
    }

    private static async Task CreateAspNetCoreHttpDmtpService(string[] args)
    {
        #region 创建基于AspNetCore的Http协议的Dmtp服务器
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureContainer(container =>
        {
            container.AddAspNetCoreLogger();
        });

        builder.Services.AddHttpMiddlewareDmtpService(config =>
        {
            config.SetDmtpOption(options =>
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
        #endregion

    }

    #region 客户端
    private static async Task CreateTcpDmtpClient()
    {
        #region 创建TcpDmtpClient
        var client = new TcpDmtpClient();

        //配置项目
        var config = new TouchSocketConfig();
        config.SetRemoteIPHost("tcp://127.0.0.1:7789");
        config.SetDmtpOption(options =>
        {
            options.VerifyToken = "Dmtp";
            options.Metadata = new Metadata().Add("a", "a");
        });

        //配置容器
        config.ConfigureContainer(a =>
        {
            //注入日志组件
            a.AddConsoleLogger();
        });

        //配置插件
        config.ConfigurePlugins(a =>
        {
            //添加插件
            //a.Add<MyPlugin>();
            //a.UseDmtpRpc();
        });

        //应用配置
        await client.SetupAsync(config);

        //连接
        await client.ConnectAsync();
        #endregion

    }
    private static async Task CreateUdpDmtpClient()
    {
        #region 创建UdpDmtpClient
        var client = new UdpDmtp();

        //配置项目
        var config = new TouchSocketConfig();
        config.SetRemoteIPHost("udp://127.0.0.1:7797");//远程地址
        config.UseUdpReceive();//使用Udp接收
        config.SetDmtpOption(options =>
        {
            options.VerifyToken = "Dmtp";
            options.Metadata = new Metadata().Add("a", "a");
        });

        //配置容器
        config.ConfigureContainer(a =>
        {
            //注入日志组件
            a.AddConsoleLogger();
        });

        //配置插件
        config.ConfigurePlugins(a =>
        {
            //添加插件
            //a.Add<MyPlugin>();
            //a.UseDmtpRpc();
        });

        //应用配置
        await client.SetupAsync(config);

        //启动
        await client.StartAsync();
        #endregion
    }
    private static async Task CreateHttpDmtpClient()
    {
        #region 创建HttpDmtpClient
        var client = new HttpDmtpClient();

        //配置项目
        var config = new TouchSocketConfig();
        config.SetRemoteIPHost("http://127.0.0.1:7789");
        config.SetDmtpOption(options =>
        {
            options.VerifyToken = "Dmtp";
            options.Metadata = new Metadata().Add("a", "a");
        });

        //配置容器
        config.ConfigureContainer(a =>
        {
            //注入日志组件
            a.AddConsoleLogger();
        });

        //配置插件
        config.ConfigurePlugins(a =>
        {
            //添加插件
            //a.Add<MyPlugin>();
            //a.UseDmtpRpc();
        });

        //应用配置
        await client.SetupAsync(config);

        //连接
        await client.ConnectAsync();
        #endregion
    }

    #endregion
}
