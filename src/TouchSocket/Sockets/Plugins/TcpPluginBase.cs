//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;

namespace TouchSocket.Sockets.Plugins
{
    /// <summary>
    /// 插件实现基类
    /// </summary>
    public abstract class TcpPluginBase : TcpPluginBase<ITcpClientBase>
    {

    }

    /// <summary>
    /// 插件实现基类
    /// </summary>
    public abstract class TcpPluginBase<TClient> : DisposableObject, ITcpPlugin, IConfigPlugin
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Order { get; set; }

        void ITcpPlugin.OnConnected(ITcpClientBase client, TouchSocketEventArgs e)
        {
            this.OnConnected((TClient)client, e);
        }

        Task ITcpPlugin.OnConnectedAsync(ITcpClientBase client, TouchSocketEventArgs e)
        {
            return this.OnConnectedAsync((TClient)client, e);
        }

        void ITcpPlugin.OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            this.OnConnecting((TClient)client, e);
        }

        Task ITcpPlugin.OnConnectingAsync(ITcpClientBase client, ClientOperationEventArgs e)
        {
            return this.OnConnectingAsync((TClient)client, e);
        }

        void ITcpPlugin.OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            this.OnDisconnected((TClient)client, e);
        }

        Task ITcpPlugin.OnDisconnectedAsync(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            return this.OnDisconnectedAsync((TClient)client, e);
        }

        void ITcpPlugin.OnIDChanged(ITcpClientBase client, IDChangedEventArgs e)
        {
            this.OnIDChanged((TClient)client, e);
        }

        Task ITcpPlugin.OnIDChangedAsync(ITcpClientBase client, IDChangedEventArgs e)
        {
            return this.OnIDChangedAsync((TClient)client, e);
        }

        void IConfigPlugin.OnLoadedConfig(object sender, ConfigEventArgs e)
        {
            this.OnLoadedConfig(sender, e);
        }

        Task IConfigPlugin.OnLoadedConfigAsync(object sender, ConfigEventArgs e)
        {
            return this.OnLoadedConfigAsync(sender, e);
        }

        void IConfigPlugin.OnLoadingConfig(object sender, ConfigEventArgs e)
        {
            this.OnLoadingConfig(sender, e);
        }

        Task IConfigPlugin.OnLoadingConfigAsync(object sender, ConfigEventArgs e)
        {
            return this.OnLoadingConfigAsync(sender, e);
        }

        void ITcpPlugin.OnReceivedData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            this.OnReceivedData((TClient)client, e);
        }

        Task ITcpPlugin.OnReceivedDataAsync(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            return this.OnReceivedDataAsync((TClient)client, e);
        }

        void ITcpPlugin.OnReceivingData(ITcpClientBase client, ByteBlockEventArgs e)
        {
            this.OnReceivingData((TClient)client, e);
        }

        Task ITcpPlugin.OnReceivingDataAsync(ITcpClientBase client, ByteBlockEventArgs e)
        {
            return this.OnReceivingDataAsync((TClient)client, e);
        }

        void ITcpPlugin.OnSendingData(ITcpClientBase client, SendingEventArgs e)
        {
            this.OnSending((TClient)client, e);
        }

        Task ITcpPlugin.OnSendingDataAsync(ITcpClientBase client, SendingEventArgs e)
        {
            return this.OnSendingDataAsync((TClient)client, e);
        }

        #region 虚函数实现

        /// <summary>
        /// 成功建立连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(TClient client, TouchSocketEventArgs e)
        {
        }

        /// <summary>
        /// 客户端连接成功后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnConnectedAsync(TClient client, TouchSocketEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在请求连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnecting(TClient client, ClientOperationEventArgs e)
        {
        }

        /// <summary>
        /// 在即将完成连接时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnConnectingAsync(TClient client, ClientOperationEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在断开连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(TClient client, ClientDisconnectedEventArgs e)
        {
        }

        /// <summary>
        /// 会话断开后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnDisconnectedAsync(TClient client, ClientDisconnectedEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnIDChanged(TClient client, TouchSocketEventArgs e)
        {
        }

        /// <summary>
        /// 当Client的ID被更改后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnIDChangedAsync(TClient client, TouchSocketEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 当载入配置时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnLoadedConfig(object sender, ConfigEventArgs e)
        {
        }

        /// <summary>
        /// 当完成配置载入时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnLoadedConfigAsync(object sender, ConfigEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 当完成配置载入时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnLoadingConfig(object sender, ConfigEventArgs e)
        {
        }

        /// <summary>
        /// 当载入配置时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnLoadingConfigAsync(object sender, ConfigEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数，当设置e.Handled=true时，终止向下传递</param>
        protected virtual void OnReceivedData(TClient client, ReceivedDataEventArgs e)
        {
        }

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnReceivedDataAsync(TClient client, ReceivedDataEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 在刚收到数据时触发，即在适配器之前。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        protected virtual void OnReceivingData(TClient client, ByteBlockEventArgs e)
        {
        }

        /// <summary>
        /// 在刚收到数据时触发，即在适配器之前。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnReceivingDataAsync(TClient client, ByteBlockEventArgs e)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数，当设置e.IsPermitOperation=false时，中断发送。</param>
        protected virtual void OnSending(TClient client, SendingEventArgs e)
        {
        }

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual Task OnSendingDataAsync(TClient client, SendingEventArgs e)
        {
            return Task.FromResult(0);
        }

        #endregion 虚函数实现
    }
}