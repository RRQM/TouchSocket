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

using TouchSocket.Core;
using TouchSocket.Dmtp;

namespace DmtpWebApplication;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.ConfigureContainer(container =>
        {
            container.AddConsoleLogger();
            container.AddDmtpRouteService();
        });


        builder.Services.AddWebSocketDmtpService(config =>
        {
            config
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .ConfigurePlugins(a =>
                {

                    a.Add<MyClassPlugin>();
                });
        });


        builder.Services.AddHttpMiddlewareDmtpService(config =>
        {
            config.SetDmtpOption(new DmtpOption()
            {
                VerifyToken = "Dmtp"
            })
            .ConfigurePlugins(a =>
            {
                //���Ӳ��
                a.Add<MyClassPlugin>();
            });
        });

        var app = builder.Build();


        app.UseWebSockets();
        app.UseWebSocketDmtp("/WebSocketDmtp");


        app.UseHttpDmtp();

        app.Run();
    }
}

internal class MyClassPlugin : PluginBase, IDmtpConnectedPlugin
{
    private readonly ILogger<MyClassPlugin> m_logger;

    public MyClassPlugin(ILogger<MyClassPlugin> logger)
    {
        this.m_logger = logger;
    }

    public async Task OnDmtpConnected(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        this.m_logger.LogInformation("DmtpConnected");
        await e.InvokeNext();
    }
}