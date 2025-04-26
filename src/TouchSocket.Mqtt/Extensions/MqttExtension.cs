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
using System.Collections.Generic;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Mqtt;

/// <summary>
/// 提供用于 MQTT 操作的扩展方法。
/// </summary>
public static class MqttExtension
{
    #region ByteBlock

    private const uint VariableByteIntegerMaxValue = 268435455;

    /// <summary>
    /// 从字节块中读取 MQTT Int16 字符串。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的字符串。</returns>
    public static string ReadMqttInt16String<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock
    {
        return ReadMqttInt16String(ref byteBlock, out _);
    }

    /// <summary>
    /// 从字节块中读取 MQTT Int16 字符串。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="length">读取的字符串长度。</param>
    /// <returns>读取的字符串。</returns>
    public static string ReadMqttInt16String<TByteBlock>(ref TByteBlock byteBlock, out ushort length)
        where TByteBlock : IByteBlock
    {
        length = byteBlock.ReadUInt16(EndianType.Big);
        return byteBlock.ReadToSpan(length).ToString(Encoding.UTF8);
    }

    /// <summary>
    /// 从字节块中读取可变字节整数。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的可变字节整数。</returns>
    public static uint ReadVariableByteInteger<TByteBlock>(ref TByteBlock byteBlock)
        where TByteBlock : IByteBlock
    {
        var multiplier = 1;
        var value = 0;
        byte encodedByte;

        do
        {
            encodedByte = byteBlock.ReadByte();
            value += (encodedByte & 0x7F) * multiplier;
            multiplier *= 128;
        } while ((encodedByte & 0x80) == 0x80);

        return (uint)value;
    }

    /// <summary>
    /// 将 MQTT 固定报头写入字节块。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="packetType">MQTT 消息类型。</param>
    /// <param name="flags">要写入的标志。</param>
    public static void WriteMqttFixedHeader<TByteBlock>(ref TByteBlock byteBlock, MqttMessageType packetType, byte flags = 0)
        where TByteBlock : IByteBlock
    {
        var fixedHeader = (int)packetType << 4;
        fixedHeader |= flags;
        byteBlock.WriteByte((byte)fixedHeader);
    }

    /// <summary>
    /// 将 MQTT Int16 字符串写入字节块。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的字符串值。</param>
    /// <returns>写入的字符串长度。</returns>
    public static ushort WriteMqttInt16String<TByteBlock>(ref TByteBlock byteBlock, string value)
        where TByteBlock : IByteBlock
    {
        var pos = byteBlock.Position;
        byteBlock.Position += 2;
        byteBlock.WriteNormalString(value, Encoding.UTF8);
        var lastPos = byteBlock.Position;
        var len = byteBlock.Position - pos - 2;
        byteBlock.Position = pos;
        byteBlock.WriteUInt16((ushort)len, EndianType.Big);
        byteBlock.Position = lastPos;
        return (ushort)len;
    }

