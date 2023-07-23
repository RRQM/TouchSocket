using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;

namespace DmtpWebApplication
{
    public class Program
    {
        public static async Task Main(string[] args)
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

            builder.Services.AddWebSocketDmtpService(() =>
            {
                return new TouchSocketConfig()
                    .SetVerifyToken("Dmtp")
                    .UseAspNetCoreContainer(builder.Services)
                    .ConfigureContainer(a =>
                    {
                        a.AddDmtpRouteService();
                    })
                    .ConfigurePlugins(a =>
                    {
                        //添加插件
                    });
            });

            //企业版功能
            builder.Services.AddHttpMiddlewareDmtpService(() =>
            {
                return new TouchSocketConfig()
                        .SetVerifyToken("Dmtp")
                        .UseAspNetCoreContainer(builder.Services)
                        .ConfigureContainer(a =>
                        {
                            a.AddDmtpRouteService();
                        })
                        .ConfigurePlugins(a =>
                        {
                            //添加插件
                        });
            });

            var app = builder.Build();
            app.UseWebSockets();
            app.UseWebSocketDmtp("/WebSocketDmtp");//WebSocketDmtp必须在UseWebSockets之后使用。

            app.UseHttpDmtp(); //HttpDmtp可以单独直接使用。

            _ = app.RunAsync("http://localhost:5174");

            //WebSocketDmtpClient连接
            var websocketDmtpClient = new WebSocketDmtpClient();
            websocketDmtpClient.Setup(new TouchSocketConfig()
                .SetVerifyToken("Dmtp")
                .SetRemoteIPHost("ws://localhost:5174/WebSocketDmtp"));
            await websocketDmtpClient.ConnectAsync();
            Console.WriteLine("WebSocketDmtpClient连接成功");

            var httpDmtpClient = new HttpDmtpClient();
            httpDmtpClient.Setup(new TouchSocketConfig()
                .SetVerifyToken("Dmtp")
                .SetRemoteIPHost("http://127.0.0.1:5174"));
            httpDmtpClient.Connect();
            Console.WriteLine("HttpDmtpClient连接成功");

            Console.ReadKey();
        }
    }
}