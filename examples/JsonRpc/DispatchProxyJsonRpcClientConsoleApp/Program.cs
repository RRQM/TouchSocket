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
using TouchSocket.JsonRpc;
using TouchSocket.Sockets;

namespace DispatchProxyJsonRpcClientConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        var rpc = MyJsonRpcDispatchProxy.Create<IJsonRpcServer, MyJsonRpcDispatchProxy>();

        while (true)
        {
            var result = rpc.TestJsonRpc(Console.ReadLine());
            Console.WriteLine(result);
        }
    }
}

/// <summary>
/// 新建一个类，继承JsonRpcDispatchProxy，亦或者RpcDispatchProxy基类。
/// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
/// </summary>
internal class MyJsonRpcDispatchProxy : JsonRpcDispatchProxy
{
    private readonly IJsonRpcClient m_client;

    public MyJsonRpcDispatchProxy()
    {
        this.m_client = CreateJsonRpcClientByTcp().GetFalseAwaitResult();
    }

    public override IJsonRpcClient GetClient()
    {
        return this.m_client;
    }

    private static async Task<IJsonRpcClient> CreateJsonRpcClientByTcp()
    {
        var client = new TcpJsonRpcClient();
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7705")
             .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n")));
        await client.ConnectAsync();
        return client;
    }
}

internal interface IJsonRpcServer
{
    [JsonRpc(MethodInvoke = true)]
    string TestJsonRpc(string str);
}