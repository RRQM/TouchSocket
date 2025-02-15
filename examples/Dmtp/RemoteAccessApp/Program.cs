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
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Sockets;

namespace RemoteAccessApp;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        try
        {
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        var service = GetTcpDmtpService();
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }

    private static async Task<TcpDmtpService> GetTcpDmtpService()
    {
        var service = await new TouchSocketConfig()//����
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRemoteAccess();//��������Զ�̷��ʲ��
                   a.Add<MyRemoteAccessPlugin>();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"//������֤���
               })
               .BuildServiceAsync<TcpDmtpService>();//�˴�build�൱��new TcpDmtpService��Ȼ��SetupAsync��Ȼ��StartAsync��
        service.Logger.Info("�������ɹ�����");
        return service;
    }

    public class MyRemoteAccessPlugin : PluginBase, IDmtpRemoteAccessingPlugin
    {
        public async Task OnRemoteAccessing(IDmtpActorObject client, RemoteAccessingEventArgs e)
        {
            //Console.WriteLine($"�пͻ�����������Զ�̲���");
            //Console.WriteLine($"���ͣ�{e.AccessType}��ģʽ��{e.AccessMode}");
            //Console.WriteLine($"����·����{e.Path}");
            //Console.WriteLine($"Ŀ��·����{e.TargetPath}");

            //Console.WriteLine("������y/n�����Ƿ����������?");

            //var input = Console.ReadLine();
            //if (input == "y")
            //{
            e.IsPermitOperation = true;
            //    return;
            //}

            //�����ǰ����޷�������ת����һ�����
            await e.InvokeNext();
        }
    }
}