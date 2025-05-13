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

using System;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;

namespace RealityProxyDmtpRpcConsoleApp;

/// <summary>
/// 调用前先启动DmtpRpcServerConsoleApp项目
/// </summary>
internal class Program
{
    private static void Main(string[] args)
    {
        var myDmtpRpcRealityProxy = new MyDmtpRpcRealityProxy<IMyRpcServer>();

        var myRpcServer = myDmtpRpcRealityProxy.GetTransparentProxy();

        var result = myRpcServer.Add(10, 20);
        Console.WriteLine(result);
        Console.ReadKey();
    }
}

/// <summary>
/// 新建一个类，按照需要，继承DmtpRpcRealityProxy，亦或者RpcRealityProxy基类。
/// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
/// </summary>
internal class MyDmtpRpcRealityProxy<T> : DmtpRpcRealityProxy<T>
{
    private readonly TcpDmtpClient m_client;

    public MyDmtpRpcRealityProxy()
    {
        this.m_client = GetTcpDmtpClient();
    }

    private static TcpDmtpClient GetTcpDmtpClient()
    {
        var client = new TcpDmtpClient();
        client.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            })
            .SetRemoteIPHost("127.0.0.1:7789")
        .SetDmtpOption(new DmtpOption()
        {
            VerifyToken = "Dmtp"
        }));
        client.ConnectAsync().GetFalseAwaitResult();
        client.Logger.Info($"连接成功，Id={client.Id}");
        return client;
    }

    public override IDmtpRpcActor GetClient()
    {
        return this.m_client.GetDmtpRpcActor();
    }
}

internal interface IMyRpcServer
{
    /// <summary>
    /// 将两个数相加
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [DmtpRpc(MethodInvoke = true)]//使用函数名直接调用
    int Add(int a, int b);
}