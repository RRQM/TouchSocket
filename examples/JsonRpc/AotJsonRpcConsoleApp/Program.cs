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

using System.Text.Json.Serialization;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc.Generators;
using TouchSocket.Sockets;

namespace JsonRpcAotConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        await CreateTcpJsonRpcService();
        await CreateHttpJsonRpcService();
        await CreateWebSocketJsonRpcService();

        var consoleAction = new ConsoleAction();
        consoleAction.OnException += ConsoleAction_OnException;
        consoleAction.Add("1", "Tcp调用", JsonRpcClientInvokeByTcp);
        consoleAction.Add("2", "Http调用", JsonRpcClientInvokeByHttp);
        consoleAction.Add("3", "WebSocket调用", JsonRpcClientInvokeByWebSocket);

        consoleAction.ShowAll();

        await consoleAction.RunCommandLineAsync();

    }

    private static void ConsoleAction_OnException(Exception obj)
    {
        ConsoleLogger.Default.Exception(obj);
    }

    private static async Task JsonRpcClientInvokeByHttp()
    {
        using var jsonRpcClient = new HttpJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("http://127.0.0.1:7706/jsonrpc"));
        await jsonRpcClient.ConnectAsync();
        jsonRpcClient.UseSystemTextJson(option =>
         {
             option.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
         });

        Console.WriteLine("连接成功");
        var result = await jsonRpcClient.TestJsonRpcAsync(new MyClass() { P1 = 10, P2 = "ABC" });
        Console.WriteLine($"Http返回结果:{result}");

        result = await jsonRpcClient.TestGetContextAsync("RRQM");
        Console.WriteLine($"Http返回结果:{result}");
    }

    private static async Task JsonRpcClientInvokeByTcp()
    {
        using var jsonRpcClient = new TcpJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7705")
             .SetTcpDataHandlingAdapter(() => new JsonPackageAdapter(System.Text.Encoding.UTF8)));
        await jsonRpcClient.ConnectAsync();
        jsonRpcClient.UseSystemTextJson(option =>
        {
            option.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        Console.WriteLine("连接成功");
        var result = await jsonRpcClient.TestJsonRpcAsync(new MyClass() { P1 = 10, P2 = "ABC" });
        Console.WriteLine($"Tcp返回结果:{result}");

        result = await jsonRpcClient.TestJsonRpcAsync(new MyClass() { P1 = 10, P2 = "ABC" });
        Console.WriteLine($"Tcp返回结果:{result}");

        result = await jsonRpcClient.TestGetContextAsync("RRQM");
        Console.WriteLine($"Tcp返回结果:{result}");
    }

    private static async Task JsonRpcClientInvokeByWebSocket()
    {
        using var jsonRpcClient = new WebSocketJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("ws://127.0.0.1:7707/ws"));//此url就是能连接到websocket的路径。
        await jsonRpcClient.ConnectAsync();
        jsonRpcClient.UseSystemTextJson(option =>
        {
            option.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });

        Console.WriteLine("连接成功");
        var result = await jsonRpcClient.TestJsonRpcAsync(new MyClass() { P1 = 10, P2 = "ABC" });
        Console.WriteLine($"WebSocket返回结果:{result}");

        result = await jsonRpcClient.TestJsonRpcAsync(new MyClass() { P1 = 10, P2 = "ABC" });
        Console.WriteLine($"WebSocket返回结果:{result}");

        result = await jsonRpcClient.TestGetContextAsync("RRQM");
        Console.WriteLine($"WebSocket返回结果:{result}");
    }

    private static async Task CreateHttpJsonRpcService()
    {
        var service = new HttpService();

        await service.SetupAsync(new TouchSocketConfig()
              .SetListenIPHosts(7706)
              .ConfigureContainer(a =>
              {
                  a.AddRpcStore(store =>
                  {
                      store.RegisterServer<IJsonRpcServer, JsonRpcServer>();
                  });

                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.UseHttpJsonRpc()
                  .UseSystemTextJson(option =>
                  {
                      option.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                  })
                  .SetJsonRpcUrl("/jsonRpc");
              }));
        await service.StartAsync();

        ConsoleLogger.Default.Info($"Http服务器已启动");
    }

    private static async Task CreateWebSocketJsonRpcService()
    {
        var service = new HttpService();

        await service.SetupAsync(new TouchSocketConfig()
              .SetListenIPHosts(7707)
              .ConfigureContainer(a =>
              {
                  a.AddRpcStore(store =>
                  {
                      store.RegisterServer<IJsonRpcServer, JsonRpcServer>();
                  });

                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  a.UseWebSocket()
                  .SetWSUrl("/ws");

                  a.UseWebSocketJsonRpc()
                  .UseSystemTextJson(option =>
                  {
                      option.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                  })
                  .SetAllowJsonRpc((socketClient, context) =>
                  {
                      //此处的作用是，通过连接的一些信息判断该ws是否执行JsonRpc。
                      return true;
                  });
              }));
        await service.StartAsync();

        ConsoleLogger.Default.Info($"WebSocket服务器已启动");
    }

    private static async Task CreateTcpJsonRpcService()
    {
        var service = new TcpService();
        await service.SetupAsync(new TouchSocketConfig()
              .SetTcpDataHandlingAdapter(() => new JsonPackageAdapter(System.Text.Encoding.UTF8))
              .SetListenIPHosts(7705)
              .ConfigureContainer(a =>
              {
                  a.AddRpcStore(store =>
                  {
                      store.RegisterServer<IJsonRpcServer, JsonRpcServer>();
                  });

                  a.AddConsoleLogger();
              })
              .ConfigurePlugins(a =>
              {
                  //a.Add<MyTcpPlugin>();

                  a.UseTcpJsonRpc()
                  .UseSystemTextJson(option =>
                  {
                      option.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
                  })
                  .SetAllowJsonRpc((socketClient) =>
                  {
                      //此处的作用是，通过连接的一些信息判断该连接是否执行JsonRpc。
                      return true;
                  });
              }));
        await service.StartAsync();
        ConsoleLogger.Default.Info($"Tcp服务器已启动");
    }


}

