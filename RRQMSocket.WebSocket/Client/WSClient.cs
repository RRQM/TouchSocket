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
using RRQMCore.ByteManager;
using RRQMSocket.Http;
using System.Text;
using System.Threading;

namespace RRQMSocket.WebSocket
{

    /// <summary>
    /// WebSocket用户终端简单实现。
    /// </summary>
    public class WSClient : WSClientBase
    {
        /// <summary>
        /// 收到WebSocket数据
        /// </summary>
        public event WSDataFrameEventHandler<WSClient> Received;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dataFrame"></param>
        protected override void OnHandleWSDataFrame(WSDataFrame dataFrame)
        {
            this.Received?.Invoke(this, dataFrame);
            base.OnHandleWSDataFrame(dataFrame);
        }
    }

    /// <summary>
    /// WebSocket用户终端。
    /// </summary>
    public class WSClientBase : HttpClientBase
    {
       

        /// <summary>
        /// 请求连接到WebSocket。
        /// </summary>
        /// <returns></returns>
        public override ITcpClient Connect()
        {
            return this.Connect(default);
        }

        /// <summary>
        /// 请求连接到WebSocket。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual ITcpClient Connect(CancellationToken token)
        {
            lock (this)
            {
                if (!this.Online)
                {
                    base.Connect();
                }

                string base64Key;
                IPHost iPHost = this.Config.GetValue<IPHost>(RRQMConfigExtensions.RemoteIPHostProperty);
                string url = iPHost.IsUri ? iPHost.Uri.PathAndQuery : string.Empty;
                HttpRequest request = WSTools.GetWSRequest(this.RemoteIPHost.ToString(), url, this.GetWebSocketVersion(), out base64Key);
                  
                this.OnHandshaking(new HttpContextEventArgs(request));   

                var response = this.Request(request, token: token);
                if (response.GetHeader("Sec-WebSocket-Accept") != WSTools.CalculateBase64Key(base64Key, Encoding.UTF8))
                {
                    this.MainSocket.Dispose();
                    throw new RRQMException("返回的应答码不正确。");
                }

                this.SetAdapter(new WebSocketDataHandlingAdapter() { MaxPackageSize=this.MaxPackageSize});
                this.SetValue(WebSocketServerPlugin.HandshakedProperty,true);
                response.Flag = true;
                this.OnHandshaked(new HttpContextEventArgs(request) { Response=response});
                return this;
            }
        }

        #region 事件

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        public event HttpContextEventHandler<WSClientBase> Handshaking;

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        public event HttpContextEventHandler<WSClientBase> Handshaked;

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaking(HttpContextEventArgs e)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IWebSocketPlugin>("OnHandshaking", this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.Handshaking?.Invoke(this,e);
        }

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(HttpContextEventArgs e)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IWebSocketPlugin>("OnHandshaking", this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            this.Handshaked?.Invoke(this,e);
        }

        #endregion 事件

        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="dataFrame"></param>
        protected virtual void OnHandleWSDataFrame(WSDataFrame dataFrame)
        {
            if (this.UsePlugin)
            {
                this.PluginsManager.Raise<IWebSocketPlugin>("OnHandleWSDataFrame",this,new WSDataFrameEventArgs(dataFrame));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.GetHandshaked())
            {
                WSDataFrame dataFrame = (WSDataFrame)requestInfo;
                this.OnHandleWSDataFrame(dataFrame);
            }
            else
            {
                if (requestInfo is HttpResponse response)
                {
                    response.Flag = false;
                    base.HandleReceivedData(byteBlock, requestInfo);
                    SpinWait.SpinUntil(() =>
                    {
                        return (bool)response.Flag;
                    }, 1000);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            this.SetValue(WebSocketServerPlugin.HandshakedProperty,false);
            base.OnDisconnected(e);
        }
    }
}