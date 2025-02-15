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

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace JsonRpcConsoleApp;

internal class Program
{
    //1.完成了JSONRPC 的基本调用方法
    //2.JSONRPC 服务端和客户端的创建
    //3.服务端进行主动通知客户端
    //4.客户端处理服务端推送的自定义消息处理
    //5.[JsonRpc(true)]特性使用 标记为true 表示直接使用方法名称，否则使用命名空间+类名+方法名 全小写
    //6.RPC上下文获取。通过上下文进行自定义消息推送
    private static async Task Main(string[] args)
    {
        //{"jsonrpc": "2.0", "method": "testjsonrpc", "params":"TouchSocket", "id": 1}

        //此处是生成代理文件，你可以将它复制到你的客户端项目中编译。
        File.WriteAllText("../../../JsonRpcProxy.cs", CodeGenerator.GetProxyCodes("JsonRpcProxy",
            new Type[] { typeof(JsonRpcServer) }, new Type[] { typeof(JsonRpcAttribute) }));

        ConsoleLogger.Default.Info("代理文件已经写入到当前项目。");

        await CreateTcpJsonRpcService();
        await CreateHttpJsonRpcService();
        await CreateWebSocketJsonRpcService();

        Console.ReadKey();
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
                      store.RegisterServer<JsonRpcServer>();
                  });
              })
              .ConfigurePlugins(a =>
              {
                  a.UseHttpJsonRpc()
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
                      store.RegisterServer<JsonRpcServer>();
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
              .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
              .SetListenIPHosts(7705)
              .ConfigureContainer(a =>
              {
                  a.AddRpcStore(store =>
                  {
                      store.RegisterServer<JsonRpcServer>();
                  });
              })
              .ConfigurePlugins(a =>
              {
                  a.UseTcpJsonRpc()
                  .SetAllowJsonRpc((socketClient) =>
                  {
                      //此处的作用是，通过连接的一些信息判断该连接是否执行JsonRpc。
                      return true;
                  });
              }));
        await service.StartAsync();
    }
}

public partial class JsonRpcServer : RpcServer
{
    /// <summary>
    /// 使用调用上下文。
    /// 可以从上下文获取调用的SessionClient。从而获得IP和Port等相关信息。
    /// </summary>
    /// <param name="callContext"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    [JsonRpc(MethodInvoke = true)]
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

    [JsonRpc(MethodInvoke = true)]
    public JObject TestJObject(JObject obj)
    {
        return obj;
    }

    [JsonRpc(MethodInvoke = true)]
    public string TestJsonRpc(string str)
    {
        return "RRQM" + str;
    }

    [JsonRpc(MethodInvoke = true)]
    public string Show(int a, int b, int c)
    {
        return $"a={a},b={b},c={c}";
    }
}

