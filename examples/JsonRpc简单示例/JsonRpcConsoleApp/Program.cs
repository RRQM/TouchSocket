using JsonRpcProxy;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Sockets;

namespace JsonRpcConsoleApp
{
    internal class Program
    {
        //1.完成了JSONRPC 的基本调用方法
        //2.JSONRPC 服务端和客户端的创建
        //3.服务端进行主动通知客户端
        //4.客户端处理服务端推送的自定义消息处理
        //5.[JsonRpc(true)]特性使用 标记为true 表示直接使用方法名称，否则使用明明空间+类名+方法名 全小写
        //6.RPC上下文获取。通过上下文进行自定义消息推送
        private static void Main(string[] args)
        {
            //{"jsonrpc": "2.0", "method": "testjsonrpc", "params":"TouchSocket", "id": 1}

            //此处是生成代理文件，你可以将它复制到你的客户端项目中编译。
            File.WriteAllText("../../../JsonRpcProxy.cs", CodeGenerator.GetProxyCodes("JsonRpcProxy",
                new Type[] { typeof(JsonRpcServer) }, new Type[] { typeof(JsonRpcAttribute) }));

            Console.WriteLine("代理文件已经写入到当前项目。");

            CreateTcpJsonRpcParser(7705);
            CreateHTTPJsonRpcParser(7706);

            JsonRpcClientInvokeByTcp();
            JsonRpcClientInvokeByHttp();
            JsonRpcClientInvokeByWebsocket();

            Console.WriteLine("请按任意键退出");

            Console.ReadKey();
        }

        private static void CreateHTTPJsonRpcParser(int port)
        {
            HttpService service = new HttpService();

            service.Setup(new TouchSocketConfig()
                 .UsePlugin()
                 .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                 .ConfigureRpcStore(a =>//Rpc的配置必须在插件之前。
                 {
                     a.RegisterServer<JsonRpcServer>();
                 })
                 .ConfigurePlugins(a =>
                 {
                     a.UseWebSocket()//启用websocket。
                     .SetWSUrl("/ws");//使用/ws路由连接。

                     a.Add<JsonRpcParserPlugin>()
                     .SetJsonRpcUrl("/jsonRpc");
                 }))
                .Start();
        }

        private static void CreateTcpJsonRpcParser(int port)
        {
            TcpService service = new TcpService();
            service.Connecting = (client, e) =>
            {
                Console.WriteLine("客户端连接成功");
                Console.WriteLine(client.ID);
            };
            service.Connected = (SocketClient client, TouchSocketEventArgs e) =>
            {
                Console.WriteLine(client.ID);
                Console.WriteLine("客户端连接完成");
            };
            service.Disconnected = (client, e) =>
            {
                Console.WriteLine("客户端连接断开");
                Console.WriteLine(client.ID);
            };
            service.Setup(new TouchSocketConfig()
                .UsePlugin()
                .SetDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))//使用这个适配器，防止粘包。但是也要求json中没有换行符。
                .SetListenIPHosts(new IPHost[] { new IPHost(port) })
                .ConfigureRpcStore(a =>
                {
                    a.RegisterServer<JsonRpcServer>();
                })
                 .ConfigurePlugins(a =>
                 {
                     a.Add<JsonRpcParserPlugin>();
                 }))
                .Start();
        }

        private static void JsonRpcClientInvokeByHttp()
        {
            JsonRpcClient jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("http://127.0.0.1:7706/jsonrpc")
                .SetJRPT(JRPT.Http));
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");
            string result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Http返回结果:{result}");

            JObject obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            JObject newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"Http返回结果:{newObj}");
        }

        private static void JsonRpcClientInvokeByTcp()
        {
            JsonRpcClient jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7705")
                .SetDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetJRPT(JRPT.Tcp));
            jsonRpcClient.Connect();

            Console.WriteLine("连接成功");
            string result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.TestJsonRpc1("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.TestGetContext("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            JObject obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            JObject newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"Tcp返回结果:{newObj}");
        }

        private static void JsonRpcClientInvokeByWebsocket()
        {
            JsonRpcClient jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:7706/ws")//此url就是能连接到websocket的路径。
                .SetDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetJRPT(JRPT.Websocket));
            jsonRpcClient.Connect();

            Console.WriteLine("连接成功");
            string result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Websocket返回结果:{result}");

            result = jsonRpcClient.TestJsonRpc1("RRQM");
            Console.WriteLine($"Websocket返回结果:{result}");

            result = jsonRpcClient.TestGetContext("RRQM");
            Console.WriteLine($"Websocket返回结果:{result}");

            JObject obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            JObject newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"Websocket返回结果:{newObj}");
        }
    }

    public class JsonRpcServer : TransientRpcServer
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
            if (callContext.Caller is HttpSocketClient socketClient)
            {
                Console.WriteLine("HTTP请求");
                var client = callContext.Caller as HttpSocketClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"HTTP请求{ip}:{port}");
            }
            else if (callContext.Caller is SocketClient)
            {
                Console.WriteLine("Tcp请求");
                var client = callContext.Caller as SocketClient;
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

        /// <summary>
        /// 当标记为true时直接使用方法名称
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [JsonRpc(true)]
        public string TestJsonRpc1(string str)
        {
            if (this.CallContext is IJsonRpcCallContext context)
            {
            }
            return "RRQM" + str;
        }
    }
}