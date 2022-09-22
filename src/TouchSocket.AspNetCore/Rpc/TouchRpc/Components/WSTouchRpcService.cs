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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Plugins;
using TouchSocket.Core.Serialization;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc.AspNetCore
{
    /// <summary>
    /// WSTouchRpcService
    /// </summary>
    public class WSTouchRpcService : DisposableObject, IWSTouchRpcService
    {
        #region SocketClient

        private readonly ConcurrentDictionary<string, WSTouchRpcSocketClient> m_tokenDic = new ConcurrentDictionary<string, WSTouchRpcSocketClient>();

        /// <summary>
        /// 数量
        /// </summary>
        public int Count => this.m_tokenDic.Count;

        /// <summary>
        /// 获取SocketClient
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WSTouchRpcSocketClient this[string id]
        {
            get
            {
                WSTouchRpcSocketClient t;
                this.TryGetSocketClient(id, out t);
                return t;
            }
        }

        /// <summary>
        /// 获取所有的客户端
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WSTouchRpcSocketClient> GetClients()
        {
            return this.m_tokenDic.Values;
        }

        /// <summary>
        /// 获取ID集合
        /// </summary>
        /// <returns></returns>
        public string[] GetIDs()
        {
            return this.m_tokenDic.Keys.ToArray();
        }

        /// <summary>
        /// 根据ID判断SocketClient是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SocketClientExist(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (this.m_tokenDic.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试获取实例
        /// </summary>
        /// <param name="id"></param>
        /// <param name="socketClient"></param>
        /// <returns></returns>
        public bool TryGetSocketClient(string id, out WSTouchRpcSocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }

            return this.m_tokenDic.TryGetValue(id, out socketClient);
        }

        internal bool TryAdd(string id, WSTouchRpcSocketClient socketClient)
        {
            return this.m_tokenDic.TryAdd(id, socketClient);
        }

        internal bool TryRemove(string id, out WSTouchRpcSocketClient socketClient)
        {
            if (string.IsNullOrEmpty(id))
            {
                socketClient = null;
                return false;
            }
            return this.m_tokenDic.TryRemove(id, out socketClient);
        }

        #endregion SocketClient

        private readonly ActionMap m_actionMap;
        private readonly SnowflakeIDGenerator m_iDGenerator;
        private readonly RpcActorGroup m_rpcActorGroup;
        private RpcStore m_rpcStore;

        /// <summary>
        /// 创建一个基于WS的Touch服务器。
        /// </summary>
        /// <param name="config"></param>
        public WSTouchRpcService(TouchSocketConfig config)
        {
            this.m_actionMap = new ActionMap();
            this.m_iDGenerator = new SnowflakeIDGenerator(4);
            this.Config = config;
            this.UsePlugin = config.IsUsePlugin;
            this.PluginsManager = config.PluginsManager;
            this.VerifyToken = config.GetValue<string>(TouchRpcConfigExtensions.VerifyTokenProperty);

            if (config.GetValue<Func<string>>(Sockets.TouchSocketConfigExtension.GetDefaultNewIDProperty) is Func<string> fun)
            {
                this.GetDefaultNewID = fun;
            }
            else
            {
                this.GetDefaultNewID = () => { return this.m_iDGenerator.NextID().ToString(); };
            }

            this.m_rpcActorGroup = new RpcActorGroup
            {
                Config = this.Config,
                OnClose = this.OnRpcServiceClose,
                OnFileTransfered = this.OnRpcServiceFileTransfered,
                GetInvokeMethod = this.GetInvokeMethod,
                OnFileTransfering = this.OnRpcServiceFileTransfering,
                OnFindRpcActor = this.OnRpcServiceFindRpcActor,
                OnHandshaked = this.OnRpcServiceHandshaked,
                OnHandshaking = this.OnRpcServiceHandshaking,
                OnReceived = this.OnRpcServiceReceived,
                OnResetID = this.OnRpcServiceResetID,
                OnStreamTransfered = this.OnRpcServiceStreamTransfered,
                OnStreamTransfering = this.OnRpcServiceStreamTransfering,
                OutputSend = this.RpcServiceOutputSend
            };

            if (config.GetValue<RpcStore>(RpcConfigExtensions.RpcStoreProperty) is RpcStore rpcStore)
            {
                rpcStore.AddRpcParser(this.GetType().Name, this);
            }
            else
            {
                new RpcStore(config.Container).AddRpcParser(this.GetType().Name, this);
            }
        }

        /// <summary>
        /// 方法映射表
        /// </summary>
        public ActionMap ActionMap { get => this.m_actionMap; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public TouchSocketConfig Config { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IContainer Container { get; private set; }

        /// <summary>
        /// 获取默认新ID。
        /// </summary>
        public Func<string> GetDefaultNewID { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public IPluginsManager PluginsManager { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcStore RpcStore => this.m_rpcStore;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool UsePlugin { get; private set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string VerifyToken { get; private set; }

        #region 通道

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public Channel CreateChannel(string targetID)
        {
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.CreateChannel();
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel CreateChannel(string targetID, int id)
        {
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.CreateChannel(id);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        #endregion 通道

        #region Rpc

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
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                client.Invoke(targetID, method, invokeOption, ref parameters, types);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
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
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.Invoke<T>(targetID, method, invokeOption, ref parameters, types);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="targetID">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void Invoke(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                client.Invoke(method, invokeOption, parameters);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="targetID">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        public T Invoke<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.Invoke<T>(method, invokeOption, parameters);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="targetID">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public Task InvokeAsync(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.InvokeAsync(method, invokeOption, parameters);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        /// <summary>
        /// 反向调用客户端Rpc
        /// </summary>
        /// <param name="targetID">客户端ID</param>
        /// <param name="method">方法名</param>
        /// <param name="invokeOption">调用配置</param>
        /// <param name="parameters">参数</param>
        /// <exception cref="TimeoutException">调用超时</exception>
        /// <exception cref="RpcSerializationException">序列化异常</exception>
        /// <exception cref="RpcInvokeException">调用内部异常</exception>
        /// <exception cref="ClientNotFindException">没有找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回值</returns>
        public Task<T> InvokeAsync<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.InvokeAsync<T>(method, invokeOption, parameters);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        #endregion Rpc

        #region RPC解析器

        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<TouchRpcAttribute>() is TouchRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<TouchRpcAttribute>() is TouchRpcAttribute attribute)
                {
                    this.m_actionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        void IRpcParser.SetRpcStore(RpcStore rpcStore)
        {
            this.m_rpcActorGroup.RpcStore = rpcStore;
            this.m_rpcStore = rpcStore;
        }

        #endregion RPC解析器

        #region File

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
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.PullFile(fileRequest, fileOperator, metadata);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
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
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.PullFileAsync(fileRequest, fileOperator, metadata);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
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
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.PushFile(fileRequest, fileOperator, metadata);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
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
            if (this.TryGetSocketClient(targetID, out WSTouchRpcSocketClient client))
            {
                return client.PushFileAsync(fileRequest, fileOperator, metadata);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(targetID));
            }
        }

        #endregion File

        /// <summary>
        /// 重新设置客户端ID。注意，该效果只作用于服务端。客户端ID不会同步改变。
        /// 如果想同步，请使用对应的SocketClient进行操作。
        /// </summary>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        public void ResetID(string oldID, string newID)
        {
            if (string.IsNullOrEmpty(oldID))
            {
                throw new ArgumentException($"“{nameof(oldID)}”不能为 null 或空。", nameof(oldID));
            }

            if (string.IsNullOrEmpty(newID))
            {
                throw new ArgumentException($"“{nameof(newID)}”不能为 null 或空。", nameof(newID));
            }

            if (oldID == newID)
            {
                return;
            }
            if (this.m_tokenDic.TryRemove(oldID, out WSTouchRpcSocketClient socketClient))
            {
                socketClient.m_id = newID;
                if (this.m_tokenDic.TryAdd(newID, socketClient))
                {
                    return;
                }
                else
                {
                    socketClient.m_id = oldID;
                    this.m_tokenDic.TryAdd(oldID, socketClient);
                    throw new Exception("ID重复");
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(oldID));
            }
        }

        #region 内部委托绑定

        private MethodInstance GetInvokeMethod(string arg)
        {
            return this.m_actionMap.GetMethodInstance(arg);
        }

        private void OnRpcServiceClose(RpcActor actor, string arg2)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            client.Close(arg2);
        }

        private void OnRpcServiceFileTransfered(RpcActor actor, FileTransferStatusEventArgs e)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnFileTransfered), client, e))
            {
                return;
            }
            this.OnFileTransfered(client, e);
        }

        private void OnRpcServiceFileTransfering(RpcActor actor, FileOperationEventArgs e)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnFileTransfering), client, e))
            {
                return;
            }
            this.OnFileTransfering(client, e);
        }

        private RpcActor OnRpcServiceFindRpcActor(string id)
        {
            if (this.TryGetSocketClient(id, out WSTouchRpcSocketClient client))
            {
                return client.m_rpcActor;
            }
            return null;
        }

        private void OnRpcServiceHandshaked(RpcActor actor, VerifyOptionEventArgs e)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnHandshaked), client, e))
            {
                return;
            }
            this.OnHandshaked(client, e);
        }

        private void OnRpcServiceHandshaking(RpcActor actor, VerifyOptionEventArgs e)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (e.Token == this.VerifyToken)
            {
                e.AddOperation(Operation.Permit);
            }
            else
            {
                e.Message = "Token不受理";
            }
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnHandshaking), client, e))
            {
                return;
            }
            this.OnHandshaking(client, e);
        }

        private void OnRpcServiceReceived(RpcActor actor, short protocol, ByteBlock byteBlock)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnReceivedProtocolData), client, new ProtocolDataEventArgs(protocol, byteBlock)))
            {
                return;
            }

            this.OnReceived(client, protocol, byteBlock);
        }

        private void OnRpcServiceResetID(RpcActor actor, WaitSetID arg2)
        {
            this.ResetID(arg2.OldID, arg2.NewID);
        }

        private void OnRpcServiceStreamTransfered(RpcActor actor, StreamStatusEventArgs e)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnStreamTransfered), client, e))
            {
                return;
            }
            this.OnStreamTransfered(client, e);
        }

        private void OnRpcServiceStreamTransfering(RpcActor actor, StreamOperationEventArgs e)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            if (this.UsePlugin && this.PluginsManager.Raise<ITouchRpcPlugin>(nameof(ITouchRpcPlugin.OnStreamTransfering), client, e))
            {
                return;
            }
            this.OnStreamTransfering(client, e);
        }

        private void PrivateDisconnected(WSTouchRpcSocketClient client, ClientDisconnectedEventArgs e)
        {
            if (this.TryRemove(client.ID, out _))
            {
                if (this.UsePlugin && this.PluginsManager.Raise<ITcpPlugin>(nameof(ITcpPlugin.OnDisconnected), client, e))
                {
                    return;
                }
                this.OnDisconnected(client, e);
            }
        }

        private void RpcServiceOutputSend(RpcActor actor, bool arg2, ArraySegment<byte>[] arg3)
        {
            WSTouchRpcSocketClient client = (WSTouchRpcSocketClient)actor.Caller;
            client.RpcActorSend(arg2, arg3);
        }

        #endregion 内部委托绑定

        /// <summary>
        /// 从WebSocket获取新客户端。
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        public async Task SwitchClientAsync(System.Net.WebSockets.WebSocket webSocket)
        {
            string id = this.GetDefaultNewID();
            WSTouchRpcSocketClient client = new WSTouchRpcSocketClient();
            if (!this.TryAdd(id, client))
            {
                throw new Exception("ID重复");
            }
            client.m_service = this;
            client.m_usePlugin = this.UsePlugin;
            this.CheckService();

            client.m_rpcActor = this.m_rpcActorGroup.CreateRpcActor(client);
            client.m_rpcActor.ID = id;
            client.m_internalDisconnected = this.PrivateDisconnected;
            await client.Start(this.Config, webSocket);
        }

        private void CheckService()
        {
            if (this.m_rpcActorGroup == null)
            {
                throw new Exception($"{nameof(RpcActorGroup)}未在{nameof(this.RpcStore)}中注册为解析器。");
            }
        }

        #region 事件

        /// <summary>
        /// 客户端断开。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnDisconnected(WSTouchRpcSocketClient client, ClientDisconnectedEventArgs e)
        {
        }

        /// <summary>
        /// 当文件传输结束之后。并不意味着完成传输，请通过<see cref="FileTransferStatusEventArgs.Result"/>属性值进行判断。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfered(WSTouchRpcSocketClient client, FileTransferStatusEventArgs e)
        {
        }

        /// <summary>
        /// 在文件传输即将进行时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnFileTransfering(WSTouchRpcSocketClient client, FileOperationEventArgs e)
        {
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(WSTouchRpcSocketClient client, VerifyOptionEventArgs e)
        {
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        protected virtual void OnHandshaking(WSTouchRpcSocketClient client, VerifyOptionEventArgs e)
        {
        }

        /// <summary>
        /// 接收到协议数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected virtual void OnReceived(WSTouchRpcSocketClient client, short protocol, ByteBlock byteBlock)
        {
        }

        /// <summary>
        /// 流数据处理，用户需要在此事件中对e.Bucket手动释放。覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfered(WSTouchRpcSocketClient client, StreamStatusEventArgs e)
        {
        }

        /// <summary>
        /// 即将接收流数据，用户需要在此事件中对e.Bucket初始化。覆盖父类方法将不会触发事件和插件。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected virtual void OnStreamTransfering(WSTouchRpcSocketClient client, StreamOperationEventArgs e)
        {
        }

        #endregion 事件

        #region 发送

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void Send(string id, short protocol, byte[] buffer, int offset, int length)
        {
            if (this.TryGetSocketClient(id, out WSTouchRpcSocketClient client))
            {
                client.Send(protocol, buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(id));
            }
        }

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="buffer">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void Send(string id, short protocol, byte[] buffer)
        {
            this.Send(id, protocol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="byteBlock">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void Send(string id, short protocol, ByteBlock byteBlock)
        {
            this.Send(id, protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void SendAsync(string id, short protocol, byte[] buffer, int offset, int length)
        {
            if (this.TryGetSocketClient(id, out WSTouchRpcSocketClient client))
            {
                client.SendAsync(protocol, buffer, offset, length);
            }
            else
            {
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription(id));
            }
        }

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="buffer">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void SendAsync(string id, short protocol, byte[] buffer)
        {
            this.SendAsync(id, protocol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 向对应ID的客户端发送
        /// </summary>
        /// <param name="id">目标ID</param>
        /// <param name="protocol">协议</param>
        /// <param name="byteBlock">数据</param>
        /// <exception cref="NotConnectedException">未连接异常</exception>
        /// <exception cref="ClientNotFindException">未找到ID对应的客户端</exception>
        /// <exception cref="Exception">其他异常</exception>
        public void SendAsync(string id, short protocol, ByteBlock byteBlock)
        {
            this.SendAsync(id, protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 发送
    }
}