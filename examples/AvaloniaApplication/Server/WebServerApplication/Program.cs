using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using WebServerApplication.Plugins;

namespace WebServerApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureContainer(a =>
            {
                a.AddAspNetCoreLogger();

                a.AddRpcStore(store =>
                {
                    store.RegisterAllFromWebServerApplication();
                });

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
                        a.UseDmtpRpc();
                        //添加插件
                        a.Add<MyDmtpPlugin>();
                    });
            });

            var app = builder.Build();

            app.UseWebSockets();
            app.UseWebSocketDmtp("/WebSocketDmtp");//WebSocketDmtp必须在UseWebSockets之后使用。

            app.Run();
        }
    }
}
