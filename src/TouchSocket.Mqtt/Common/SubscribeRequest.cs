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

namespace TouchSocket.Mqtt;

public readonly record struct SubscribeRequest
{
    /// <summary>
    /// 初始化 <see cref="SubscribeRequest"/> 类的新实例。
    /// </summary>
    /// <param name="topic">订阅的主题。</param>
    /// <param name="qosLevel">服务质量等级。</param>
    public SubscribeRequest(string topic, QosLevel qosLevel)
    {
        this.Topic = topic;
        this.m_options.SetQosLevel(0, qosLevel);
    }

    /// <summary>
    /// 初始化 <see cref="SubscribeRequest"/> 类的新实例。
    /// </summary>
    /// <param name="topic">订阅的主题。</param>
    /// <param name="options">选项字节。</param>
    public SubscribeRequest(string topic, byte options)
    {
        this.Topic = topic;
        this.m_options = options;
    }

    /// <summary>
    /// 初始化 <see cref="SubscribeRequest"/> 类的新实例。
    /// </summary>
    /// <param name="topic">订阅的主题。</param>
    /// <param name="qosLevel">服务质量等级。</param>
    /// <param name="noLocal">是否不接收本地发布的消息。</param>
    /// <param name="retainAsPublished">是否保留消息的发布状态。</param>
    /// <param name="retainHandling">保留消息的处理方式。</param>
    public SubscribeRequest(string topic, QosLevel qosLevel, bool noLocal, bool retainAsPublished, MqttRetainHandling retainHandling) : this(topic, qosLevel)
    {
        this.m_options.SetBit(2, noLocal);
        this.m_options.SetBit(3, retainAsPublished);
        this.m_options.SetRetainHandling(4, retainHandling);
    }

    private readonly byte m_options;

    /// <summary>
    /// 获取服务质量等级。
    /// </summary>
    public QosLevel QosLevel => this.m_options.GetQosLevel(0);
    /// <summary>
    /// 获取订阅的主题。
    /// </summary>
    public string Topic { get; }

    /// <summary>
    /// 获取是否不接收本地发布的消息。
    /// </summary>
    public bool NoLocal => this.m_options.GetBit(2);

    /// <summary>
    /// 获取是否保留消息的发布状态。
    /// </summary>
    public bool RetainAsPublished => this.m_options.GetBit(3);

    /// <summary>
    /// 获取保留消息的处理方式。
    /// </summary>
    public MqttRetainHandling RetainHandling => this.m_options.GetRetainHandling(4);
}