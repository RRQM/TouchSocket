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

using System.ComponentModel;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.DmtpRpc.Generators;
using TouchSocket.Sockets;

namespace GeneratorRpcProxyConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //创建服务器
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(7789)
               .ConfigureContainer(a =>
               {
                   a.AddConsoleLogger();
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<MyRpcServer>();
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
               });

        await service.SetupAsync(config);
        await service.StartAsync();

        service.Logger.Info($"{service.GetType().Name}已启动");

        //创建客户端
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

        //此处的Login方法则是vs源代码自动生成的，可以f12查看。
        Console.WriteLine(await client.GetDmtpRpcActor().LoginAsync("123", "abc"));
        Console.ReadKey();
    }
}

public partial class MyRpcServer : SingletonRpcServer
{
    [DmtpRpc]
    public bool Login(string account, string password)
    {
        if (account == "123" && password == "abc")
        {
            return true;
        }

        return false;
    }
}

/// <summary>
/// GeneratorRpcProxy的标识，表明这个接口应该被生成其他源代码。
/// ConsoleApp2.MyRpcServer参数是整个rpc调用的前缀，即：除方法名的所有，包括服务的类名。
/// </summary>
[GeneratorRpcProxy(Prefix = "GeneratorRpcProxyConsoleApp.MyRpcServer")]
internal interface Test
{
    [Description("这是登录方法")]//该作用是生成注释
    [DmtpRpc]//表明该方法应该被代理，也可以通过参数，直接设置调用键
    bool Login(string account, string password);
}