internal class MyTcpPlugin : PluginBase, ITcpReceivedPlugin, ITcpSendingPlugin
{
    private readonly ILog logger;

    public MyTcpPlugin(ILog logger)
    {
        this.logger = logger;
    }
    public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
    {
        if (e.RequestInfo is JsonPackage package)
        {
            this.logger.Info($"MyTcpPlugin=>{package.DataString}");
        }
        await e.InvokeNext();
    }

    public async Task OnTcpSending(ITcpSession client, SendingEventArgs e)
    {
        this.logger.Info($"MyTcpPlugin=>{e.Memory.Span.ToString(System.Text.Encoding.UTF8)}");
        await e.InvokeNext();
    }
}

#region System.Text.Json序列化
[JsonSerializable(typeof(MyClass))]
[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
#endregion


[GeneratorRpcProxy]
public interface IJsonRpcServer : IRpcServer
{
    [JsonRpc(MethodInvoke = true)]
    string Show(int a, int b, int c);

    /// <summary>
    /// 使用调用上下文。
    /// 可以从上下文获取调用的SessionClient。从而获得IP和Port等相关信息。
    /// </summary>
    /// <param name="callContext"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    [JsonRpc(MethodInvoke = true)]
    string TestGetContext(ICallContext callContext, string str);

    [JsonRpc(MethodInvoke = true)]
    string TestJsonRpc(MyClass myClass);
}

public partial class JsonRpcServer : SingletonRpcServer, IJsonRpcServer
{

    public string TestGetContext(ICallContext callContext, string str)
    {
        if (callContext.Caller is IHttpSessionClient socketClient)
        {
            if (socketClient.Protocol == Protocol.WebSocket)
            {
                Console.WriteLine("WebSocket请求");
                var client = callContext.Caller as IHttpSessionClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"WebSocket请求{ip}:{port}");
            }
            else
            {
                Console.WriteLine("HTTP请求");
                var client = callContext.Caller as IHttpSessionClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"HTTP请求{ip}:{port}");
            }
        }
        else if (callContext.Caller is ITcpSessionClient sessionClient)
        {
            Console.WriteLine("Tcp请求");
            var ip = sessionClient.IP;
            var port = sessionClient.Port;
            Console.WriteLine($"Tcp请求{ip}:{port}");
        }
        return "RRQM" + str;
    }


    public string TestJsonRpc(MyClass myClass)
    {
        return $"P1={myClass.P1},P2={myClass.P2}";
    }

    public string Show(int a, int b, int c)
    {
        return $"a={a},b={b},c={c}";
    }
}

public class MyClass
{
    public int P1 { get; set; }
    public string? P2 { get; set; }
}

