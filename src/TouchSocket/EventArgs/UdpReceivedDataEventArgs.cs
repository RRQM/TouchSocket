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

using System.Net;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// UdpReceivedDataEventArgs 类，继承自 ReceivedDataEventArgs 类
/// 用于封装 UDP 接收到的数据及相关信息
/// </summary>
public class UdpReceivedDataEventArgs : ReceivedDataEventArgs
{
    /// <summary>
    /// 构造函数
    /// 初始化 UdpReceivedDataEventArgs 对象
    /// </summary>
    /// <param name="endPoint">接收数据的终结点</param>
    /// <param name="byteBlock">接收到的数据块</param>
    /// <param name="requestInfo">请求信息，提供关于此次接收请求的元数据</param>
    public UdpReceivedDataEventArgs(EndPoint endPoint, ByteBlock byteBlock, IRequestInfo requestInfo) : base(byteBlock, requestInfo)
    {
        this.EndPoint = endPoint;
    }

    /// <summary>
    /// 接收终结点
    /// 表示数据是从哪个终结点接收的
    /// </summary>
    public EndPoint EndPoint { get; }
}