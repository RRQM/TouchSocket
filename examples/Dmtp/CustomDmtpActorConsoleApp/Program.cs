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

using CustomDmtpActorConsoleApp.SimpleDmtpRpc;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace CustomDmtpActorConsoleApp;

/// <summary>
/// 开发自定义DmtpActor。
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await GetTcpDmtpService();
        var client = await GetTcpDmtpClient();

        while (true)
        {
            var methodName = Console.ReadLine();
            var actor = client.GetSimpleDmtpRpcActor();

            try
            {
                actor.Invoke(methodName);
                Console.WriteLine("调用成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var client = await new TouchSocketConfig()
               .SetRemoteIPHost("127.0.0.1:7789")
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "File"
               })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
               })
               .ConfigurePlugins(a =>
               {
                   a.UseSimpleDmtpRpc();

                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromSeconds(3))
                   .SetMaxFailCount(3);
               })
               .BuildClientAsync<TcpDmtpClient>();

        client.Logger.Info("连接成功");
        return client;
    }

    private static async Task<TcpDmtpService> GetTcpDmtpService()
    {
        var service = new TcpDmtpService();

        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();

                   a.AddDmtpRouteService();//添加路由策略
               })
               .ConfigurePlugins(a =>
               {
                   a.UseSimpleDmtpRpc()
                   .RegisterRpc(new MyServer());
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "File"//连接验证口令。
               });

        await service.SetupAsync(config);
        await service.StartAsync();
        service.Logger.Info("服务器成功启动");
        return service;
    }
}

internal class MyServer
{
    public void SayHello()
    {
        Console.WriteLine("Hello");
    }
}