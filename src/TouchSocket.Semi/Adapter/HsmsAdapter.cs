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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Semi;

/// <summary>
/// HSMS 消息适配器，用于将 TCP 字节流解析为 <see cref="HsmsMessage"/>。
/// </summary>
public class HsmsAdapter : CustomDataHandlingAdapter<HsmsMessage>
{
    /// <inheritdoc/>
    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref HsmsMessage request)
    {
        if (reader.BytesRemaining < 4)
        {
            return FilterResult.Cache;
        }

        var bodyLength = (int)TouchSocketBitConverter.BigEndian.To<uint>(reader.GetSpan(4));

        if (reader.BytesRemaining < 4 + bodyLength)
        {
            return FilterResult.Cache;
        }

        reader.Advance(4);

        var message = new HsmsMessage();
        message.Unpackage(ref reader);
        request = message;
        return FilterResult.Success;
    }
}
