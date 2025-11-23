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
using TouchSocket.Sockets;
using TouchSocket.WebApi;

namespace DispatchProxyWebApiConsoleApp;

internal class Program
{
    #region WebApi客户端DispatchProxy代理调用
    /// <summary>
    /// 使用DispatchProxy生成调用代理
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        var api = MyWebApiDispatchProxy.Create<IApiServer, MyWebApiDispatchProxy>();
        while (true)
        {
            Console.WriteLine("请输入两个数，中间用空格隔开，回车确认");
            var str = Console.ReadLine();
            var strs = str.Split(' ');
            var a = int.Parse(strs[0]);
            var b = int.Parse(strs[1]);

            var sum = api.Sum(a, b);
            Console.WriteLine(sum);
        }
    }
    #endregion
}

#region WebApi客户端DispatchProxy代理类实现
/// <summary>
/// 新建一个类，继承WebApiDispatchProxy，亦或者RpcDispatchProxy基类。
/// 然后实现抽象方法，主要是能获取到调用的IRpcClient派生接口。
/// </summary>
internal class MyWebApiDispatchProxy : WebApiDispatchProxy
{
    private readonly WebApiClient m_client;

    public MyWebApiDispatchProxy()
    {
        this.m_client = CreateWebApiClient();
    }

    private static WebApiClient CreateWebApiClient()
    {
        var client = new WebApiClient();
        client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7789")
            .ConfigurePlugins(a =>
            {
                a.UseReconnection<WebApiClient>();
            }));
        client.ConnectAsync();
        Console.WriteLine("连接成功");
        return client;
    }

    public override IWebApiClientBase GetClient()
    {
        return this.m_client;
    }
}
#endregion

#region WebApi客户端DispatchProxy代理接口定义
internal interface IApiServer
{
    [Router("ApiServer/[action]ab")]
    [Router("ApiServer/[action]")]
    [WebApi(Method = HttpMethodType.Get)]
    int Sum(int a, int b);
}
#endregion