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

using System.ComponentModel;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace RpcAopConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("RPC AOP 示例");
        Console.WriteLine("演示RpcActionFilter的使用");

        await StartServerAsync();

        // 创建客户端进行测试
        await TestRpcCallAsync();

        Console.ReadKey();
    }

    #region RpcAop启动服务
    /// <summary>
    /// 启动带有AOP的RPC服务
    /// </summary>
    private static async Task StartServerAsync()
    {
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()
            .SetListenIPHosts(7790)
            .ConfigureContainer(a =>
            {
                a.AddRpcStore(store =>
                {
                    store.RegisterServer<MyRpcServer>();
                });
            })
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            });

        await service.SetupAsync(config);
        await service.StartAsync();
        
        service.Logger.Info("RPC服务已启动，端口：7790");
    }
    #endregion

    #region RpcAop测试调用
    /// <summary>
    /// 测试RPC调用，查看AOP效果
    /// </summary>
    private static async Task TestRpcCallAsync()
    {
        var client = new TcpDmtpClient();
        await client.SetupAsync(new TouchSocketConfig()
            .SetRemoteIPHost("127.0.0.1:7790")
            .ConfigurePlugins(a =>
            {
                a.UseDmtpRpc();
            }));
        await client.ConnectAsync();

        client.Logger.Info("客户端已连接");

        // 调用Add方法
        var result = await client.GetDmtpRpcActor().InvokeAsync("Add", typeof(int), InvokeOption.WaitInvoke, 10, 20);
        client.Logger.Info($"调用Add(10, 20)的结果：{result}");

        // 调用需要权限的方法
        try
        {
            await client.GetDmtpRpcActor().InvokeAsync("AdminMethod", typeof(string), InvokeOption.WaitInvoke);
        }
        catch (Exception ex)
        {
            client.Logger.Warning($"调用AdminMethod失败：{ex.Message}");
        }
    }
    #endregion
}

#region RpcAop定义基础特性
/// <summary>
/// 自定义RPC操作筛选器特性，用于记录日志
/// </summary>
public class MyRpcActionFilterAttribute : RpcActionFilterAttribute
{
    public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        if (callContext.Caller is ITcpSessionClient client)
        {
            client.Logger.Info($"即将执行Rpc-{callContext.RpcMethod.Name}");
        }
        return await Task.FromResult(invokeResult);
    }

    public override async Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
    {
        if (callContext.Caller is ITcpSessionClient client)
        {
            client.Logger.Info($"执行RPC-{callContext.RpcMethod.Name}结束，状态={invokeResult.Status}");
        }
        
        return await base.ExecutedAsync(callContext, parameters, invokeResult, exception);
    }
}
#endregion

#region RpcAop定义权限验证特性
/// <summary>
/// 权限验证筛选器，用于限制访问
/// </summary>
public class AuthorizationFilterAttribute : RpcActionFilterAttribute
{
    public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        // 模拟权限验证逻辑
        if (callContext.Caller is ITcpSessionClient client)
        {
            // 这里简化示例，实际应用中可以从客户端的其他属性或自定义验证逻辑获取权限信息
            // 示例：模拟检查是否有管理员权限
            var hasPermission = false; // 实际应用中应该通过某种方式验证用户权限
            
            if (!hasPermission)
            {
                // 没有权限，设置失败状态
                invokeResult.Status = InvokeStatus.UnFound;
                invokeResult.Message = "无权访问此方法";
                client.Logger.Warning($"用户无权访问方法：{callContext.RpcMethod.Name}");
            }
        }
        
        return await Task.FromResult(invokeResult);
    }
}
#endregion

#region RpcAop定义异常处理特性
/// <summary>
/// 全局异常处理筛选器
/// </summary>
public class ExceptionHandlerFilterAttribute : RpcActionFilterAttribute
{
    public override async Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
    {
        if (exception != null)
        {
            if (callContext.Caller is ITcpSessionClient client)
            {
                client.Logger.Error($"执行RPC方法 {callContext.RpcMethod.Name} 时发生异常：{exception.Message}");
            }
            
            // 可以将异常转换为更友好的错误信息
            invokeResult.Status = InvokeStatus.Exception;
            invokeResult.Message = $"服务器内部错误：{exception.Message}";
        }
        
        return await base.ExecutedAsync(callContext, parameters, invokeResult, exception);
    }
}
#endregion

#region RpcAop使用特性类
/// <summary>
/// 使用RpcActionFilter特性的RPC服务类
/// </summary>
[MyRpcActionFilter]
[ExceptionHandlerFilter]
public partial class MyRpcServer : SingletonRpcServer
{
    /// <summary>
    /// 将两个数相加，使用方法级别的特性
    /// </summary>
    [DmtpRpc(MethodInvoke = true)]
    [Description("将两个数相加")]
    [MyRpcActionFilter]
    public int Add(int a, int b)
    {
        this.m_logger.Info("正在执行Add方法");
        var sum = a + b;
        return sum;
    }

    /// <summary>
    /// 需要管理员权限才能访问的方法
    /// </summary>
    [DmtpRpc(MethodInvoke = true)]
    [Description("管理员方法")]
    [AuthorizationFilter]
    public string AdminMethod()
    {
        return "这是管理员才能访问的方法";
    }

    private readonly ILog m_logger;

    public MyRpcServer(ILog logger)
    {
        this.m_logger = logger;
    }
}
#endregion

#region RpcAop使用特性接口
/// <summary>
/// 在接口上使用RpcActionFilter特性
/// </summary>
[MyRpcActionFilter]
public interface IMyRpcServer : ISingletonRpcServer
{
    [MyRpcActionFilter]
    int Add(int a, int b);
}

/// <summary>
/// 实现带有特性的接口
/// </summary>
[ExceptionHandlerFilter]
public partial class MyRpcServerImpl : SingletonRpcServer, IMyRpcServer
{
    [MyRpcActionFilter]
    public int Add(int a, int b)
    {
        return a + b;
    }
}
#endregion

#region RpcAop互斥特性基类
/// <summary>
/// 互斥特性基类示例
/// </summary>
public class MyBaseAttribute : RpcActionFilterAttribute
{
    public override Type[] MutexAccessTypes => new Type[] { typeof(MyBaseAttribute) };
    
    public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        if (callContext.Caller is ITcpSessionClient client)
        {
            client.Logger.Info($"[MyBase] 执行 {callContext.RpcMethod.Name}");
        }
        return await Task.FromResult(invokeResult);
    }
}
#endregion

#region RpcAop互斥特性子类
/// <summary>
/// 互斥特性子类1
/// </summary>
public class MyAttribute : MyBaseAttribute
{
    public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        if (callContext.Caller is ITcpSessionClient client)
        {
            client.Logger.Info($"[My] 执行 {callContext.RpcMethod.Name}");
        }
        return await Task.FromResult(invokeResult);
    }
}

/// <summary>
/// 互斥特性子类2
/// </summary>
public class My2Attribute : MyBaseAttribute
{
    public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
    {
        if (callContext.Caller is ITcpSessionClient client)
        {
            client.Logger.Info($"[My2] 执行 {callContext.RpcMethod.Name}");
        }
        return await Task.FromResult(invokeResult);
    }
}
#endregion

#region RpcAop互斥特性应用
/// <summary>
/// 展示互斥特性的接口
/// </summary>
public interface IMutexInterface
{
    [My]
    [My2]
    void Test();
}
#endregion
