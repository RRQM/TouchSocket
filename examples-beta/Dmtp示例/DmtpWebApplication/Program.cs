using TouchSocket.Core;
using TouchSocket.Dmtp;
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
                        //��Ӳ��
                    });
            });

            //��ҵ�湦��
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
                            //��Ӳ��
                        });
            });

            var app = builder.Build();
            app.UseWebSockets();
            app.UseWebSocketDmtp("/WebSocketDmtp");//WebSocketDmtp������UseWebSockets֮��ʹ�á�

            app.UseHttpDmtp(); //HttpDmtp���Ե���ֱ��ʹ�á�

            _ = app.RunAsync("http://localhost:5174");

            //WebSocketDmtpClient����
            var websocketDmtpClient = new WebSocketDmtpClient();
            websocketDmtpClient.Setup(new TouchSocketConfig()
                .SetVerifyToken("Dmtp")
                .SetRemoteIPHost("ws://localhost:5174/WebSocketDmtp"));
            await websocketDmtpClient.ConnectAsync();
            Console.WriteLine("WebSocketDmtpClient���ӳɹ�");

            var httpDmtpClient = new HttpDmtpClient();
            httpDmtpClient.Setup(new TouchSocketConfig()
                .SetVerifyToken("Dmtp")
                .SetRemoteIPHost("http://127.0.0.1:5174"));
            httpDmtpClient.Connect();
            Console.WriteLine("HttpDmtpClient���ӳɹ�");

            Console.ReadKey();
        }
    }
}