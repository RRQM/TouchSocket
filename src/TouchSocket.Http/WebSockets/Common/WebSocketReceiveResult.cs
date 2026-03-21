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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// 表示 WebSocket 异步读取的结果。
/// </summary>
public readonly struct WebSocketReceiveResult : IDisposable
{
    private readonly WSDataFrame m_dataFrame;

    /// <summary>
    /// 初始化 <see cref="WebSocketReceiveResult"/>。
    /// </summary>
    /// <param name="dataFrame">接收到的数据帧，连接关闭时为 <see langword="null"/>。</param>
    /// <param name="message">关闭消息，仅在 <see cref="IsCompleted"/> 为 <see langword="true"/> 时有效。</param>
    /// <param name="isCompleted">是否已完成（连接已关闭）。</param>
    public WebSocketReceiveResult(WSDataFrame dataFrame, string message, bool isCompleted)
    {
        this.m_dataFrame = dataFrame;
        this.Message = message;
        this.IsCompleted = isCompleted;
    }

    /// <summary>
    /// 获取接收到的 WebSocket 数据帧。
    /// </summary>
    public WSDataFrame DataFrame => this.m_dataFrame;

    /// <summary>
    /// 获取一个值，指示连接是否已关闭。
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// 获取关闭消息。
    /// </summary>
    public string Message { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.m_dataFrame?.Dispose();
    }
}
