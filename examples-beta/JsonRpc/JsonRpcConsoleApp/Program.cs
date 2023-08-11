using JsonRpcProxy;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace JsonRpcConsoleApp
{
    internal class Program
    {
        //1.完成了JSONRPC 的基本调用方法
        //2.JSONRPC 服务端和客户端的创建
        //3.服务端进行主动通知客户端
        //4.客户端处理服务端推送的自定义消息处理
        //5.[JsonRpc(true)]特性使用 标记为true 表示直接使用方法名称，否则使用命名空间+类名+方法名 全小写
        //6.RPC上下文获取。通过上下文进行自定义消息推送
        private static void Main(string[] args)
        {
            //{"jsonrpc": "2.0", "method": "testjsonrpc", "params":"TouchSocket", "id": 1}

            //此处是生成代理文件，你可以将它复制到你的客户端项目中编译。
            File.WriteAllText("../../../JsonRpcProxy.cs", CodeGenerator.GetProxyCodes("JsonRpcProxy",
                new Type[] { typeof(JsonRpcServer) }, new Type[] { typeof(JsonRpcAttribute) }));

            ConsoleLogger.Default.Info("代理文件已经写入到当前项目。");

            CreateTcpJsonRpcService();
            CreateHTTPJsonRpcParser();

            Console.ReadKey();
        }

        private static void CreateHTTPJsonRpcParser()
        {
            var service = new HttpService();

            service.Setup(new TouchSocketConfig()
                 .SetListenIPHosts(7706)
                 .ConfigurePlugins(a =>
                 {
                     a.UseWebSocket()//启用websocket。
                     .SetWSUrl("/ws");//使用/ws路由连接。

                     a.UseHttpJsonRpc()
                     .ConfigureRpcStore(store =>
                     {
                         store.RegisterServer<JsonRpcServer>();
                     })
                     .SetJsonRpcUrl("/jsonRpc");
                 }))
                .Start();

            ConsoleLogger.Default.Info($"Http服务器已启动");
            ConsoleLogger.Default.Info($"Http服务器已启动");
        }

        private static void CreateTcpJsonRpcService()
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()
                .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetListenIPHosts(7705)
                 .ConfigurePlugins(a =>
                 {
                     a.UseTcpJsonRpc()
                     .ConfigureRpcStore(store =>
                     {
                         store.RegisterServer<JsonRpcServer>();
                     });
                 }))
                .Start();
        }

        
    }

    public class JsonRpcServer : RpcServer
    {
        /// <summary>
        /// 使用调用上下文。
        /// 可以从上下文获取调用的SocketClient。从而获得IP和Port等相关信息。
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [JsonRpc(MethodFlags = MethodFlags.IncludeCallContext)]
        public string TestGetContext(ICallContext callContext, string str)
        {
            if (callContext.Caller is IHttpSocketClient socketClient)
            {
                Console.WriteLine("HTTP请求");
                var client = callContext.Caller as IHttpSocketClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"HTTP请求{ip}:{port}");
            }
            else if (callContext.Caller is ISocketClient)
            {
                Console.WriteLine("Tcp请求");
                var client = callContext.Caller as ISocketClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"Tcp请求{ip}:{port}");
            }
            return "RRQM" + str;
        }

        [JsonRpc]
        public JObject TestJObject(JObject obj)
        {
            return obj;
        }

        [JsonRpc]
        public string TestJsonRpc(string str)
        {
            return "RRQM" + str;
        }
    }
}