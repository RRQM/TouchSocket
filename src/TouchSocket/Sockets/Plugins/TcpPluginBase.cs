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
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets.Plugins
{
    /// <summary>
    /// 插件实现基类
    /// </summary>
    public abstract class TcpPluginBase : DisposableObject, ITcpPlugin
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [DependencyInject]
        public ILog Logger { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [DependencyInject]
        public IPluginsManager PluginsManager { get; set; }

        void ITcpPlugin.OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            this.OnConnecting(client, e);
        }

        void ITcpPlugin.OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            this.OnDisconnected(client, e);
        }

        void ITcpPlugin.OnIDChanged(ITcpClientBase client, TouchSocketEventAgrs e)
        {
            this.OnIDChanged(client, e);
        }

        void ITcpPlugin.OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            this.OnReceivedData(client, e);
        }

        void ITcpPlugin.OnSendingData(ITcpClientBase client, SendingEventArgs e)
        {
            this.OnSending(client, e);
        }

        void ITcpPlugin.OnConnected(ITcpClientBase client, TouchSocketEventAgrs e)
        {
            this.OnConnected(client, e);
        }

        /// <summary>
        /// 成功建立连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(ITcpClientBase client, TouchSocketEventAgrs e)
        {

        }

        /// <summary>
        /// 在请求连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {

        }

        /// <summary>
        /// 在断开连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {

        }

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnIDChanged(ITcpClientBase client, TouchSocketEventAgrs e)
        {

        }

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数，当设置e.Handled=true时，终止向下传递</param>
        protected virtual void OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {

        }

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数，当设置e.IsPermitOperation=false时，中断发送。</param>
        protected virtual void OnSending(ITcpClientBase client, SendingEventArgs e)
        {

        }
    }
}
