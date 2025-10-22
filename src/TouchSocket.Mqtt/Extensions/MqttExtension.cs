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
/// 提供用于MQTT协议操作的扩展方法集合。
/// </summary>
/// <remarks>
/// 此类包含了MQTT协议中常用的数据读写、编码解码和协议处理相关的扩展方法，
/// 包括可变字节整数处理、固定头部操作、字符串读写、QoS级别处理以及MQTT v5.0属性操作等。
/// </remarks>
public static class MqttExtension
{
    #region ByteBlock

    private const uint VariableByteIntegerMaxValue = 268435455;

    /// <summary>
    /// 从字节读取器中读取MQTT Int16字符串。
    /// </summary>
    /// <typeparam name="TReader">字节读取器的类型，必须实现<see cref="IBytesReader"/>接口。</typeparam>
    /// <param name="reader">要读取的字节读取器。</param>
    /// <returns>读取的字符串内容。</returns>
    /// <remarks>
    /// MQTT Int16字符串格式：前2个字节表示字符串长度（大端字节序），后跟UTF-8编码的字符串内容。
    /// </remarks>
    public static string ReadMqttInt16String<TReader>(ref TReader reader)
        where TReader : IBytesReader

    {
        return ReadMqttInt16String(ref reader, out _);
    }

    /// <summary>
    /// 从字节读取器中读取MQTT Int16字符串，并输出字符串长度。
    /// </summary>
    /// <typeparam name="TReader">字节读取器的类型，必须实现<see cref="IBytesReader"/>接口。</typeparam>
    /// <param name="reader">要读取的字节读取器。</param>
    /// <param name="length">输出参数，返回读取的字符串字节长度。</param>
    /// <returns>读取的字符串内容。</returns>
    /// <remarks>
    /// MQTT Int16字符串格式：前2个字节表示字符串长度（大端字节序），后跟UTF-8编码的字符串内容。
    /// 如果长度为0，则返回空字符串。
    /// </remarks>
    public static string ReadMqttInt16String<TReader>(ref TReader reader, out ushort length)
        where TReader : IBytesReader

    {
        length = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
        if (length == 0)
        {
            return string.Empty;
        }
        var span = reader.GetSpan(length).Slice(0, length);

        var str = span.ToString(Encoding.UTF8);
        reader.Advance(length);
        return str;
    }

    public static ReadOnlyMemory<byte> ReadMqttInt16Memory<TReader>(ref TReader reader)
        where TReader : IBytesReader

    {
        var length = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
        if (length == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }
        var span = reader.GetSpan(length).Slice(0, length);
        var memory = new byte[length];
        span.CopyTo(memory);
        reader.Advance(length);
        return new ReadOnlyMemory<byte>(memory);
    }

    /// <summary>
    /// 从字节读取器中读取MQTT可变字节整数。
    /// </summary>
    /// <typeparam name="TReader">字节读取器的类型，必须实现<see cref="IBytesReader"/>接口。</typeparam>
    /// <param name="reader">要读取的字节读取器。</param>
    /// <returns>读取的可变字节整数值。</returns>
    /// <remarks>
    /// MQTT可变字节整数使用1-4个字节编码，每个字节的最高位表示是否有后续字节，
    /// 低7位存储实际数据。最大值为268,435,455。
    /// </remarks>
    public static uint ReadVariableByteInteger<TReader>(ref TReader reader)
        where TReader : IBytesReader

    {
        var multiplier = 1;
        var value = 0;
        byte encodedByte;

        do
        {
            encodedByte = ReaderExtension.ReadValue<TReader, byte>(ref reader);
            value += (encodedByte & 0x7F) * multiplier;
            multiplier *= 128;
        } while ((encodedByte & 0x80) == 0x80);

        return (uint)value;
    }

