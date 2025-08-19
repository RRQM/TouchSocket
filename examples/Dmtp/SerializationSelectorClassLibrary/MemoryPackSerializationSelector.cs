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

using MemoryPack;
using Newtonsoft.Json;
using System;
using System.IO;
using TouchSocket.Core;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;

namespace SerializationSelectorClassLibrary;

public class MemoryPackSerializationSelector : ISerializationSelector
{
    public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IBytesReader
    {
        var len = ReaderExtension.ReadValue<TByteBlock, int>(ref byteBlock);
        var span = byteBlock.ReadToSpan(len);
        return MemoryPackSerializer.Deserialize(parameterType, span);
    }

    public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IBytesWriter
    {
        var pos = byteBlock.Position;
        byteBlock.Seek(4, SeekOrigin.Current);
        var memoryPackWriter = new MemoryPackWriter<TByteBlock>(ref byteBlock, null);

        MemoryPackSerializer.Serialize(parameter.GetType(), ref memoryPackWriter, parameter);

        var newPos = byteBlock.Position;
        byteBlock.Position = pos;
        WriterExtension.WriteValue(ref byteBlock,(int)memoryPackWriter.WrittenCount);
        byteBlock.Position = newPos;
    }
}

internal sealed class DefaultSerializationSelector : ISerializationSelector
{
    /// <summary>
    /// 根据指定的序列化类型反序列化字节块中的数据。
    /// </summary>
    /// <param name="byteBlock">包含序列化数据的字节块。</param>
    /// <param name="serializationType">指定的序列化类型。</param>
    /// <param name="parameterType">预期反序列化出的对象类型。</param>
    /// <returns>反序列化后的对象。</returns>
    /// <exception cref="RpcException">抛出当未识别序列化类型时。</exception>
    public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IBytesReader
    {
        // 根据序列化类型选择不同的反序列化方式
        switch (serializationType)
        {
            case SerializationType.FastBinary:
                // 使用FastBinary格式进行反序列化
                return FastBinaryFormatter.Deserialize(ref byteBlock, parameterType);
            case SerializationType.SystemBinary:
                // 检查字节块是否为null
                if (ReaderExtension.ReadIsNull<TByteBlock>(ref byteBlock))
                {
                    // 如果为null，则返回该类型的默认值
                    return parameterType.GetDefault();
                }

                // 使用SystemBinary格式进行反序列化
                using (var block = byteBlock.ReadByteBlock())
                {
                    // 将字节块转换为流并进行反序列化
                    return SerializeConvert.BinaryDeserialize(block.AsStream());
                }
            case SerializationType.Json:
                // 检查字节块是否为null
                if (ReaderExtension.ReadIsNull<TReader>(ref byteBlock))
                {
                    // 如果为null，则返回该类型的默认值
                    return parameterType.GetDefault();
                }

                // 使用Json格式进行反序列化
                return JsonConvert.DeserializeObject(ReaderExtension.ReadString<TReader>(ref byteBlock), parameterType);

            case SerializationType.Xml:
                // 检查字节块是否为null
                if (ReaderExtension.ReadIsNull<TReader>(ref byteBlock))
                {
                    // 如果为null，则返回该类型的默认值
                    return parameterType.GetDefault();
                }
                // 使用Xml格式进行反序列化
                return SerializeConvert.XmlDeserializeFromBytes(byteBlock.ReadBytesPackage(), parameterType);
            case (SerializationType)4:
                {
                    var len = ReaderExtension.ReadValue<TReader,int>(ref byteBlock);
                    var span = byteBlock.ReadToSpan(len);
                    return MemoryPackSerializer.Deserialize(parameterType, span);
                }
            default:
                // 如果序列化类型未识别，则抛出异常
                throw new RpcException("未指定的反序列化方式");
        }
    }

    /// <summary>
    /// 序列化参数
    /// </summary>
    /// <param name="byteBlock">字节块引用，用于存储序列化后的数据</param>
    /// <param name="serializationType">序列化类型，决定了使用哪种方式序列化</param>
    /// <param name="parameter">待序列化的参数对象</param>
    /// <typeparam name="TByteBlock">字节块类型，必须实现IByteBlock接口</typeparam>
    public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IBytesWriter
    {
        // 根据序列化类型选择不同的序列化方法
        switch (serializationType)
        {
            case SerializationType.FastBinary:
                {
                    // 使用FastBinaryFormatter进行序列化
                    FastBinaryFormatter.Serialize(ref byteBlock, parameter);
                    break;
                }
            case SerializationType.SystemBinary:
                {
                    // 参数为null时，写入空值标记
                    if (parameter is null)
                    {
                        byteBlock.WriteNull();
                    }
                    else
                    {
                        // 参数不为null时，标记并序列化参数
                        byteBlock.WriteNotNull();
                        using (var block = new ByteBlock(1024 * 64))
                        {
                            // 使用System.Runtime.Serialization.BinaryFormatter进行序列化
                            SerializeConvert.BinarySerialize(block.AsStream(), parameter);
                            // 将序列化后的字节块写入byteBlock
                            byteBlock.WriteByteBlock(block);
                        }
                    }
                    break;
                }
            case SerializationType.Json:
                {
                    // 参数为null时，写入空值标记
                    if (parameter is null)
                    {
                        byteBlock.WriteNull();
                    }
                    else
                    {
                        // 参数不为null时，标记并转换为JSON字符串
                        byteBlock.WriteNotNull();
                        WriterExtension.WriteString(ref byteBlock,(string)JsonConvert.SerializeObject(parameter));
                    }
                    break;
                }
            case SerializationType.Xml:
                {
                    // 参数为null时，写入空值标记
                    if (parameter is null)
                    {
                        byteBlock.WriteNull();
                    }
                    else
                    {
                        // 参数不为null时，标记并转换为Xml字节
                        byteBlock.WriteNotNull();
                        byteBlock.WriteBytesPackage(SerializeConvert.XmlSerializeToBytes(parameter));
                    }
                    break;
                }
            case (SerializationType)4:
                {
                    var pos = byteBlock.Position;
                    byteBlock.Seek(4, SeekOrigin.Current);
                    var memoryPackWriter = new MemoryPackWriter<TByteBlock>(ref byteBlock, null);

                    MemoryPackSerializer.Serialize(parameter.GetType(), ref memoryPackWriter, parameter);

                    var newPos = byteBlock.Position;
                    byteBlock.Position = pos;
                    WriterExtension.WriteValue(ref byteBlock,(int)memoryPackWriter.WrittenCount);
                    byteBlock.Position = newPos;

                    break;
                }
            default:
                // 抛出异常，提示未指定的序列化方式
                throw new RpcException("未指定的序列化方式");
        }
    }
}