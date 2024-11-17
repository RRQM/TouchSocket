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

using System;

namespace TouchSocket.Dmtp
{
    /// <inheritdoc/>
    [Obsolete($"此接口由于表述不清，已被弃用，请使用{nameof(TcpDmtpSessionClient)}代替。", true)]
    public class TcpDmtpSocketClient
    {
    }

    /// <inheritdoc/>
    [Obsolete($"此接口由于表述不清，已被弃用，请使用{nameof(ITcpDmtpSessionClient)}代替。", true)]
    public interface ITcpDmtpSocketClient
    {
    }

    /// <inheritdoc/>
    [Obsolete($"此接口由于表述不清，已被弃用，请使用{nameof(HttpDmtpSessionClient)}代替。", true)]
    public class HttpDmtpSocketClient
    {
    }

    /// <inheritdoc/>
    [Obsolete($"此接口由于表述不清，已被弃用，请使用{nameof(IHttpDmtpSessionClient)}代替。", true)]
    public interface IHttpDmtpSocketClient
    {
    }

    /// <inheritdoc/>
    [Obsolete($"此接口由于表述不清，已被弃用，请使用{nameof(IDmtpCreatedChannelPlugin)}代替。", true)]
    public interface IDmtpCreateChannelPlugin
    {
    }
}