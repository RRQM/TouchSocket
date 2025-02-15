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

using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace HostingWorkerService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices(services =>
        {
            //����TcpService��
            services.AddTcpService(config =>
            {
                config.SetListenIPHosts(7789);
            });

            //����TcpClient
            //ע�⣬Client��ķ�������������StartAsyncʱ������ִ��ConnectAsync��������Ҫ�������ӣ�������������ֵ������
            services.AddSingletonTcpClient(config =>
            {
                config.SetRemoteIPHost("127.0.0.1:7789");
            });

            services.AddServiceHostedService<IMyTcpService, MyTcpService>(config =>
            {
                config.SetListenIPHosts(7790);
            });

            //���������ܵ�����
            services.AddServiceHostedService<INamedPipeService, NamedPipeService>(config =>
            {
                config.SetPipeName("pipe7789");
            });
        });

        var host = builder.Build();
        host.Run();
    }
}

internal class MyTcpService : TcpService, IMyTcpService
{
}

internal interface IMyTcpService : ITcpService
{
}