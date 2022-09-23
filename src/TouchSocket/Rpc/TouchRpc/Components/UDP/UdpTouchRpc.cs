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
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Config;
using TouchSocket.Core.Dependency;
using TouchSocket.Core.Extensions;
using TouchSocket.Core.Log;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// UDP Rpc解释器
    /// </summary>
    public class UdpTouchRpc : UdpSessionBase, IUdpTouchRpc
    {
        private readonly Timer m_timer;
        private readonly ConcurrentDictionary<EndPoint, UdpRpcActor> m_udpRpcActors;
        private readonly ActionMap m_actionMap;
        private RpcStore m_rpcStore;
        private SerializationSelector m_serializationSelector;
        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpTouchRpc()
        {
            this.m_timer = new Timer((obj) =>
            {
                this.m_udpRpcActors.Remove((kv) =>
                {
                    if (--kv.Value.Tick < 0)
                    {
                        return true;
                    }
                    return false;
                });
            }, null, 1000 * 30, 1000 * 30);
            this.m_actionMap = new ActionMap();
            this.m_udpRpcActors = new ConcurrentDictionary<EndPoint, UdpRpcActor>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Func<IRpcClient, bool> TryCanInvoke { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RpcStore RpcStore => this.m_rpcStore;

        /// <summary>
        /// 序列化选择器
        /// </summary>
        public SerializationSelector SerializationSelector => this.m_serializationSelector;

        bool IRpcActor.IsHandshaked => throw new NotImplementedException();

        string IRpcActor.RootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        ResponseType IRpcActor.ResponseType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        RpcActor ITouchRpc.RpcActor => throw new NotImplementedException();

        /// <summary>
        /// 方法映射表
        /// </summary>
        public ActionMap ActionMap { get => this.m_actionMap; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="invokeOption"></param>
        /// <param name="parameters"></param>
        public void Invoke(string method, IInvokeOption invokeOption, params object[] parameters)
        {
            this.GetUdpRpcActor().Invoke(method, invokeOption, parameters);
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
            return this.GetUdpRpcActor().Invoke<T>(method, invokeOption, parameters);
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
            return this.GetUdpRpcActor().Invoke<T>(method, invokeOption, ref parameters, types);
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
            this.GetUdpRpcActor().Invoke(method, invokeOption, ref parameters, types);
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
            return this.GetUdpRpcActor().InvokeAsync(method, invokeOption, parameters);
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
            return this.GetUdpRpcActor().InvokeAsync<T>(method, invokeOption, parameters);
        }

        #region RPC解析器
        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<TouchRpcAttribute>() is TouchRpcAttribute attribute)
                {
                    this.m_actionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
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
        void IRpcParser.SetRpcStore(RpcStore rpcService)
        {
            this.m_rpcStore = rpcService;
        }

        #endregion


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Ping(int timeout = 5000)
        {
            return this.GetUdpRpcActor().Ping(timeout);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public void Send(short protocol, byte[] buffer)
        {
            this.GetUdpRpcActor().Send(protocol, buffer);
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
            this.GetUdpRpcActor().Send(protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        public void Send(short protocol, ByteBlock dataByteBlock)
        {
            this.GetUdpRpcActor().Send(protocol, dataByteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        public void Send(short protocol)
        {
            this.GetUdpRpcActor().Send(protocol);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public void SendAsync(short protocol, byte[] buffer)
        {
            this.GetUdpRpcActor().SendAsync(protocol, buffer);
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
            this.GetUdpRpcActor().SendAsync(protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="dataByteBlock"></param>
        public void SendAsync(short protocol, ByteBlock dataByteBlock)
        {
            this.GetUdpRpcActor().SendAsync(protocol, dataByteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="protocol"></param>
        public void SendAsync(short protocol)
        {
            this.GetUdpRpcActor().SendAsync(protocol);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            this.m_udpRpcActors.Clear();
            base.Dispose(disposing);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.GetUdpRpcActor(remoteEndPoint).InputReceivedData(byteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="config"></param>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.m_serializationSelector = (SerializationSelector)config.GetValue(TouchRpcConfigExtensions.SerializationSelectorProperty);

            if (config.GetValue<RpcStore>(RpcConfigExtensions.RpcStoreProperty) is RpcStore rpcStore)
            {
                rpcStore.AddRpcParser(this.GetType().Name, this);
            }
            else
            {
                new RpcStore(config.Container).AddRpcParser(this.GetType().Name, this);
            }
            base.LoadConfig(config);
        }

        private UdpRpcActor GetUdpRpcActor(EndPoint endPoint)
        {
            if (!this.m_udpRpcActors.TryGetValue(endPoint, out UdpRpcActor udpRpcActor))
            {
                udpRpcActor = new UdpRpcActor(this, endPoint, this.Container.Resolve<ILog>());
                udpRpcActor.Caller = new UdpCaller(this, endPoint);
                udpRpcActor.RpcStore = this.m_rpcStore;
                udpRpcActor.GetInvokeMethod = this.GetInvokeMethod;
                udpRpcActor.SerializationSelector = this.m_serializationSelector;
                this.m_udpRpcActors.TryAdd(endPoint, udpRpcActor);
            }
            udpRpcActor.Tick++;
            return udpRpcActor;
        }

        private MethodInstance GetInvokeMethod(string arg)
        {
            return this.m_actionMap.GetMethodInstance(arg);
        }

        private UdpRpcActor GetUdpRpcActor()
        {
            if (this.RemoteIPHost == null)
            {
                throw new ArgumentNullException(nameof(this.RemoteIPHost));
            }
            return this.GetUdpRpcActor(this.RemoteIPHost.EndPoint);
        }

        Result IRpcActor.PullFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Task<Result> IRpcActor.PullFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Result IRpcActor.PushFile(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Task<Result> IRpcActor.PushFileAsync(FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        bool IRpcActor.ChannelExisted(int id)
        {
            throw new NotImplementedException();
        }

        Channel IRpcActor.CreateChannel()
        {
            throw new NotImplementedException();
        }

        Channel IRpcActor.CreateChannel(int id)
        {
            throw new NotImplementedException();
        }

        Result IRpcActor.SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Task<Result> IRpcActor.SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        bool IRpcActor.TrySubscribeChannel(int id, out Channel channel)
        {
            throw new NotImplementedException();
        }

        void IRpcActor.ResetID(string newID, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Channel IIDRpcActor.CreateChannel(string targetID)
        {
            throw new NotImplementedException();
        }

        Channel IIDRpcActor.CreateChannel(string targetID, int id)
        {
            throw new NotImplementedException();
        }

        Result IIDRpcActor.PullFile(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Task<Result> IIDRpcActor.PullFileAsync(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Result IIDRpcActor.PushFile(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Task<Result> IIDRpcActor.PushFileAsync(string targetID, FileRequest fileRequest, FileOperator fileOperator, Metadata metadata)
        {
            throw new NotImplementedException();
        }

        Task IIDRpcActor.InvokeAsync(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        Task<T> IIDRpcActor.InvokeAsync<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        void IIDRpcActor.Invoke(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        T IIDRpcActor.Invoke<T>(string targetID, string method, IInvokeOption invokeOption, params object[] parameters)
        {
            throw new NotImplementedException();
        }


        void IIDRpcActor.Invoke(string targetID, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            throw new NotImplementedException();
        }

        T IIDRpcActor.Invoke<T>(string targetID, string method, IInvokeOption invokeOption, ref object[] parameters, Type[] types)
        {
            throw new NotImplementedException();
        }
    }
}