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

using System.Threading.RateLimiting;
using TouchSocket.Rpc;
using TouchSocket.Rpc.RateLimiting;
using TouchSocket.Sockets;

namespace RpcRateLimitingConsoleApp;

#region Rpc限流自定义分区键限流器类
// 内部类 RpcFixedWindowLimiter 继承自 RateLimiterPolicy<string>，
// 用于实现基于固定窗口的限流策略，特别适用于远程过程调用 (RPC) 场景。
internal class RpcFixedWindowLimiter : RateLimiterPolicy<string>
{
    // 私有成员变量，存储了创建限流器时使用的配置选项。
    private readonly FixedWindowRateLimiterOptions m_options;

    // 构造函数接受一个 FixedWindowRateLimiterOptions 类型的参数，并将其赋值给 m_options 成员变量。
    public RpcFixedWindowLimiter(FixedWindowRateLimiterOptions options)
    {
        this.m_options = options;
    }

    // 重写基类的方法以返回一个字符串类型的分区键。
    // 根据传入的 ICallContext 对象，尝试从其中获取调用者的 TCP 会话信息。
    // 如果 callContext.Caller 是 ITcpSession 类型，则返回该会话的 IP 地址作为分区键。
    // 如果不是基于 TCP 协议的调用，则返回字符串 "any" 作为分区键。
    protected override string GetPartitionKey(ICallContext callContext)
    {
        if (callContext.Caller is ITcpSession tcpSession)
        {
            return tcpSession.IP;
        }

        // 如果是基于 tcp 协议的调用，理论上不会执行到这里。
        return "any";
    }

    // 重写基类的方法以返回一个新的 RateLimiter 实例。
    // 使用 m_options 创建一个 FixedWindowRateLimiter 实例，并返回它。
    protected override RateLimiter NewRateLimiter(string partitionKey)
    {
        return new FixedWindowRateLimiter(this.m_options);
    }
}
#endregion
