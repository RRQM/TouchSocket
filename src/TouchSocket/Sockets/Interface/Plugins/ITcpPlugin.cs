//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp系插件接口
    /// </summary>
    public interface ITcpPlugin : IPlugin
    {
        /// <summary>
        /// 客户端连接成功后触发    
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnConnected(ITcpClientBase client, TouchSocketEventAgrs e);

        /// <summary>
        ///在即将完成连接时触发。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e);

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e);

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e);

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        void OnSendingData(ITcpClientBase client, SendingEventArgs e);

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void OnIDChanged(ITcpClientBase client, TouchSocketEventAgrs e);
    }
}
