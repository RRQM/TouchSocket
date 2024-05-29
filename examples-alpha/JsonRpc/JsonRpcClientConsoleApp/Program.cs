using JsonRpcProxy;
using Newtonsoft.Json.Linq;
using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Sockets;

namespace JsonRpcClientConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
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
            Console.WriteLine("连接成功");
            var result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Http返回结果:{result}");

            result = jsonRpcClient.TestGetContext("RRQM");
            Console.WriteLine($"Http返回结果:{result}");

            var obj = new JObject();
            obj.Add("A", "A");
            obj.Add("B", 10);
            obj.Add("C", 100.1);
            var newObj = jsonRpcClient.TestJObject(obj);
            Console.WriteLine($"Http返回结果:{newObj}");
        }

        private static async Task JsonRpcClientInvokeByTcp()
        {
            using var jsonRpcClient = new TcpJsonRpcClient();
            await jsonRpcClient.SetupAsync(new TouchSocketConfig()
                 .SetRemoteIPHost("127.0.0.1:7705")
                 .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n")));
            await jsonRpcClient.ConnectAsync();

            Console.WriteLine("连接成功");
            var result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"Tcp返回结果:{result}");

            result = jsonRpcClient.TestJsonRpc("RRQM");
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

        private static async Task JsonRpcClientInvokeByWebSocket()
        {
            using var jsonRpcClient = new WebSocketJsonRpcClient();
            await jsonRpcClient.SetupAsync(new TouchSocketConfig()
                 .SetRemoteIPHost("ws://127.0.0.1:7707/ws"));//此url就是能连接到websocket的路径。
            await jsonRpcClient.ConnectAsync();

            Console.WriteLine("连接成功");
            var result = jsonRpcClient.TestJsonRpc("RRQM");
            Console.WriteLine($"WebSocket返回结果:{result}");

            result = jsonRpcClient.TestJsonRpc("RRQM");
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