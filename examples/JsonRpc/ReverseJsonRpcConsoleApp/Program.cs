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
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ReverseJsonRpcConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await GetService();
        var client = await GetClient();

        Console.ReadKey();
    }

    private static async Task<WebSocketJsonRpcClient> GetClient()
    {
        var jsonRpcClient = new WebSocketJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .ConfigureContainer(a =>
             {
                 a.AddRpcStore(store =>
                 {
                     store.RegisterServer<ReverseJsonRpcServer>();
                 });
             })
             .SetRemoteIPHost("ws://127.0.0.1:7707/ws"));//此url就是能连接到websocket的路径。
        await jsonRpcClient.ConnectAsync();

        return jsonRpcClient;
    }

    private static async Task<HttpService> GetService()
    {
        var service = new HttpService();

        await service.SetupAsync(new TouchSocketConfig()
              .SetListenIPHosts(7707)
              .ConfigureContainer(a =>
              {
                  a.AddRpcStore(store =>
                  {
                  });
              })
              .ConfigurePlugins(a =>
              {
                  a.UseWebSocket()
                  .SetWSUrl("/ws");

                  a.UseWebSocketJsonRpc()
                  .SetAllowJsonRpc((socketClient, context) =>
                  {
                      //此处的作用是，通过连接的一些信息判断该ws是否执行JsonRpc。
                      //当然除了此处可以设置外，也可以通过socketClient.SetJsonRpc(true)直接设置。
                      return true;
                  });

                  a.Add<MyPluginClass>();
              }));
        await service.StartAsync();
        return service;
    }
}

internal class MyPluginClass : PluginBase, IWebSocketHandshakedPlugin
{
    public async Task OnWebSocketHandshaked(IWebSocket client, HttpContextEventArgs e)
    {
        try
        {
            //获取JsonRpcActionClient，用于执行反向Rpc
            var jsonRpcClient = ((IHttpSessionClient)client.Client).GetJsonRpcActionClient();

            var result = await jsonRpcClient.InvokeTAsync<int>("Add", InvokeOption.WaitInvoke, 10, 20);
            Console.WriteLine($"反向调用成功，结果={result}");

            //Stopwatch stopwatch = Stopwatch.StartNew();
            //for (int i = 0; i < 10000; i++)
            //{
            //    //调用Rpc，此处可以使用代理
            //    var result = await jsonRpcClient.InvokeTAsync<int>("Add", InvokeOption.WaitInvoke, 10, 20);
            //}
            //stopwatch.Stop();
            //Console.WriteLine(stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        await e.InvokeNext();
    }
}

public partial class ReverseJsonRpcServer : RpcServer
{
    [JsonRpc(MethodInvoke = true)]
    public int Add(int a, int b)
    {
        return a + b;
    }
}