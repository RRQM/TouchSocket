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
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.Rpc.DmtpRpc.Generators;

namespace RecommendRpcConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
                   a.AddFileLogger();
                   a.AddRpcStore(store =>
                   {
                       ////此处使用限定名称，因为源代码生成时，也会生成TouchSocket.Rpc.Generators.IUserServer的接口
                       //store.RegisterServer<RpcClassLibrary.ServerInterface.IUserServer, UserServer>();

                       //此处使用的是源生成注册，具体可看文档》Rpc》注册服务
                       store.RegisterAllFromRpcImplementationClassLibrary();
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"
               });//设定连接口令，作用类似账号密码

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
                 VerifyToken = "Dmtp"
             }));
        await client.ConnectAsync();

        //Login即为在RpcClassLibrary中自动生成的项目
        var response =await client.GetDmtpRpcActor().LoginAsync(new RpcClassLibrary.Models.LoginRequest() { Account = "Account", Password = "Account" });
        Console.WriteLine(response.Result);
        Console.ReadKey();
    }
}