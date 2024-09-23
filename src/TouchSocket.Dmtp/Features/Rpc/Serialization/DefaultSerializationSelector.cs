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
using System;
using System.Runtime.Serialization;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 默认序列化选择器，实现了<see cref="ISerializationSelector"/>接口
    /// </summary>
    public sealed class DefaultSerializationSelector : ISerializationSelector
    {
        /// <summary>
        /// 默认序列化选择器的构造函数。
        /// </summary>
        /// <remarks>
        /// 初始化默认的序列化选择器，并设置快速序列化上下文为默认上下文。
        /// </remarks>
        public DefaultSerializationSelector()
        {
            // 初始化快速序列化上下文为默认的上下文
            this.FastSerializerContext = FastBinaryFormatter.DefaultFastSerializerContext;
        }

        /// <summary>
        /// 快速序列化上下文属性
        /// </summary>
        public FastSerializerContext FastSerializerContext { get; set; }

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
        /// <param name="byteBlock">包含序列化数据的字节块。</param>
        /// <param name="serializationType">指定的序列化类型。</param>
        /// <param name="parameterType">预期反序列化出的对象类型。</param>
        /// <returns>反序列化后的对象。</returns>
        /// <exception cref="RpcException">抛出当未识别序列化类型时。</exception>
        public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IByteBlock
        {
            // 根据序列化类型选择不同的反序列化方式
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    // 使用FastBinary格式进行反序列化
                    return FastBinaryFormatter.Deserialize(ref byteBlock, parameterType, this.FastSerializerContext);
                case SerializationType.SystemBinary:
                    // 检查字节块是否为null
                    if (byteBlock.ReadIsNull())
                    {
                        // 如果为null，则返回该类型的默认值
                        return parameterType.GetDefault();
                    }

                    // 使用SystemBinary格式进行反序列化
                    using (var block = byteBlock.ReadByteBlock())
                    {
                        // 将字节块转换为流并进行反序列化
                        return SerializeConvert.BinaryDeserialize(block.AsStream(), SerializationBinder);
                    }
                case SerializationType.Json:
                    // 检查字节块是否为null
                    if (byteBlock.ReadIsNull())
                    {
                        // 如果为null，则返回该类型的默认值
                        return parameterType.GetDefault();
                    }

                    // 使用Json格式进行反序列化
                    return JsonConvert.DeserializeObject(byteBlock.ReadString(), parameterType, this.JsonSerializerSettings);

                case SerializationType.Xml:
                    // 检查字节块是否为null
                    if (byteBlock.ReadIsNull())
                    {
                        // 如果为null，则返回该类型的默认值
                        return parameterType.GetDefault();
                    }
                    // 使用Xml格式进行反序列化
                    return SerializeConvert.XmlDeserializeFromBytes(byteBlock.ReadBytesPackage(), parameterType);
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
        public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IByteBlock
        {
            // 根据序列化类型选择不同的序列化方法
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        // 使用FastBinaryFormatter进行序列化
                        FastBinaryFormatter.Serialize(ref byteBlock, parameter, this.FastSerializerContext);
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
                            byteBlock.WriteString(JsonConvert.SerializeObject(parameter, JsonSerializerSettings));
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
                default:
                    // 抛出异常，提示未指定的序列化方式
                    throw new RpcException("未指定的序列化方式");
            }
        }
    }
}