// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// DmtpRpc配置选项
/// </summary>
public class DmtpRpcOption : DmtpFeatureOption
{
    internal readonly GlobalQueueRpcDispatcher m_globalQueueRpcDispatcher = new();

    public DmtpRpcOption()
    {
        this.StartProtocol = 20;
    }

    /// <summary>
    /// 创建RPC调度器的委托
    /// </summary>
    public Func<IDmtpActor, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>> CreateDispatcher { get; set; }

    /// <summary>
    /// 创建DmtpRpc实例的委托
    /// </summary>
    public Func<IDmtpActor, IRpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>, DmtpRpcActor> CreateDmtpRpcActor { get; set; }

    /// <summary>
    /// 序列化选择器，默认使用<see cref="DefaultSerializationSelector"/>
    /// </summary>
    public ISerializationSelector SerializationSelector { get; set; } = new DefaultSerializationSelector();

    /// <summary>
    /// 配置默认的序列化选择器
    /// </summary>
    /// <param name="selector">用于配置默认序列化选择器的操作</param>
    public void ConfigureDefaultSerializationSelector(Action<DefaultSerializationSelector> selector)
    {
        var serializationSelector = new DefaultSerializationSelector();
        selector.Invoke(serializationSelector);
        this.SerializationSelector = serializationSelector;
    }

    /// <summary>
    /// 设置创建DmtpRpc实例的委托
    /// </summary>
    /// <param name="createDmtpRpcActor">创建DmtpRpc实例的委托</param>
    public void SetCreateDmtpRpcActor(Func<IDmtpActor, IRpcServerProvider, IRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>, DmtpRpcActor> createDmtpRpcActor)
    {
        this.CreateDmtpRpcActor = createDmtpRpcActor;
    }

    /// <summary>
    /// 使用并发调度器处理请求
    /// </summary>
    public void UseConcurrencyDispatcher()
    {
        this.CreateDispatcher = (actor) => new ConcurrencyRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();
    }

    /// <summary>
    /// 使用全局队列RPC调度器配置RPC特性
    /// </summary>
    public void UseGlobalQueueRpcDispatcher()
    {
        this.CreateDispatcher = (actor) => this.m_globalQueueRpcDispatcher;
    }

    /// <summary>
    /// 使用即时RPC调度器配置RPC特性
    /// </summary>
    public void UseImmediateRpcDispatcher()
    {
        this.CreateDispatcher = (actor) => new ImmediateRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();
    }

    /// <summary>
    /// 使用队列RPC调度器配置RPC特性
    /// </summary>
    public void UseQueueRpcDispatcher()
    {
        this.CreateDispatcher = (actor) => new QueueRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>();
    }

    internal class GlobalQueueRpcDispatcher : QueueRpcDispatcher<IDmtpActor, IDmtpRpcCallContext>
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
}