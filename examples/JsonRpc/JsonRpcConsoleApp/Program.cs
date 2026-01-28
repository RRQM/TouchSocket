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
using System.Text;
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


#if DEBUG
        //此处是生成代理文件，你可以将它复制到你的客户端项目中编译。
        File.WriteAllText("../../../JsonRpcProxy.cs", CodeGenerator.GetProxyCodes("JsonRpcProxy",
            new Type[] { typeof(JsonRpcServer) }, new Type[] { typeof(JsonRpcAttribute) }));

#endif


        ConsoleLogger.Default.Info("代理文件已经写入到当前项目。");

        await CreateTcpJsonRpcService();
        await CreateHttpJsonRpcService();
        await CreateWebSocketJsonRpcService();

        Console.ReadKey();
    }

    private static async Task CreateHttpJsonRpcService()
    {
        #region 创建HttpJsonRpc服务端
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
                 var plugin= a.UseHttpJsonRpc(options =>
                  {
                      options.SetAllowJsonRpc("/jsonRpc");
                  });

                  //可以获取到ActionMap，从而获知注册了哪些方法。
                  //foreach (var item in plugin.ActionMap)
                  //{
                  //    Console.WriteLine(item.Key, item.Value);
                  //}
              }));
        await service.StartAsync();
        #endregion


        ConsoleLogger.Default.Info($"Http服务器已启动");
    }

    private static async Task CreateWebSocketJsonRpcService()
    {
        #region 创建WebSocketJsonRpc服务端
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
                  //添加WebSocket功能
                  a.UseWebSocket(options =>
                  {
                      options.SetUrl("/ws");//设置url直接可以连接。
                      options.SetAutoPong(true);//当收到ping报文时自动回应pong
                  });

                  a.UseWebSocketJsonRpc(options =>
                  {
                      options.SetAllowJsonRpc((socketClient, httpContext) =>
                      {
                          //此处的作用是，通过连接的一些信息判断该ws是否执行JsonRpc。
                          return true;
                      });
                  });
              }));
        await service.StartAsync();
        #endregion


        ConsoleLogger.Default.Info($"WebSocket服务器已启动");
    }

    private static async Task CreateTcpJsonRpcService()
    {
        #region 创建TcpJsonRpc服务端
        var service = new TcpService();

        var config = new TouchSocketConfig();
        config.SetListenIPHosts(7705);

        //设置json数据处理适配器。
        config.SetTcpDataHandlingAdapter(() => new JsonPackageAdapter(Encoding.UTF8));

        //配置容器
        config.ConfigureContainer(a =>
        {
            a.AddConsoleLogger();

            a.AddRpcStore(store =>
            {
                store.RegisterServer<JsonRpcServer>();
            });
        });

        //配置插件
        config.ConfigurePlugins(a =>
        {
            a.UseTcpJsonRpc(optiosn =>
            {
                optiosn.SetAllowJsonRpc((socketClient) =>
                {
                    //此处的作用是，通过连接的一些信息判断该连接是否执行JsonRpc。
                    return true;
                });

                #region JsonRpcSystemTextJson配置
                optiosn.UseSystemTextJsonFormatter(jsonOptions =>
                {
                    //配置项
                });
                #endregion
            });
        });
        await service.SetupAsync(config);
        await service.StartAsync();
        #endregion

    }
}

#region 声明JsonRpc服务
public partial class JsonRpcServer : SingletonRpcServer
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


#endregion
