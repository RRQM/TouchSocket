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
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 响应数据。
/// </summary>
public readonly struct ResponsedData
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="byteBlock">响应的数据</param>
    /// <param name="requestInfo">请求信息</param>
    public ResponsedData(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        this.ByteBlock = byteBlock;
        this.RequestInfo = requestInfo;
    }

    /// <summary>
    /// 数据
    /// </summary>
    [Obsolete($"使用此属性可能带来不必要的性能消耗，请使用{nameof(ByteBlock)}代替")]
    public byte[] Data => this.ByteBlock?.ToArray();

    /// <summary>
    /// ByteBlock
    /// </summary>
    public ByteBlock ByteBlock { get; }

    /// <summary>
    /// RequestInfo
    /// </summary>
    public IRequestInfo RequestInfo { get; }
}