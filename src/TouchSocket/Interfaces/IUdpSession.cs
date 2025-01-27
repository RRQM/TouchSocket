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

namespace TouchSocket.Sockets;

/// <summary>
/// 定义一个UDP会话接口，该接口继承自多个与UDP通信相关的接口。
/// 整合了UDP会话的基础功能、客户端发送功能、UDP客户端发送特性和接收客户端特性。
/// </summary>
public interface IUdpSession : IUdpSessionBase, IClientSender, IUdpClientSender, IReceiverClient<IUdpReceiverResult>
{
    /// <summary>
    /// 数据处理适配器
    /// </summary>
    UdpDataHandlingAdapter DataHandlingAdapter { get; }

    /// <summary>
    /// 收到UDP数据包时触发的事件处理程序
    /// </summary>
    /// <remarks>
    /// 该属性用于处理接收到的UDP数据包。当有数据包到达时，会调用此事件处理程序。
    /// 实现 <see cref="IUdpSession"/> 接口的实例将提供处理收到的数据包的方法。
    /// </remarks>
    UdpReceivedEventHandler<IUdpSession> Received { get; set; }
}