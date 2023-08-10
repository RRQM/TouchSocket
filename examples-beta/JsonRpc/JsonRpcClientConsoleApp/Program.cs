using JsonRpcProxy;
using Newtonsoft.Json.Linq;
using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Sockets;

namespace JsonRpcClientConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        private static void JsonRpcClientInvokeByHttp()
        {
            var jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("http://127.0.0.1:7706/jsonrpc")
                .SetJRPT(JRPT.Http));
            jsonRpcClient.Connect();
            Console.WriteLine("连接成功");
            var result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Http返回结果:{result}");

            var obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            var newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"Http返回结果:{newObj}");
        }

        private static void JsonRpcClientInvokeByTcp()
        {
            var jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7705")
                .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .SetJRPT(JRPT.Tcp));
            jsonRpcClient.Connect();

            Console.WriteLine("连接成功");
            var result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.TestJsonRpc1("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.TestGetContext("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            var obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            var newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"Tcp返回结果:{newObj}");
        }

        private static void JsonRpcClientInvokeByWebSocket()
        {
            var jsonRpcClient = new JsonRpcClient();
            jsonRpcClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("ws://127.0.0.1:7706/ws")//此url就是能连接到websocket的路径。
                .SetJRPT(JRPT.WebSocket));
            jsonRpcClient.Connect();

            Console.WriteLine("连接成功");
            var result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"WebSocket返回结果:{result}");

            result = jsonRpcClient.TestJsonRpc1("RRQM");
            Console.WriteLine($"WebSocket返回结果:{result}");

            result = jsonRpcClient.TestGetContext("RRQM");
            Console.WriteLine($"WebSocket返回结果:{result}");

            var obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            var newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"WebSocket返回结果:{newObj}");
        }
    }
}