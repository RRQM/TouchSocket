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

using System.IO.Pipelines;

namespace TouchSocket.Sockets;

/// <summary>
/// 表示传输相关的配置选项。
/// </summary>
public class TransportOption
{
    /// <summary>
    /// 初始化 <see cref="TransportOption"/> 类的新实例。
    /// </summary>
    public TransportOption()
    {
        this.ReceivePipeOptions = GetDefaultReceive();
        this.SendPipeOptions = GetDefaultSend();
    }

    /// <summary>
    /// 获取或设置最大缓冲区大小（字节）。
    /// </summary>
    public int MaxBufferSize { get; set; } = 1024 * 1024 * 10;

    /// <summary>
    /// 获取或设置最小缓冲区大小（字节）。
    /// </summary>
    public int MinBufferSize { get; set; } = 1024;

    /// <summary>
    /// 获取或设置接收管道的选项。
    /// </summary>
    public PipeOptions ReceivePipeOptions { get; set; }

    /// <summary>
    /// 获取或设置发送管道的选项。
    /// </summary>
    public PipeOptions SendPipeOptions { get; set; }

    private static PipeOptions GetDefaultReceive()
    {
        return new PipeOptions(
                pool: null,
                readerScheduler: null,
                writerScheduler: null,
                pauseWriterThreshold: -1,
                resumeWriterThreshold: -1,
                minimumSegmentSize: -1,
                useSynchronizationContext: true);
    }

    private static PipeOptions GetDefaultSend()
    {
        return new PipeOptions(
                pool: null,
                readerScheduler: null,
                writerScheduler: null,
                pauseWriterThreshold: -1,
                resumeWriterThreshold: -1,
                minimumSegmentSize: -1,
                useSynchronizationContext: true);
    }
}