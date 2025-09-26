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

using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.Json;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc;

/// <summary>
/// 默认序列化选择器，实现了<see cref="ISerializationSelector"/>接口
/// </summary>
public class DefaultSerializationSelector : ISerializationSelector
{
    private JsonSerializerOptions m_jsonSerializerOptions;

    private bool m_useSystemTextJson;

    /// <summary>
    /// 快速序列化上下文属性
    /// </summary>
    public FastSerializerContext FastSerializerContext { get; set; } = FastBinaryFormatter.DefaultFastSerializerContext;

    /// <summary>
    /// Json序列化配置
    /// </summary>
    public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();

    /// <summary>
    /// 序列化绑定器属性
    /// </summary>
    public SerializationBinder SerializationBinder { get; set; }

    /// <summary>
    /// 根据指定的序列化类型反序列化字节块中的数据。
    /// </summary>
    /// <param name="reader">包含序列化数据的字节块。</param>
    /// <param name="serializationType">指定的序列化类型。</param>
    /// <param name="parameterType">预期反序列化出的对象类型。</param>
    /// <returns>反序列化后的对象。</returns>
    /// <exception cref="RpcException">抛出当未识别序列化类型时。</exception>
    public virtual object DeserializeParameter<TReader>(ref TReader reader, SerializationType serializationType, Type parameterType) where TReader : IBytesReader

    {
        switch (serializationType)
        {
            case SerializationType.FastBinary:
                return FastBinaryFormatter.Deserialize(ref reader, parameterType, this.FastSerializerContext);

            case SerializationType.SystemBinary:
                if (ReaderExtension.ReadIsNull(ref reader))
                {
                    return parameterType.GetDefault();
                }

                using (var block = ReaderExtension.ReadByteBlock(ref reader))
                {
                    return SerializeConvert.BinaryDeserialize(block.AsStream(), this.SerializationBinder);
                }
            case SerializationType.Json:
                {
                    if (ReaderExtension.ReadIsNull(ref reader))
                    {
                        return parameterType.GetDefault();
                    }

                    if (this.m_useSystemTextJson)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize(ReaderExtension.ReadString(ref reader), parameterType, this.m_jsonSerializerOptions);
                    }

                    return JsonConvert.DeserializeObject(ReaderExtension.ReadString(ref reader), parameterType, this.JsonSerializerSettings);
                }
            case SerializationType.Xml:
                if (ReaderExtension.ReadIsNull(ref reader))
                {
                    return parameterType.GetDefault();
                }
                return SerializeConvert.XmlDeserializeFromBytes(ReaderExtension.ReadByteSpan(ref reader).ToArray(), parameterType);

            default:
                throw new RpcException("未指定的反序列化方式");
        }
    }

    /// <summary>
    /// 序列化参数
    /// </summary>
    /// <param name="writer">字节块引用，用于存储序列化后的数据</param>
    /// <param name="serializationType">序列化类型，决定了使用哪种方式序列化</param>
    /// <param name="parameter">待序列化的参数对象</param>
    /// <typeparam name="TWriter">字节块类型，必须实现IByteBlock接口</typeparam>
    public virtual void SerializeParameter<TWriter>(ref TWriter writer, SerializationType serializationType, in object parameter) where TWriter : IBytesWriter

    {
        switch (serializationType)
        {
            case SerializationType.FastBinary:
                {
                    FastBinaryFormatter.Serialize(ref writer, parameter, this.FastSerializerContext);
                    break;
                }
            case SerializationType.SystemBinary:
                {
                    if (parameter is null)
                    {
                        WriterExtension.WriteNull(ref writer);
                    }
                    else
                    {
                        WriterExtension.WriteNotNull(ref writer);
                        using (var block = new ByteBlock(1024 * 64))
                        {
                            SerializeConvert.BinarySerialize(block.AsStream(), parameter);
                            WriterExtension.WriteByteBlock(ref writer, block);
                        }
                    }
                    break;
                }
            case SerializationType.Json:
                {
                    if (this.m_useSystemTextJson)
                    {
                        if (parameter is null)
                        {
                            WriterExtension.WriteNull(ref writer);
                        }
                        else
                        {
                            WriterExtension.WriteNotNull(ref writer);
                            WriterExtension.WriteString(ref writer, System.Text.Json.JsonSerializer.Serialize(parameter, parameter.GetType(), this.m_jsonSerializerOptions));
                        }
                        return;
                    }
                    if (parameter is null)
                    {
                        WriterExtension.WriteNull(ref writer);
                    }
                    else
                    {
                        WriterExtension.WriteNotNull(ref writer);
                        WriterExtension.WriteString(ref writer, JsonConvert.SerializeObject(parameter, this.JsonSerializerSettings));
                    }
                    break;
                }
            case SerializationType.Xml:
                {
                    if (parameter is null)
                    {
                        WriterExtension.WriteNull(ref writer);
                    }
                    else
                    {
                        WriterExtension.WriteNotNull(ref writer);
                        WriterExtension.WriteByteSpan(ref writer, SerializeConvert.XmlSerializeToBytes(parameter));
                    }
                    break;
                }
            default:
                throw new RpcException("未指定的序列化方式");
        }
    }

    /// <summary>
    /// 使用System.Text.Json进行序列化
    /// </summary>
    /// <param name="options"></param>
    public void UseSystemTextJson(Action<JsonSerializerOptions> options)
    {
        var serializerOptions = new JsonSerializerOptions();
        options.Invoke(serializerOptions);
        this.m_useSystemTextJson = true;
        this.m_jsonSerializerOptions = serializerOptions;
    }
}