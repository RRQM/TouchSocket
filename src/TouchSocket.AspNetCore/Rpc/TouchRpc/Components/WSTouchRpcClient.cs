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
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Log;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc.AspNetCore
{
    /// <summary>
    /// WSTouchRpcClient
    /// </summary>
    public class WSTouchRpcClient : DisposableObject, IWSTouchRpcClient, IRpcActor
    {
        private readonly byte[] m_buffer = new byte[1024 * 64];
        private ClientWebSocket m_client;
        private TouchSocketConfig m_config;
        private int m_failCount;
        private IPHost m_remoteIPHost;
        private RpcActor m_rpcActor;
        private Timer m_timer;

        /// <summary>
        /// 创建一个WSTouchRpcClient实例。
        /// </summary>
        public WSTouchRpcClient()
        {
            this.m_rpcActor = new RpcActor(false)
            {
                OutputSend = this.RpcActorSend,
                OnHandshaked = this.OnRpcActorHandshaked,
                OnReceived = this.OnRpcActorReceived,
                OnClose = this.OnRpcServiceClose,
                OnStreamTransfering = this.OnRpcActorStreamTransfering,
                OnStreamTransfered = this.OnRpcActorStreamTransfered,
                OnFileTransfering = this.OnRpcActorFileTransfering,
                OnFileTransfered = this.OnRpcActorFileTransfered,
                Caller = this
            };
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public event ClientDisconnectedEventHandler<WSTouchRpcClient> Disconnected;

        /// <summary>
        /// 客户端配置
        /// </summary>
        public TouchSocketConfig Config => this.m_config;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ID => this.m_rpcActor.ID;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsHandshaked => this.m_rpcActor.IsHandshaked;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ILog Logger => this.m_rpcActor.Logger;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPHost RemoteIPHost => this.m_remoteIPHost;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public ResponseType ResponseType { get => this.m_rpcActor.ResponseType; set => this.m_rpcActor.ResponseType = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string RootPath { get => this.m_rpcActor.RootPath; set => this.m_rpcActor.RootPath = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcActor RpcActor => this.m_rpcActor;

        /// <summary>
        /// RpcStore
        /// </summary>
        public RpcStore RpcStore { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public SerializationSelector SerializationSelector => this.m_rpcActor.SerializationSelector;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get => this.m_rpcActor.TryCanInvoke; set => this.m_rpcActor.TryCanInvoke = value; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UsePlugin { get; private set; }

        #region 插件

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <typeparam name="TPlugin">插件类型</typeparam>
        /// <returns>插件类型实例</returns>
        public TPlugin AddPlugin<TPlugin>() where TPlugin : IPlugin
        {
            var plugin = this.Container.Resolve<TPlugin>();
            this.AddPlugin(plugin);
            return plugin;
        }

        /// <summary>
        /// 添加插件
        /// </summary>
        /// <param name="plugin">插件</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddPlugin(IPlugin plugin)
        {
            this.PluginsManager.Add(plugin);
        }

        /// <summary>
        /// 清空插件
        /// </summary>
        public void ClearPlugins()
        {
            this.PluginsManager.Clear();
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <param name="plugin"></param>
        public void RemovePlugin(IPlugin plugin)
        {
            this.PluginsManager.Remove(plugin);
        }

        /// <summary>
        /// 移除插件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemovePlugin<T>() where T : IPlugin
        {
            this.PluginsManager.Remove(typeof(T));
        }

        #endregion 插件

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ChannelExisted(int id)
        {
            return this.m_rpcActor.ChannelExisted(id);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="msg"></param>
        public void Close(string msg)
        {
            if (this.m_client?.State != WebSocketState.Open)
            {
                return;
            }
            this.m_client.SafeDispose();
            this.m_rpcActor.SafeDispose();
            this.BreakOut($"调用{nameof(Close)}", true);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        public async Task ConnectAsync(Metadata metadata = null, CancellationToken token = default, int timeout = 5000)
        {
            if (!this.RemoteIPHost.IsUri)
            {
                throw new Exception("RemoteIPHost必须为Uri格式。");
            }
            if (this.m_client == null || this.m_client.State != WebSocketState.Open)
            {
                this.m_client.SafeDispose();
                this.m_client = new ClientWebSocket();
                await this.m_client.ConnectAsync(this.RemoteIPHost.Uri, token);
                this.BeginReceive(null);
            }

            if (this.IsHandshaked)
            {
                return;
            }

            this.m_rpcActor.Handshake(this.Config.GetValue<string>(TouchRpcConfigExtensions.VerifyTokenProperty), token, timeout, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public Channel CreateChannel()
        {
            return this.m_rpcActor.CreateChannel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel CreateChannel(int id)
        {
            return this.m_rpcActor.CreateChannel(id);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public Channel CreateChannel(string targetID)
        {
            return this.m_rpcActor.CreateChannel(targetID);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel CreateChannel(string targetID, int id)
        {
            return this.m_rpcActor.CreateChannel(targetID, id);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Ping(int timeout = 5000)
        {
            return this.m_rpcActor.Ping(timeout);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="newID"></param>
        /// <param name="cancellationToken"></param>
        public void ResetID(string newID, CancellationToken cancellationToken = default)
        {
            this.m_rpcActor.ResetID(newID, cancellationToken);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public void Send(short protocol, byte[] buffer)
        {
            this.m_rpcActor.Send(protocol, buffer);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(short protocol, byte[] buffer, int offset, int length)
        {
            this.m_rpcActor.Send(protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        public void Send(short protocol, ByteBlock dataByteBlock)
        {
            this.m_rpcActor.Send(protocol, dataByteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        public void Send(short protocol)
        {
            this.m_rpcActor.Send(protocol);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public void SendAsync(short protocol, byte[] buffer)
        {
            this.m_rpcActor.SendAsync(protocol, buffer);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendAsync(short protocol, byte[] buffer, int offset, int length)
        {
            this.m_rpcActor.SendAsync(protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        public void SendAsync(short protocol, ByteBlock dataByteBlock)
        {
            this.m_rpcActor.SendAsync(protocol, dataByteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        public void SendAsync(short protocol)
        {
            this.m_rpcActor.SendAsync(protocol);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.SendStream(stream, streamOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.SendStreamAsync(stream, streamOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="ipHost"></param>
        /// <returns></returns>
        public IWSTouchRpcClient Setup(string ipHost)
        {
            TouchSocketConfig config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(ipHost));
            return this.Setup(config);
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="clientConfig"></param>
        /// <exception cref="Exception"></exception>
        public IWSTouchRpcClient Setup(TouchSocketConfig clientConfig)
        {
            this.m_config = clientConfig;
            this.LoadConfig(this.m_config);
            return this;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool TrySubscribeChannel(int id, out Channel channel)
        {
            return this.m_rpcActor.TrySubscribeChannel(id, out channel);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_client.SafeDispose();
            this.m_rpcActor.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected virtual void LoadConfig(TouchSocketConfig config)
        {
            if (config == null)
            {
                throw new Exception("配置文件为空");
            }
            this.m_remoteIPHost = config.GetValue<IPHost>(Sockets.TouchSocketConfigExtension.RemoteIPHostProperty);
            this.Container = config.Container;
            this.UsePlugin = config.IsUsePlugin;
            this.PluginsManager = config.PluginsManager;

            this.m_rpcActor.Logger = this.Container.Resolve<ILog>();
            this.RootPath = this.Config.GetValue<string>(TouchRpcConfigExtensions.RootPathProperty);
            this.ResponseType = this.Config.GetValue<ResponseType>(TouchRpcConfigExtensions.ResponseTypeProperty);
            this.m_rpcActor.SerializationSelector = this.Config.GetValue<SerializationSelector>(TouchRpcConfigExtensions.SerializationSelectorProperty);
        }

        /// <summary>
        /// 已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(ClientDisconnectedEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITcpPlugin>("OnDisconnected", this, e))
            {
                return;
            }
            this.Disconnected?.Invoke(this, e);
        }

        private async void BeginReceive(ByteBlock byteBlock)
        {
            try
            {
                if (byteBlock == null)
                {
                    byteBlock = new ByteBlock();
                }
                var result = await this.m_client.ReceiveAsync(this.m_buffer, default);
                if (result.Count == 0)
                {
                    this.BreakOut("远程终端主动关闭", false);
                }
                byteBlock.Write(this.m_buffer, 0, result.Count);
                if (result.EndOfMessage)
                {
                    try
                    {
                        this.m_rpcActor.InputReceivedData(byteBlock);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        byteBlock.SafeDispose();
                    }
                    this.BeginReceive(null);
                }
                else
                {
                    this.BeginReceive(byteBlock);
                }
            }
            catch (System.Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        private void BreakOut(string msg, bool manual)
        {
            this.m_client.SafeDispose();
            this.m_rpcActor.SafeDispose();
            this.OnDisconnected(new ClientDisconnectedEventArgs(manual, msg));
        }

        #region FileTransfer

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result PullFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PullFile(fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result PullFile(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PullFile(targetID, fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> PullFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PullFileAsync(fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> PullFileAsync(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PullFileAsync(targetID, fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result PushFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PushFile(fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result PushFile(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PushFile(targetID, fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> PushFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PushFileAsync(fileRequest, fileOperator, metadata);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="fileRequest"></param>
        /// <param name="fileOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> PushFileAsync(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata = null)
        {
            return this.m_rpcActor.PushFileAsync(targetID, fileRequest, fileOperator, metadata);
        }

        #endregion FileTransfer

        #region RPC

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.m_rpcActor.Invoke(method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_rpcActor.Invoke<T>(method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T Invoke<T>(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            return this.m_rpcActor.Invoke<T>(method, invokeOption, ref parameters, types);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <param name="types"></param>
        public void Invoke(string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            this.m_rpcActor.Invoke(method, invokeOption, ref parameters, types);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void Invoke(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.m_rpcActor.Invoke(targetID, method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T Invoke<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_rpcActor.Invoke<T>(targetID, method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <param name="types"></param>
        public void Invoke(string targetID, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            this.m_rpcActor.Invoke(targetID, method, invokeOption, ref parameters, types);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public T Invoke<T>(string targetID, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            return this.m_rpcActor.Invoke<T>(targetID, method, invokeOption, ref parameters, types);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task InvokeAsync(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_rpcActor.InvokeAsync(method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<T> InvokeAsync<T>(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_rpcActor.InvokeAsync<T>(method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task InvokeAsync(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_rpcActor.InvokeAsync(targetID, method, invokeOption, parameters);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetID"></param>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Task<T> InvokeAsync<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            return this.m_rpcActor.InvokeAsync<T>(targetID, method, invokeOption, parameters);
        }

        #endregion RPC

        #region 内部委托绑定

        private void OnRpcActorFileTransfered(RpcActor actor, FileTransferStatusEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnFileTransfered", this, e))
            {
                return;
            }
            this.OnFileTransfered(e);
        }

        private void OnRpcActorFileTransfering(RpcActor actor, FileOperationEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnFileTransfering", this, e))
            {
                return;
            }
            this.OnFileTransfering(e);
        }

        private void OnRpcActorHandshaked(RpcActor actor, VerifyOptionEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnHandshaked", this, e))
            {
                return;
            }
            this.OnHandshaked(e);
        }

        private void OnRpcActorReceived(RpcActor actor, short protocol, ByteBlock byteBlock)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnReceivedProtocolData", this, new ProtocolDataEventArgs(protocol, byteBlock)))
            {
                return;
            }

            this.OnReceived(protocol, byteBlock);
        }

        private void OnRpcActorStreamTransfered(RpcActor actor, StreamStatusEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnStreamTransfered", this, e))
            {
                return;
            }
            this.OnStreamTransfered(e);
        }

        private void OnRpcActorStreamTransfering(RpcActor actor, StreamOperationEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnStreamTransfering", this, e))
            {
                return;
            }
            this.OnStreamTransfering(e);
        }

        private void OnRpcServiceClose(RpcActor actor, string arg2)
        {
            this.Close(arg2);
        }

        private async void RpcActorSend(RpcActor actor, bool isAsync, TransferByte[] transferBytes)
        {
            using ByteBlock byteBlock = new ByteBlock();
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Buffer, item.Offset, item.Length);
            }
            await this.m_client.SendAsync(byteBlock.Buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        #endregion 内部委托绑定

        #region 事件

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        public event TransferFileEventHandler<WSTouchRpcClient> FileTransfered;

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        public event FileOperationEventHandler<WSTouchRpcClient> FileTransfering;

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public event VerifyOptionEventHandler<WSTouchRpcClient> Handshaked;

        /// <summary>
        /// 接收到数据
        /// </summary>
        public event ProtocolReceivedEventHandler<WSTouchRpcClient> Received;

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        public event StreamStatusEventHandler<WSTouchRpcClient> StreamTransfered;

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        public event StreamOperationEventHandler<WSTouchRpcClient> StreamTransfering;

        #endregion 事件

        #region 事件触发

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileTransfered(FileTransferStatusEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnFileTransfered", this, e))
            {
                return;
            }
            try
            {
                this.FileTransfered?.Invoke(this, e);
            }
            catch (System.Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(FileTransfered)}中发生异常", ex);
            }
        }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileTransfering(FileOperationEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnFileTransfering", this, e))
            {
                return;
            }
            try
            {
                this.FileTransfering?.Invoke(this, e);
            }
            catch (System.Exception ex)
            {
                this.Logger.Debug(LogType.Error, this, $"在事件{nameof(FileTransfering)}中发生异常", ex);
            }
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(VerifyOptionEventArgs e)
        {
            this.m_timer.SafeDispose();

            if (this.Config.GetValue<HeartbeatValue>(TouchRpcConfigExtensions.HeartbeatFrequencyProperty) is HeartbeatValue heartbeat)
            {
                this.m_timer = new Timer((obj) =>
                {
                    if (this.Ping())
                    {
                        this.m_failCount = 0;
                    }
                    else
                    {
                        if (++this.m_failCount > heartbeat.MaxFailCount)
                        {
                            this.Close("自动心跳失败次数达到最大，已清理连接。");
                            this.m_timer.SafeDispose();
                        }
                    }
                }, null, heartbeat.Interval, heartbeat.Interval);
            }

            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnHandshaked", this, e))
            {
                return;
            }

            this.Handshaked?.Invoke(this, e);
        }

        /// <summary>
        /// 收到数据。
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void OnReceived(short protocol, ByteBlock byteBlock)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnReceivedProtocolData", this, new ProtocolDataEventArgs(protocol, byteBlock)))
            {
                return;
            }
            this.Received?.Invoke(this, protocol, byteBlock);
        }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfered(StreamStatusEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnStreamTransfered", this, e))
            {
                return;
            }
            this.StreamTransfered?.Invoke(this, e);
        }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfering(StreamOperationEventArgs e)
        {
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>("OnStreamTransfering", this, e))
            {
                return;
            }
            this.StreamTransfering?.Invoke(this, e);
        }

        #endregion 事件触发

        #region RPC解析器

        void IRpcParser.OnRegisterServer(IRpcServer provider, MethodInstance[] methodInstances)
        {
        }

        void IRpcParser.OnUnregisterServer(IRpcServer provider, MethodInstance[] methodInstances)
        {
        }

        void IRpcParser.SetRpcStore(RpcStore rpcStore)
        {
            this.RpcStore = rpcStore;
        }

        #endregion RPC解析器
    }
}