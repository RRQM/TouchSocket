using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 能够基于Dmtp协议，提供Rpc的功能
    /// </summary>
    public class DmtpRpcFeature : PluginBase, IRpcParser, IDmtpFeature
    {
        /// <summary>
        /// 能够基于Dmtp协议，提供Rpc的功能
        /// </summary>
        /// <param name="container"></param>
        public DmtpRpcFeature(IContainer container)
        {
            this.RpcStore = container.IsRegistered(typeof(RpcStore)) ? container.Resolve<RpcStore>() : new RpcStore(container);
            this.RpcStore.AddRpcParser(this);
            this.CreateDmtpRpcActor = this.PrivateCreateDmtpRpcActor;
            this.SetProtocolFlags(20);
        }

        /// <summary>
        /// 方法映射表
        /// </summary>
        public ActionMap ActionMap { get; } = new ActionMap(false);

        /// <summary>
        /// 创建DmtpRpc实例
        /// </summary>
        public Func<IDmtpActor, DmtpRpcActor> CreateDmtpRpcActor { get; set; }

        /// <inheritdoc/>
        public ushort ReserveProtocolSize => 5;

        /// <inheritdoc/>
        public RpcStore RpcStore { get; }

        /// <summary>
        /// 序列化选择器
        /// </summary>
        public SerializationSelector SerializationSelector { get; set; } = new DefaultSerializationSelector();

        /// <inheritdoc/>
        public ushort StartProtocol { get; set; }

        /// <summary>
        /// 设置创建DmtpRpc实例
        /// </summary>
        /// <param name="createDmtpRpcActor"></param>
        /// <returns></returns>
        public DmtpRpcFeature SetCreateDmtpRpcActor(Func<IDmtpActor, DmtpRpcActor> createDmtpRpcActor)
        {
            this.CreateDmtpRpcActor = createDmtpRpcActor;
            return this;
        }

        /// <summary>
        /// 设置<see cref="DmtpRpcFeature"/>的起始协议。
        /// <para>
        /// 默认起始为：20，保留5个协议长度。
        /// </para>
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public DmtpRpcFeature SetProtocolFlags(ushort start)
        {
            this.StartProtocol = start;
            return this;
        }

        /// <summary>
        /// 设置序列化选择器。默认使用<see cref="DefaultSerializationSelector"/>
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public DmtpRpcFeature SetSerializationSelector(SerializationSelector selector)
        {
            this.SerializationSelector = selector;
            return this;
        }

        private MethodInstance GetInvokeMethod(string name)
        {
            return this.ActionMap.GetMethodInstance(name);
        }

        private DmtpRpcActor PrivateCreateDmtpRpcActor(IDmtpActor smtpActor)
        {
            return new DmtpRpcActor(smtpActor);
        }

        #region Rpc配置

        void IRpcParser.OnRegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        void IRpcParser.OnUnregisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
                {
                    this.ActionMap.Remove(attribute.GetInvokenKey(methodInstance));
                }
            }
        }

        #endregion Rpc配置

        #region Config

        /// <inheritdoc/>
        protected override void Loaded(IPluginsManager pluginsManager)
        {
            base.Loaded(pluginsManager);
            pluginsManager.Add<IDmtpActorObject, DmtpVerifyEventArgs>(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking),this.OnDmtpHandshaking);
            pluginsManager.Add<IDmtpActorObject, DmtpMessageEventArgs>(nameof(IDmtpReceivedPlugin.OnDmtpReceived),this.OnDmtpReceived);
        }

        private Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var smtpRpcActor = CreateDmtpRpcActor(client.DmtpActor);
            smtpRpcActor.RpcStore = this.RpcStore;
            smtpRpcActor.SerializationSelector = this.SerializationSelector;
            smtpRpcActor.GetInvokeMethod = this.GetInvokeMethod;

            smtpRpcActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpRpcActor(smtpRpcActor);

            return e.InvokeNext();
        }

        private Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            if (client.DmtpActor.GetDmtpRpcActor() is DmtpRpcActor smtpRpcActor)
            {
                if (smtpRpcActor.InputReceivedData(e.DmtpMessage))
                {
                    e.Handled = true;
                    return EasyTask.CompletedTask;
                }
            }

            return e.InvokeNext();
        }

        #endregion Config
    }
}