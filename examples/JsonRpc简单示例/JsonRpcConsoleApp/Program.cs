using System;
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.XREF.Newtonsoft.Json.Linq;
using TouchSocket.Http;
using TouchSocket.Rpc;
using TouchSocket.Rpc.JsonRpc;
using TouchSocket.Rpc.TouchRpc;
using TouchSocket.Sockets;

namespace JsonRpcConsoleApp
{
    internal class Program
    {
        //JSONRPC 通讯基础示例 TPC和HTTP 未来考虑扩展升级WebSocket
        //1.完成了JSONRPC 的基本调用方法
        //2.JSONRPC 服务端和客户端的创建
        //3.服务端进行主动通知客户端
        //4.客户端处理服务端推送的自定义消息处理
        //5.[JsonRpc(true)]特性使用 标记为true 表示直接使用方法名称，否则使用明明空间+类名+方法名 全小写
        //6.RPC上下文获取。通过上下文进行自定义消息推送
        private static void Main(string[] args)
        {
            RpcStore rpcStore = new RpcStore(new TouchSocket.Core.Dependency.Container());

            //添加解析器，解析器根据传输协议，序列化方式的不同，调用RPC服务
            rpcStore.AddRpcParser("tcpJsonRpcParser ", CreateTcpJsonRpcParser());
            rpcStore.AddRpcParser("httpJsonRpcParser ", CreateHTTPJsonRpcParser());

            //注册当前程序集的所有服务
            rpcStore.RegisterAllServer();

            //分享代理，代理文件可通过RRQMTool远程获取。
            //rpcService.ShareProxy(new IPHost(8848));

            //或者直接本地导出代理文件。
            //ServerCellCode[] cellCodes = rpcStore.GetProxyInfo(typeof(JsonRpcAttribute));//当想导出全部时，RpcStore.ProxyAttributeMap.Values.ToArray()
            //string codeString = CodeGenerator.ConvertToCode("RRQMProxy", cellCodes);

            JsonRpcClientInvokeByTcp();
            JsonRpcClientInvokeByHttp();

            Console.WriteLine("请按任意键退出");
            Console.ReadKey();
        }


        private static void JsonRpcClientInvokeByHttp()
        {
            JsonRpcClient jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("http://127.0.0.1:7706/jsonrpc")
                .SetJRPT(JRPT.Http));
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");
            string result = jsonRpcClient.Invoke<string>("jsonrpcconsoleapp.server.testjsonrpc", InvokeOption.WaitInvoke, "RRQM");
            Console.WriteLine($"Http返回结果:{result}");

            JObject obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            JObject newObj = jsonRpcClient.Invoke<JObject>("jsonrpcconsoleapp.server.testjobject", InvokeOption.WaitInvoke, obj);
            Console.WriteLine($"Http返回结果:{newObj}");
        }

        private static void JsonRpcClientInvokeByTcp()
        {
            JsonRpcClient jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7705")
                .SetJRPT(JRPT.Tcp));
            jsonRpcClient.Connect();

            //客户端收到信息处理函数
            Func<ByteBlock, IRequestInfo, bool> func = (ByteBlock byteBlock, IRequestInfo requestInfo) =>
            {
                var str = System.Text.Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);
                //可以自行拦截做消息处理分发

                if(str== "Hello Word")
                {
                    Console.WriteLine("收到服务端通知自定义消息");
                    //自定义消息不继续传递
                    return false;
                }
                Console.WriteLine(str);
                //表示是否继续向下传递。 true是 false 否
                return true;
            };

            jsonRpcClient.OnHandleReceivedData = func;
            Console.WriteLine("连接成功");
            string result = jsonRpcClient.Invoke<string>("jsonrpcconsoleapp.server.testjsonrpc", InvokeOption.WaitInvoke, "RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.Invoke<string>("TestJsonRpc1", InvokeOption.WaitInvoke, "RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.Invoke<string>("jsonrpcconsoleapp.server.testgetcontext", InvokeOption.WaitInvoke, "RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            JObject obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            JObject newObj = jsonRpcClient.Invoke<JObject>("jsonrpcconsoleapp.server.testjobject", InvokeOption.WaitInvoke, obj);
            Console.WriteLine($"Tcp返回结果:{newObj}");

            NotifyAll();
        }
        static List<SocketClient> Clients = new List<SocketClient>();  
        private static IRpcParser CreateTcpJsonRpcParser()
        {
            TcpService service = new TcpService();
            service.Connecting += (client, e) =>
            {
                Console.WriteLine("客户端连接成功");
                client.SetDataHandlingAdapter(new TerminatorPackageAdapter("\r\n"));
                Console.WriteLine(client.ID);
            };
            service.Connected += (SocketClient client, TouchSocketEventArgs e) =>
            {
                Clients.Add(client);
                Console.WriteLine(client.ID);
                Console.WriteLine("客户端连接完成");
            };
            service.Disconnected += (client, e) =>
            {
                Clients.Remove(client);
                Console.WriteLine("客户端连接断开");
                Console.WriteLine(client.ID);
            };
            service.Setup(new TouchSocketConfig()
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7705) }))
                .Start();

            return service.AddPlugin<JsonRpcParserPlugin>();
        }
        /// <summary>
        /// 通知所有客户端
        /// </summary>
        private static void NotifyAll()
        {
            for(int i = 0; i < Clients.Count; i++)
            {
                //如果在线
                if (Clients[i].Online)
                {
                    try
                    {
                        Clients[i].Send(Encoding.UTF8.GetBytes("Hello Word"));
                    }
                    catch (Exception ex)
                    {
                        //有可能判断的时候在线发送的时候不在线
                        Console.WriteLine(ex.Message);
                    }
                
                }
            }
        }

        private static IRpcParser CreateHTTPJsonRpcParser()
        {
            HttpService service = new HttpService();

            service.Setup(new TouchSocketConfig().UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7706) }))
                .Start();

            return service.AddPlugin<JsonRpcParserPlugin>()
                 .SetJsonRpcUrl("/jsonRpc");
        }
    }

    public class Server : RpcServer
    {
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
          
            return "RRQM" + str;
        }
        [JsonRpc]
        [TouchRpc(MethodFlags = MethodFlags.IncludeCallContext)]//使用调用上才文
        public string TestGetContext(ICallContext callContext, string str)
        {
            if (callContext.Caller is HttpSocketClient)
            {
                Console.WriteLine("HTTP请求");
                var client = callContext.Caller as HttpSocketClient;
                var ip = client.IP;
                var port = client.Port;
                Console.WriteLine($"HTTP请求{ip}:{port}");

            }

            if (callContext.Caller is SocketClient)
            {
                Console.WriteLine("Tcp请求");
                var client = callContext.Caller as SocketClient;
                var ip = client.IP;
                var port = client.Port;

                client.Send(Encoding.UTF8.GetBytes("Hello Word"));
                Console.WriteLine($"Tcp请求{ip}:{port}");

            }
            return "RRQM" + str;
        }
        [JsonRpc]
        public JObject TestJObject(JObject obj)
        {
            return obj;
        }
    }
}