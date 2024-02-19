using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //企业版才需要
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var builder = WebApplication.CreateBuilder(args);

            //在整个通用Host中，所有组件会共用一个容器。
            //所以建议使用ConfigureContainer统一设置。
            builder.Services.ConfigureContainer(container =>
            {
                container.AddConsoleLogger();
                container.AddDmtpRouteService();
            });

            //添加WebSocket协议的Dmtp
            //客户端使用WebSocketDmtpClient
            builder.Services.AddWebSocketDmtpService(config =>
            {
                config
                    .SetDmtpOption(new DmtpOption()
                    {
                        VerifyToken = "Dmtp"
                    })
                    .ConfigurePlugins(a =>
                    {
                        //添加插件
                        a.Add<MyClassPlugin>();
                    });
            });

            //企业版功能
            //添加基于Http升级协议的Dmtp。
            //客户端使用HttpDmtpClient
            builder.Services.AddHttpMiddlewareDmtpService(config =>
            {
                config.SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .ConfigurePlugins(a =>
                {
                    //添加插件
                    a.Add<MyClassPlugin>();
                });
            });

            var app = builder.Build();

            //WebSocketDmtp必须在UseWebSockets之后使用。
            app.UseWebSockets();
            app.UseWebSocketDmtp("/WebSocketDmtp");

            //HttpDmtp可以单独直接使用。
            app.UseHttpDmtp();

            app.Run();
        }
    }

    class MyClassPlugin : PluginBase, IDmtpHandshakedPlugin
    {
        private readonly ILogger<MyClassPlugin> m_logger;

        public MyClassPlugin(ILogger<MyClassPlugin> logger)
        {
            this.m_logger = logger;
        }
        public async Task OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            m_logger.LogInformation("DmtpHandshaked");
            await e.InvokeNext();
        }
    }
}