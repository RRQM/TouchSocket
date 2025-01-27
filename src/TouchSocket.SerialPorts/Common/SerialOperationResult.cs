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

using System.IO.Ports;

namespace TouchSocket.SerialPorts;

/// <summary>
/// 串行操作结果的结构体。
/// 用于封装一次串行操作（如读取）的结果，包括传输的字节数和操作类型。
/// </summary>
public readonly struct SerialOperationResult
{
    /// <summary>
    /// 已传输的字节数。
    /// </summary>
    public readonly int BytesTransferred { get; }

    /// <summary>
    /// 串行数据事件类型。
    /// </summary>
    public SerialData EventType { get; }

    /// <summary>
    /// 初始化<see cref="SerialOperationResult"/>结构体的实例。
    /// </summary>
    /// <param name="bytesToRead">要读取的字节数。</param>
    /// <param name="eventType">串行数据事件类型。</param>
    public SerialOperationResult(int bytesToRead, SerialData eventType)
    {
        this.BytesTransferred = bytesToRead;
        this.EventType = eventType;
    }
}