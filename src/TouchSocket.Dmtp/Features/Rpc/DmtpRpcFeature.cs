//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// 能够基于Dmtp协议，提供Rpc的功能
/// </summary>
public class DmtpRpcFeature : PluginBase, IDmtpFeature, IDmtpHandshakingPlugin, IDmtpReceivedPlugin
{
    private readonly IRpcServerProvider m_rpcServerProvider;

    /// <summary>
    /// 能够基于Dmtp协议，提供Rpc的功能
    /// </summary>
    /// <param name="resolver"></param>
    public DmtpRpcFeature(IServiceProvider resolver)
    {
        var rpcServerProvider = resolver.Resolve<IRpcServerProvider>();

        if (rpcServerProvider != null)
        {
            this.RegisterServer(rpcServerProvider.GetMethods());
            this.m_rpcServerProvider = rpcServerProvider;
        }

        this.CreateDmtpRpcActor = PrivateCreateDmtpRpcActor;
        this.SetProtocolFlags(20);

        this.UseConcurrencyDispatcher();
    }

    /// <summary>
    /// 方法映射表
    /// </summary>
    public ActionMap ActionMap { get; } = new ActionMap(false);

    /// <summary>
    /// 获取或设置一个函数，该函数创建一个RPC调度器，用于处理IDmtpActor的RPC调用。
    /// </summary>
    /// <value>
    /// 一个函数，接受一个IDmtpActor实例作为参数，并返回一个IRpcDispatcher接口，该接口泛型化于IDmtpActor和IDmtpRpcCallContext。
    /// </value>
    public Func<IDmtpActor, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>> CreateDispatcher { get; set; }

    /// <summary>
    /// 创建DmtpRpc实例
    /// </summary>
    public Func<IDmtpActor, IRpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>, DmtpRpcActor> CreateDmtpRpcActor { get; set; }

    /// <inheritdoc/>
    public ushort ReserveProtocolSize => 5;

    /// <summary>
    /// 序列化选择器
    /// </summary>
    public ISerializationSelector SerializationSelector { get; set; } = new DefaultSerializationSelector();

    /// <inheritdoc/>
    public ushort StartProtocol { get; set; }

    /// <summary>
    /// 配置默认的序列化选择器。
    /// </summary>
    /// <param name="selector">用于配置默认序列化选择器的操作。</param>
    /// <returns>返回当前的 <see cref="DmtpRpcFeature"/> 实例，以支持链式调用。</returns>
    public DmtpRpcFeature ConfigureDefaultSerializationSelector(Action<DefaultSerializationSelector> selector)
    {
        var serializationSelector = new DefaultSerializationSelector();
        selector.Invoke(serializationSelector);
        this.SerializationSelector = serializationSelector;
        return this;
    }

    /// <summary>
    /// 设置创建DmtpRpc实例
    /// </summary>
    /// <param name="createDmtpRpcActor"></param>
    /// <returns></returns>
    public DmtpRpcFeature SetCreateDmtpRpcActor(Func<IDmtpActor, IRpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>, DmtpRpcActor> createDmtpRpcActor)
    {
        this.CreateDmtpRpcActor = createDmtpRpcActor;
        return this;
    }

    #region Dispatcher

    private readonly GlobalQueueRpcDispatcher m_globalQueueRpcDispatcher = new();

    /// <summary>
    /// 使用并发调度器处理请求
    /// </summary>
    /// <returns>返回当前实例，以支持链式调用</returns>
    public DmtpRpcFeature UseConcurrencyDispatcher()
    {
        // 设置创建调度器的委托，使用支持并发的 Rpc 调度器
        this.CreateDispatcher = (actor) => new ConcurrencyRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();
        // 支持链式调用，返回当前实例
        return this;
    }

    /// <summary>
    /// 使用全局队列RPC调度器配置RPC特性。
    /// </summary>
    /// <returns>返回配置了全局队列RPC调度器的DmtpRpcFeature实例。</returns>
    public DmtpRpcFeature UseGlobalQueueRpcDispatcher()
    {
        // 设置创建调度器的委托，使用GlobalQueueRpcDispatcher实现
        this.CreateDispatcher = (actor) => this.m_globalQueueRpcDispatcher;
        return this;
    }

    /// <summary>
    /// 使用即时RPC调度器配置RPC特性
    /// </summary>
    /// <returns>返回配置了即时RPC调度器的DmtpRpcFeature实例</returns>
    public DmtpRpcFeature UseImmediateRpcDispatcher()
    {
        // 设置创建调度器的委托，使用ImmediateRpcDispatcher实现
        this.CreateDispatcher = (actor) => new ImmediateRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();
        return this;
    }

    /// <summary>
    /// 使用队列RPC调度器配置RPC特性
    /// </summary>
    /// <returns>返回配置了队列RPC调度器的DmtpRpcFeature实例</returns>
    public DmtpRpcFeature UseQueueRpcDispatcher()
    {
        // 设置创建调度器的委托，使用QueueRpcDispatcher实现
        this.CreateDispatcher = (actor) => new QueueRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();
        return this;
    }

    private class GlobalQueueRpcDispatcher : QueueRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>
    {
        public bool Pin { get; set; } = true;

        protected override void Dispose(bool disposing)
        {
            if (this.Pin)
            {
                return;
            }
            base.Dispose(disposing);
        }
    }

    #endregion Dispatcher

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
    public DmtpRpcFeature SetSerializationSelector(ISerializationSelector selector)
    {
        this.SerializationSelector = selector;
        return this;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_globalQueueRpcDispatcher.Pin = false;
            this.m_globalQueueRpcDispatcher.Dispose();
        }
        base.Dispose(disposing);
    }

    private static DmtpRpcActor PrivateCreateDmtpRpcActor(IDmtpActor dmtpActor, IRpcServerProvider rpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext> dispatcher)
    {
        return new DmtpRpcActor(dmtpActor, rpcServerProvider, dmtpActor.Client.Resolver, dispatcher);
    }

    private RpcMethod GetInvokeMethod(string name)
    {
        return this.ActionMap.GetRpcMethod(name);
    }

    private void RegisterServer(RpcMethod[] rpcMethods)
    {
        foreach (var rpcMethod in rpcMethods)
        {
            if (rpcMethod.GetAttribute<DmtpRpcAttribute>() is DmtpRpcAttribute attribute)
            {
                this.ActionMap.Add(attribute.GetInvokeKey(rpcMethod), rpcMethod);
            }
        }
    }

    #region Config

    /// <inheritdoc/>
    public async Task OnDmtpHandshaking(IDmtpActorObject client, DmtpVerifyEventArgs e)
    {
        var dmtpRpcActor = this.CreateDmtpRpcActor(client.DmtpActor, this.m_rpcServerProvider, this.CreateDispatcher.Invoke(client.DmtpActor));

        dmtpRpcActor.SerializationSelector = this.SerializationSelector;
        dmtpRpcActor.GetInvokeMethod = this.GetInvokeMethod;

        dmtpRpcActor.SetProtocolFlags(this.StartProtocol);
        client.DmtpActor.TryAddActor<DmtpRpcActor>(dmtpRpcActor);

        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    public async Task OnDmtpReceived(IDmtpActorObject client, DmtpMessageEventArgs e)
    {
        var dmtpRpcActor = client.DmtpActor.GetActor<DmtpRpcActor>();
        if (dmtpRpcActor != null)
        {
            if (await dmtpRpcActor.InputReceivedData(e.DmtpMessage).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                e.Handled = true;
                return;
            }
        }
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Config
}