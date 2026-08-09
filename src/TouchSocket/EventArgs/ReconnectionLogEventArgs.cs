//------------------------------------------------------------------------------
//  此代码版权（除特别声明外）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

namespace TouchSocket.Sockets;

/// <summary>
/// 重连日志类型。
/// </summary>
public enum ReconnectionLogType
{
    /// <summary>
    /// 重连失败。
    /// </summary>
    Failed,

    /// <summary>
    /// 达到最大次数后放弃重连。
    /// </summary>
    GiveUp,

    /// <summary>
    /// 重连成功。
    /// </summary>
    Success,
}

/// <summary>
/// 重连日志回调参数。
/// </summary>
public sealed class ReconnectionLogEventArgs
{
    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="type">日志类型。</param>
    /// <param name="attempts">当前尝试次数。</param>
    /// <param name="maxRetryCount">最大重连次数。</param>
    /// <param name="message">日志消息。</param>
    /// <param name="exception">异常信息。</param>
    public ReconnectionLogEventArgs(
        ReconnectionLogType type,
        int attempts,
        int maxRetryCount,
        string message,
        Exception? exception = null)
    {
        this.Type = type;
        this.Attempts = attempts;
        this.MaxRetryCount = maxRetryCount;
        this.Message = message;
        this.Exception = exception;
    }

    /// <summary>
    /// 当前尝试次数。
    /// </summary>
    public int Attempts { get; }

    /// <summary>
    /// 异常信息。
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// 日志消息。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 最大重连次数。
    /// </summary>
    public int MaxRetryCount { get; }

    /// <summary>
    /// 日志类型。
    /// </summary>
    public ReconnectionLogType Type { get; }
}
