using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 能够基于Dmtp协议，提供Rpc的功能
    /// </summary>
    public class DmtpRpcFeature : PluginBase, IRpcParser, IDmtpHandshakedPlugin, IDmtpReceivedPlugin, IDmtpFeature
    {
        /// <summary>
        /// 能够基于Dmtp协议，提供Rpc的功能
        /// </summary>
        /// <param name="container"></param>
        public DmtpRpcFeature(IContainer container)
        {
            this.RpcStore = container.IsRegistered(typeof(RpcStore)) ? container.Resolve<RpcStore>() : new RpcStore(container);
            this.RpcStore.AddRpcParser(this);
            this.SetProtocolFlags(20);
        }

        /// <summary>
        /// 方法映射表
        /// </summary>
        public ActionMap ActionMap { get; } = new ActionMap(false);

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

        /// <summary>
        /// 获取<see cref="DmtpRpcActor"/>
        /// </summary>
        /// <param name="smtpActor"></param>
        /// <returns></returns>
        protected DmtpRpcActor CreateDmtpRpcActor(IDmtpActor smtpActor)
        {
            return new DmtpRpcActor(smtpActor);
        }

        private MethodInstance GetInvokeMethod(string name)
        {
            return this.ActionMap.GetMethodInstance(name);
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

        Task IDmtpHandshakedPlugin<IDmtpActorObject>.OnDmtpHandshaked(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var smtpRpcActor = this.CreateDmtpRpcActor(client.DmtpActor);
            smtpRpcActor.RpcStore = this.RpcStore;
            smtpRpcActor.SerializationSelector = this.SerializationSelector;
            smtpRpcActor.GetInvokeMethod = this.GetInvokeMethod;

            smtpRpcActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpRpcActor(smtpRpcActor);

            return e.InvokeNext();
        }

        Task IDmtpReceivedPlugin<IDmtpActorObject>.OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
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