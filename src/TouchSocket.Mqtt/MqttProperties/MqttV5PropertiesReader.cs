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

using System.Diagnostics;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 读取Mqtt v5属性的类。
/// </summary>
/// <typeparam name="TByteBlock">实现IByteBlock接口的类型。</typeparam>
public readonly ref struct MqttV5PropertiesReader<TByteBlock> where TByteBlock : IByteBlock
{
    private readonly int m_endPosition;

    /// <summary>
    /// 初始化MqttV5PropertiesReader类的新实例。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    public MqttV5PropertiesReader(ref TByteBlock byteBlock)
    {
        var length = MqttExtension.ReadVariableByteInteger(ref byteBlock);
        this.m_endPosition = (int)(byteBlock.Position + length);
    }

    /// <summary>
    /// 尝试读取属性标识符。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <param name="identifier">读取的属性标识符。</param>
    /// <returns>如果读取成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public bool TryRead(ref TByteBlock byteBlock, out MqttPropertyId identifier)
    {
        if (byteBlock.Position >= this.m_endPosition)
        {
            identifier = MqttPropertyId.None;
            return false;
        }
        identifier = (MqttPropertyId)byteBlock.ReadByte();
        Debug.WriteLine($"PropertyId:{identifier}");
        return true;
    }

    /// <summary>
    /// 读取有效载荷格式指示符。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>有效载荷格式指示符。</returns>
    public MqttPayloadFormatIndicator ReadPayloadFormatIndicator(ref TByteBlock byteBlock)
    {
        return (MqttPayloadFormatIndicator)byteBlock.ReadByte();
    }

    /// <summary>
    /// 读取消息过期间隔。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>消息过期间隔。</returns>
    public uint ReadMessageExpiryInterval(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt32(EndianType.Big);
    }

    /// <summary>
    /// 读取内容类型。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>内容类型。</returns>
    public string ReadContentType(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取响应主题。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>响应主题。</returns>
    public string ReadResponseTopic(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取关联数据。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>关联数据。</returns>
    public byte[] ReadCorrelationData(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttBinaryData(ref byteBlock);
    }

    /// <summary>
    /// 读取订阅标识符。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>订阅标识符。</returns>
    public uint ReadSubscriptionIdentifier(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadVariableByteInteger(ref byteBlock);
    }

    /// <summary>
    /// 读取会话过期间隔。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>会话过期间隔。</returns>
    public uint ReadSessionExpiryInterval(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt32(EndianType.Big);
    }

    /// <summary>
    /// 读取分配的客户端标识符。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>分配的客户端标识符。</returns>
    public string ReadAssignedClientIdentifier(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取服务器保持连接时间。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>服务器保持连接时间。</returns>
    public ushort ReadServerKeepAlive(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt16(EndianType.Big);
    }

    /// <summary>
    /// 读取认证方法。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>认证方法。</returns>
    public string ReadAuthenticationMethod(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取认证数据。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>认证数据。</returns>
    public byte[] ReadAuthenticationData(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttBinaryData(ref byteBlock);
    }

    /// <summary>
    /// 读取请求问题信息标志。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>请求问题信息标志。</returns>
    public bool ReadRequestProblemInformation(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadByte() == 1;
    }

    /// <summary>
    /// 读取遗嘱延迟间隔。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>遗嘱延迟间隔。</returns>
    public uint ReadWillDelayInterval(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt32(EndianType.Big);
    }

    /// <summary>
    /// 读取请求响应信息标志。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>请求响应信息标志。</returns>
    public bool ReadRequestResponseInformation(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadByte() == 1;
    }

    /// <summary>
    /// 读取响应信息。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>响应信息。</returns>
    public string ReadResponseInformation(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取服务器引用。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>服务器引用。</returns>
    public string ReadServerReference(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取原因字符串。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>原因字符串。</returns>
    public string ReadReasonString(ref TByteBlock byteBlock)
    {
        return MqttExtension.ReadMqttInt16String(ref byteBlock);
    }

    /// <summary>
    /// 读取接收最大值。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>接收最大值。</returns>
    public ushort ReadReceiveMaximum(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt16(EndianType.Big);
    }

    /// <summary>
    /// 读取主题别名最大值。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>主题别名最大值。</returns>
    public ushort ReadTopicAliasMaximum(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt16(EndianType.Big);
    }

    /// <summary>
    /// 读取主题别名。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>主题别名。</returns>
    public ushort ReadTopicAlias(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt16(EndianType.Big);
    }

    /// <summary>
    /// 读取最大服务质量。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>最大服务质量。</returns>
    public QosLevel ReadMaximumQoS(ref TByteBlock byteBlock)
    {
        return (QosLevel)byteBlock.ReadByte();
    }

    /// <summary>
    /// 读取保留可用标志。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>保留可用标志。</returns>
    public bool ReadRetainAvailable(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadByte() == 1;
    }

    /// <summary>
    /// 读取用户属性。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>用户属性。</returns>
    public MqttUserProperty ReadUserProperty(ref TByteBlock byteBlock)
    {
        var name = MqttExtension.ReadMqttInt16String(ref byteBlock);
        var value = MqttExtension.ReadMqttInt16String(ref byteBlock);
        return new MqttUserProperty(name, value);
    }

    /// <summary>
    /// 读取最大数据包大小。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>最大数据包大小。</returns>
    public uint ReadMaximumPacketSize(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadUInt32(EndianType.Big);
    }

    /// <summary>
    /// 读取通配符订阅可用标志。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>通配符订阅可用标志。</returns>
    public bool ReadWildcardSubscriptionAvailable(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadByte() == 1;
    }

    /// <summary>
    /// 读取订阅标识符可用标志。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>订阅标识符可用标志。</returns>
    public bool ReadSubscriptionIdentifiersAvailable(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadByte() == 1;
    }

    /// <summary>
    /// 读取共享订阅可用标志。
    /// </summary>
    /// <param name="byteBlock">字节块引用。</param>
    /// <returns>共享订阅可用标志。</returns>
    public bool ReadSharedSubscriptionAvailable(ref TByteBlock byteBlock)
    {
        return byteBlock.ReadByte() == 1;
    }
}


///// <summary>
///// 读取Mqtt v5属性的类。
///// </summary>
///// <typeparam name="TByteBlock">实现IByteBlock接口的类型。</typeparam>
//public readonly struct MqttV5PropertiesReader<TByteBlock> where TByteBlock : IByteBlock
//{
//    /// <summary>
//    /// 初始化MqttV5PropertiesReader类的新实例。
//    /// </summary>
//    /// <param name="byteBlock">字节块引用。</param>
//    public MqttV5PropertiesReader(ref TByteBlock byteBlock)
//    {
//        var length = MqttExtension.ReadVariableByteInteger(ref byteBlock);
//        var endPosition = byteBlock.Position + length;
//        var properties = new List<MqttUserProperty>();
//        while (byteBlock.Position < endPosition)
//        {
//            var identifier = (MqttPropertyId)byteBlock.ReadByte();
//            Debug.WriteLine($"PropertyId:{identifier}");
//            switch (identifier)
//            {
//                case MqttPropertyId.None:
//                    break;

//                case MqttPropertyId.PayloadFormatIndicator:
//                    {
//                        this.PayloadFormatIndicator = (MqttPayloadFormatIndicator)byteBlock.ReadByte();
//                    }
//                    break;

//                case MqttPropertyId.MessageExpiryInterval:
//                    {
//                        this.MessageExpiryInterval = byteBlock.ReadUInt32(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.ContentType:
//                    {
//                        this.ContentType = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.ResponseTopic:
//                    {
//                        this.ResponseTopic = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.CorrelationData:
//                    {
//                        this.CorrelationData = MqttExtension.ReadMqttBinaryData(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.SubscriptionIdentifier:
//                    {
//                        this.SubscriptionIdentifier = MqttExtension.ReadVariableByteInteger(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.SessionExpiryInterval:
//                    {
//                        this.SessionExpiryInterval = byteBlock.ReadUInt32(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.AssignedClientIdentifier:
//                    {
//                        this.AssignedClientIdentifier = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.ServerKeepAlive:
//                    {
//                        this.ServerKeepAlive = byteBlock.ReadUInt16(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.AuthenticationMethod:
//                    {
//                        this.AuthenticationMethod = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.AuthenticationData:
//                    {
//                        this.AuthenticationData = MqttExtension.ReadMqttBinaryData(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.RequestProblemInformation:
//                    {
//                        this.RequestProblemInformation = byteBlock.ReadByte() == 1;
//                    }
//                    break;

//                case MqttPropertyId.WillDelayInterval:
//                    {
//                        this.WillDelayInterval = byteBlock.ReadUInt32(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.RequestResponseInformation:
//                    {
//                        this.RequestResponseInformation = byteBlock.ReadByte() == 1;
//                    }
//                    break;

//                case MqttPropertyId.ResponseInformation:
//                    {
//                        this.ResponseInformation = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.ServerReference:
//                    {
//                        this.ServerReference = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.ReasonString:
//                    {
//                        this.ReasonString = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                    }
//                    break;

//                case MqttPropertyId.ReceiveMaximum:
//                    {
//                        this.ReceiveMaximum = byteBlock.ReadUInt16(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.TopicAliasMaximum:
//                    {
//                        this.TopicAliasMaximum = byteBlock.ReadUInt16(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.TopicAlias:
//                    {
//                        this.TopicAlias = byteBlock.ReadUInt16(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.MaximumQoS:
//                    {
//                        this.MaximumQoS = (QosLevel)byteBlock.ReadByte();
//                    }
//                    break;

//                case MqttPropertyId.RetainAvailable:
//                    {
//                        this.RetainAvailable = byteBlock.ReadByte() == 1;
//                    }
//                    break;

//                case MqttPropertyId.UserProperty:
//                    {
//                        var name = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                        var value = MqttExtension.ReadMqttInt16String(ref byteBlock);
//                        properties.Add(new MqttUserProperty(name, value));
//                    }
//                    break;

//                case MqttPropertyId.MaximumPacketSize:
//                    {
//                        this.MaximumPacketSize = byteBlock.ReadUInt32(EndianType.Big);
//                    }
//                    break;

//                case MqttPropertyId.WildcardSubscriptionAvailable:
//                    {
//                        this.WildcardSubscriptionAvailable = byteBlock.ReadByte() == 1;
//                    }
//                    break;

//                case MqttPropertyId.SubscriptionIdentifiersAvailable:
//                    {
//                        this.SubscriptionIdentifiersAvailable = byteBlock.ReadByte() == 1;
//                    }
//                    break;

//                case MqttPropertyId.SharedSubscriptionAvailable:
//                    {
//                        this.SharedSubscriptionAvailable = byteBlock.ReadByte() == 1;
//                    }
//                    break;

//                default:
//                    break;
//            }
//        }

//        this.UserProperties = properties;
//    }

//    /// <summary>
//    /// 获取分配的客户端标识符。
//    /// </summary>
//    public string AssignedClientIdentifier { get; }

//    /// <summary>
//    /// 获取认证数据。
//    /// </summary>
//    public byte[] AuthenticationData { get; }

//    /// <summary>
//    /// 获取认证方法。
//    /// </summary>
//    public string AuthenticationMethod { get; }

//    /// <summary>
//    /// 获取内容类型。
//    /// </summary>
//    public string ContentType { get; }

//    /// <summary>
//    /// 获取关联数据。
//    /// </summary>
//    public byte[] CorrelationData { get; }

//    /// <summary>
//    /// 获取最大数据包大小。
//    /// </summary>
//    public uint MaximumPacketSize { get; }

//    /// <summary>
//    /// 获取最大服务质量。
//    /// </summary>
//    public QosLevel MaximumQoS { get; }

//    /// <summary>
//    /// 获取消息过期间隔。
//    /// </summary>
//    public uint MessageExpiryInterval { get; }

//    /// <summary>
//    /// 获取有效载荷格式指示符。
//    /// </summary>
//    public MqttPayloadFormatIndicator PayloadFormatIndicator { get; }

//    /// <summary>
//    /// 获取原因字符串。
//    /// </summary>
//    public string ReasonString { get; }

//    /// <summary>
//    /// 获取接收最大值。
//    /// </summary>
//    public ushort ReceiveMaximum { get; }

//    /// <summary>
//    /// 获取请求问题信息标志。
//    /// </summary>
//    public bool RequestProblemInformation { get; }

//    /// <summary>
//    /// 获取请求响应信息标志。
//    /// </summary>
//    public bool RequestResponseInformation { get; }

//    /// <summary>
//    /// 获取响应信息。
//    /// </summary>
//    public string ResponseInformation { get; }

//    /// <summary>
//    /// 获取响应主题。
//    /// </summary>
//    public string ResponseTopic { get; }

//    /// <summary>
//    /// 获取保留可用标志。
//    /// </summary>
//    public bool RetainAvailable { get; }

//    /// <summary>
//    /// 获取服务器保持连接时间。
//    /// </summary>
//    public ushort ServerKeepAlive { get; }

//    /// <summary>
//    /// 获取服务器引用。
//    /// </summary>
//    public string ServerReference { get; }

//    /// <summary>
//    /// 获取会话过期间隔。
//    /// </summary>
//    public uint SessionExpiryInterval { get; }

//    /// <summary>
//    /// 获取共享订阅可用标志。
//    /// </summary>
//    public bool SharedSubscriptionAvailable { get; }

//    /// <summary>
//    /// 获取订阅标识符。
//    /// </summary>
//    public uint SubscriptionIdentifier { get; }

//    /// <summary>
//    /// 获取订阅标识符可用标志。
//    /// </summary>
//    public bool SubscriptionIdentifiersAvailable { get; }

//    /// <summary>
//    /// 获取主题别名。
//    /// </summary>
//    public ushort TopicAlias { get; }

//    /// <summary>
//    /// 获取主题别名最大值。
//    /// </summary>
//    public ushort TopicAliasMaximum { get; }

//    /// <summary>
//    /// 获取用户属性列表。
//    /// </summary>
//    public IReadOnlyList<MqttUserProperty> UserProperties { get; }

//    /// <summary>
//    /// 获取通配符订阅可用标志。
//    /// </summary>
//    public bool WildcardSubscriptionAvailable { get; }

//    /// <summary>
//    /// 获取遗嘱延迟间隔。
//    /// </summary>
//    public uint WillDelayInterval { get; }
//}