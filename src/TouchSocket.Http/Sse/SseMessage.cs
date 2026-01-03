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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http;

/// <summary>
/// 表示服务器发送事件(SSE)消息
/// </summary>
public class SseMessage
{
    private string m_eventType;

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get => this.m_eventType ?? "message"; init => this.m_eventType = value; }

    /// <summary>
    /// 消息数据
    /// </summary>
    public string Data { get; init; }

    /// <summary>
    /// 消息ID
    /// </summary>
    public string EventId { get; init; }

    /// <summary>
    /// 重连时间(毫秒)
    /// </summary>
    public TimeSpan? ReconnectionInterval { get; init; }

    /// <summary>
    /// 是否为注释
    /// </summary>
    public bool IsComment => !string.IsNullOrEmpty(this.Comment);

    /// <summary>
    /// 注释内容
    /// </summary>
    public string Comment { get; init; }
}
