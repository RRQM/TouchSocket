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
                //��ҵ�����Ҫ
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var builder = WebApplication.CreateBuilder(args);

            //������ͨ��Host�У���������Ṳ��һ��������
            //���Խ���ʹ��ConfigureContainerͳһ���á�
            builder.Services.ConfigureContainer(container =>
            {
                container.AddConsoleLogger();
                container.AddDmtpRouteService();
            });

            //���WebSocketЭ���Dmtp
            //�ͻ���ʹ��WebSocketDmtpClient
            builder.Services.AddWebSocketDmtpService(config =>
            {
                config
                    .SetDmtpOption(new DmtpOption()
                    {
                        VerifyToken = "Dmtp"
                    })
                    .ConfigurePlugins(a =>
                    {
                        //��Ӳ��
                        a.Add<MyClassPlugin>();
                    });
            });

            //��ҵ�湦��
            //��ӻ���Http����Э���Dmtp��
            //�ͻ���ʹ��HttpDmtpClient
            builder.Services.AddHttpMiddlewareDmtpService(config =>
            {
                config.SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                })
                .ConfigurePlugins(a =>
                {
                    //��Ӳ��
                    a.Add<MyClassPlugin>();
                });
            });

            var app = builder.Build();

            //WebSocketDmtp������UseWebSockets֮��ʹ�á�
            app.UseWebSockets();
            app.UseWebSocketDmtp("/WebSocketDmtp");

            //HttpDmtp���Ե���ֱ��ʹ�á�
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