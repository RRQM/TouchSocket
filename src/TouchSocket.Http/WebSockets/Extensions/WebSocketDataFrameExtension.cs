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
/// WebSocket数据帧扩展类
/// </summary>
public static class WebSocketDataFrameExtension
{
    public static readonly ReadOnlyMemory<byte> DefaultMaskingKey = "RRQM".ToUtf8Bytes();

    /// <summary>
    /// 当数据类型为<see cref="WSDataType.Text"/>时，将数据帧转换为文本消息。
    /// </summary>
    /// <param name="dataFrame">要转换的数据帧。</param>
    /// <param name="encoding">使用的编码方式。如果未指定（默认值），将使用UTF8编码。</param>
    /// <returns>转换后的文本消息。</returns>
    public static string ToText(this WSDataFrame dataFrame, Encoding encoding = default)
    {
        // 根据指定的编码方式或默认的UTF8编码，将数据帧的负载数据转换为字符串
        return dataFrame.PayloadData.Span.ToString(encoding == default ? Encoding.UTF8 : encoding);
    }
}