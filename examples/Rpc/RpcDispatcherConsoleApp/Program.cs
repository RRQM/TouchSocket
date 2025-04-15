// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.Rpc.Generators;

namespace RpcDispatcherConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<IMyRpcServer, MyRpcServer>();//注册服务
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc()
                   .UseConcurrencyDispatcher();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Rpc"//连接验证口令。
               });

        await service.SetupAsync(config);

        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");

        var client = new TcpDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            })
            .SetDmtpOption(new DmtpOption()
            {
                VerifyToken = "Rpc"//连接验证口令。
            }));
        await client.ConnectAsync();

        //Task.Run(() => 
        //{

        //});

        DmtpInvokeOption dmtpInvokeOption = new DmtpInvokeOption()
        {
            FeedbackType = FeedbackType.OnlySend
        };
        for (var i = 0; i < 10; i++)
        {
            var actor = client.GetDmtpRpcActor();
            await actor.OutputAsync(i, dmtpInvokeOption);
        }

        while (true)
        {
            Console.ReadLine();
        }
    }
}


[GeneratorRpcProxy]
interface IMyRpcServer : IRpcServer
{
    [Reenterable(false)]
    [Description("登录")]//服务描述，在生成代理时，会变成注释。
    [DmtpRpc(MethodInvoke = true)]//服务注册的函数键，此处为显式指定。默认不传参的时候，为该函数类全名+方法名的全小写。
    Task Output(int value);
}

public class MyRpcServer : RpcServer, IMyRpcServer
{
    public async Task Output(int value)
    {
        await Task.Delay(new Random().Next(1, 1000));
        Console.WriteLine(value);
    }
}