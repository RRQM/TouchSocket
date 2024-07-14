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

            //����WebSocketЭ���Dmtp
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
                        //���Ӳ��
                        a.Add<MyClassPlugin>();
                    });
            });

            //��ҵ�湦��
            //���ӻ���Http����Э���Dmtp��
            //�ͻ���ʹ��HttpDmtpClient
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

            //WebSocketDmtp������UseWebSockets֮��ʹ�á�
            app.UseWebSockets();
            app.UseWebSocketDmtp("/WebSocketDmtp");

            //HttpDmtp���Ե���ֱ��ʹ�á�
            app.UseHttpDmtp();

            app.Run();
        }
    }

    internal class MyClassPlugin : PluginBase, IDmtpHandshakedPlugin
    {
        private readonly ILogger<MyClassPlugin> m_logger;

        public MyClassPlugin(ILogger<MyClassPlugin> logger)
        {
            this.m_logger = logger;
        }

        public async Task OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            this.m_logger.LogInformation("DmtpHandshaked");
            await e.InvokeNext();
        }
    }
}