//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using RRQMCore;
using RRQMCore.Log;

namespace RRQMSocket.Plugins
{
    /// <summary>
    /// 插件实现基类
    /// </summary>
    public class TcpPluginBase: ITcpPlugin
    {
        /// <summary>
        /// 表示是否已释放
        /// </summary>
        protected bool disposedValue;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public byte Order { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// 成功建立连接
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnConnected(ITcpClientBase client, RRQMEventArgs e)
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
        protected virtual void OnIDChanged(ITcpClientBase client, RRQMEventArgs e)
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

        void ITcpPlugin.OnConnected(ITcpClientBase client, RRQMEventArgs e)
        {
            this.OnConnected(client, e);
        }

        void ITcpPlugin.OnConnecting(ITcpClientBase client, ClientOperationEventArgs e)
        {
            this.OnConnecting(client, e);
        }

        void ITcpPlugin.OnDisconnected(ITcpClientBase client, ClientDisconnectedEventArgs e)
        {
            this.OnDisconnected(client, e);
        }

        void ITcpPlugin.OnIDChanged(ITcpClientBase client, RRQMEventArgs e)
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

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Udp插件实现类
    /// </summary>
    public class UdpSessionPluginBase : IUdpSessionPlugin
    {
        /// <summary>
        /// 判断是否已释放
        /// </summary>
        protected bool disposedValue;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public byte Order { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get ; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        void IUdpSessionPlugin.OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e)
        {
            this.OnReceivedData(client,e);
        }

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnReceivedData(IUdpSession client, UdpReceivedDataEventArgs e)
        {

        }


        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~UdpSessionPluginBase()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Token系插件接口
    /// </summary>
    public class TokenPluginBase: TcpPluginBase,ITokenPlugin
    {
        /// <summary>
        /// 收到非正常连接。
        /// 一般地，这是由其他类型客户端发起的连接。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        protected virtual void OnAbnormalVerify(ITcpClientBase client, ReceivedDataEventArgs e)
        {

        }

        /// <summary>
        /// 在完成握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(ITcpClientBase client, MesEventArgs e)
        {

        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        protected virtual void OnVerifyToken(ITcpClientBase client, VerifyOptionEventArgs e)
        {

        }

        /// <summary>
        /// 处理Token收到的数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleTokenData(ITcpClientBase client, ReceivedDataEventArgs e)
        {

        }

        void ITokenPlugin.OnAbnormalVerify(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            this.OnAbnormalVerify(client, e);
        }

        void ITokenPlugin.OnHandleTokenData(ITcpClientBase client, ReceivedDataEventArgs e)
        {
            this.OnHandleTokenData(client, e);
        }

        void ITokenPlugin.OnHandshaked(ITcpClientBase client, MesEventArgs e)
        {
            this.OnHandshaked(client, e);
        }

        void ITokenPlugin.OnVerifyToken(ITcpClientBase client, VerifyOptionEventArgs e)
        {
            this.OnVerifyToken(client, e);
        }
    }

    /// <summary>
    /// Protocol系插件接口
    /// </summary>
    public class ProtocolPluginBase : TokenPluginBase, IProtocolPlugin
    {
        /// <summary>
        /// 收到协议数据
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandleProtocolData(ITcpClientBase client, ProtocolDataEventArgs e)
        {

        }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfered(ITcpClientBase client, StreamStatusEventArgs e)
        {

        }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfering(ITcpClientBase client, StreamOperationEventArgs e)
        {

        }

        void IProtocolPlugin.OnHandleProtocolData(ITcpClientBase client, ProtocolDataEventArgs e)
        {
            this.OnHandleProtocolData(client, e);
        }

        void IProtocolPlugin.OnStreamTransfered(ITcpClientBase client, StreamStatusEventArgs e)
        {
            this.OnStreamTransfered(client, e);
        }

        void IProtocolPlugin.OnStreamTransfering(ITcpClientBase client, StreamOperationEventArgs e)
        {
            this.OnStreamTransfering(client, e);
        }
    }
}
