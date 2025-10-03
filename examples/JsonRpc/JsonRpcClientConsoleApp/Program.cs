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

using JsonRpcProxy;
using Newtonsoft.Json.Linq;
using System.Text;
using TouchSocket.Core;
using TouchSocket.JsonRpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace JsonRpcClientConsoleApp;

internal class Program
{
    private static void ConsoleAction_OnException(Exception obj)
    {
        ConsoleLogger.Default.Exception(obj);
    }

    private static async Task<IJsonRpcClient> CreateHttpJsonRpcClient()
    {
        #region 创建HttpJsonRpc客户端
        var jsonRpcClient = new HttpJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("http://127.0.0.1:7706/jsonrpc"));
        await jsonRpcClient.ConnectAsync();
        return jsonRpcClient;
        #endregion

    }

    private static async Task<IJsonRpcClient> CreateTcpJsonRpcClient()
    {
        #region 创建TcpJsonRpc客户端
        var jsonRpcClient = new TcpJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7705")
             .SetTcpDataHandlingAdapter(() => new JsonPackageAdapter(Encoding.UTF8)));
        await jsonRpcClient.ConnectAsync();

        return jsonRpcClient;
        #endregion

    }

    static async Task JsonRpcInvoke(IJsonRpcClient client)
    {
        #region Invoke直接调用
        var str = await client.InvokeTAsync<string>("TestJsonRpc", InvokeOption.WaitInvoke, "123");
        #endregion
    }

    static async Task RpcTestJsonRpc(IJsonRpcClient client)
    {
        #region JsonRpcProxy调用
        var result = await client.TestJsonRpcAsync("RRQM");
        #endregion

        Console.WriteLine($"Tcp返回结果:{result}");
    }

    private static async Task<IJsonRpcClient> CreateWebSocketJsonRpcClient()
    {
        #region 创建WebSocketJsonRpc客户端
        var jsonRpcClient = new WebSocketJsonRpcClient();
        await jsonRpcClient.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("ws://127.0.0.1:7707/ws"));//此url就是能连接到websocket的路径。
        await jsonRpcClient.ConnectAsync();

        return jsonRpcClient;
        #endregion
    }

    private static async Task JsonRpcClientInvokeByHttp()
    {
        var jsonRpcClient = await CreateHttpJsonRpcClient();

        Console.WriteLine("连接成功");
        var result = await jsonRpcClient.TestJsonRpcAsync("RRQM");
        Console.WriteLine($"Http返回结果:{result}");

        result = await jsonRpcClient.TestGetContextAsync("RRQM");
        Console.WriteLine($"Http返回结果:{result}");

        var obj = new JObject();
        obj.Add("A", "A");
        obj.Add("B", 10);
        obj.Add("C", 100.1);
        var newObj = await jsonRpcClient.TestJObjectAsync(obj);
        Console.WriteLine($"Http返回结果:{newObj}");
    }

    private static async Task JsonRpcClientInvokeByTcp()
    {
        var jsonRpcClient = await CreateTcpJsonRpcClient();

        Console.WriteLine("连接成功");
        var result = await jsonRpcClient.TestJsonRpcAsync("RRQM");
        Console.WriteLine($"Tcp返回结果:{result}");

        result = await jsonRpcClient.TestJsonRpcAsync("RRQM");
        Console.WriteLine($"Tcp返回结果:{result}");

        result = await jsonRpcClient.TestGetContextAsync("RRQM");
        Console.WriteLine($"Tcp返回结果:{result}");

        var obj = new JObject();
        obj.Add("A", "A");
        obj.Add("B", 10);
        obj.Add("C", 100.1);
        var newObj = await jsonRpcClient.TestJObjectAsync(obj);
        Console.WriteLine($"Tcp返回结果:{newObj}");
    }

    private static async Task JsonRpcClientInvokeByWebSocket()
    {
        var jsonRpcClient = await CreateWebSocketJsonRpcClient();

        Console.WriteLine("连接成功");
        var result = await jsonRpcClient.TestJsonRpcAsync("RRQM");
        Console.WriteLine($"WebSocket返回结果:{result}");

        result = await jsonRpcClient.TestJsonRpcAsync("RRQM");
        Console.WriteLine($"WebSocket返回结果:{result}");

        result = await jsonRpcClient.TestGetContextAsync("RRQM");
        Console.WriteLine($"WebSocket返回结果:{result}");

        var obj = new JObject();
        obj.Add("A", "A");
        obj.Add("B", 10);
        obj.Add("C", 100.1);
        var newObj = await jsonRpcClient.TestJObjectAsync(obj);
        Console.WriteLine($"WebSocket返回结果:{newObj}");
    }

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
}