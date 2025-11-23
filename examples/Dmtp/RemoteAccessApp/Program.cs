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
        var service = await new TouchSocketConfig()//配置
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               #region 远程文件系统配置插件
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRemoteAccess();//使用Dmtp远程访问插件
                   a.Add<MyRemoteAccessPlugin>();
               })
               #endregion
               .SetDmtpOption(options=>
               {
                   options.VerifyToken = "Dmtp";//连接验证口令
               })
               .BuildServiceAsync<TcpDmtpService>();//此处build相当于new TcpDmtpService，然后SetupAsync，然后StartAsync。
        service.Logger.Info("服务器成功启动");
        return service;
    }

    #region 远程文件系统响应端插件
    public class MyRemoteAccessPlugin : PluginBase, IDmtpRemoteAccessingPlugin
    {
        public async Task OnRemoteAccessing(IDmtpActorObject client, RemoteAccessingEventArgs e)
        {
            //Console.WriteLine($"有客户端正在请求远程操作");
            //Console.WriteLine($"类型：{e.AccessType}，模式：{e.AccessMode}");
            //Console.WriteLine($"源路径：{e.Path}");
            //Console.WriteLine($"目标路径：{e.TargetPath}");

            //Console.WriteLine("请输入y/n，表示是否允许该操作?");

            //var input = Console.ReadLine();
            //if (input == "y")
            //{
            e.IsPermitOperation = true;
            //    return;
            //}

            //如果当前插件无法处理，请转至下一个插件
            await e.InvokeNext();
        }
    }
    #endregion
}