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
    /// WSTouchRpcSocketClient
    /// </summary>
    public class WSTouchRpcSocketClient : DisposableObject, IRpcActor, IPluginObject
    {
        internal ClientDisconnectedEventHandler<WSTouchRpcSocketClient> m_internalDisconnected;
        internal string m_id;
        internal RpcActor m_rpcActor;
        internal WSTouchRpcService m_service;
        internal bool m_usePlugin;
        private readonly byte[] m_buffer = new byte[1024 * 64];
        private WebSocket m_client;

        /// <summary>
        /// 配置
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container => this.Config?.Container;

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
        public IPluginsManager PluginsManager => this.Config?.PluginsManager;

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
        public SerializationSelector SerializationSelector => this.m_rpcActor.SerializationSelector;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get => this.m_rpcActor.TryCanInvoke; set => this.m_rpcActor.TryCanInvoke = value; }

        /// <summary>
        /// 是否已启动插件
        /// </summary>
        public bool UsePlugin => this.m_usePlugin;

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
        /// 关闭通信
        /// </summary>
        /// <param name="msg"></param>
        public void Close(string msg)
        {
            this.BreakOut(msg, true);
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
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool TrySubscribeChannel(int id, out Channel channel)
        {
            return this.m_rpcActor.TrySubscribeChannel(id, out channel);
        }

        internal async void RpcActorSend(bool isAsync, ArraySegment<byte>[] transferBytes)
        {
            using ByteBlock byteBlock = new ByteBlock();
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Array, item.Offset, item.Count);
            }
            await this.m_client.SendAsync(byteBlock.Buffer, System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        internal Task Start(TouchSocketConfig config, System.Net.WebSockets.WebSocket webSocket)
        {
            this.Config = config;
            this.m_client = webSocket;
            return this.BeginReceive();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.m_disposedValue)
            {
                return;
            }
            base.Dispose(disposing);
            this.BreakOut($"{nameof(Dispose)}断开链接", true);
        }

        private async Task BeginReceive()
        {
            ByteBlock byteBlock = null;
            try
            {
                while (true)
                {
                    var result = await this.m_client.ReceiveAsync(this.m_buffer, default);
                    if (result.Count == 0)
                    {
                        byteBlock.SafeDispose();
                        break;
                    }
                    if (byteBlock == null)
                    {
                        byteBlock = new ByteBlock();
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
                            byteBlock = null;
                        }
                    }
                }

                this.BreakOut("远程终端主动关闭", false);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this)
            {
                if (this.m_disposedValue)
                {
                    return;
                }
                this.Dispose();
                this.m_client.SafeDispose();
                this.m_rpcActor.SafeDispose();
                this.m_internalDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(manual, msg));
            }
        }

        private RpcActor FindRpcActor(string id)
        {
            if (this.m_service.TryGetSocketClient(id, out WSTouchRpcSocketClient socketClient))
            {
                return socketClient.m_rpcActor;
            }
            return null;
        }
    }
}