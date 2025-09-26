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

namespace TouchSocket.NamedPipe;

/// <summary>
/// 命名管道客户端接口
/// </summary>
public interface INamedPipeClient : INamedPipeSession, ISetupConfigObject, IConnectableClient, IClientSender, IReceiverClient<IReceiverResult>
{
    /// <summary>
    /// 成功连接到服务器
    /// </summary>
    ConnectedEventHandler<INamedPipeClient> Connected { get; set; }

    /// <summary>
    /// 准备连接的时候
    /// </summary>
    ConnectingEventHandler<INamedPipeClient> Connecting { get; set; }

    /// <summary>
    /// 获取或设置管道客户端关闭时的事件处理程序。
    /// </summary>
    /// <remarks>
    /// 此事件用于在管道客户端连接关闭后执行清理操作或其他响应措施。
    /// </remarks>
    ClosedEventHandler<INamedPipeClient> Closed { get; set; }

    /// <summary>
    /// 获取或设置管道客户端即将关闭时的事件处理程序。
    /// </summary>
    /// <remarks>
    /// 此事件用于在管道客户端连接关闭之前执行必要的资源释放或其他预关闭操作。
    /// </remarks>
    ClosingEventHandler<INamedPipeClient> Closing { get; set; }

    /// <summary>
    /// 获取或设置接收到数据时的事件处理程序。
    /// </summary>
    /// <remarks>
    /// 此事件用于在管道客户端从服务器接收到数据时进行处理，以实现数据的及时处理或存储。
    /// </remarks>
    ReceivedEventHandler<INamedPipeClient> Received { get; set; }
}