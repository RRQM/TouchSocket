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

/// <summary>
/// 表示一个分发消息的类。
/// </summary>
sealed class DistributeMessage : IDisposable
{
    /// <summary>
    /// 初始化 <see cref="DistributeMessage"/> 类的新实例。
    /// </summary>
    /// <param name="topicName">主题名称。</param>
    /// <param name="retain">是否保留消息。</param>
    /// <param name="qosLevel">服务质量级别。</param>
    /// <param name="sharedPayload">共享的有效负载。</param>
    public DistributeMessage(string topicName, bool retain, QosLevel qosLevel, SharedPayload sharedPayload)
    {
        this.TopicName = topicName;
        this.Retain = retain;
        this.QosLevel = qosLevel;
        this.SharedPayload = sharedPayload;
    }

    /// <summary>
    /// 获取主题名称。
    /// </summary>
    public string TopicName { get; }

    /// <summary>
    /// 获取一个值，该值指示消息是否被保留。
    /// </summary>
    public bool Retain { get; }

    /// <summary>
    /// 获取服务质量级别。
    /// </summary>
    public QosLevel QosLevel { get; }

    /// <summary>
    /// 获取共享的有效负载。
    /// </summary>
    public SharedPayload SharedPayload { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.SharedPayload.Dispose();
    }
}
