//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 能够基于Dmtp协议，提供Rpc的功能
    /// </summary>
    public class DmtpRpcFeature : PluginBase, IDmtpFeature
    {
        private readonly IRpcServerProvider m_rpcServerProvider;
        private readonly IResolver m_resolver;

        /// <summary>
        /// 能够基于Dmtp协议，提供Rpc的功能
        /// </summary>
        /// <param name="resolver"></param>
        public DmtpRpcFeature(IResolver resolver)
        {
            if (resolver.IsRegistered<IRpcServerProvider>())
            {
                var rpcServerProvider = resolver.Resolve<IRpcServerProvider>();
                this.RegisterServer(rpcServerProvider.GetMethods());
                this.m_rpcServerProvider = rpcServerProvider;
            }

            this.CreateDmtpRpcActor = this.PrivateCreateDmtpRpcActor;
            this.SetProtocolFlags(20);
            this.m_resolver = resolver;
        }

        /// <summary>
        /// 方法映射表
        /// </summary>
        public ActionMap ActionMap { get; } = new ActionMap(false);

        /// <summary>
        /// 创建DmtpRpc实例
        /// </summary>
        public Func<IDmtpActor, IRpcServerProvider, DmtpRpcActor> CreateDmtpRpcActor { get; set; }

        /// <inheritdoc/>
        public ushort ReserveProtocolSize => 5;

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
        public DmtpRpcFeature SetCreateDmtpRpcActor(Func<IDmtpActor, IRpcServerProvider, DmtpRpcActor> createDmtpRpcActor)
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

        private DmtpRpcActor PrivateCreateDmtpRpcActor(IDmtpActor dmtpActor, IRpcServerProvider rpcServerProvider)
        {
            return new DmtpRpcActor(dmtpActor, rpcServerProvider,this.m_resolver);
        }

        private void RegisterServer(MethodInstance[] methodInstances)
        {
            foreach (var methodInstance in methodInstances)
            {
                if (methodInstance.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
                {
                    this.ActionMap.Add(attribute.GetInvokenKey(methodInstance), methodInstance);
                }
            }
        }

        #region Config

        /// <inheritdoc/>
        protected override void Loaded(IPluginManager pluginManager)
        {
            base.Loaded(pluginManager);
            pluginManager.Add<IDmtpActorObject, DmtpVerifyEventArgs>(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this.OnDmtpHandshaking);
            pluginManager.Add<IDmtpActorObject, DmtpMessageEventArgs>(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this.OnDmtpReceived);
        }

        private async Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
        {
            var dmtpRpcActor = this.CreateDmtpRpcActor(client.DmtpActor,this.m_rpcServerProvider);
            dmtpRpcActor.SerializationSelector = this.SerializationSelector;
            dmtpRpcActor.GetInvokeMethod = this.GetInvokeMethod;

            dmtpRpcActor.SetProtocolFlags(this.StartProtocol);
            client.DmtpActor.SetDmtpRpcActor(dmtpRpcActor);

            await e.InvokeNext();
        }

        private async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
        {
            if (client.DmtpActor.GetDmtpRpcActor() is DmtpRpcActor dmtpRpcActor)
            {
                if (await dmtpRpcActor.InputReceivedData(e.DmtpMessage).ConfigureFalseAwait())
                {
                    e.Handled = true;
                    return;
                }
            }
            await e.InvokeNext().ConfigureFalseAwait();
        }

        #endregion Config
    }
}