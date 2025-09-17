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

using System.Threading;

namespace TouchSocket.Rpc;

/// <summary>
/// 表示RPC调用的选项配置，用于控制调用行为和反馈机制。
/// </summary>
/// <remarks>
/// InvokeOption提供了对RPC调用过程的精细控制，包括超时设置、反馈类型和取消机制。
/// 通过预定义的静态属性可以快速使用常见的配置组合。
/// </remarks>
public class InvokeOption
{
    static InvokeOption()
    {
        OnlySend = new InvokeOption(millisecondsTimeout: 5000)
        {
            FeedbackType = FeedbackType.OnlySend
        };

        WaitSend = new InvokeOption(millisecondsTimeout: 5000)
        {
            FeedbackType = FeedbackType.WaitSend
        };

        WaitInvoke = new InvokeOption(millisecondsTimeout: 5000)
        {
            FeedbackType = FeedbackType.WaitInvoke
        };
    }

    /// <summary>
    /// 初始化<see cref="InvokeOption"/>类的新实例，使用默认配置。
    /// </summary>
    /// <remarks>
    /// 默认配置：超时时间为5000毫秒，反馈类型为<see cref="FeedbackType.WaitInvoke"/>。
    /// </remarks>
    public InvokeOption()
    {
    }

    /// <summary>
    /// 使用指定的超时时间初始化<see cref="InvokeOption"/>类的新实例。
    /// </summary>
    /// <param name="millisecondsTimeout">调用超时时间，以毫秒为单位。</param>
    public InvokeOption(int millisecondsTimeout)
    {
        this.Timeout = millisecondsTimeout;
    }

    /// <summary>
    /// 获取仅发送模式的预定义调用选项。
    /// </summary>
    /// <value>
    /// 配置为仅发送模式的<see cref="InvokeOption"/>实例，
    /// 超时时间为5000毫秒，反馈类型为<see cref="FeedbackType.OnlySend"/>。
    /// </value>
    /// <remarks>
    /// 在此模式下，调用方发送请求后立即返回，不等待任何响应。
    /// 适用于单向通信或不需要响应结果的场景。
    /// </remarks>
    public static InvokeOption OnlySend { get; }

    /// <summary>
    /// 获取等待调用完成模式的预定义调用选项。
    /// </summary>
    /// <value>
    /// 配置为等待调用完成模式的<see cref="InvokeOption"/>实例，
    /// 超时时间为5000毫秒，反馈类型为<see cref="FeedbackType.WaitInvoke"/>。
    /// </value>
    /// <remarks>
    /// 在此模式下，调用方会等待直到远程方法执行完成并返回结果。
    /// 这是最常用的RPC调用模式，提供完整的请求-响应语义。
    /// </remarks>
    public static InvokeOption WaitInvoke { get; }

    /// <summary>
    /// 获取等待发送完成模式的预定义调用选项。
    /// </summary>
    /// <value>
    /// 配置为等待发送完成模式的<see cref="InvokeOption"/>实例，
    /// 超时时间为5000毫秒，反馈类型为<see cref="FeedbackType.WaitSend"/>。
    /// </value>
    /// <remarks>
    /// 在此模式下，调用方会等待直到请求数据成功发送到对方，但不等待执行结果。
    /// 适用于需要确保数据传输成功但不需要等待执行结果的场景。
    /// </remarks>
    public static InvokeOption WaitSend { get; }

    /// <summary>
    /// 获取或设置调用反馈类型。
    /// </summary>
    /// <value>指定RPC调用的反馈机制，默认为<see cref="FeedbackType.WaitInvoke"/>。</value>
    /// <remarks>
    /// 反馈类型决定了调用方等待响应的方式：
    /// <list type="bullet">
    /// <item><description><see cref="FeedbackType.OnlySend"/>：仅发送，不等待响应</description></item>
    /// <item><description><see cref="FeedbackType.WaitSend"/>：等待发送完成</description></item>
    /// <item><description><see cref="FeedbackType.WaitInvoke"/>：等待调用完成</description></item>
    /// </list>
    /// </remarks>
    public FeedbackType FeedbackType { get; set; } = FeedbackType.WaitInvoke;

    /// <summary>
    /// 获取或设置调用超时时间。
    /// </summary>
    /// <value>调用超时时间，以毫秒为单位，默认为5000毫秒。</value>
    /// <remarks>
    /// 超时时间用于控制RPC调用的最长等待时间。
    /// 如果在指定时间内没有收到响应，调用将被取消并抛出超时异常。
    /// 设置合理的超时时间可以避免调用无限期等待。
    /// </remarks>
    public int Timeout { get; set; } = 5000;

    /// <summary>
    /// 获取或设置用于取消调用的取消令牌。
    /// </summary>
    /// <value>用于取消RPC调用操作的<see cref="CancellationToken"/>。</value>
    /// <remarks>
    /// 取消令牌允许调用方在任何时候主动取消正在进行的RPC调用。
    /// 当令牌被触发时，调用将立即停止并抛出<see cref="System.OperationCanceledException"/>。
    /// 这提供了比超时机制更精确的调用控制能力。
    /// </remarks>
    public CancellationToken Token { get; set; }
}