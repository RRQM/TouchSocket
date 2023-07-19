////------------------------------------------------------------------------------
////  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
////  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
////  CSDN博客：https://blog.csdn.net/qq_40374647
////  哔哩哔哩视频：https://space.bilibili.com/94253567
////  Gitee源代码仓库：https://gitee.com/RRQM_Home
////  Github源代码仓库：https://github.com/RRQM
////  API首页：http://rrqm_home.gitee.io/touchsocket/
////  交流QQ群：234762506
////  感谢您的下载和使用
////------------------------------------------------------------------------------
////------------------------------------------------------------------------------
//using System;
//using System.Collections.Concurrent;
//using System.IO;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using TouchSocket.Core;
//using TouchSocket.Dmtp;
//using TouchSocket.Dmtp.Rpc;
//using TouchSocket.Sockets;

//namespace TouchSocket.Dmtp.Rpc
//{
//    /// <summary>
//    /// UDP Rpc解释器
//    /// </summary>
//    public partial class UdpTouchRpc : UdpSessionBase, IUdpTouchRpc
//    {
//        private readonly Timer m_timer;
//        private readonly ConcurrentDictionary<EndPoint, UdpRpcActor> m_udpRpcActors;
//        private RpcStore m_rpcStore;

//        /// <summary>
//        /// 构造函数
//        /// </summary>
//        public UdpTouchRpc()
//        {
//            this.m_timer = new Timer((obj) =>
//            {
//                this.m_udpRpcActors.Remove((kv) =>
//                {
//                    if (--kv.Value.Tick < 0)
//                    {
//                        return true;
//                    }
//                    return false;
//                });
//            }, null, 1000 * 30, 1000 * 30);
//            this.ActionMap = new ActionMap();
//            this.m_udpRpcActors = new ConcurrentDictionary<EndPoint, UdpRpcActor>();
//        }

//        /// <summary>
//        /// 方法映射表
//        /// </summary>
//        public ActionMap ActionMap { get; private set; }

//        /// <summary>
//        /// 不需要握手，所以此值一直为True。
//        /// </summary>
//        public bool IsHandshaked => true;


//        /// <inheritdoc/>
//        public RpcActor RpcActor => this.GetUdpRpcActor();

//        /// <inheritdoc/>
//        public RpcStore RpcStore { get => m_rpcStore; }

//        /// <inheritdoc/>
//        public SerializationSelector SerializationSelector { get; private set; }

//        /// <inheritdoc/>
//        public Func<IRpcClient, bool> 123 { get; set; }


//        #region RPC解析器

//        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
//        {
//            foreach (MethodInstance methodInstance in methodInstances)
//            {
//                if (methodInstance.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
//                {
//                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
//                }
//            }
//        }

//        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
//        {
//            foreach (MethodInstance methodInstance in methodInstances)
//            {
//                if (methodInstance.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
//                {
//                    this.ActionMap.Remove(attribute.GetInvokenKey(methodInstance));
//                }
//            }
//        }

//        void IRpcParser.SetRpcStore(RpcStore rpcService)
//        {
//            //this.RpcStore = rpcService;
//        }

//        #endregion RPC解析器


//        /// <inheritdoc/>
//        public bool Ping(int timeout = 5000)
//        {
//            return this.GetUdpRpcActor().Ping(timeout);
//        }


//        /// <inheritdoc/>
//        public void Send(short protocol, byte[] buffer, int offset, int length)
//        {
//            this.GetUdpRpcActor().Send(protocol, buffer, offset, length);
//        }

//        /// <inheritdoc/>
//        public Task SendAsync(short protocol, byte[] buffer, int offset, int length)
//        {
//            return this.GetUdpRpcActor().SendAsync(protocol, buffer, offset, length);
//        }

//        Result IDmtpFileTransferActor.SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata)
//        {
//            throw new NotImplementedException();
//        }

//        Task<Result> IDmtpFileTransferActor.SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata)
//        {
//            throw new NotImplementedException();
//        }

//        bool IDmtpFileTransferActor.TrySubscribeChannel(int id, out IDmtpChannel channel)
//        {
//            throw new NotImplementedException();
//        }

//        /// <inheritdoc/>
//        protected override void Dispose(bool disposing)
//        {
//            this.m_timer.SafeDispose();
//            this.m_udpRpcActors.Clear();
//            base.Dispose(disposing);
//        }

//        /// <inheritdoc/>
//        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
//        {
//            this.GetUdpRpcActor(remoteEndPoint).InputReceivedData(byteBlock);
//        }

//        /// <inheritdoc/>
//        protected override void LoadConfig(TouchSocketConfig config)
//        {
//            base.LoadConfig(config);

//            this.m_rpcStore = new RpcStore(this.Container);
//            if (config.GetValue(RpcConfigExtensions.ConfigureRpcStoreProperty) is Action<RpcStore> action)
//            {
//                action.Invoke(this.m_rpcStore);
//            }

//            this.m_rpcStore.AddRpcParser( this);
//        }

//        private MethodInstance GetInvokeMethod(string arg)
//        {
//            return this.ActionMap.GetMethodInstance(arg);
//        }

//        private UdpRpcActor GetUdpRpcActor(EndPoint endPoint)
//        {
//            if (!this.m_udpRpcActors.TryGetValue(endPoint, out UdpRpcActor udpRpcActor))
//            {
//                udpRpcActor = new UdpRpcActor(this, endPoint, this.Container.Resolve<ILog>())
//                {
//                    Caller = new UdpCaller(this, endPoint),
//                    RpcStore = RpcStore,
//                    GetInvokeMethod = GetInvokeMethod,
//                    SerializationSelector = SerializationSelector
//                };
//                this.m_udpRpcActors.TryAdd(endPoint, udpRpcActor);
//            }
//            udpRpcActor.Tick++;
//            return udpRpcActor;
//        }

//        private UdpRpcActor GetUdpRpcActor()
//        {
//            if (this.RemoteIPHost == null)
//            {
//                throw new ArgumentNullException(nameof(this.RemoteIPHost));
//            }
//            return this.GetUdpRpcActor(this.RemoteIPHost.EndPoint);
//        }
//    }
//}