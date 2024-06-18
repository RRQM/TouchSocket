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
using System.Buffers;
using System.Runtime.Serialization;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Rpc;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// 默认序列化选择器
    /// </summary>
    public sealed class DefaultSerializationSelector : ISerializationSelector
    {
        public DefaultSerializationSelector()
        {
            this.FastSerializerContext = FastBinaryFormatter.DefaultFastSerializerContext;
        }

        public FastSerializerContext FastSerializerContext { get; set; }
        public SerializationBinder SerializationBinder { get; set; }

        public object DeserializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, Type parameterType) where TByteBlock : IByteBlock
        {
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        return FastBinaryFormatter.Deserialize(ref byteBlock, parameterType, this.FastSerializerContext);
                    }
                case SerializationType.SystemBinary:
                    {
                        if (byteBlock.ReadIsNull())
                        {
                            return parameterType.GetDefault();
                        }

                        using (var block = byteBlock.ReadByteBlock())
                        {
                            return SerializeConvert.BinaryDeserialize(block.AsStream(), SerializationBinder);
                        }
                    }
                case SerializationType.Json:
                    {
                        if (byteBlock.ReadIsNull())
                        {
                            return parameterType.GetDefault();
                        }

                        return JsonConvert.DeserializeObject(byteBlock.ReadString(), parameterType);
                    }
                default:
                    throw new RpcException("未指定的反序列化方式");
            }
        }

        public void SerializeParameter<TByteBlock>(ref TByteBlock byteBlock, SerializationType serializationType, in object parameter) where TByteBlock : IByteBlock
        {
            switch (serializationType)
            {
                case SerializationType.FastBinary:
                    {
                        FastBinaryFormatter.Serialize(ref byteBlock, parameter);
                        break;
                    }
                case SerializationType.SystemBinary:
                    {
                        if (parameter is null)
                        {
                            byteBlock.WriteNull();
                        }
                        else
                        {
                            byteBlock.WriteNotNull();
                            using (var block = new ByteBlock(1024 * 64))
                            {
                                SerializeConvert.BinarySerialize(block.AsStream(), parameter);
                                byteBlock.WriteByteBlock(block);
                            }
                        }
                        break;
                    }
                case SerializationType.Json:
                    {
                        if (parameter is null)
                        {
                            byteBlock.WriteNull();
                        }
                        else
                        {
                            byteBlock.WriteNotNull();
                            byteBlock.WriteString(JsonConvert.SerializeObject(parameter));
                        }
                        break;
                    }
                default:
                    throw new RpcException("未指定的序列化方式");
            }
        }
    }
}