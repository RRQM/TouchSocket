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
using System.IO;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Sockets;
using TouchSocket.XmlRpc;

namespace XmlRpcServerApp;

internal class Program
{
    private static void Main(string[] args)
    {
        var service = new HttpService();

        service.SetupAsync(new TouchSocketConfig()
            .ConfigureContainer(a =>
            {
                a.AddConsoleLogger();
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<XmlServer>();

#if DEBUG
                    File.WriteAllText("../../../RpcProxy.cs", store.GetProxyCodes("RpcProxy", new Type[] { typeof(XmlRpcAttribute) }));
                    ConsoleLogger.Default.Info("成功生成代理");
#endif
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseXmlRpc()
                .SetXmlRpcUrl("/xmlRpc");
            })
            .SetListenIPHosts(7789));
        service.StartAsync();

        service.Logger.Info("服务器已启动");
        Console.ReadKey();
    }

    private static void a()
    {
    }
}

public partial class XmlServer : RpcServer
{
    [XmlRpc(MethodInvoke = true)]
    public int Sum(int a, int b)
    {
        return a + b;
    }

    [XmlRpc(MethodInvoke = true)]
    public int TestClass(MyClass myClass)
    {
        return myClass.A + myClass.B;
    }
}

public class MyClass
{
    public int A { get; set; }
    public int B { get; set; }
}