    /// <summary>
    /// 将MQTT固定头部写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="packetType">MQTT消息类型。</param>
    /// <param name="flags">要写入的控制标志，默认为0。</param>
    /// <remarks>
    /// MQTT固定头部的第一个字节由消息类型（高4位）和控制标志（低4位）组成。
    /// </remarks>
    public static void WriteMqttFixedHeader<TWriter>(ref TWriter writer, MqttMessageType packetType, byte flags = 0)
        where TWriter : IBytesWriter

    {
        var fixedHeader = (int)packetType << 4;
        fixedHeader |= flags;
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)fixedHeader);
    }

    /// <summary>
    /// 将字符串以MQTT Int16格式写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">要写入的字符串值。</param>
    /// <returns>写入的字符串字节长度。</returns>
    /// <remarks>
    /// MQTT Int16字符串格式：先写入2字节的长度值（大端字节序），再写入UTF-8编码的字符串内容。
    /// </remarks>
    public static ushort WriteMqttInt16String<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        var pos = writer.WrittenCount;
        var span = writer.GetSpan(2);
        writer.Advance(2);

        WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
        var len = writer.WrittenCount - pos - 2;
        span.WriteValue<ushort>((ushort)len, EndianType.Big);
        return (ushort)len;
    }

    /// <summary>
    /// 以MQTT Int16格式将二进制数据写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，需实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">要写入的二进制数据。</param>
    /// <returns>写入的字节长度。</returns>
    public static ushort WriteMqttInt16Memory<TWriter>(ref TWriter writer, ReadOnlyMemory<byte> value)
        where TWriter : IBytesWriter

    {
        var pos = writer.WrittenCount;
        var span = writer.GetSpan(2);
        writer.Advance(2);
        writer.Write(value.Span);
        var len = writer.WrittenCount - pos - 2;
        span.WriteValue<ushort>((ushort)len, EndianType.Big);
        return (ushort)len;
    }

    /// <summary>
    /// 将可变字节整数写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">要写入的整数值。</param>
    /// <exception cref="ArgumentOutOfRangeException">当值超过268,435,455时抛出。</exception>
    /// <remarks>
    /// MQTT可变字节整数使用1-4个字节编码，每个字节的最高位表示是否有后续字节，
    /// 低7位存储实际数据。最大值为268,435,455。
    /// </remarks>
    public static void WriteVariableByteInteger<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

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
            WriterExtension.WriteValue<TWriter, byte>(ref writer, encodedByte);
        } while (value > 0);
    }

    #endregion ByteBlock

    /// <summary>
    /// 获取字节的高4位。
    /// </summary>
    /// <param name="byte">要获取高4位的字节。</param>
    /// <returns>字节的高4位值（0-15）。</returns>
    public static int GetHeight4(this byte @byte)
    {
        int height;
        height = (@byte & 0xf0) >> 4;
        return height;
    }

    /// <summary>
    /// 获取字节的低4位。
    /// </summary>
    /// <param name="byte">要获取低4位的字节。</param>
    /// <returns>字节的低4位值（0-15）。</returns>
    public static int GetLow4(this byte @byte)
    {
        int low;
        low = @byte & 0x0f;//0x0f(00001111)
        return low;
    }

    /// <summary>
    /// 获取MQTT Int16字符串在传输时所需的总字节长度。
    /// </summary>
    /// <param name="value">字符串值。</param>
    /// <returns>字符串的总传输长度（包括2字节长度前缀）。</returns>
    /// <remarks>
    /// 返回值包括2字节的长度前缀和UTF-8编码后的字符串内容长度。
    /// 如果字符串为空或null，则返回2（仅长度前缀）。
    /// </remarks>
    public static ushort GetMqttInt16StringLength(string value)
    {
        if (value.IsNullOrEmpty())
        {
            return 2;
        }
        return (ushort)(Encoding.UTF8.GetByteCount(value) + 2);
    }

    /// <summary>
    /// 获取可变字节整数编码后所需的字节数。
    /// </summary>
    /// <param name="value">要编码的整数值。</param>
    /// <returns>编码后需要的字节数（1-4字节）。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当值超过268,435,455时抛出。</exception>
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

    /// <summary>
    /// 从字节的指定位置获取保留消息处理方式。
    /// </summary>
    /// <param name="byte">包含保留处理信息的字节。</param>
    /// <param name="index">起始位索引。</param>
    /// <returns>保留消息处理方式。</returns>
    public static MqttRetainHandling GetRetainHandling(this byte @byte, int index)
    {
        return (MqttRetainHandling)((@byte.GetBit(index + 1) ? 1 : 0) * 2 + (@byte.GetBit(index) ? 1 : 0));
    }

    /// <summary>
    /// 在字节的指定位置设置保留消息处理方式。
    /// </summary>
    /// <param name="b">要设置的字节引用。</param>
    /// <param name="bitNumber">起始位索引。</param>
    /// <param name="value">要设置的保留消息处理方式。</param>
    /// <returns>设置后的字节值。</returns>
    public static byte SetRetainHandling(this ref byte b, int bitNumber, MqttRetainHandling value)
    {
        b = (byte)(b & ~(3 << bitNumber) | (byte)value << bitNumber);
        return b;
    }

    #endregion RetainHandling

    /// <summary>
    /// 获取两个QoS级别中的最大值。
    /// </summary>
    /// <param name="qosLevel1">第一个QoS级别。</param>
    /// <param name="qosLevel2">第二个QoS级别。</param>
    /// <returns>两个QoS级别中的最大值。</returns>
    public static QosLevel MaxQosLevel(QosLevel qosLevel1, QosLevel qosLevel2)
    {
        return qosLevel1 > qosLevel2 ? qosLevel1 : qosLevel2;
    }

    /// <summary>
    /// 获取两个QoS级别中的最小值。
    /// </summary>
    /// <param name="qosLevel1">第一个QoS级别。</param>
    /// <param name="qosLevel2">第二个QoS级别。</param>
    /// <returns>两个QoS级别中的最小值。</returns>
    public static QosLevel MinQosLevel(QosLevel qosLevel1, QosLevel qosLevel2)
    {
        return qosLevel1 < qosLevel2 ? qosLevel1 : qosLevel2;
    }

    /// <summary>
    /// 从字节读取器中读取MQTT二进制数据。
    /// </summary>
    /// <typeparam name="TReader">字节读取器的类型，必须实现<see cref="IBytesReader"/>接口。</typeparam>
    /// <param name="reader">要读取的字节读取器。</param>
    /// <returns>读取的二进制数据。</returns>
    /// <exception cref="Exception">当读取的字节数与预期长度不匹配时抛出。</exception>
    /// <remarks>
    /// MQTT二进制数据格式：前2个字节表示数据长度（大端字节序），后跟二进制数据内容。
    /// </remarks>
    public static ReadOnlyMemory<byte> ReadMqttBinaryData<TReader>(ref TReader reader) where TReader : IBytesReader
    {
        var length = ReaderExtension.ReadValue<TReader, ushort>(ref reader, EndianType.Big);
        var data = new byte[length];
        var r = reader.Read(data);
        if (r != length)
        {
            throw new Exception($"Expected {length} bytes, but received {r} bytes.");
        }
        return new ReadOnlyMemory<byte>(data);
    }

    #region QosLevel

    /// <summary>
    /// 从字节的指定索引获取QoS级别。
    /// </summary>
    /// <param name="byte">包含QoS信息的字节。</param>
    /// <param name="index">起始位索引。</param>
    /// <returns>解析出的QoS级别。</returns>
    public static QosLevel GetQosLevel(this byte @byte, int index)
    {
        return (QosLevel)((@byte.GetBit(index + 1) ? 1 : 0) * 2 + (@byte.GetBit(index) ? 1 : 0));
    }

    /// <summary>
    /// 在字节的指定位置设置QoS级别。
    /// </summary>
    /// <param name="b">要设置QoS级别的字节引用。</param>
    /// <param name="bitNumber">起始位索引。</param>
    /// <param name="value">要设置的QoS级别。</param>
    /// <returns>设置后的字节值。</returns>
    public static byte SetQosLevel(this ref byte b, int bitNumber, QosLevel value)
    {
        b = (byte)(b & ~(3 << bitNumber) | (byte)value << bitNumber);
        return b;
    }

    #endregion QosLevel

    /// <summary>
    /// 将字节数组转换为MQTT Int16值。
    /// </summary>
    /// <param name="buffer">包含数据的字节数组。</param>
    /// <param name="offset">开始转换的偏移量。</param>
    /// <returns>转换后的Int16值。</returns>
    /// <remarks>
    /// 按照大端字节序从指定偏移量读取2个字节并转换为Int16值。
    /// </remarks>
    public static short ToMqttInt16(this byte[] buffer, int offset)
    {
        var data = new byte[] { buffer[offset + 1], buffer[offset] };
        return BitConverter.ToInt16(data, 0);
    }

    #region MqttV5Properties

    /// <summary>
    /// 写入分配的客户端标识符属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">分配的客户端标识符值。</param>
    public static void WriteAssignedClientIdentifier<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.AssignedClientIdentifier, value);
    }

    /// <summary>
    /// 写入认证数据属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">认证数据的字节值。</param>
    /// <remarks>
    /// 如果认证数据为空，则不写入任何内容。
    /// </remarks>
    public static void WriteAuthenticationData<TWriter>(ref TWriter writer, ReadOnlySpan<byte> value)
        where TWriter : IBytesWriter

    {
        if (value.IsEmpty)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.AuthenticationData);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, (ushort)value.Length, EndianType.Big);
        writer.Write(value);
    }

    /// <summary>
    /// 写入认证方法属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">认证方法的字符串值。</param>
    public static void WriteAuthenticationMethod<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.AuthenticationMethod, value);
    }

    /// <summary>
    /// 写入内容类型属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">内容类型的字符串值。</param>
    public static void WriteContentType<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.ContentType, value);
    }

    /// <summary>
    /// 写入相关性数据属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">相关性数据的字节值。</param>
    /// <remarks>
    /// 如果相关性数据为空，则不写入任何内容。
    /// </remarks>
    public static void WriteCorrelationData<TWriter>(ref TWriter writer, ReadOnlySpan<byte> value)
        where TWriter : IBytesWriter

    {
        if (value.IsEmpty)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.CorrelationData);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, (ushort)value.Length, EndianType.Big);
        writer.Write(value);
    }

    /// <summary>
    /// 写入最大数据包大小属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">最大数据包大小值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteMaximumPacketSize<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.MaximumPacketSize);
        WriterExtension.WriteValue<TWriter, uint>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入最大QoS属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">最大QoS级别值。</param>
    /// <remarks>
    /// 如果值为<see cref="QosLevel.ExactlyOnce"/>，则不写入任何内容。
    /// </remarks>
    public static void WriteMaximumQoS<TWriter>(ref TWriter writer, QosLevel value)
        where TWriter : IBytesWriter

    {
        if (value == QosLevel.ExactlyOnce)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.MaximumQoS);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)value);
    }

    /// <summary>
    /// 写入消息过期间隔属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">消息过期间隔值（秒）。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteMessageExpiryInterval<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.MessageExpiryInterval);
        WriterExtension.WriteValue<TWriter, uint>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入负载格式指示符属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">负载格式指示符值。</param>
    /// <remarks>
    /// 如果值为<see cref="MqttPayloadFormatIndicator.Unspecified"/>，则不写入任何内容。
    /// </remarks>
    public static void WritePayloadFormatIndicator<TWriter>(ref TWriter writer, MqttPayloadFormatIndicator value)
        where TWriter : IBytesWriter

    {
        if (value == MqttPayloadFormatIndicator.Unspecified)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.PayloadFormatIndicator);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)value);
    }

    /// <summary>
    /// 写入原因字符串属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">原因字符串值。</param>
    public static void WriteReasonString<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.ReasonString, value);
    }

    /// <summary>
    /// 写入接收最大值属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">接收最大值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteReceiveMaximum<TWriter>(ref TWriter writer, ushort value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.ReceiveMaximum);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入请求问题信息属性（布尔值版本）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">是否请求问题信息。</param>
    /// <remarks>
    /// 只有当值为<see langword="true"/>时才写入属性。
    /// </remarks>
    public static void WriteRequestProblemInformation<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBytesWriter

    {
        if (value)
        {
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.RequestProblemInformation);
            WriterExtension.WriteValue<TWriter, byte>(ref writer, 1);
        }
    }

    /// <summary>
    /// 写入请求问题信息属性（整数值版本）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">请求问题信息的整数值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteRequestProblemInformation<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.RequestProblemInformation);
        WriterExtension.WriteValue<TWriter, uint>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入请求响应信息属性（布尔值版本）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">是否请求响应信息。</param>
    /// <remarks>
    /// 只有当值为<see langword="true"/>时才写入属性。
    /// </remarks>
    public static void WriteRequestResponseInformation<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBytesWriter

    {
        if (value)
        {
            WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.RequestResponseInformation);
            WriterExtension.WriteValue<TWriter, byte>(ref writer, 1);
        }
    }

    /// <summary>
    /// 写入请求响应信息属性（整数值版本）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">请求响应信息的整数值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteRequestResponseInformation<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.RequestResponseInformation);
        WriterExtension.WriteValue<TWriter, uint>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入响应信息属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">响应信息字符串值。</param>
    public static void WriteResponseInformation<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.ResponseInformation, value);
    }

    /// <summary>
    /// 写入响应主题属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">响应主题字符串值。</param>
    public static void WriteResponseTopic<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.ResponseTopic, value);
    }

    /// <summary>
    /// 写入保留可用属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">保留功能是否可用。</param>
    /// <remarks>
    /// 只有当值为<see langword="false"/>时才写入属性（写入0）。
    /// </remarks>
    public static void WriteRetainAvailable<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBytesWriter

    {
        if (value)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.RetainAvailable);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, 0);
    }

    /// <summary>
    /// 写入服务器保持连接时间属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">服务器保持连接时间值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteServerKeepAlive<TWriter>(ref TWriter writer, ushort value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.ServerKeepAlive);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, value);
    }

    /// <summary>
    /// 写入服务器引用属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">服务器引用字符串值。</param>
    public static void WriteServerReference<TWriter>(ref TWriter writer, string value)
        where TWriter : IBytesWriter

    {
        WriteStringProperty(ref writer, MqttPropertyId.ServerReference, value);
    }

    /// <summary>
    /// 写入会话过期间隔属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">会话过期间隔值（秒）。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteSessionExpiryInterval<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.SessionExpiryInterval);
        WriterExtension.WriteValue<TWriter, uint>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入共享订阅可用属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">共享订阅功能是否可用。</param>
    /// <remarks>
    /// 只有当值为<see langword="false"/>时才写入属性（写入0）。
    /// </remarks>
    public static void WriteSharedSubscriptionAvailable<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBytesWriter

    {
        if (value)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.SharedSubscriptionAvailable);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, 0);
    }

    /// <summary>
    /// 写入订阅标识符属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="subscriptionIdentifier">订阅标识符值。</param>
    public static void WriteSubscriptionIdentifier<TWriter>(ref TWriter writer, uint subscriptionIdentifier)
        where TWriter : IBytesWriter

    {
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.SubscriptionIdentifier);
        WriteVariableByteInteger(ref writer, subscriptionIdentifier);
    }

    /// <summary>
    /// 写入订阅标识符可用属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">订阅标识符功能是否可用。</param>
    /// <remarks>
    /// 只有当值为<see langword="false"/>时才写入属性（写入0）。
    /// </remarks>
    public static void WriteSubscriptionIdentifiersAvailable<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBytesWriter

    {
        if (value)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.SubscriptionIdentifiersAvailable);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, 0);
    }

    /// <summary>
    /// 写入主题别名最大值属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">主题别名最大值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteTopicAliasMaximum<TWriter>(ref TWriter writer, ushort value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.TopicAliasMaximum);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入多个用户属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="userProperties">用户属性集合。</param>
    /// <remarks>
    /// 如果用户属性集合为null，则不写入任何内容。
    /// </remarks>
    public static void WriteUserProperties<TWriter>(ref TWriter writer, IReadOnlyList<MqttUserProperty> userProperties) where TWriter : IBytesWriter

    {
        if (userProperties is null)
        {
            return;
        }
        foreach (var userProperty in userProperties)
        {
            WriteUserProperty(ref writer, userProperty.Name, userProperty.Value);
        }
    }

    /// <summary>
    /// 写入用户属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="name">用户属性的名称。</param>
    /// <param name="value">用户属性的值。</param>
    /// <remarks>
    /// 如果名称或值为空或null，则不写入任何内容。
    /// </remarks>
    public static void WriteUserProperty<TWriter>(ref TWriter writer, string name, string value)
        where TWriter : IBytesWriter

    {
        if (name.IsNullOrEmpty() || value.IsNullOrEmpty())
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.UserProperty);
        WriteMqttInt16String(ref writer, name);
        WriteMqttInt16String(ref writer, value);
    }

    /// <summary>
    /// 写入通配符订阅可用属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">通配符订阅功能是否可用。</param>
    /// <remarks>
    /// 只有当值为<see langword="false"/>时才写入属性（写入0）。
    /// </remarks>
    public static void WriteWildcardSubscriptionAvailable<TWriter>(ref TWriter writer, bool value)
        where TWriter : IBytesWriter

    {
        if (value)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.WildcardSubscriptionAvailable);
        WriterExtension.WriteValue<TWriter, byte>(ref writer, 0);
    }

    /// <summary>
    /// 写入遗嘱延迟间隔属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">遗嘱延迟间隔值（秒）。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteWillDelayInterval<TWriter>(ref TWriter writer, uint value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.WillDelayInterval);
        WriterExtension.WriteValue<TWriter, uint>(ref writer, value, EndianType.Big);
    }

    /// <summary>
    /// 写入字符串类型的MQTT属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="propertyId">属性标识符。</param>
    /// <param name="value">属性值。</param>
    /// <remarks>
    /// 如果值为空或null，则不写入任何内容。此方法用于内部实现，统一处理字符串类型属性的写入。
    /// </remarks>
    private static void WriteStringProperty<TWriter>(ref TWriter writer, MqttPropertyId propertyId, string value)
        where TWriter : IBytesWriter

    {
        if (value.IsNullOrEmpty())
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)propertyId);
        WriteMqttInt16String(ref writer, value);
    }

    /// <summary>
    /// 写入主题别名属性。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器的类型，必须实现<see cref="IBytesWriter"/>接口。</typeparam>
    /// <param name="writer">要写入的字节写入器。</param>
    /// <param name="value">主题别名值。</param>
    /// <remarks>
    /// 如果值为0，则不写入任何内容。
    /// </remarks>
    public static void WriteTopicAlias<TWriter>(ref TWriter writer, ushort value)
        where TWriter : IBytesWriter

    {
        if (value == 0)
        {
            return;
        }
        WriterExtension.WriteValue<TWriter, byte>(ref writer, (byte)MqttPropertyId.TopicAlias);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, value, EndianType.Big);
    }

    #endregion MqttV5Properties
}