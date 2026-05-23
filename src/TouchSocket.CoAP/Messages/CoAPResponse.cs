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

namespace TouchSocket.CoAP;

/// <summary>
/// CoAP 响应消息，实现 <see cref="IWaitHandle"/> 以支持 CON 消息的请求-响应匹配。
/// </summary>
public sealed class CoAPResponse : CoAPMessage, IWaitHandle
{
    /// <inheritdoc/>
    public int Sign
    {
        get => (int)this.MessageId;
        set => this.MessageId = (ushort)value;
    }

    /// <summary>
    /// 获取或设置响应码。
    /// </summary>
    public CoAPResponseCode ResponseCode
    {
        get => (CoAPResponseCode)this.Code;
        set => this.Code = (byte)value;
    }

    /// <summary>
    /// 获取一个值，指示该响应是否为成功响应（2.xx）。
    /// </summary>
    public bool IsSuccess => this.CodeClass == 2;
}
