using System;
using TouchSocket.Core.Config;
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
            Console.WriteLine("连接成功");
            string result = jsonRpcClient.Invoke<string>("jsonrpcconsoleapp.server.testjsonrpc", InvokeOption.WaitInvoke, "RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            JObject obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            JObject newObj = jsonRpcClient.Invoke<JObject>("jsonrpcconsoleapp.server.testjobject", InvokeOption.WaitInvoke, obj);
            Console.WriteLine($"Tcp返回结果:{newObj}");
        }

        private static IRpcParser CreateTcpJsonRpcParser()
        {
            TcpService service = new TcpService();
            service.Connecting += (client, e) =>
            {
                client.SetDataHandlingAdapter(new TerminatorPackageAdapter("\r\n"));
            };

            service.Setup(new TouchSocketConfig()
                .UsePlugin()
                .SetListenIPHosts(new IPHost[] { new IPHost(7705) }))
                .Start();

            return service.AddPlugin<JsonRpcParserPlugin>();
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

        [JsonRpc]
        public JObject TestJObject(JObject obj)
        {
            return obj;
        }
    }
}