    /// <summary>
    /// 将可变字节整数写入字节块。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的值。</param>
    public static void WriteVariableByteInteger<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value > VariableByteIntegerMaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The value must be less than or equal to {VariableByteIntegerMaxValue}.");
        }
        do
        {
            var encodedByte = (byte)(value % 128);
            value /= 128;
            if (value > 0)
            {
                encodedByte |= 128;
            }
            byteBlock.WriteByte(encodedByte);
        } while (value > 0);
    }

    #endregion ByteBlock

    /// <summary>
    /// 获取字节的高 4 位。
    /// </summary>
    /// <param name="byte">要获取高 4 位的字节。</param>
    /// <returns>高 4 位。</returns>
    public static int GetHeight4(this byte @byte)
    {
        int height;
        height = (@byte & 0xf0) >> 4;
        return height;
    }

    /// <summary>
    /// 获取字节的低 4 位。
    /// </summary>
    /// <param name="byte">要获取低 4 位的字节。</param>
    /// <returns>低 4 位。</returns>
    public static int GetLow4(this byte @byte)
    {
        int low;
        low = @byte & 0x0f;//0x0f(00001111)
        return low;
    }

    /// <summary>
    /// 获取 MQTT Int16 字符串的长度。
    /// </summary>
    /// <param name="value">字符串值。</param>
    /// <returns>字符串的长度。</returns>
    public static ushort GetMqttInt16StringLength(string value)
    {
        if (value.IsNullOrEmpty())
        {
            return 2;
        }
        return (ushort)(Encoding.UTF8.GetByteCount(value) + 2);
    }

    public static int GetVariableByteIntegerCount(int value)
    {
        if (value > VariableByteIntegerMaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The value must be less than or equal to {VariableByteIntegerMaxValue}.");
        }

        var count = 0;
        do
        {
            var encodedByte = (byte)(value % 128);
            value /= 128;
            if (value > 0)
            {
                encodedByte |= 128;
            }
            count++;
        }
        while (value > 0);

        return count;
    }

    #region RetainHandling

    public static MqttRetainHandling GetRetainHandling(this byte @byte, int index)
    {
        return (MqttRetainHandling)((@byte.GetBit(index + 1) ? 1 : 0) * 2 + (@byte.GetBit(index) ? 1 : 0));
    }

    public static byte SetRetainHandling(this ref byte b, int bitNumber, MqttRetainHandling value)
    {
        b = (byte)(b & ~(3 << bitNumber) | (byte)value << bitNumber);
        return b;
    }

    #endregion RetainHandling

    /// <summary>
    /// 获取最大 QoS 级别。
    /// </summary>
    /// <param name="qosLevel1">第一个 QoS 级别。</param>
    /// <param name="qosLevel2">第二个 QoS 级别。</param>
    /// <returns>最大 QoS 级别。</returns>
    public static QosLevel MaxQosLevel(QosLevel qosLevel1, QosLevel qosLevel2)
    {
        return qosLevel1 > qosLevel2 ? qosLevel1 : qosLevel2;
    }

    /// <summary>
    /// 获取最小 QoS 级别。
    /// </summary>
    /// <param name="qosLevel1">第一个 QoS 级别。</param>
    /// <param name="qosLevel2">第二个 QoS 级别。</param>
    /// <returns>最小 QoS 级别。</returns>
    public static QosLevel MinQosLevel(QosLevel qosLevel1, QosLevel qosLevel2)
    {
        return qosLevel1 < qosLevel2 ? qosLevel1 : qosLevel2;
    }

    /// <summary>
    /// 从字节块中读取 MQTT 二进制数据。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的二进制数据。</returns>
    public static byte[] ReadMqttBinaryData<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        var length = byteBlock.ReadUInt16(EndianType.Big);
        var data = new byte[length];
        var r = byteBlock.Read(data);
        if (r != length)
        {
            throw new Exception();
        }
        return data;
    }

    #region QosLevel

    /// <summary>
    /// 从字节的指定索引获取 QoS 级别。
    /// </summary>
    /// <param name="byte">要获取 QoS 级别的字节。</param>
    /// <param name="index">要获取 QoS 级别的索引。</param>
    /// <returns>QoS 级别。</returns>
    public static QosLevel GetQosLevel(this byte @byte, int index)
    {
        return (QosLevel)((@byte.GetBit(index + 1) ? 1 : 0) * 2 + (@byte.GetBit(index) ? 1 : 0));
    }

    /// <summary>
    /// 在指定位号设置字节中的 QoS 级别。
    /// </summary>
    /// <param name="b">要设置 QoS 级别的字节。</param>
    /// <param name="bitNumber">要设置 QoS 级别的位号。</param>
    /// <param name="value">要设置的 QoS 级别。</param>
    public static byte SetQosLevel(this ref byte b, int bitNumber, QosLevel value)
    {
        b = (byte)(b & ~(3 << bitNumber) | (byte)value << bitNumber);
        return b;
    }

    #endregion QosLevel

    /// <summary>
    /// 将缓冲区转换为 MQTT Int16 值。
    /// </summary>
    /// <param name="buffer">要转换的缓冲区。</param>
    /// <param name="offset">开始转换的偏移量。</param>
    /// <returns>转换后的 Int16 值。</returns>
    public static short ToMqttInt16(this byte[] buffer, int offset)
    {
        var data = new byte[] { buffer[offset + 1], buffer[offset] };
        return BitConverter.ToInt16(data, 0);
    }

    #region MqttV5Properties

    public static void WriteAssignedClientIdentifier<TByteBlock>(ref TByteBlock byteBlock, string value) where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.AssignedClientIdentifier, value);
    }

    /// <summary>
    /// 写入认证数据。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">认证数据的值。</param>
    public static void WriteAuthenticationData<TByteBlock>(ref TByteBlock byteBlock, ReadOnlySpan<byte> value)
        where TByteBlock : IByteBlock
    {
        if (value.IsEmpty)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.AuthenticationData);
        byteBlock.WriteUInt16((ushort)value.Length, EndianType.Big);
        byteBlock.Write(value);
    }

    /// <summary>
    /// 写入认证方法。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">认证方法的值。</param>
    public static void WriteAuthenticationMethod<TByteBlock>(ref TByteBlock byteBlock, string value)
        where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.AuthenticationMethod, value);
    }

    /// <summary>
    /// 写入内容类型。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">内容类型的值。</param>
    public static void WriteContentType<TByteBlock>(ref TByteBlock byteBlock, string value)
        where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.ContentType, value);
    }

    /// <summary>
    /// 写入相关性数据。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">相关性数据的值。</param>
    public static void WriteCorrelationData<TByteBlock>(ref TByteBlock byteBlock, ReadOnlySpan<byte> value)
        where TByteBlock : IByteBlock
    {
        if (value.IsEmpty)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.CorrelationData);
        byteBlock.WriteUInt16((ushort)value.Length, EndianType.Big);
        byteBlock.Write(value);
    }

    /// <summary>
    /// 写入最大数据包大小。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">最大数据包大小的值。</param>
    public static void WriteMaximumPacketSize<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.MaximumPacketSize);
        byteBlock.WriteUInt32(value, EndianType.Big);
    }

    public static void WriteMaximumQoS<TByteBlock>(ref TByteBlock byteBlock, QosLevel value) where TByteBlock : IByteBlock
    {
        if (value == QosLevel.ExactlyOnce)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.MaximumQoS);
        byteBlock.WriteByte((byte)value);
    }

    /// <summary>
    /// 写入消息过期间隔。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">消息过期间隔的值。</param>
    public static void WriteMessageExpiryInterval<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.MessageExpiryInterval);
        byteBlock.WriteUInt32(value, EndianType.Big);
    }

    /// <summary>
    /// 写入负载格式指示符。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">负载格式指示符的值。</param>
    public static void WritePayloadFormatIndicator<TByteBlock>(ref TByteBlock byteBlock, MqttPayloadFormatIndicator value)
        where TByteBlock : IByteBlock
    {
        if (value == MqttPayloadFormatIndicator.Unspecified)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.PayloadFormatIndicator);
        byteBlock.WriteByte((byte)value);
    }

    public static void WriteReasonString<TByteBlock>(ref TByteBlock byteBlock, string value) where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.ReasonString, value);
    }

    /// <summary>
    /// 写入接收最大值。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">接收最大值的值。</param>
    public static void WriteReceiveMaximum<TByteBlock>(ref TByteBlock byteBlock, ushort value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.ReceiveMaximum);
        byteBlock.WriteUInt16(value, EndianType.Big);
    }

    /// <summary>
    /// 写入请求问题信息。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">请求问题信息的值。</param>
    public static void WriteRequestProblemInformation<TByteBlock>(ref TByteBlock byteBlock, bool value)
        where TByteBlock : IByteBlock
    {
        if (value)
        {
            byteBlock.WriteByte((byte)MqttPropertyId.RequestProblemInformation);
            byteBlock.WriteByte(1);
        }
    }

    /// <summary>
    /// 写入请求问题信息。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">请求问题信息的值。</param>
    public static void WriteRequestProblemInformation<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.RequestProblemInformation);
        byteBlock.WriteUInt32(value, EndianType.Big);
    }

    /// <summary>
    /// 写入请求响应信息。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">请求响应信息的值。</param>
    public static void WriteRequestResponseInformation<TByteBlock>(ref TByteBlock byteBlock, bool value)
        where TByteBlock : IByteBlock
    {
        if (value)
        {
            byteBlock.WriteByte((byte)MqttPropertyId.RequestResponseInformation);
            byteBlock.WriteByte(1);
        }
    }

    /// <summary>
    /// 写入请求响应信息。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">请求响应信息的值。</param>
    public static void WriteRequestResponseInformation<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.RequestResponseInformation);
        byteBlock.WriteUInt32(value, EndianType.Big);
    }

    public static void WriteResponseInformation<TByteBlock>(ref TByteBlock byteBlock, string value) where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.ResponseInformation, value);
    }

    /// <summary>
    /// 写入响应主题。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">响应主题的值。</param>
    public static void WriteResponseTopic<TByteBlock>(ref TByteBlock byteBlock, string value)
        where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.ResponseTopic, value);
    }

    public static void WriteRetainAvailable<TByteBlock>(ref TByteBlock byteBlock, bool value) where TByteBlock : IByteBlock
    {
        if (value)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.RetainAvailable);
        byteBlock.WriteByte(0);
    }

    public static void WriteServerKeepAlive<TByteBlock>(ref TByteBlock byteBlock, ushort value) where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.ServerKeepAlive);
        byteBlock.WriteUInt16(value);
    }

    public static void WriteServerReference<TByteBlock>(ref TByteBlock byteBlock, string value) where TByteBlock : IByteBlock
    {
        WriteStringProperty(ref byteBlock, MqttPropertyId.ServerReference, value);
    }

    /// <summary>
    /// 写入会话过期间隔。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">会话过期间隔的值。</param>
    public static void WriteSessionExpiryInterval<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.SessionExpiryInterval);
        byteBlock.WriteUInt32(value, EndianType.Big);
    }

    public static void WriteSharedSubscriptionAvailable<TByteBlock>(ref TByteBlock byteBlock, bool value) where TByteBlock : IByteBlock
    {
        if (value)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.SharedSubscriptionAvailable);
        byteBlock.WriteByte(0);
    }

    public static void WriteSubscriptionIdentifier<TByteBlock>(ref TByteBlock byteBlock, uint subscriptionIdentifier) where TByteBlock : IByteBlock
    {
        byteBlock.WriteByte((byte)MqttPropertyId.SubscriptionIdentifier);
        WriteVariableByteInteger(ref byteBlock, subscriptionIdentifier);
    }

    public static void WriteSubscriptionIdentifiersAvailable<TByteBlock>(ref TByteBlock byteBlock, bool value) where TByteBlock : IByteBlock
    {
        if (value)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.SubscriptionIdentifiersAvailable);
        byteBlock.WriteByte(0);
    }

    /// <summary>
    /// 写入主题别名最大值。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">主题别名最大值的值。</param>
    public static void WriteTopicAliasMaximum<TByteBlock>(ref TByteBlock byteBlock, ushort value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.TopicAliasMaximum);
        byteBlock.WriteUInt16(value, EndianType.Big);
    }

    public static void WriteUserProperties<TByteBlock>(ref TByteBlock byteBlock, IReadOnlyList<MqttUserProperty> userProperties) where TByteBlock : IByteBlock
    {
        if (userProperties is null)
        {
            return;
        }
        foreach (var userProperty in userProperties)
        {
            WriteUserProperty(ref byteBlock, userProperty.Name, userProperty.Value);
        }
    }

    /// <summary>
    /// 写入用户属性。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="name">用户属性的名称。</param>
    /// <param name="value">用户属性的值。</param>
    public static void WriteUserProperty<TByteBlock>(ref TByteBlock byteBlock, string name, string value)
        where TByteBlock : IByteBlock
    {
        if (name.IsNullOrEmpty() || value.IsNullOrEmpty())
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.UserProperty);
        WriteMqttInt16String(ref byteBlock, name);
        WriteMqttInt16String(ref byteBlock, value);
    }

    public static void WriteWildcardSubscriptionAvailable<TByteBlock>(ref TByteBlock byteBlock, bool value) where TByteBlock : IByteBlock
    {
        if (value)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.WildcardSubscriptionAvailable);
        byteBlock.WriteByte(0);
    }

    /// <summary>
    /// 写入遗嘱延迟时间间隔。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块的类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">遗嘱延迟时间间隔的值。</param>
    public static void WriteWillDelayInterval<TByteBlock>(ref TByteBlock byteBlock, uint value)
        where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.WillDelayInterval);
        byteBlock.WriteUInt32(value, EndianType.Big);
    }

    private static void WriteStringProperty<TByteBlock>(ref TByteBlock byteBlock, MqttPropertyId propertyId, string value)
        where TByteBlock : IByteBlock
    {
        if (value.IsNullOrEmpty())
        {
            return;
        }
        byteBlock.WriteByte((byte)propertyId);
        WriteMqttInt16String(ref byteBlock, value);
    }

    public static void WriteTopicAlias<TByteBlock>(ref TByteBlock byteBlock, ushort value) where TByteBlock : IByteBlock
    {
        if (value == 0)
        {
            return;
        }
        byteBlock.WriteByte((byte)MqttPropertyId.TopicAlias);
        byteBlock.WriteUInt16(value, EndianType.Big);
    }

    #endregion MqttV5Properties
}