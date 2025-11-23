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

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcCallContextConsoleApp;

/// <summary>
/// Rpc调用上下文客户端示例
/// </summary>
public static class ClientExamples
{
    /// <summary>
    /// 测试单例服务
    /// </summary>
    public static async Task TestSingletonServerAsync()
    {
        var client = new TcpDmtpClient();
        await client.ConnectAsync("127.0.0.1:7790");

        var dmtpRpcActor = client.GetDmtpRpcActor();

        //测试登录
        var loginResult = await dmtpRpcActor.InvokeTAsync<bool>("Login", InvokeOption.WaitInvoke, "123", "abc");
        Console.WriteLine($"登录结果: {loginResult}");

        //测试获取调用者信息
        var callerInfo = await dmtpRpcActor.InvokeTAsync<string>("GetCallerInfo", InvokeOption.WaitInvoke);
        Console.WriteLine($"调用者信息: {callerInfo}");

        await client.CloseAsync();
    }

    /// <summary>
    /// 测试瞬态服务
    /// </summary>
    public static async Task TestTransientServerAsync()
    {
        var client = new TcpDmtpClient();
        await client.ConnectAsync("127.0.0.1:7791");

        var dmtpRpcActor = client.GetDmtpRpcActor();

        //测试登录
        var loginResult = await dmtpRpcActor.InvokeTAsync<bool>("Login", InvokeOption.WaitInvoke, "123", "abc");
        Console.WriteLine($"瞬态服务登录结果: {loginResult}");

        //测试获取方法信息
        var methodInfo = await dmtpRpcActor.InvokeTAsync<string>("GetMethodInfo", InvokeOption.WaitInvoke);
        Console.WriteLine($"方法信息: {methodInfo}");

        await client.CloseAsync();
    }

    /// <summary>
    /// 测试IRpcCallContextAccessor服务
    /// </summary>
    public static async Task TestAccessorServerAsync()
    {
        var client = new TcpDmtpClient();
        await client.ConnectAsync("127.0.0.1:7792");

        var dmtpRpcActor = client.GetDmtpRpcActor();

        //测试从Accessor获取上下文
        var result = await dmtpRpcActor.InvokeTAsync<string>("TestGetCallContextFromCallContextAccessor", InvokeOption.WaitInvoke);
        Console.WriteLine($"Accessor测试结果: {result}");

        //测试异步方法中使用Accessor
        var asyncResult = await dmtpRpcActor.InvokeTAsync<string>("TestAsyncCallContextAccessor", InvokeOption.WaitInvoke);
        Console.WriteLine($"异步Accessor测试结果: {asyncResult}");

        await client.CloseAsync();
    }

    /// <summary>
    /// 测试任务取消
    /// </summary>
    public static async Task TestCancellationAsync()
    {
        var client = new TcpDmtpClient();
        await client.ConnectAsync("127.0.0.1:7793");

        var dmtpRpcActor = client.GetDmtpRpcActor();

        try
        {
            //创建一个指定时间可取消令牌源，可用于取消Rpc的调用
            using (var tokenSource = new CancellationTokenSource(3000))
            {
                var invokeOption = new DmtpInvokeOption()
                {
                    FeedbackType = FeedbackType.WaitInvoke,
                    Token = tokenSource.Token //3秒后取消
                };

                var result = await dmtpRpcActor.InvokeTAsync<int>("TestCancellationToken", invokeOption);
                Console.WriteLine($"任务完成，迭代次数: {result}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("任务已被客户端取消");
        }

        await client.CloseAsync();
    }
}
