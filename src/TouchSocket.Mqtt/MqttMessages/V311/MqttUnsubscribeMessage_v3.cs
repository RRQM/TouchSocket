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
/// 表示 Mqtt 卸载消息。
/// </summary>
public sealed partial class MqttUnsubscribeMessage : MqttIdentifierMessage
{
    private readonly List<string> m_topicFilters = new List<string>();

    /// <summary>
    /// 初始化 <see cref="MqttUnsubscribeMessage"/> 类的新实例。
    /// </summary>
    /// <param name="topics">要取消订阅的主题。</param>
    public MqttUnsubscribeMessage(params string[] topics)
    {
        ThrowHelper.ThrowIfNull(topics, nameof(topics));

        if (topics.Length < 1)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(topics), topics.Length, 1);
        }

        this.m_topicFilters.AddRange(topics);
        this.SetFixedFlagsWith0010();
    }

    internal MqttUnsubscribeMessage()
    {
    }

    /// <inheritdoc/>
    public override MqttMessageType MessageType => MqttMessageType.Unsubscribe;

    /// <summary>
    /// 获取要取消订阅的主题过滤器列表。
    /// </summary>
    public IReadOnlyList<string> TopicFilters => this.m_topicFilters;

    /// <inheritdoc/>
    protected override void BuildVariableBodyWithMqtt3<TWriter>(ref TWriter writer)
    {
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, this.MessageId, EndianType.Big);
        foreach (var topicFilter in this.TopicFilters)
        {
            MqttExtension.WriteMqttInt16String(ref writer, topicFilter);
        }
    }

    /// <inheritdoc/>
    protected override int GetMinimumRemainingLength()
    {
        return 0;
    }

    /// <inheritdoc/>
    protected override void SetFlags(byte value)
    {
        base.SetFlags(value);
        this.ThrowIfFlagsNot0010();
    }

    /// <inheritdoc/>
    protected override void UnpackWithMqtt3<TReader>(ref TReader reader)
    {
        this.MessageId = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
        while (!this.EndOfByteBlock(reader))
        {
            this.m_topicFilters.Add(MqttExtension.ReadMqttInt16String(ref reader));
        }
    }
}