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
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeDmtpConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            ConsoleLogger.Default.Info(ex.Message);
        }
        var service = await GetService();
        var client = await GetClient();

        Console.ReadKey();
    }

    private static async Task<NamedPipeDmtpClient> GetClient()
    {
        var client = new NamedPipeDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();
             })
             .SetPipeName("TouchSocketPipe")//设置管道名称
             .SetDmtpOption(new DmtpOption()
             {
                 VerifyToken = "Dmtp"
             }));
        await client.ConnectAsync();

        client.Logger.Info("连接成功");
        return client;
    }

    private static async Task<NamedPipeDmtpService> GetService()
    {
        var service = new NamedPipeDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetPipeName("TouchSocketPipe")//设置管道名称
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
               });

        await service.SetupAsync(config);

        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");

        return service;
    }
}